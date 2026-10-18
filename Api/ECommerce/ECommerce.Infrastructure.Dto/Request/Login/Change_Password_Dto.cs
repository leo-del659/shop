using System;

namespace ECommerce.Infrastructure.Dto.Request.Login
{
    /// <summary>
    /// 修改登录密码请求（适用于管理员和商户端）
    /// 原密码校验通过后，使用 Argon2id 加盐哈希存储新密码
    /// </summary>
    [Serializable]
    public class Change_Password_Dto
    {
        /// <summary>
        /// 原密码（明文，后端用 Argon2id 校验）
        /// </summary>
        public string? oldPassword { get; set; }

        /// <summary>
        /// 新密码（明文，长度不少于 6 位）
        /// </summary>
        public string? newPassword { get; set; }
    }
}
