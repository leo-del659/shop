using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class AdminOrderService : Base_Service, IAdminOrderService
    {
        private readonly ClientOrder_EFCore _order_EFCore;
        private readonly OrderPay_EFCore _pay_EFCore;
        private readonly OrderDelivery_EFCore _delivery_EFCore;
        private readonly OrderRefund_EFCore _refund_EFCore;
        private readonly OrderStatusLog_EFCore _statusLog_EFCore;
        private readonly _111Context _db;

        public AdminOrderService(
            ClientOrder_EFCore order_EFCore,
            OrderPay_EFCore pay_EFCore,
            OrderDelivery_EFCore delivery_EFCore,
            OrderRefund_EFCore refund_EFCore,
            OrderStatusLog_EFCore statusLog_EFCore,
            _111Context db)
        {
            _order_EFCore = order_EFCore;
            _pay_EFCore = pay_EFCore;
            _delivery_EFCore = delivery_EFCore;
            _refund_EFCore = refund_EFCore;
            _statusLog_EFCore = statusLog_EFCore;
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<Api_Response> GetList(int page, int size, string? keyword, int? status, string? clientId = null)
        {
            int total;
            // 先取分页后的主单（避免在 Select 投影里嵌套 IQueryable，EF Core 无法翻译）
            var orders = _order_EFCore
                .Query(out total, page, size, false, o => o.CreateTime,
                    o => (string.IsNullOrEmpty(keyword) || (o.OrderNo != null && o.OrderNo.Contains(keyword)))
                       && (!status.HasValue || o.OrderStatus == (OrderStatus?)status)
                       && (string.IsNullOrEmpty(clientId) || o.ClientId == clientId))
                .ToList();

            // 合并后明细内联在 client_order.items_json，直接解析并按 OrderId 分组
            var itemsMap = orders
                .ToDictionary(o => o.Id ?? string.Empty, o => (o.ParseItems() ?? new List<ClientOrderItemDto>())
                    .Select(x => (object)new
                    {
                        x.GoodsId,
                        x.GoodsName,
                        x.BuyNum,
                        x.GoodsImg
                    }).ToList());

            // 批量查买家名称（订单编辑弹窗需要展示只读用户字段）
            // 小程序订单(ORD开头)的 client_id 即买家 userId（sys_users.u_id），名称从 sys_users 解析
            var clientIds = orders.Select(o => o.ClientId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
            var clientMap = _db.SysUsers
                .Where(m => clientIds.Contains(m.UId))
                .Select(m => new { m.UId, m.UName })
                .ToList()
                .ToDictionary(m => m.UId ?? string.Empty, m => m.UName);

            // 批量查退款原因：一笔订单可能有多条退款记录，取最新一条的原因
            var orderIdList = orders.Select(o => o.Id).ToList();
            var refundReasonMap = _refund_EFCore
                .Query(r => orderIdList.Contains(r.OrderId))
                .Select(r => new { r.OrderId, r.RefundReason, r.CreateTime })
                .ToList()
                .GroupBy(r => r.OrderId)
                .ToDictionary(g => g.Key ?? string.Empty, g => g.OrderByDescending(r => r.CreateTime).First().RefundReason);

            var list = orders.Select(o => new
            {
                id = o.Id,
                orderNo = o.OrderNo,
                clientId = o.ClientId,
                clientName = clientMap.TryGetValue(o.ClientId ?? string.Empty, out var mn) ? mn : null,
                // sell_client_id 列已删除，字段保留（恒为 null）以维持响应结构，避免管理后台列错位
                sellClientId = (string?)null,
                sellClientName = (string?)null,
                merchantId = o.MerchantId,
                totalAmount = o.TotalAmount,
                orderStatus = o.OrderStatus,
                payStatus = o.PayStatus,
                remark = o.Remark,
                goodsInfo = o.GoodsInfo,
                settleType = (int?)null,
                settleChannel = (int?)null,
                createTime = o.CreateTime,
                // 订单商品明细（含快照名称），前端用于展示「商品」列
                items = itemsMap.TryGetValue(o.Id, out var it) ? it : new List<object>(),
                // 最近一次退款原因，退款管理页使用
                refundReason = refundReasonMap.TryGetValue(o.Id, out var rr) ? rr : null,
                // 用户信息（小程序订单）：receiver_name / receiver_mobile 列已删除，仅保留 address
                receiverName = (string?)null,
                receiverMobile = (string?)null,
                address = o.Address
            }).ToList();

            return Task.FromResult(Ok("ok", new { total, list }));
        }

        public Task<Api_Response> Get(string id)
        {
            var t = _order_EFCore.Find(id);
            if (t == null) return Task.FromResult(Fail("订单不存在"));
            var items = new { };
            return Task.FromResult(Ok("ok", t));
        }

        public Task<Api_Response> Add(ClientOrder t)
        {
            t.Id = Config.NewSequentialGuid();
            // 订单号：ORD + yyMMdd + 当日4位序号（如 ORD2608120001），简单且自带序号
            if (string.IsNullOrWhiteSpace(t.OrderNo))
            {
                var prefix = "ORD" + DateTime.Now.ToString("yyMMdd");
                int maxSeq = 0;
                var exist = _order_EFCore
                    .Query(o => o.OrderNo != null && o.OrderNo.StartsWith(prefix))
                    .Select(o => o.OrderNo!)
                    .ToList();
                foreach (var no in exist)
                {
                    var tail = no.Length > prefix.Length ? no.Substring(prefix.Length) : string.Empty;
                    if (int.TryParse(tail, out var n) && n > maxSeq) maxSeq = n;
                }
                t.OrderNo = $"{prefix}{(maxSeq + 1).ToString("D4")}";
            }
            t.OrderStatus = t.OrderStatus ?? OrderStatus.PendingAccept;     // 1 待接单—— 与小程序端状态标准对齐
            t.PayStatus = t.PayStatus ?? PayStatus.Unpaid;         // 0 未支付
            t.CreateTime = DateTime.Now;
            t.UpdateTime = DateTime.Now;
            _order_EFCore.Add(t);
            return Task.FromResult(Get_Result(_order_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Update(ClientOrder t)
        {
            var old = _order_EFCore.Find(t.Id);
            if (old == null) return Task.FromResult(Fail("订单不存在"));
            // 订单号方案已统一，后台编辑直接采用传入的商户
            old.ClientId = t.ClientId;
            old.MerchantId = t.MerchantId;
            old.TotalAmount = t.TotalAmount;
            old.OrderStatus = t.OrderStatus;
            old.Remark = t.Remark;
            old.GoodsInfo = t.GoodsInfo;
            old.UpdateTime = DateTime.Now;
            _order_EFCore.Update(old);
            return Task.FromResult(Get_Result(_order_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Remove(string id)
        {
            var t = _order_EFCore.Find(id);
            if (t == null) return Task.FromResult(Fail("订单不存在"));
            _order_EFCore.Delete(t);
            return Task.FromResult(Get_Result(_order_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Pay(string orderId, string payNo, decimal payAmount, int payType)
        {
            var order = _order_EFCore.Find(orderId);
            if (order == null) return Task.FromResult(Fail("订单不存在"));
            _pay_EFCore.Add(new OrderPay
            {
                Id = Config.NewSequentialGuid(),
                OrderId = orderId,
                PayNo = payNo,
                PayAmount = payAmount,
                PayType = payType,
                PayStatus = PayStatus.Paid,
                CreateTime = DateTime.Now
            });
            order.PayStatus = PayStatus.Paid;        // 已支付
            order.OrderStatus = OrderStatus.Accepted;      // 待发货（已支付，等待发货）—— 与小程序端状态标准对齐
            _order_EFCore.Update(order);
            return Task.FromResult(Get_Result(_order_EFCore.SaveChanges()));
        }

        public async Task<Api_Response> Delivery(string orderId, string logisticsId, string trackingNo)
        {
            // 统一使用注入的 _111Context，避免 _order_EFCore/_delivery_EFCore 与 _db 可能不是同一
            // DbContext 实例导致修改/新增不被同一次 SaveChanges 提交（这也是发货后状态未变更的根因）。
            var order = await _db.ClientOrders.FindAsync(orderId);
            if (order == null) return Fail("订单不存在");
            // 未支付无法发货
            if (order.PayStatus != PayStatus.Paid)
                return Fail("订单未支付，无法发货");
            // 已发货 / 退款中 / 已退款 / 已驳回 / 已取消 均不允许重复或越序发货
            if (order.OrderStatus == OrderStatus.Shipped)
                return Fail("订单已发货，请勿重复发货");
            if (order.OrderStatus == OrderStatus.Refunding || order.OrderStatus == OrderStatus.Refunded || order.OrderStatus == OrderStatus.Rejected || order.OrderStatus == OrderStatus.Cancelled)
                return Fail("退款/驳回/取消流程中的订单无法发货");
            // 快递单号自动生成（与数据库同类型：字符串），若前端未传则兜底生成
            if (string.IsNullOrWhiteSpace(trackingNo))
                trackingNo = $"KD{DateTime.Now:yyyyMMddHHmmssfff}{new Random().Next(100000, 999999)}";

            _db.OrderDeliveries.Add(new OrderDelivery
            {
                Id = Config.NewSequentialGuid(),
                OrderId = orderId,
                LogisticsId = logisticsId,
                TrackingNo = trackingNo,
                DeliveryStatus = 1,
                CreateTime = DateTime.Now
            });

            order.OrderStatus = OrderStatus.Shipped;      // 已发货—— 与小程序端状态标准对齐（3 = 已发货）
            order.UpdateTime = DateTime.Now;

            // 结算出库：将下单时锁定的库存转为真实出库。
            // 同时扣减总库存、释放锁定（LockStock -=），使后台「可用 = 总库存 - 锁定」保持一致；
            // 仅对存在库存记录的商品处理（与 CreateOrderAsync 的锁定逻辑对称）。
            // 注意：不在此处减 Good.Stock / GoodsSku.Stock —— 它们已在下单时扣减，
            // 代表买家端可购买库存，出库时不应再次减少。
            var orderItems = order.ParseItems();
            var goodsQty = orderItems
                .Where(i => !string.IsNullOrWhiteSpace(i.GoodsId) && (i.BuyNum ?? 0) > 0)
                .GroupBy(i => i.GoodsId!)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.BuyNum ?? 0));

            foreach (var kv in goodsQty)
            {
                var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.GoodsId == kv.Key);
                if (stock == null) continue;

                stock.TotalStock = (stock.TotalStock ?? 0) - kv.Value;
                stock.LockStock = Math.Max(0, (stock.LockStock ?? 0) - kv.Value);
                stock.UpdateTime = DateTime.Now;

                _db.StockOuts.Add(new StockOut
                {
                    Id = Config.NewSequentialGuid(),
                    GoodsId = kv.Key,
                    OutNum = kv.Value,
                    BatchNo = order.OrderNo,
                    CreateTime = DateTime.Now
                });
            }

            // 记录订单状态变更日志，便于后续排查
            _db.OrderStatusLogs.Add(new OrderStatusLog
            {
                Id = Config.NewSequentialGuid(),
                OrderId = order.Id,
                OrderStatus = OrderStatus.Shipped,
                Remark = $"后台发货，物流：{logisticsId}，快递单号：{trackingNo}",
                CreateTime = DateTime.Now
            });

            // 统一 _db 提交：订单状态、发货记录、库存扣减、出库记录、状态日志一次性原子提交
            var rows = await _db.SaveChangesAsync();
            return rows > 0 ? Ok("发货成功") : Fail("发货失败，未影响任何数据");
        }

        public Task<Api_Response> Refund(string orderId, string refundNo, decimal refundAmount, string reason)
        {
            var order = _order_EFCore.Find(orderId);
            if (order == null) return Task.FromResult(Fail("订单不存在"));
            // 未支付无法退款
            if (order.PayStatus != PayStatus.Paid)
                return Task.FromResult(Fail("订单未支付，无法退款"));
            // 退款中 / 已退款 不允许重复退款
            if (order.OrderStatus == OrderStatus.Refunding || order.OrderStatus == OrderStatus.Refunded)
                return Task.FromResult(Fail("订单已处于退款流程，请勿重复退款"));
            _refund_EFCore.Add(new OrderRefund
            {
                Id = Config.NewSequentialGuid(),
                OrderId = orderId,
                RefundNo = refundNo,
                RefundAmount = refundAmount,
                RefundReason = reason,
                RefundStatus = 0,
                CreateTime = DateTime.Now
            });
            order.OrderStatus = OrderStatus.Refunding;      // 退款中
            _order_EFCore.Update(order);
            _refund_EFCore.SaveChanges();
            _order_EFCore.SaveChanges();
            return Task.FromResult(Ok("已提交退款申请", new { refundStatus = 0 }));
        }

        public Task<Api_Response> Return(string orderId, string returnNo, int returnNum, string reason)
        {
            var order = _order_EFCore.Find(orderId);
            if (order == null) return Task.FromResult(Fail("订单不存在"));
            // 合并 order_return 后：后台「退货」即退货退款，统一写入 order_refund（RefundType=2）
            _refund_EFCore.Add(new OrderRefund
            {
                Id = Config.NewSequentialGuid(),
                OrderId = orderId,
                RefundNo = returnNo,
                RefundType = 2,
                RefundNum = returnNum,
                RefundReason = reason,
                RefundStatus = 0,
                CreateTime = DateTime.Now
            });
            return Task.FromResult(Get_Result(_refund_EFCore.SaveChanges()));
        }

        /// <summary>
        /// 自动生成订单号：ORD{yyMMdd}{4位当日序号}
        /// 同日现存最大序号 + 1，例如 ORD2608120001
        /// </summary>
        public Task<Api_Response> GenerateOrderNo()
        {
            var now = DateTime.Now;
            string prefix = "ORD" + now.ToString("yyMMdd");
            int maxSeq = 0;
            var exist = _order_EFCore
                .Query(o => o.OrderNo != null && o.OrderNo.StartsWith(prefix))
                .Select(o => o.OrderNo!)
                .ToList();
            foreach (var no in exist)
            {
                var tail = no.Length > prefix.Length ? no.Substring(prefix.Length) : string.Empty;
                if (int.TryParse(tail, out var n) && n > maxSeq) maxSeq = n;
            }
            int seq = maxSeq + 1;
            string orderNo = $"{prefix}{seq.ToString("D4")}";
            return Task.FromResult(Ok("ok", new { orderNo }));
        }

        /// <summary>
        /// 管理首页多维销售分析看板。
        /// 一次聚合多个维度，避免前端多次零散请求：
        ///   1) 销售周趋势（近七周，已支付订单按付款周汇总）
        ///   2) 分类销售占比（商户账单 GoodsId → Good.CategoryId → 分类名）
        ///   3) 退款趋势（近七周，已退款单按创建周汇总）
        ///   4) 商户销售排行 Top10（商户账单按商户汇总金额）
        /// 所有金额统一转为 decimal 并四舍五入到分，保证前端展示一致。
        /// </summary>
        public async Task<Api_Response> Dashboard()
        {
            // 1) 已支付订单（销售周趋势）
            var paidOrders = await _db.ClientOrders
                .Where(o => o.PayStatus == PayStatus.Paid && o.PayTime != null)
                .Select(o => new { o.PayTime, o.TotalAmount })
                .AsNoTracking()
                .ToListAsync();

            // 2) 商户账单（分类占比 + 商户排行，含 GoodsId/MerchantId/Amount）
            var bills = await _db.MerchantBills
                .Where(b => b.Amount != null)
                .Select(b => new { b.GoodsId, b.MerchantId, b.Amount })
                .AsNoTracking()
                .ToListAsync();

            // 3) 退款（退款周趋势）
            var refunds = await _db.OrderRefunds
                .Select(r => new { r.RefundAmount, r.RefundStatus, r.CreateTime })
                .AsNoTracking()
                .ToListAsync();

            // 商品 → 分类 映射（小表全量缓存）
            var goodsCat = await _db.Goods
                .Select(g => new { g.Id, g.CategoryId })
                .AsNoTracking()
                .ToListAsync();
            var catMap = goodsCat
                .Where(g => !string.IsNullOrEmpty(g.CategoryId))
                .ToDictionary(g => g.Id ?? string.Empty, g => g.CategoryId!);
            var catNames = await _db.GoodsCategories
                .Select(c => new { c.Id, c.CategoryName })
                .AsNoTracking()
                .ToListAsync();
            var catNameMap = catNames.ToDictionary(c => c.Id ?? string.Empty, c => c.CategoryName);

            // 商户名称映射
            var merchantIds = bills
                .Where(b => !string.IsNullOrEmpty(b.MerchantId))
                .Select(b => b.MerchantId!)
                .Distinct()
                .ToList();
            var merchantNames = await _db.Merchants
                .Where(m => merchantIds.Contains(m.Id))
                .Select(m => new { m.Id, m.MerchantName })
                .AsNoTracking()
                .ToListAsync();
            var merchantNameMap = merchantNames.ToDictionary(m => m.Id ?? string.Empty, m => m.MerchantName);

            // 周维度（近七周，周一为起点）
            var now = DateTime.Now;
            var baseWeek = GetWeekStart(now);
            var weeks = Enumerable.Range(0, 7)
                .Select(i => baseWeek.AddDays(-(6 - i) * 7))
                .ToList();

            var salesTrend = weeks.Select(w =>
            {
                var next = w.AddDays(7);
                var cnt = paidOrders.Count(o => o.PayTime >= w && o.PayTime < next);
                var amt = paidOrders
                    .Where(o => o.PayTime >= w && o.PayTime < next)
                    .Sum(o => (decimal?)(o.TotalAmount ?? 0)) ?? 0m;
                return new
                {
                    label = $"{w.Month}/{w.Day}周",
                    orderCount = cnt,
                    salesAmount = Math.Round(amt, 2)
                };
            }).ToList();

            var refundTrend = weeks.Select(w =>
            {
                var next = w.AddDays(7);
                var items = refunds.Where(r => r.RefundStatus == 1 && r.CreateTime >= w && r.CreateTime < next);
                return new
                {
                    label = $"{w.Month}/{w.Day}周",
                    refundCount = items.Count(),
                    refundAmount = Math.Round(items.Sum(r => (decimal?)(r.RefundAmount ?? 0)) ?? 0m, 2)
                };
            }).ToList();

            // 分类销售占比（账单金额按商品分类汇总）
            var categorySales = bills
                .Where(b => !string.IsNullOrEmpty(b.GoodsId) && b.Amount != null)
                .GroupBy(b => catMap.TryGetValue(b.GoodsId!, out var cid) ? cid : "__unknown__")
                .Select(g => new
                {
                    categoryId = g.Key,
                    categoryName = g.Key != "__unknown__" && catNameMap.TryGetValue(g.Key, out var cn) && !string.IsNullOrEmpty(cn)
                        ? cn : "未分类",
                    amount = Math.Round(g.Sum(b => b.Amount ?? 0), 2),
                    count = g.Count()
                })
                .OrderByDescending(x => x.amount)
                .ToList();

            // 商户销售排行 Top10
            var topMerchants = bills
                .Where(b => !string.IsNullOrEmpty(b.MerchantId) && b.Amount != null)
                .GroupBy(b => b.MerchantId!)
                .Select(g => new
                {
                    merchantId = g.Key,
                    merchantName = merchantNameMap.TryGetValue(g.Key, out var mn) && !string.IsNullOrEmpty(mn) ? mn : g.Key,
                    amount = Math.Round(g.Sum(b => b.Amount ?? 0), 2),
                    orderCount = g.Count()
                })
                .OrderByDescending(x => x.amount)
                .Take(10)
                .ToList();

            return Ok("ok", new
            {
                salesTrend,
                refundTrend,
                categorySales,
                topMerchants
            });
        }

        /// <summary>
        /// 取某天所在周的开始（周一 00:00:00），与前端看板周口径一致
        /// </summary>
        private static DateTime GetWeekStart(DateTime d)
        {
            var dt = d.Date;
            int day = (int)dt.DayOfWeek;
            if (day == 0) day = 7; // 周日记为 7
            return dt.AddDays(-(day - 1));
        }

    }
}
