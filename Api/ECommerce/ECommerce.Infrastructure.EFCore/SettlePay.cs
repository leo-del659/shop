using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 结算支付
    /// </summary>
    public partial class SettlePay
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 结算记录ID
        /// </summary>
        public string? SettleId { get; set; }
        /// <summary>
        /// 支付流水号
        /// </summary>
        public string? PayNo { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal? PayAmount { get; set; }
        /// <summary>
        /// 支付状态（枚举 PayStatus：0未付 1已付）
        /// </summary>
        public PayStatus? PayStatus { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
