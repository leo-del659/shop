using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 用户端登录账号（消费者手机号验证码登录，免注册）
    /// 说明：自 2026-10-17 起移除密码/盐字段——消费者登录仅走手机号+短信验证码，
    ///       不再支持账号密码登录；表结构增加用户名称与头像字段（用户资料）。
    /// 账号是否可用由 client.is_ban 兜底校验。
    /// </summary>
    public partial class ClientLogin
    {
        public long Id { get; set; }
        /// <summary>用户ID（消费者 sys_users.UId）</summary>
        public string ClientId { get; set; } = null!;
        /// <summary>登录账号（手机号）</summary>
        public string Account { get; set; } = null!;
        /// <summary>用户名称（默认取手机号，可在设置页修改）</summary>
        public string? Name { get; set; }
        /// <summary>头像地址（相对地址 /api/file/...，可为空）</summary>
        public string? Avatar { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
