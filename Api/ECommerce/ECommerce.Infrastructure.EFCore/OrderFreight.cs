using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 订单运费单
    /// 记录订单的运费计算明细与运费支付状态，与 client_order 一对一（按 order_id 关联）。
    /// 运费独立于商品金额：client_order.total_amount 仍为商品金额，
    /// 结算/对账口径不受影响；本表 total_amount = 商品金额 + 运费金额，供前端展示应付合计。
    /// </summary>
    public partial class OrderFreight
    {
        /// <summary>
        /// 运费标识（主键）
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// 运费单号：FR + yyMMddHHmmss + 3 位随机
        /// </summary>
        public string? FreightNo { get; set; }

        /// <summary>
        /// 关联订单ID
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// 关联订单编号
        /// </summary>
        public string? OrderNo { get; set; }

        /// <summary>
        /// 下单用户ID（买家）
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public string? MerchantId { get; set; }

        /// <summary>
        /// 商品金额（运费计费基数）
        /// </summary>
        public decimal? GoodsAmount { get; set; }

        /// <summary>
        /// 运费金额
        /// </summary>
        public decimal? FreightAmount { get; set; }

        /// <summary>
        /// 计费件数（按件计费的件数口径 = 明细购买数量合计）
        /// </summary>
        public int? ItemCount { get; set; }

        /// <summary>
        /// 计费方式：1 按件计费 / 2 满额包邮
        /// </summary>
        public int? ChargeType { get; set; }

        /// <summary>
        /// 计费方式文案（冗余存储，前端可直接展示）
        /// </summary>
        public string? ChargeTypeText { get; set; }

        /// <summary>
        /// 计费规则说明（如「首件 ¥8.00，续件 ¥2.00/件，共 2 件」）
        /// </summary>
        public string? ChargeRule { get; set; }

        /// <summary>
        /// 是否包邮：0 否 / 1 是
        /// </summary>
        public int? FreeShipping { get; set; }

        /// <summary>
        /// 应付合计（商品金额 + 运费金额）
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 运费支付状态（枚举 PayStatus：0 未支付 / 1 已支付）
        /// </summary>
        public PayStatus? PayStatus { get; set; }

        /// <summary>
        /// 运费支付时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 生效时间（本次运费计算结果的生效时刻）
        /// </summary>
        public DateTime? EffectiveTime { get; set; }

        /// <summary>
        /// 是否作废：0 正常 / 1 作废（订单取消/退款时置 1，历史留痕）
        /// </summary>
        public int? IsBan { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }
    }
}
