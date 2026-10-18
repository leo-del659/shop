using System;
using ECommerce.Infrastructure.EFCore;

namespace ECommerce.Infrastructure.Dto.Response.Order
{
    /// <summary>
    /// 运费明细（前端 camelCase 命名与之一一对应）
    /// 说明：freightAmount 为运费金额，goodsAmount 为商品金额，
    /// totalAmount = goodsAmount + freightAmount，为订单应付合计。
    /// </summary>
    public class FreightDetailDto
    {
        /// <summary>
        /// 运费标识（未落库的预览计算为 null）
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 运费单号
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
        /// 商品金额（计费基数）
        /// </summary>
        public decimal GoodsAmount { get; set; }

        /// <summary>
        /// 运费金额
        /// </summary>
        public decimal FreightAmount { get; set; }

        /// <summary>
        /// 计费方式：1 按件计费 / 2 满额包邮
        /// </summary>
        public int ChargeType { get; set; }

        /// <summary>
        /// 计费方式文案（如「按件计费」「满额包邮」）
        /// </summary>
        public string ChargeTypeText { get; set; } = string.Empty;

        /// <summary>
        /// 计费规则说明
        /// </summary>
        public string ChargeRule { get; set; } = string.Empty;

        /// <summary>
        /// 是否包邮
        /// </summary>
        public bool FreeShipping { get; set; }

        /// <summary>
        /// 应付合计（商品金额 + 运费金额）
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 运费支付状态：0 未支付 / 1 已支付
        /// </summary>
        public PayStatus? PayStatus { get; set; }

        /// <summary>
        /// 运费支付状态文案
        /// </summary>
        public string PayStatusText { get; set; } = string.Empty;

        /// <summary>
        /// 运费支付时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime? EffectiveTime { get; set; }

        /// <summary>
        /// 运费展示文案：包邮时「包邮」，否则「¥8.00」
        /// </summary>
        public string FreightAmountText { get; set; } = string.Empty;

        /// <summary>
        /// 合计展示文案：如「¥353.00」
        /// </summary>
        public string TotalAmountText { get; set; } = string.Empty;

        /// <summary>
        /// 计费件数
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
