using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Order;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.Dto.Response.Order;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.MQ;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class OrderService : Base_Service, IOrderService
    {
        private readonly _111Context _db;
        private readonly IRealtimeNotifier _notifier;
        private readonly IOrderEventPublisher _publisher;
        private readonly RabbitMQOptions _mqOptions;

        public OrderService(_111Context db, IRealtimeNotifier notifier, IOrderEventPublisher publisher, IOptions<RabbitMQOptions> mqOptions)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _mqOptions = mqOptions?.Value ?? throw new ArgumentNullException(nameof(mqOptions));
        }

        public async Task<Api_Response> CreateOrderAsync(string userId, CreateOrderRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("用户未登录");
            }

            if (dto == null)
            {
                return Fail("订单参数不能为空");
            }

            if (dto.Items == null || dto.Items.Count == 0)
            {
                return Fail("订单商品不能为空");
            }

            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.GoodsId))
                {
                    return Fail("商品ID不能为空");
                }

                if (item.BuyNum <= 0)
                {
                    return Fail("购买数量必须大于0");
                }
            }

            var now = DateTime.Now;
            var orderId = Guid.NewGuid().ToString("N");
            // 订单号：ORD + yyMMdd + 当日4位序号（如 ORD2608120001），简单且自带序号
            var prefix = "ORD" + now.ToString("yyMMdd");
            int maxSeq = 0;
            var exist = _db.ClientOrders
                .Where(o => o.OrderNo != null && o.OrderNo.StartsWith(prefix))
                .Select(o => o.OrderNo!)
                .ToList();
            foreach (var no in exist)
            {
                var tail = no.Length > prefix.Length ? no.Substring(prefix.Length) : string.Empty;
                if (int.TryParse(tail, out var n) && n > maxSeq) maxSeq = n;
            }
            var orderNo = $"{prefix}{(maxSeq + 1).ToString("D4")}";

            decimal totalAmount = 0m;
            // 供货商户：通过 MerchantGood 关联表从下单商品解析，使商户端 /api/merchant/orders 能查到该订单。
            // 注：client_order 仅单列 MerchantId，多商户混购时取首个解析到的商户（现有数据模型约束）。
            string? orderMerchantId = null;
            var orderItems = new List<ClientOrderItemDto>();
            var responseItems = new List<OrderItemDto>();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in dto.Items)
                {
                    // 消费侧商品链路已统一到 good 表，避免 goods_info 双表不一致导致下单失败
                    var goods = await _db.Goods.FirstOrDefaultAsync(g => g.Id == item.GoodsId);

                    if (goods == null)
                    {
                        await transaction.RollbackAsync();
                        return Fail($"商品不存在：{item.GoodsId}");
                    }

                    // good 表已移除 client_id：售卖商户不再从商品取，改为取订单自身的 clientId（见下方赋值）

                    if (goods.IsSale != 1)
                    {
                        await transaction.RollbackAsync();
                        return Fail($"商品已下架：{goods.GoodsName}");
                    }

                    if (goods.Stock < item.BuyNum)
                    {
                        await transaction.RollbackAsync();
                        return Fail($"商品库存不足：{goods.GoodsName}");
                    }

                    // 若选择了 SKU，则以该规格的单价（sku.Price）作为成交单价；否则使用商品售卖价
                    string? specId = null;
                    string? specName = null;
                    GoodsSku? sku = null;
                    decimal salePrice = goods.SalePrice > 0 ? goods.SalePrice : goods.Price;
                    if (!string.IsNullOrWhiteSpace(item.SkuId))
                    {
                        sku = await _db.GoodsSkus
                            .FirstOrDefaultAsync(s => s.Id == item.SkuId && s.GoodsId == goods.Id);
                        if (sku != null)
                        {
                            specId = sku.Id;
                            specName = sku.SkuName;
                            if (sku.Price > 0) salePrice = sku.Price;

                            // SKU 维度库存校验与扣减：SKU 商品以 sku.Stock 作为买家端可购买库存
                            if (sku.Stock < item.BuyNum)
                            {
                                await transaction.RollbackAsync();
                                return Fail($"商品库存不足：{goods.GoodsName}（{sku.SkuName}）");
                            }
                            sku.Stock -= item.BuyNum;
                        }
                    }

                    // 解析该商品所属商户：通过 MerchantGood 关联表找到其 MerchantId，
                    // 用于把订单归属到商户，使商户端订单列表能查询到。
                    if (string.IsNullOrEmpty(orderMerchantId))
                    {
                        var sg = await _db.MerchantGood
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.GoodsId == goods.Id);
                        if (sg != null && !string.IsNullOrEmpty(sg.MerchantId))
                            orderMerchantId = sg.MerchantId;
                    }

                    decimal subAmount = salePrice * item.BuyNum;

                    totalAmount += subAmount;

                    // 锁定库存（商品级）：扣减买家端可购买库存，使前端展示的「可购买库存」减少
                    goods.Stock -= item.BuyNum;

                    // 锁定库存（库存表）：累加 LockStock，管理后台可用/可购买库存 = 总库存 - 锁定 将相应减少
                    var stock = await _db.Stocks
                        .FirstOrDefaultAsync(s => s.GoodsId == goods.Id);
                    if (stock != null)
                    {
                        stock.LockStock = (stock.LockStock ?? 0) + item.BuyNum;
                        stock.UpdateTime = now;
                    }

                    orderItems.Add(new ClientOrderItemDto
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        GoodsId = goods.Id,
                        GoodsName = goods.GoodsName,
                        SpecId = specId,
                        SpecName = specName,
                        GoodsImg = goods.CoverImg,
                        BuyPrice = (double)salePrice,
                        BuyNum = item.BuyNum,
                        SubAmount = (double)subAmount,
                        IsBan = 0,
                        CreateTime = now.ToString("yyyy-MM-ddTHH:mm:ss"),
                        UpdateTime = now.ToString("yyyy-MM-ddTHH:mm:ss")
                    });

                    responseItems.Add(new OrderItemDto
                    {
                        GoodsId = goods.Id,
                        SkuId = specId,
                        GoodsName = goods.GoodsName,
                        GoodsImg = goods.CoverImg,
                        BuyNum = item.BuyNum,
                        BuyPrice = salePrice,
                        SubAmount = subAmount
                    });
                }

                // 商品信息单行摘要，与历史订单格式保持一致："商品名×数量, 商品名×数量"
                var goodsInfo = string.Join(", ", orderItems
                    .Where(i => !string.IsNullOrWhiteSpace(i.GoodsName))
                    .Select(i => $"{i.GoodsName}×{i.BuyNum ?? 1}"));

                var order = new ClientOrder
                {
                    Id = orderId,
                    OrderNo = orderNo,
                    ClientId = userId,
                    MerchantId = orderMerchantId,
                    GoodsInfo = goodsInfo,
                    TotalAmount = (double)totalAmount,
                    OrderStatus = OrderStatus.PendingAccept,
                    RejectReason = null,
                    Remark = dto.Remark,
                    ReceiverName = dto.ReceiverName,
                    ReceiverMobile = dto.ReceiverMobile,
                    Address = dto.Address,
                    IsBan = 0,
                    CreateTime = now,
                    UpdateTime = now
                };

                // client_order_item 已合并进 client_order.items_json（明细 JSON 化）
                order.ItemsJson = orderItems.ToItemsJson();
                _db.Set<ClientOrder>().Add(order);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // 实时推送：小程序新订单产生，通知所有打开首页的管理后台立即刷新
                await NotifyAsync("order.created", new
                {
                    orderId = order.Id,
                    orderNo = order.OrderNo,
                    totalAmount = totalAmount,
                    createTime = now
                });

                // 消息队列：① order.created → 库存预警消费者 / 跨实例广播消费者；
                //          ② 延迟关单消息 → 超时未支付时由自动关单消费者执行关单（回收库存）。
                await PublishOrderEventAsync(OrderEventTypes.Created, order);
                await PublishAutoCancelAsync(order);

                return Ok("下单成功", new OrderResponseDto
                {
                    OrderId = order.Id,
                    OrderNo = order.OrderNo!,
                    TotalAmount = totalAmount,
                    OrderStatus = OrderStatus.PendingAccept,
                    Remark = dto.Remark,
                    Address = order.Address,
                    Items = responseItems
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 解析当前登录用户「可能拥有的所有身份 u_id」。
        /// 历史上因用户体系重构，同一手机号可能对应多个 u_id（旧订单 owner 用的是旧 u_id），
        /// 直接按当前 JWT 的 u_id 查订单会漏掉历史订单。这里把「当前 u_id + 同手机号下的所有 u_id」都纳入，
        /// 调用方用 IN 查询兜底，使数据层与历史状态解耦。mobile 为空（旧 token 无该声明）时仅用当前 u_id。
        /// </summary>
        private async Task<List<string>> ResolveOwnerIdsAsync(string userId, string? mobile)
        {
            var ids = new HashSet<string> { userId };
            if (!string.IsNullOrWhiteSpace(mobile))
            {
                // sys_users.u_account 即注册手机号：同一手机号可能对应多个历史账号
                var fromSys = await _db.SysUsers
                    .AsNoTracking()
                    .Where(u => u.UAccount == mobile)
                    .Select(u => u.UId)
                    .ToListAsync();
                foreach (var id in fromSys) ids.Add(id);

                // member_address.user_id 也可能持有旧 u_id（历史注册残留）
                var fromAddr = await _db.MemberAddresses
                    .AsNoTracking()
                    .Where(a => a.Mobile == mobile)
                    .Select(a => a.UserId)
                    .ToListAsync();
                foreach (var id in fromAddr)
                {
                    if (!string.IsNullOrWhiteSpace(id)) ids.Add(id);
                }
            }
            return ids.ToList();
        }

        /// <summary>
        /// B 方案：历史订单 / 地址 owner 迁移。
        /// 设计背景：sys_users.u_account 有 UNIQUE 约束，同一手机号只有一个规范账号（canonical UId）。
        /// 但历史上用户体系重构前产生的 member_address.user_id / client_order.client_id 可能仍指向
        /// 旧 UId（孤儿），导致「当前登录身份查不到旧订单/旧地址」。A 方案用 mobile 兜底读，
        /// 本方法则主动把孤儿引用迁到规范 UId，从数据层根治，让兜底仅作安全网。
        /// 流程：① 取规范 UId = sys_users 中该手机号的当前账号；② 收集该手机号下、除规范 UId 外的
        /// 所有历史 owner UId（来自 member_address.user_id）；③ 将这些旧 UId 名下的订单与地址
        /// 的 owner 字段改写为规范 UId。幂等：无规范账号或无孤儿时直接返回 0，不写库。
        /// </summary>
        public async Task<int> ConsolidateLegacyOwnersByMobileAsync(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile)) return 0;

            // ① 规范 UId：该手机号当前生效的 sys_users 账号
            var canonical = await _db.SysUsers
                .AsNoTracking()
                .Where(u => u.UAccount == mobile)
                .Select(u => u.UId)
                .FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(canonical)) return 0; // 尚未注册，无规范账号可迁往

            // ② 收集历史 owner UId（member_address 中指向旧账号的残留）
            var legacyIds = await _db.MemberAddresses
                .AsNoTracking()
                .Where(a => a.Mobile == mobile && a.UserId != canonical && a.UserId != null)
                .Select(a => a.UserId!)
                .Distinct()
                .ToListAsync();
            if (legacyIds.Count == 0) return 0; // 无孤儿，幂等跳过

            // ③ 逐旧 UId 迁移历史订单与地址的 owner 字段
            int total = 0;
            foreach (var legacy in legacyIds)
            {
                total += await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE clientorders SET client_id = {0} WHERE client_id = {1}", canonical, legacy);
                total += await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE member_address SET user_id = {0} WHERE user_id = {1}", canonical, legacy);
            }

            return total;
        }

        public async Task<Api_Response> GetOrdersAsync(string userId, string? mobile = null, int? status = null, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            // 解析「当前 u_id + 同手机号下的所有历史 u_id」，用 IN 查询兜底，
            // 确保用户体系重构后旧订单 owner 仍是旧 u_id 时也能被当前登录身份查到。
            var ownerIds = await ResolveOwnerIdsAsync(userId, mobile);

            var query = _db.ClientOrders
                .AsNoTracking()
                .Where(o => ownerIds.Contains(o.ClientId) && o.IsBan != 1);

            if (status.HasValue)
            {
                if (status.Value == 1)
                {
                    // 待付款：未支付的订单（含未支付待接单）
                    query = query.Where(o => o.OrderStatus == OrderStatus.PendingAccept && (o.PayStatus == PayStatus.Unpaid || o.PayStatus == null));
                }
                else if (status.Value == 2)
                {
                    // 待发货：已支付待接单(OrderStatus = OrderStatus.PendingAccept 且 PayStatus = PayStatus.Paid) 或 已接单备货中(OrderStatus = OrderStatus.Accepted)
                    query = query.Where(o => o.OrderStatus == OrderStatus.Accepted || (o.OrderStatus == OrderStatus.PendingAccept && o.PayStatus == PayStatus.Paid));
                }
                else
                {
                    query = query.Where(o => o.OrderStatus == (OrderStatus?)status.Value);
                }
            }

            var orders = await query
                .OrderByDescending(o => o.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 合并后明细内联在 client_order.items_json，直接解析并按 OrderId 分组
            var itemsByOrder = orders
                .ToDictionary(o => o.Id, o => o.ParseItems());

            // 批量加载商户名称
            var merchantIds = orders.Select(o => o.MerchantId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            var merchants = await _db.Merchants
                .AsNoTracking()
                .Where(s => merchantIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s);

            var result = orders.Select(order => MapOrderSummary(order, itemsByOrder, merchants)).ToList();

            return Ok("ok", result);
        }

        public async Task<Api_Response> GetOrderDetailAsync(string userId, string orderId, string? mobile = null)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Fail("订单ID不能为空");
            }

            var ownerIds = await ResolveOwnerIdsAsync(userId, mobile);

            var order = await _db.ClientOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && ownerIds.Contains(o.ClientId) && o.IsBan != 1);

            if (order == null)
            {
                return Fail("订单不存在");
            }

            var merchant = await _db.Merchants.AsNoTracking().FirstOrDefaultAsync(s => s.Id == order.MerchantId);

            var summary = MapOrderSummary(order, new Dictionary<string, List<ClientOrderItemDto>>
            {
                [order.Id] = order.ParseItems()
            }, merchant == null ? null : new Dictionary<string, Merchant> { [merchant.Id] = merchant });

            return Ok("ok", summary);
        }

        public async Task<Api_Response> CancelOrderAsync(string userId, string orderId, string? mobile = null, string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Fail("订单ID不能为空");
            }

            var ownerIds = await ResolveOwnerIdsAsync(userId, mobile);

            var order = await _db.ClientOrders
                .FirstOrDefaultAsync(o => o.Id == orderId && ownerIds.Contains(o.ClientId));

            if (order == null)
            {
                return Fail("订单不存在");
            }

            if (order.OrderStatus != OrderStatus.PendingAccept)
            {
                return Fail("当前订单状态不可取消");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // 释放锁定的库存：回滚买家端可购买库存、SKU 库存与库存表的锁定库存，避免库存被永久占用
                var items = order.ParseItems();

                foreach (var it in items)
                {
                    int buyNum = it.BuyNum ?? 0;
                    if (buyNum <= 0 || string.IsNullOrWhiteSpace(it.GoodsId))
                    {
                        continue;
                    }

                    var goods = await _db.Goods.FirstOrDefaultAsync(g => g.Id == it.GoodsId);
                    if (goods != null)
                    {
                        goods.Stock += buyNum;
                        goods.UpdateTime = DateTime.Now;
                    }

                    if (!string.IsNullOrWhiteSpace(it.SpecId))
                    {
                        var sku = await _db.GoodsSkus
                            .FirstOrDefaultAsync(s => s.Id == it.SpecId && s.GoodsId == it.GoodsId);
                        if (sku != null)
                        {
                            sku.Stock += buyNum;
                        }
                    }

                    var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.GoodsId == it.GoodsId);
                    if (stock != null)
                    {
                        stock.LockStock = Math.Max(0, (stock.LockStock ?? 0) - buyNum);
                        stock.UpdateTime = DateTime.Now;
                    }
                }

                order.OrderStatus = OrderStatus.Cancelled;
                order.Remark = string.IsNullOrWhiteSpace(reason) ? "用户取消订单" : reason;
                order.UpdateTime = DateTime.Now;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            // 实时推送：订单取消，通知首页刷新
            await NotifyAsync("order.updated", new { orderId = order.Id, orderNo = order.OrderNo, status = order.OrderStatus });

            // 消息队列：order.cancelled（用户取消/商户拒单/超时关单统一事件）
            await PublishOrderEventAsync(OrderEventTypes.Cancelled, order, new { reason = order.Remark });

            return Ok("取消成功");
        }

        /// <summary>
        /// 买家删除订单：逻辑删除（IsBan=1），仅买家本人可操作。
        /// 删除后订单对买家不可见；管理后台/商户端如需查看需显式过滤 IsBan=1。
        /// </summary>
        public async Task<Api_Response> DeleteOrderAsync(string userId, string orderId, string? mobile = null)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Fail("订单ID不能为空");
            }

            var ownerIds = await ResolveOwnerIdsAsync(userId, mobile);

            var order = await _db.ClientOrders
                .FirstOrDefaultAsync(o => o.Id == orderId && ownerIds.Contains(o.ClientId) && o.IsBan != 1);

            if (order == null)
            {
                return Fail("订单不存在");
            }

            order.IsBan = 1;
            order.UpdateTime = DateTime.Now;

            await _db.SaveChangesAsync();

            // 实时推送：订单删除，通知列表刷新
            await NotifyAsync("order.deleted", new { orderId = order.Id, orderNo = order.OrderNo });

            // 消息队列：order.deleted
            await PublishOrderEventAsync(OrderEventTypes.Deleted, order);

            return Ok("删除成功");
        }

        public async Task<Api_Response> PayCallbackAsync(string orderId, PayCallbackRequestDto? dto = null, string? transactionId = null)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Fail("订单ID不能为空");
            }

            var order = await _db.ClientOrders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return Fail("订单不存在");
            }

            // 原子幂等：仅当未支付时翻转支付状态，返回受影响行数。
            // 并发重复回调中仅一个线程能拿到 affected=1，其余直接视为已处理，杜绝重复入账。
            var affected = await _db.ClientOrders
                .Where(o => o.Id == order.Id && (o.PayStatus == PayStatus.Unpaid || o.PayStatus == null))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.PayStatus, PayStatus.Paid)
                    .SetProperty(p => p.PayTime, DateTime.Now)
                    .SetProperty(p => p.UpdateTime, DateTime.Now));

            if (affected == 0)
            {
                return Ok("支付回调成功（已处理）", new
                {
                    orderId = order.Id,
                    orderNo = order.OrderNo,
                });
            }

            // 同步内存实体，避免后续 SaveChanges 覆盖数据库已提交的支付状态
            order.PayStatus = PayStatus.Paid;
            order.PayTime = DateTime.Now;
            order.UpdateTime = DateTime.Now;

            // 记录支付流水：微信流水号优先落库（供对账核对），缺失时退化为本地生成
            var payRecord = new OrderPay
            {
                Id = Config.NewSequentialGuid(),
                OrderId = order.Id,
                PayNo = string.IsNullOrWhiteSpace(transactionId)
                    ? $"PAY{DateTime.Now:yyyyMMddHHmmssfff}{new Random().Next(1000, 9999)}"
                    : transactionId,
                PayAmount = (decimal?)order.TotalAmount,
                PayType = 11, // 微信支付
                PayStatus = PayStatus.Paid, // 已付
                CreateTime = DateTime.Now
            };
            await _db.OrderPays.AddAsync(payRecord);

            await _db.SaveChangesAsync();

            // 实时推送：订单支付成功，通知首页刷新（未支付/已支付统计与待办会实时变化）
            await NotifyAsync("order.paid", new { orderId = order.Id, orderNo = order.OrderNo, payStatus = (int)PayStatus.Paid });

            // 消息队列：order.paid
            await PublishOrderEventAsync(OrderEventTypes.Paid, order, new { payNo = payRecord.PayNo });

            return Ok("支付回调成功", new
            {
                orderId = order.Id,
                orderNo = order.OrderNo,
            });
        }

        /// <summary>
        /// 商家发货：原逻辑把已支付(待发货, status=2) 订单标记为已发货(待收货, status=3)。
        /// 说明：client_order 的 sell_client_id 列已删除，且 good 表不含 client_id，
        /// 订单已无法解析「售卖商户」，原「仅售卖商户本人可操作」的越权校验失去依据。
        /// 为避免放开越权风险，本方法当前直接返回失败；发货请由商户端执行
        /// /api/merchant/orders/{id}/action（action=ship，2→3）。
        /// </summary>
        public Task<Api_Response> ShipOrderAsync(string userId, string orderId, string? mobile = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult(Fail("用户未登录"));
            }

            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Task.FromResult(Fail("订单ID不能为空"));
            }

            var exists = _db.ClientOrders.Any(o => o.Id == orderId);
            if (!exists)
            {
                return Task.FromResult(Fail("订单不存在"));
            }

            return Task.FromResult(Fail("商家发货功能已下线：订单未保存售卖商户信息"));
        }

        /// <summary>
        /// 买家确认收货：将已发货(待收货, status=3) 订单标记为已完成(status=4)。
        /// 仅买家本人可操作（校验 ClientId == 当前登录用户）。
        /// 对应小程序端「确认收货」按钮调用的 POST /api/mini-program/orders/{id}/receive。
        /// </summary>
        public async Task<Api_Response> ConfirmReceiptAsync(string userId, string orderId, string? mobile = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("用户未登录");
            }

            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Fail("订单ID不能为空");
            }

            var ownerIds = await ResolveOwnerIdsAsync(userId, mobile);

            var order = await _db.ClientOrders
                .FirstOrDefaultAsync(o => o.Id == orderId && ownerIds.Contains(o.ClientId));

            if (order == null)
            {
                return Fail("订单不存在");
            }

            if (order.OrderStatus != OrderStatus.Shipped)
            {
                return Fail("当前订单状态不可确认收货");
            }

            order.OrderStatus = OrderStatus.Completed;
            order.CompleteTime = DateTime.Now;
            order.UpdateTime = DateTime.Now;

            await _db.SaveChangesAsync();

            // 实时推送：订单已完成，前端买家端进度条走到「已完成」并可评价
            await NotifyAsync("order.received", new { orderId = order.Id, orderNo = order.OrderNo, status = order.OrderStatus });

            // 消息队列：order.completed → 结算消费者自动生成结算单（订单完成自动结算）
            await PublishOrderEventAsync(OrderEventTypes.Completed, order, new { completeTime = order.CompleteTime });

            return Ok("确认收货成功");
        }

        /// <summary>
        /// 安全发布实时事件：即使通知器异常也不影响主流程（下单/支付/取消的返回）。
        /// </summary>
        private async Task NotifyAsync(string type, object? payload)
        {
            try
            {
                await _notifier.PublishAsync(type, payload);
            }
            catch
            {
                // 实时推送失败不应阻断业务，忽略
            }
        }

        /// <summary>
        /// 发布订单事件到 RabbitMQ（routing key = 事件类型）。
        /// Broker 不可用时由发布器自动降级进程内总线；双保险 try/catch 保证业务零感知。
        /// </summary>
        private async Task PublishOrderEventAsync(string type, ClientOrder order, object? data = null)
        {
            try
            {
                await _publisher.PublishAsync(new OrderEventMessage
                {
                    Type = type,
                    OrderId = order.Id,
                    OrderNo = order.OrderNo,
                    ClientId = order.ClientId,
                    MerchantId = order.MerchantId,
                    TotalAmount = order.TotalAmount,
                    Data = data
                });
            }
            catch
            {
                // 消息投递失败不阻断业务（发布器内部已降级，此处双保险）
            }
        }

        /// <summary>
        /// 投递延迟关单消息：AutoCancelDelayMinutes 分钟后若订单仍未支付，
        /// 自动关单消费者核对状态后执行关单（释放锁定的库存）。
        /// </summary>
        private async Task PublishAutoCancelAsync(ClientOrder order)
        {
            try
            {
                await _publisher.PublishDelayedAsync(new OrderEventMessage
                {
                    Type = OrderEventTypes.AutoCancel,
                    OrderId = order.Id,
                    OrderNo = order.OrderNo,
                    ClientId = order.ClientId,
                    MerchantId = order.MerchantId,
                    TotalAmount = order.TotalAmount
                }, TimeSpan.FromMinutes(Math.Max(1, _mqOptions.AutoCancelDelayMinutes)));
            }
            catch
            {
                // 忽略：极端情况下该订单不会自动关单，可由管理后台手动取消
            }
        }

        private OrderSummaryDto MapOrderSummary(ClientOrder order, IDictionary<string, List<ClientOrderItemDto>> itemsByOrder, IDictionary<string, Merchant>? merchants = null)
        {
            var items = new List<OrderItemDto>();

            if (itemsByOrder.TryGetValue(order.Id, out var orderItems))
            {
                items = orderItems.Select(item => new OrderItemDto
                {
                    GoodsId = item.GoodsId ?? string.Empty,
                    SkuId = item.SpecId ?? string.Empty,
                    GoodsName = item.GoodsName,
                    GoodsImg = item.GoodsImg,
                    BuyNum = item.BuyNum ?? 0,
                    BuyPrice = ParseDecimal(item.BuyPrice),
                    SubAmount = ParseDecimal(item.SubAmount),
                }).ToList();
            }

            Merchant? merchant = null;
            merchants?.TryGetValue(order.MerchantId ?? string.Empty, out merchant);

            return new OrderSummaryDto
            {
                Id = order.Id,
                OrderNo = order.OrderNo ?? string.Empty,
                TotalAmount = ParseDecimal(order.TotalAmount),
                OrderStatus = order.OrderStatus ?? OrderStatus.PendingPayment,
                PayStatus = order.PayStatus ?? PayStatus.Unpaid,
                Remark = order.Remark,
                ReceiverName = order.ReceiverName,
                ReceiverMobile = order.ReceiverMobile,
                Address = order.Address,
                // sell_client_id 列已删除，售卖商户名称不再可解析，保留 DTO 字段但恒为 null
                SellClientName = null,
                MerchantId = order.MerchantId,
                MerchantName = merchant?.MerchantName,
                CreateTime = order.CreateTime,
                PayTime = order.PayTime,
                ShipTime = order.ShipTime,
                CompleteTime = order.CompleteTime,
                Items = items
            };
        }

        private decimal ParseDecimal(double? value)
        {
            return value.HasValue ? (decimal)value.Value : 0m;
        }

    }
}