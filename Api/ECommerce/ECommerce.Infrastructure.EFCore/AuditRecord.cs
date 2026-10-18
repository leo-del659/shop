using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 审核记录表
    /// </summary>
    public partial class AuditRecord
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 业务类型 商品/商户/商户
        /// </summary>
        public string? BizType { get; set; }
        /// <summary>
        /// 业务主键
        /// </summary>
        public string? BizId { get; set; }
        /// <summary>
        /// 1通过 2驳回
        /// </summary>
        public int? AuditStatus { get; set; }
        /// <summary>
        /// 审核意见
        /// </summary>
        public string? AuditRemark { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string? Auditor { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
