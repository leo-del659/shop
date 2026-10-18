using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Login
{
    [Serializable]
    public class Login_Request_Dto
    {

        public string?  account { get; set; }

        public string? password { get; set; }

        /// <summary>
        /// 登陆凭据 一次有效
        /// </summary>
        public string? code { get; set; }
    }
}
