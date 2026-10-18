using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Login
{
    /// <summary>
    /// 手机号 + 验证码 登录请求
    /// </summary>
    [Serializable]
    public class Sms_Login_Request_Dto
    {
        /// <summary>
        /// 手机号码（即后台账号 u_account）
        /// </summary>
        public string? mobile { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        public string? code { get; set; }
    }
}
