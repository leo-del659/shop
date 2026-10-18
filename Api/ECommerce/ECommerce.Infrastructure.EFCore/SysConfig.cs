using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统参数配置
    /// </summary>
    public partial class SysConfig
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 配置键
        /// </summary>
        public string? ConfigKey { get; set; }
        /// <summary>
        /// 配置值
        /// </summary>
        public string? ConfigValue { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
