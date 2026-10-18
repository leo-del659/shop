using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户结算记录
    /// </summary>
    public partial class SettleRecord
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 买家用户ID（sys_users.u_id）
        /// </summary>
        public string? ClientId { get; set; }
        /// <summary>
        /// 结算单号
        /// </summary>
        public string? SettleNo { get; set; }
        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal? SettleAmount { get; set; }
        /// <summary>
        /// 1线上 2线下
        /// </summary>
        public int? SettleType { get; set; }
        /// <summary>
        /// 0待结算 1已结算 2已退款
        /// </summary>
        public int? Status { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
