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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 订单运费服务
    ///
    /// 计费规则（当前版本，常量集中在下方，便于后续改为后台可配置）：
    ///   - 商品金额 ≥ 满额门槛 → 包邮（计费方式 2）
    ///   - 否则按件计费（计费方式 1）：首件 + 续件 × (件数 - 1)
    ///
    /// 与订单金额的关系：运费独立记账，不并入 client_order.total_amount，
    /// 避免影响商户结算/对账口径；应付合计保存在 order_freight.total_amount。
    /// </summary>
    [Inject_]
    public class FreightService : Base_Service, IFreightService
    {
        /// <summary>满额包邮门槛（元）</summary>
        private const decimal FreeShippingThreshold = 99m;

        /// <summary>首件运费（元）</summary>
        private const decimal FirstItemFee = 8m;

        /// <summary>续件运费（元/件）</summary>
        private const decimal AdditionalItemFee = 2m;

        /// <summary>计费方式：按件计费</summary>
        private const int ChargeTypePerItem = 1;

        /// <summary>计费方式：满额包邮</summary>
        private const int ChargeTypeFreeShipping = 2;

        private readonly _111Context _db;

        public FreightService(_111Context db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>一次计费结果</summary>
        private class FreightQuote
        {
            public decimal GoodsAmount { get; set; }
            public int ItemCount { get; set; }
            public decimal FreightAmount { get; set; }
            public int ChargeType { get; set; }
            public string ChargeTypeText { get; set; } = string.Empty;
            public string ChargeRule { get; set; } = string.Empty;
            public bool FreeShipping { get; set; }
            public decimal TotalAmount => GoodsAmount + FreightAmount;
        }

        #region 对外接口

        /// <summary>计算运费</summary>
        public async Task<Api_Response> CalcAsync(string userId, FreightCalcRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId)) return Fail("用户未登录");
            if (dto == null) return Fail("运费计算参数不能为空");

            ClientOrder? order = null;
            decimal goodsAmount;
            int itemCount;

            if (!string.IsNullOrWhiteSpace(dto.OrderId))
            {
                // 方式一：按已有订单计算（权威口径，直接取订单落库金额与明细）
                var ownerIds = await ResolveOwnerIdsAsync(userId, null);
                order = await _db.ClientOrders
                    .FirstOrDefaultAsync(o => o.Id == dto.OrderId
                        && ownerIds.Contains(o.ClientId)
                        && (o.IsBan == null || o.IsBan != 1));
                if (order == null) return Fail("订单不存在");

                goodsAmount = (decimal)(order.TotalAmount ?? 0);
                itemCount = order.ParseItems().Sum(i => i.BuyNum ?? 0);
                if (itemCount <= 0) itemCount = 1;
            }
            else
            {
                // 方式二：下单前预览计算（按商品明细实时询价，不落库）
                if (dto.Items == null || dto.Items.Count == 0) return Fail("请提供订单ID或商品明细");

                goodsAmount = 0m;
                itemCount = 0;
                foreach (var it in dto.Items)
                {
                    if (string.IsNullOrWhiteSpace(it.GoodsId) || it.BuyNum <= 0) continue;

                    var goods = await _db.Goods.AsNoTracking()
                        .FirstOrDefaultAsync(g => g.Id == it.GoodsId);
                    if (goods == null) continue;

                    // 与下单同口径：SKU 价优先，其次商品售价，最后基础价
                    decimal price = goods.SalePrice > 0 ? goods.SalePrice : goods.Price;
                    if (!string.IsNullOrWhiteSpace(it.SkuId))
                    {
                        var sku = await _db.GoodsSkus.AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == it.SkuId && s.GoodsId == goods.Id);
                        if (sku != null && sku.Price > 0) price = sku.Price;
                    }

                    goodsAmount += price * it.BuyNum;
                    itemCount += it.BuyNum;
                }

                if (itemCount <= 0) return Fail("商品明细无效，无法计算运费");
            }

            var quote = ComputeFreight(goodsAmount, itemCount);
            var now = DateTime.Now;

            // 已有订单：计算即落库（生效时间刷新为本次计算时刻）；已支付的运费单不再重算
            if (order != null)
            {
                var entity = await EnsureFreightAsync(order, quote, now);
                await _db.SaveChangesAsync();
                return Ok("ok", ToDto(entity));
            }

            // 预览计算：返回未落库的报价（id 为空，前端仅用于展示）
            return Ok("ok", new FreightDetailDto
            {
                Id = null,
                FreightNo = null,
                OrderId = null,
                OrderNo = null,
                GoodsAmount = quote.GoodsAmount,
                FreightAmount = quote.FreightAmount,
                ChargeType = quote.ChargeType,
                ChargeTypeText = quote.ChargeTypeText,
                ChargeRule = quote.ChargeRule,
                FreeShipping = quote.FreeShipping,
                TotalAmount = quote.TotalAmount,
                PayStatus = PayStatus.Unpaid,
                PayStatusText = PayStatusTextOf(0),
                PayTime = null,
                EffectiveTime = now,
                FreightAmountText = FreightTextOf(quote.FreightAmount, quote.FreeShipping),
                TotalAmountText = Money(quote.TotalAmount),
                ItemCount = quote.ItemCount
            });
        }

        /// <summary>运费支付（提交即完成，无需审核）</summary>
        public async Task<Api_Response> PayAsync(string userId, string? mobile, FreightPayRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId)) return Fail("用户未登录");
            if (dto == null || string.IsNullOrWhiteSpace(dto.OrderId)) return Fail("订单ID不能为空");

            var ownerIds = await ResolveOwnerIdsAsync(userId, mobile);
            var order = await _db.ClientOrders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId
                    && ownerIds.Contains(o.ClientId)
                    && (o.IsBan == null || o.IsBan != 1));
            if (order == null) return Fail("订单不存在");
            if (order.OrderStatus == OrderStatus.Cancelled) return Fail("订单已取消，无需支付运费");

            var now = DateTime.Now;

            // 幂等：已支付的运费单直接返回，避免重复扣款
            var entity = await _db.OrderFreights
                .FirstOrDefaultAsync(f => f.OrderId == order.Id
                    && (dto.FreightId == null || f.Id == dto.FreightId)
                    && (f.IsBan == null || f.IsBan != 1));

            if (entity != null && entity.PayStatus == PayStatus.Paid)
            {
                return Ok("运费已支付", ToDto(entity));
            }

            decimal goodsAmount = (decimal)(order.TotalAmount ?? 0);
            int itemCount = order.ParseItems().Sum(i => i.BuyNum ?? 0);
            if (itemCount <= 0) itemCount = 1;

            var quote = ComputeFreight(goodsAmount, itemCount);

            if (entity == null)
            {
                entity = NewEntity(order, quote, now);
                _db.OrderFreights.Add(entity);
            }
            else
            {
                ApplyQuote(entity, order, quote, now);
            }

            entity.PayStatus = PayStatus.Paid;
            entity.PayTime = now;
            entity.EffectiveTime = now;
            if (!string.IsNullOrWhiteSpace(dto.Remark)) entity.Remark = dto.Remark;
            entity.UpdateTime = now;

            await _db.SaveChangesAsync();

            return Ok("运费支付成功", ToDto(entity));
        }

        /// <summary>按订单查询运费明细（无记录时实时报价，不落库）</summary>
        public async Task<Api_Response> GetByOrderAsync(string userId, string? mobile, string orderId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return Fail("用户未登录");
            if (string.IsNullOrWhiteSpace(orderId)) return Fail("订单ID不能为空");

            var ownerIds = await ResolveOwnerIdsAsync(userId, mobile);
            var order = await _db.ClientOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId
                    && ownerIds.Contains(o.ClientId)
                    && (o.IsBan == null || o.IsBan != 1));
            if (order == null) return Fail("订单不存在");

            var entity = await _db.OrderFreights.AsNoTracking()
                .Where(f => f.OrderId == order.Id && (f.IsBan == null || f.IsBan != 1))
                .OrderByDescending(f => f.CreateTime)
                .FirstOrDefaultAsync();

            if (entity != null) return Ok("ok", ToDto(entity));

            // 尚未生成运费单：按订单当前参数实时报价
            decimal goodsAmount = (decimal)(order.TotalAmount ?? 0);
            int itemCount = order.ParseItems().Sum(i => i.BuyNum ?? 0);
            if (itemCount <= 0) itemCount = 1;
            var quote = ComputeFreight(goodsAmount, itemCount);

            return Ok("ok", new FreightDetailDto
            {
                Id = null,
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                GoodsAmount = quote.GoodsAmount,
                FreightAmount = quote.FreightAmount,
                ChargeType = quote.ChargeType,
                ChargeTypeText = quote.ChargeTypeText,
                ChargeRule = quote.ChargeRule,
                FreeShipping = quote.FreeShipping,
                TotalAmount = quote.TotalAmount,
                PayStatus = PayStatus.Unpaid,
                PayStatusText = PayStatusTextOf(0),
                EffectiveTime = DateTime.Now,
                FreightAmountText = FreightTextOf(quote.FreightAmount, quote.FreeShipping),
                TotalAmountText = Money(quote.TotalAmount),
                ItemCount = quote.ItemCount
            });
        }

        #endregion

        #region 计费核心

        /// <summary>
        /// 运费计算规则：满额包邮，否则按件计费（首件 + 续件）。
        /// </summary>
        private FreightQuote ComputeFreight(decimal goodsAmount, int itemCount)
        {
            if (goodsAmount < 0) goodsAmount = 0m;
            if (itemCount <= 0) itemCount = 1;

            if (goodsAmount >= FreeShippingThreshold)
            {
                return new FreightQuote
                {
                    GoodsAmount = goodsAmount,
                    ItemCount = itemCount,
                    FreightAmount = 0m,
                    ChargeType = ChargeTypeFreeShipping,
                    ChargeTypeText = "满额包邮",
                    ChargeRule = $"订单满 {Money(FreeShippingThreshold)} 免运费",
                    FreeShipping = true
                };
            }

            decimal freight = FirstItemFee + AdditionalItemFee * (itemCount - 1);
            if (freight < 0) freight = 0m;

            return new FreightQuote
            {
                GoodsAmount = goodsAmount,
                ItemCount = itemCount,
                FreightAmount = freight,
                ChargeType = ChargeTypePerItem,
                ChargeTypeText = "按件计费",
                ChargeRule = $"首件 {Money(FirstItemFee)}，续件 {Money(AdditionalItemFee)}/件，共 {itemCount} 件",
                FreeShipping = false
            };
        }

        /// <summary>取订单已有运费单；不存在则新建（未支付）。已支付的运费单原样返回，不覆盖。</summary>
        private async Task<OrderFreight> EnsureFreightAsync(ClientOrder order, FreightQuote quote, DateTime now)
        {
            var entity = await _db.OrderFreights
                .FirstOrDefaultAsync(f => f.OrderId == order.Id && (f.IsBan == null || f.IsBan != 1));

            if (entity == null)
            {
                entity = NewEntity(order, quote, now);
                _db.OrderFreights.Add(entity);
                return entity;
            }

            // 已支付：保留原金额与支付信息，避免重算导致金额漂移
            if (entity.PayStatus == PayStatus.Paid) return entity;

            ApplyQuote(entity, order, quote, now);
            return entity;
        }

        private OrderFreight NewEntity(ClientOrder order, FreightQuote quote, DateTime now)
        {
            var entity = new OrderFreight
            {
                Id = Config.NewSequentialGuid(),
                FreightNo = $"FR{now:yyyyMMddHHmmss}{new Random().Next(100, 999)}",
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.ClientId,
                MerchantId = order.MerchantId,
                IsBan = 0,
                PayStatus = PayStatus.Unpaid,
                CreateTime = now
            };
            ApplyQuote(entity, order, quote, now);
            return entity;
        }

        private void ApplyQuote(OrderFreight entity, ClientOrder order, FreightQuote quote, DateTime now)
        {
            entity.OrderId = order.Id;
            entity.OrderNo = order.OrderNo;
            entity.UserId = order.ClientId;
            entity.MerchantId = order.MerchantId;
            entity.GoodsAmount = quote.GoodsAmount;
            entity.FreightAmount = quote.FreightAmount;
            entity.ItemCount = quote.ItemCount;
            entity.ChargeType = quote.ChargeType;
            entity.ChargeTypeText = quote.ChargeTypeText;
            entity.ChargeRule = quote.ChargeRule;
            entity.FreeShipping = quote.FreeShipping ? 1 : 0;
            entity.TotalAmount = quote.TotalAmount;
            entity.EffectiveTime = now;
            entity.UpdateTime = now;
            if (entity.CreateTime == null) entity.CreateTime = now;
        }

        private FreightDetailDto ToDto(OrderFreight entity)
        {
            decimal freight = entity.FreightAmount ?? 0m;
            decimal goods = entity.GoodsAmount ?? 0m;
            decimal total = entity.TotalAmount ?? (goods + freight);
            bool free = (entity.FreeShipping ?? 0) == 1;
            int payStatus = (int)(entity.PayStatus ?? PayStatus.Unpaid);

            return new FreightDetailDto
            {
                Id = entity.Id,
                FreightNo = entity.FreightNo,
                OrderId = entity.OrderId,
                OrderNo = entity.OrderNo,
                GoodsAmount = goods,
                FreightAmount = freight,
                ChargeType = entity.ChargeType ?? ChargeTypePerItem,
                ChargeTypeText = entity.ChargeTypeText ?? string.Empty,
                ChargeRule = entity.ChargeRule ?? string.Empty,
                FreeShipping = free,
                TotalAmount = total,
                PayStatus = (PayStatus?)payStatus,
                PayStatusText = PayStatusTextOf(payStatus),
                PayTime = entity.PayTime,
                EffectiveTime = entity.EffectiveTime,
                FreightAmountText = FreightTextOf(freight, free),
                TotalAmountText = Money(total),
                ItemCount = entity.ItemCount ?? 0,
                Remark = entity.Remark
            };
        }

        #endregion

        #region 辅助

        private static string PayStatusTextOf(int status) => status == 1 ? "已支付" : "未支付";

        /// <summary>金额展示：¥8.00 / 包邮</summary>
        private static string FreightTextOf(decimal amount, bool freeShipping)
        {
            if (freeShipping || amount <= 0m) return "包邮";
            return Money(amount);
        }

        private static string Money(decimal amount) =>
            "¥" + amount.ToString("0.00", CultureInfo.InvariantCulture);

        /// <summary>
        /// 解析当前登录用户「可能拥有的所有身份 u_id」（与 OrderService 同口径）：
        /// 同一手机号在历史数据中可能存在多个 u_id，订单 owner 可能指向旧 u_id。
        /// </summary>
        private async Task<List<string>> ResolveOwnerIdsAsync(string userId, string? mobile)
        {
            var ids = new HashSet<string> { userId };
            if (!string.IsNullOrWhiteSpace(mobile))
            {
                var fromSys = await _db.SysUsers
                    .AsNoTracking()
                    .Where(u => u.UAccount == mobile)
                    .Select(u => u.UId)
                    .ToListAsync();
                foreach (var id in fromSys) ids.Add(id);

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

        #endregion
    }
}
