using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统角色
    /// </summary>
    public partial class SysRole
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 角色名称
        /// </summary>
        public string? RoleName { get; set; }
        /// <summary>
        /// 角色权限标识
        /// </summary>
        public string? RoleKey { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public int? IsBan { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
