using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    public partial class SysMenu
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 父菜单
        /// </summary>
        public string? ParentId { get; set; }
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string? MenuName { get; set; }
        /// <summary>
        /// 路由
        /// </summary>
        public string? Path { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; }
        /// <summary>
        /// 1菜单 0按钮
        /// </summary>
        public int? IsMenu { get; set; }
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
