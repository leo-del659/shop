using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 物流公司
    /// </summary>
    public partial class LogisticsCompany
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 物流公司名称
        /// </summary>
        public string? CompanyName { get; set; }
        /// <summary>
        /// 物流编码
        /// </summary>
        public string? Code { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public int? IsBan { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
