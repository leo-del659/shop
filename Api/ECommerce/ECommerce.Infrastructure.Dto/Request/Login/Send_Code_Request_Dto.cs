using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Login
{
    /// <summary>
    /// 发送短信验证码请求
    /// </summary>
    [Serializable]
    public class Send_Code_Request_Dto
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        public string? mobile { get; set; }

        /// <summary>
        /// 验证码类型 1-商户 2-商户 3-管理员（后台）
        /// </summary>
        public int? type { get; set; }

        /// <summary>
        /// 请求来源 IP（由 Controller 从 HttpContext 填充）
        /// </summary>
        public string? ip { get; set; }
    }
}
