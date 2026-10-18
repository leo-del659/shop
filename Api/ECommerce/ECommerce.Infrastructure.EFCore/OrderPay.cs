using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 订单支付
    /// </summary>
    public partial class OrderPay
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 订单ID
        /// </summary>
        public string? OrderId { get; set; }
        /// <summary>
        /// 支付流水号
        /// </summary>
        public string? PayNo { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal? PayAmount { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public int? PayType { get; set; }
        /// <summary>
        /// 支付状态（枚举 PayStatus：0未付 1已付）
        /// </summary>
        public PayStatus? PayStatus { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
