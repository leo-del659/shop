using System;

namespace ECommerce.Infrastructure.Dto.Request.Goods
{
    /// <summary>
    /// 审核处理 DTO
    /// </summary>
    public class AuditHandleDto
    {
        /// <summary>
        /// 业务主键（商品/商户/商户ID）
        /// </summary>
        public string? id { get; set; }
        /// <summary>
        /// 业务类型 商品/商户/商户
        /// </summary>
        public string? biz_type { get; set; }
        /// <summary>
        /// 1通过 2驳回
        /// </summary>
        public int? audit_status { get; set; }
        /// <summary>
        /// 审核意见
        /// </summary>
        public string? audit_remark { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string? auditor { get; set; }
    }
}
