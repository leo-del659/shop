using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户资质
    /// </summary>
    public partial class MerchantQualification
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 商户ID
        /// </summary>
        public string? MerchantId { get; set; }
        /// <summary>
        /// 资质类型
        /// </summary>
        public string? QualType { get; set; }
        /// <summary>
        /// 资质编号
        /// </summary>
        public string? QualNo { get; set; }
        /// <summary>
        /// 资质文件
        /// </summary>
        public string? FileUrl { get; set; }
        /// <summary>
        /// 到期日期
        /// </summary>
        public string? ExpireDate { get; set; }
        /// <summary>
        /// 是否作废
        /// </summary>
        public int? IsBan { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
