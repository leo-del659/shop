using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 结算退款
    /// </summary>
    public partial class SettleRefund
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 结算记录ID
        /// </summary>
        public string? SettleId { get; set; }
        /// <summary>
        /// 退款单号
        /// </summary>
        public string? RefundNo { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal? RefundAmount { get; set; }
        /// <summary>
        /// 0未退 1已退
        /// </summary>
        public int? RefundStatus { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
