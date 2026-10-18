using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统部门
    /// </summary>
    public partial class SysDepartment
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 上级部门
        /// </summary>
        public string? ParentId { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string? DeptName { get; set; }
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
