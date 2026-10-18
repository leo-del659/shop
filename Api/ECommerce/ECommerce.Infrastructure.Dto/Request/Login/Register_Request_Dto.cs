using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Login
{
    /// <summary>
    /// 消费者（C端）账号注册请求
    /// 通过手机号 + 短信验证码完成注册，注册成功后会自动写入一条登录日志并返回登录凭据
    /// </summary>
    [Serializable]
    public class Register_Request_Dto
    {
        /// <summary>
        /// 手机号码（注册后作为登录账号 u_account）
        /// </summary>
        public string? mobile { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        public string? code { get; set; }
    }
}
