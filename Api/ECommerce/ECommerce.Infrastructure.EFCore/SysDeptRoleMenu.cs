using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 部门角色菜单权限
    /// </summary>
    public partial class SysDeptRoleMenu
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 部门角色ID
        /// </summary>
        public string? DeptRoleId { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public string? MenuId { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
