using System;

namespace ECommerce.Infrastructure.Attributes
{
    /// <summary>
    /// 接口/按钮级权限标识。
    /// 标记在 Controller 或 Action 上时，Permission_Filter 会校验当前登录用户
    /// （按 部门+角色）是否拥有该权限 key（对应 sys_menu.Path）。
    /// 例：[Permission_("/api/Order/Refund")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class Permission_Attribute : Attribute
    {
        /// <summary>
        /// 所需权限 key，通常等于对应按钮菜单的 sys_menu.Path
        /// </summary>
        public string Key { get; }

        public Permission_Attribute(string key)
        {
            Key = key;
        }
    }
}
