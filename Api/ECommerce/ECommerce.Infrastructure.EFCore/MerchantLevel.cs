using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户等级
    /// </summary>
    public partial class MerchantLevel
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 等级名称
        /// </summary>
        public string? LevelName { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
