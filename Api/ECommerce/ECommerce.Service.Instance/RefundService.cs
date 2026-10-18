using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Order;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.Dto.Response.Order;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class RefundService : Base_Service, IRefundService
    {
        private readonly _111Context _db;

        public RefundService(_111Context db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// 可退款条件：买家已支付，且订单未取消/未退款；同一订单不允许重复退款。
        /// 数据库 order_status 实际值：1待接单 2备货中 3配送中 4已签收 9已取消/拒绝。
        /// </summary>
        private static bool IsRefundable(ClientOrder order) =>
            order.PayStatus == PayStatus.Paid && order.OrderStatus != OrderStatus.Cancelled;

        private static string OrderStatusText(int? status) => status switch
        {
            0 => "待付款",
            1 => "已付款",
            2 => "已发货",
            3 => "已完成",
            4 => "已取消",
            5 => "退款中",
            6 => "已退款",
            7 => "已驳回",
            _ => "未知"
        };

        public async Task<Api_Response> ApplyAsync(string userId, ApplyRefundRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId)) return Fail("用户未登录");
            if (string.IsNullOrWhiteSpace(dto.OrderId)) return Fail("订单ID不能为空");
            if (dto.RefundAmount <= 0) return Fail("退款金额必须大于0");

            var order = await _db.ClientOrders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.ClientId == userId);
            if (order == null) return Fail("订单不存在");

            if (!IsRefundable(order))
                return Fail("当前订单状态不可申请退款");

            // 同一订单不允许重复退款（已有一条成功退款记录即不可再退）
            var existed = await _db.OrderRefunds
                .AsNoTracking()
                .AnyAsync(r => r.OrderId == order.Id && r.UserId == userId && r.RefundStatus == 1);
            if (existed) return Fail("该订单已退款，不可重复申请");

            if (dto.RefundAmount > (decimal)(order.TotalAmount ?? 0))
                return Fail("退款金额不能超过订单实付金额");

            int type = dto.RefundType == 1 ? 1 : 2;

            // 解析关联商品（整单或指定商品）：明细已从 client_order_item 合并进 client_order.items_json
            var orderItems = order.ParseItems();
            ClientOrderItemDto? targetItem = null;
            if (!string.IsNullOrWhiteSpace(dto.GoodsId))
            {
                targetItem = orderItems
                    .FirstOrDefault(i => i.GoodsId == dto.GoodsId);
            }
            targetItem ??= orderItems
                .OrderBy(i => i.CreateTime)
                .FirstOrDefault();

            int refundNum = dto.RefundNum
                ?? (type == 2 && targetItem != null ? (targetItem.BuyNum ?? 1) : 1);

            var now = DateTime.Now;
            var refundId = Guid.NewGuid().ToString("N");
            var refundNo = $"RF{now:yyyyMMddHHmmssfff}{new Random().Next(100, 999)}";

            var refund = new OrderRefund
            {
                Id = refundId,
                OrderId = order.Id,
                UserId = userId,
                RefundNo = refundNo,
                RefundAmount = dto.RefundAmount,
                RefundReason = dto.RefundReason,
                RefundType = type,
                RefundNum = type == 2 ? refundNum : (int?)null,
                GoodsId = targetItem?.GoodsId,
                GoodsName = targetItem?.GoodsName,
                GoodsImg = targetItem?.GoodsImg,
                // 提交即退款成功，无需审核
                RefundStatus = 1,
                HandleTime = now,
                CreateTime = now
            };

            _db.OrderRefunds.Add(refund);

            // 订单置为已取消（退款成功），并同步对账账单状态
            order.OrderStatus = OrderStatus.Cancelled;
            order.UpdateTime = now;

            // 库存回退：未发货释放锁定，已发货且退货则回仓
            await RestoreStockOnRefundAsync(order.Id, order.OrderNo ?? string.Empty, (refund.RefundType ?? 2) == 2);

            // 同步 merchant_bill（如果已生成）为已取消，确保余额统计即时扣减
            var bill = await _db.MerchantBills
                .FirstOrDefaultAsync(b => b.OrderNo == order.OrderNo && b.MerchantId == order.MerchantId);
            if (bill != null)
            {
                bill.OrderStatus = OrderStatus.Cancelled;
                bill.UpdateTime = now;
            }

            await _db.SaveChangesAsync();

            return Ok("退款成功", new
            {
                id = refundId,
                refundNo,
                refundType = type,
                refundStatus = 1
            });
        }

        public async Task<Api_Response> GetMyRefundsAsync(string userId, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var query = _db.OrderRefunds.AsNoTracking().Where(r => r.UserId == userId);
            var total = await query.CountAsync();
            var list = await query
                .OrderByDescending(r => r.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderIds = list.Select(r => r.OrderId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList()!;
            var orderNos = await _db.ClientOrders.AsNoTracking()
                .Where(o => orderIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.OrderNo);

            var result = list.Select(r => new RefundListItemDto
            {
                Id = r.Id,
                RefundNo = r.RefundNo,
                OrderId = r.OrderId,
                OrderNo = r.OrderId != null && orderNos.TryGetValue(r.OrderId, out var no) ? no : null,
                RefundAmount = r.RefundAmount ?? 0m,
                RefundType = r.RefundType ?? 2,
                RefundTypeText = RefundTypeText.Of(r.RefundType),
                RefundStatus = r.RefundStatus ?? 0,
                RefundStatusText = RefundStatusText.Of(r.RefundStatus),
                RefundReason = r.RefundReason,
                GoodsName = r.GoodsName,
                GoodsImg = r.GoodsImg,
                CreateTime = r.CreateTime
            }).ToList();

            return Ok("ok", new { total, list = result });
        }

        public async Task<Api_Response> GetDetailAsync(string userId, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return Fail("退款单ID不能为空");

            var refund = await _db.OrderRefunds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (refund == null) return Fail("退款单不存在");

            var order = await _db.ClientOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == refund.OrderId);
            var items = (order?.ParseItems() ?? new List<ClientOrderItemDto>())
                .OrderBy(i => i.CreateTime)
                .ToList();

            var detail = new RefundDetailDto
            {
                Id = refund.Id,
                RefundNo = refund.RefundNo,
                OrderId = refund.OrderId,
                OrderNo = order?.OrderNo,
                RefundAmount = refund.RefundAmount ?? 0m,
                RefundType = refund.RefundType ?? 2,
                RefundTypeText = RefundTypeText.Of(refund.RefundType),
                RefundStatus = refund.RefundStatus ?? 0,
                RefundStatusText = RefundStatusText.Of(refund.RefundStatus),
                RefundReason = refund.RefundReason,
                RefundNum = refund.RefundNum,
                GoodsName = refund.GoodsName,
                GoodsImg = refund.GoodsImg,
                HandleRemark = refund.HandleRemark,
                HandleTime = refund.HandleTime,
                CreateTime = refund.CreateTime,
                OrderTotal = ParseDecimal(order?.TotalAmount),
                OrderStatus = order?.OrderStatus ?? OrderStatus.PendingPayment,
                OrderStatusText = OrderStatusText((int?)(order?.OrderStatus)),
                Items = items.Select(i => new RefundOrderItemDto
                {
                    GoodsId = i.GoodsId,
                    GoodsName = i.GoodsName,
                    GoodsImg = i.GoodsImg,
                    BuyNum = i.BuyNum ?? 0,
                    BuyPrice = ParseDecimal(i.BuyPrice),
                    SubAmount = ParseDecimal(i.SubAmount)
                }).ToList(),
                Return = null
            };

            return Ok("ok", detail);
        }

        public async Task<Api_Response> ReviewAsync(string adminId, RefundReviewRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Id)) return Fail("退款单ID不能为空");

            var refund = await _db.OrderRefunds.FirstOrDefaultAsync(r => r.Id == dto.Id);
            if (refund == null) return Fail("退款单不存在");
            if (refund.RefundStatus != 0) return Fail("该退款单已处理，不能重复审核");

            var now = DateTime.Now;
            refund.HandleRemark = dto.Remark;
            refund.HandleTime = now;
            refund.HandleBy = adminId;

            if (dto.Approve)
            {
                refund.RefundStatus = 1; // 已退款（退货退款场景下即「已退货」）
                // 审核通过：订单状态置为「已退款」
                if (!string.IsNullOrEmpty(refund.OrderId))
                {
                    var order = await _db.ClientOrders.FirstOrDefaultAsync(o => o.Id == refund.OrderId);
                    if (order != null)
                    {
                        order.OrderStatus = OrderStatus.Refunded; // 已退款
                        // 退款/退货完成：对称加回库存（与下单锁定、发货出库对应）
                        await RestoreStockOnRefundAsync(refund.OrderId, order.OrderNo ?? string.Empty, (refund.RefundType ?? 2) == 2);
                    }
                }
            }
            else
            {
                refund.RefundStatus = 2; // 驳回
                // 审核驳回：订单状态置为「已驳回」
                if (!string.IsNullOrEmpty(refund.OrderId))
                {
                    var order = await _db.ClientOrders.FirstOrDefaultAsync(o => o.Id == refund.OrderId);
                    if (order != null) order.OrderStatus = OrderStatus.Rejected; // 已驳回
                }
            }

            await _db.SaveChangesAsync();

            return Ok(dto.Approve ? "已同意退款" : "已驳回退款", new
            {
                id = refund.Id,
                refundStatus = refund.RefundStatus,
                returnStatus = refund.RefundStatus
            });
        }

        /// <summary>
        /// 退款/退货完成时对称加回库存，与下单锁定、发货出库互为逆操作：
        /// - 未发货（货从未出仓）：释放下单时锁定的预留（Stock.LockStock -=），并恢复买家端可购买库存（Good.Stock / GoodsSku.Stock +=）
        /// - 已发货且退货（货物理回仓）：库存表总库存加回（Stock.TotalStock +=）并写 StockIn 入库记录，同时恢复买家端可购买库存
        /// - 已发货仅退款（用户留货）：货未回仓，不加任何库存
        /// 仅处理存在 Stock 记录的商品（与 CreateOrderAsync / Delivery 的锁定、出库逻辑对称）。
        /// 注意：本方法只暂存变更，由调用方末尾统一 _db.SaveChangesAsync() 原子提交。
        /// </summary>
        private async Task RestoreStockOnRefundAsync(string orderId, string orderNo, bool isReturn)
        {
            // 合并后明细内联在 client_order.items_json，加载订单后直接解析
            var order = await _db.ClientOrders.FindAsync(orderId);
            var items = order?.ParseItems() ?? new List<ClientOrderItemDto>();

            // 是否已发货出库：以是否存在该订单的 StockOut 记录判断（与 Delivery 写 StockOut 对称）
            bool shipped = await _db.StockOuts.AnyAsync(s => s.BatchNo == orderNo);

            // 已发货且仅退款（用户留货）：货未回仓，不加库存
            if (shipped && !isReturn) return;

            // 1) 恢复买家端可购买库存（下单时已扣减，与锁定逻辑对称）
            foreach (var it in items)
            {
                int qty = it.BuyNum ?? 0;
                if (qty <= 0 || string.IsNullOrWhiteSpace(it.GoodsId)) continue;

                var goods = await _db.Goods.FirstOrDefaultAsync(g => g.Id == it.GoodsId);
                if (goods != null)
                {
                    goods.Stock += qty;
                    goods.UpdateTime = DateTime.Now;
                }

                if (!string.IsNullOrWhiteSpace(it.SpecId))
                {
                    var sku = await _db.GoodsSkus
                        .FirstOrDefaultAsync(s => s.Id == it.SpecId && s.GoodsId == it.GoodsId);
                    if (sku != null) sku.Stock += qty;
                }
            }

            // 2) 库存表：按商品聚合处理（未发货释放预留；已发货退货则货回仓）
            var goodsQty = items
                .Where(i => !string.IsNullOrWhiteSpace(i.GoodsId) && (i.BuyNum ?? 0) > 0)
                .GroupBy(i => i.GoodsId!)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.BuyNum ?? 0));

            foreach (var kv in goodsQty)
            {
                var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.GoodsId == kv.Key);
                if (stock == null) continue;

                if (shipped)
                {
                    // 已发货且退货：货物理回仓
                    stock.TotalStock = (stock.TotalStock ?? 0) + kv.Value;
                    stock.UpdateTime = DateTime.Now;

                    _db.StockIns.Add(new StockIn
                    {
                        Id = Config.NewSequentialGuid(),
                        GoodsId = kv.Key,
                        InNum = kv.Value,
                        BatchNo = orderNo,
                        CreateTime = DateTime.Now
                    });
                }
                else
                {
                    // 未发货：释放下单时锁定的预留
                    stock.LockStock = Math.Max(0, (stock.LockStock ?? 0) - kv.Value);
                    stock.UpdateTime = DateTime.Now;
                }
            }
        }

        public async Task<Api_Response> GetAdminListAsync(int page = 1, int pageSize = 20, int? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var query = _db.OrderRefunds.AsNoTracking().AsQueryable();
            if (status.HasValue) query = query.Where(r => r.RefundStatus == status.Value);

            var total = await query.CountAsync();
            var list = await query
                .OrderByDescending(r => r.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderIds = list.Select(r => r.OrderId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList()!;
            var orderNos = await _db.ClientOrders.AsNoTracking()
                .Where(o => orderIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.OrderNo);

            var result = list.Select(r => new RefundListItemDto
            {
                Id = r.Id,
                RefundNo = r.RefundNo,
                OrderId = r.OrderId,
                OrderNo = r.OrderId != null && orderNos.TryGetValue(r.OrderId, out var no) ? no : null,
                RefundAmount = r.RefundAmount ?? 0m,
                RefundType = r.RefundType ?? 2,
                RefundTypeText = RefundTypeText.Of(r.RefundType),
                RefundStatus = r.RefundStatus ?? 0,
                RefundStatusText = RefundStatusText.Of(r.RefundStatus),
                RefundReason = r.RefundReason,
                GoodsName = r.GoodsName,
                GoodsImg = r.GoodsImg,
                CreateTime = r.CreateTime
            }).ToList();

            return Ok("ok", new { total, list = result });
        }

        private decimal ParseDecimal(double? value) => value.HasValue ? (decimal)value.Value : 0m;

        /// <summary>
        /// 后台退款详情：不限定 userId，管理员可看任意退款单
        /// </summary>
        public async Task<Api_Response> GetAdminDetailAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return Fail("退款单ID不能为空");

            var refund = await _db.OrderRefunds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
            if (refund == null) return Fail("退款单不存在");

            var order = await _db.ClientOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == refund.OrderId);
            var items = (order?.ParseItems() ?? new List<ClientOrderItemDto>())
                .OrderBy(i => i.CreateTime)
                .ToList();

            // 处理人姓名查一下（HandleBy 存的是 UId）
            string? handleByName = null;
            if (!string.IsNullOrEmpty(refund.HandleBy))
            {
                var u = await _db.SysUsers.AsNoTracking()
                    .Where(x => x.UId == refund.HandleBy || x.UAccount == refund.HandleBy || x.UName == refund.HandleBy)
                    .FirstOrDefaultAsync();
                handleByName = u?.UName ?? u?.UAccount ?? refund.HandleBy;
            }

            var detail = new RefundDetailDto
            {
                Id = refund.Id,
                RefundNo = refund.RefundNo,
                OrderId = refund.OrderId,
                OrderNo = order?.OrderNo,
                RefundAmount = refund.RefundAmount ?? 0m,
                RefundType = refund.RefundType ?? 2,
                RefundTypeText = RefundTypeText.Of(refund.RefundType),
                RefundStatus = refund.RefundStatus ?? 0,
                RefundStatusText = RefundStatusText.Of(refund.RefundStatus),
                RefundReason = refund.RefundReason,
                RefundNum = refund.RefundNum,
                GoodsName = refund.GoodsName,
                GoodsImg = refund.GoodsImg,
                HandleRemark = refund.HandleRemark,
                HandleTime = refund.HandleTime,
                CreateTime = refund.CreateTime,
                OrderTotal = ParseDecimal(order?.TotalAmount),
                OrderStatus = order?.OrderStatus ?? OrderStatus.PendingPayment,
                OrderStatusText = OrderStatusText((int?)(order?.OrderStatus)),
                Items = items.Select(i => new RefundOrderItemDto
                {
                    GoodsId = i.GoodsId,
                    GoodsName = i.GoodsName,
                    GoodsImg = i.GoodsImg,
                    BuyNum = i.BuyNum ?? 0,
                    BuyPrice = ParseDecimal(i.BuyPrice),
                    SubAmount = ParseDecimal(i.SubAmount)
                }).ToList(),
                Return = null
            };

            return Ok("ok", new { detail, handleByName });
        }
    }
}
