using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统登录日志表
    /// </summary>
    public partial class SysLoginLog
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 关联用户ID(sys_users.u_id)
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 登录账号(冗余)
        /// </summary>
        public string Account { get; set; } = null!;
        /// <summary>
        /// 登录IP地址
        /// </summary>
        public string? LoginIp { get; set; }
        /// <summary>
        /// 登录地点
        /// </summary>
        public string? LoginAddress { get; set; }
        /// <summary>
        /// 登陆的凭据
        /// </summary>
        public string? Code { get; set; }
        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginTime { get; set; }
    }
}
