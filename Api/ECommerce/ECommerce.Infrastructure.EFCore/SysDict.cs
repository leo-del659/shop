using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统字典
    /// </summary>
    public partial class SysDict
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 字典类型
        /// </summary>
        public string? DictType { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string? DictLabel { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string? DictValue { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public int? IsBan { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
