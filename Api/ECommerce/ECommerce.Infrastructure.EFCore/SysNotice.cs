using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统通知公告
    /// </summary>
    public partial class SysNotice
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 标题
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string? Content { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public int? IsBan { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
