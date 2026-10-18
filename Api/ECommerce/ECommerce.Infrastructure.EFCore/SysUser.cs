using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统用户表
    /// </summary>
    public partial class SysUser
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string UId { get; set; } = null!;
        /// <summary>
        /// 登录账号，唯一
        /// </summary>
        public string? UAccount { get; set; }
        /// <summary>
        /// 名字
        /// </summary>
        public string? UName { get; set; }
        /// <summary>
        /// 加密后的密码
        /// </summary>
        public string? UPassword { get; set; }
        /// <summary>
        /// 加密盐值
        /// </summary>
        public string? USalt { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? UCreateTime { get; set; }
        /// <summary>
        /// 是否禁用 0正常 1禁用
        /// </summary>
        public int? UIsBan { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        public string? Dept { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public string? Role { get; set; }
        /// <summary>
        /// 头像地址（消费者小程序端）
        /// </summary>
        public string? Avatar { get; set; }
    }
}
