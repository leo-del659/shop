using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 短信验证码表
    /// </summary>
    public partial class SmsCode
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 手机号码
        /// </summary>
        public string? Mobile { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string? Code { get; set; }
        /// <summary>
        /// 验证码类型 1-商户 2-商户 3-管理员
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// 是否已使用 0-否 1-是
        /// </summary>
        public int? IsUsed { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpireTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 发送IP
        /// </summary>
        public string? Ip { get; set; }
    }
}
