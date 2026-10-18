using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 部门角色绑定
    /// </summary>
    public partial class SysDeptRole
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 部门ID
        /// </summary>
        public string? DepartmentId { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public string? RoleId { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public int? IsBan { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
