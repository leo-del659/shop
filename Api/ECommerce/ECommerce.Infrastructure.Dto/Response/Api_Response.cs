using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Response
{
    [Serializable]
    public class Api_Response
    {
        /// <summary>
        /// 响应结果0/-1/-500
        /// </summary>
        public Api_Code code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string? message { get; set; }

        /// <summary>
        /// 响应参数
        /// </summary>
        public object? data { get; set; }

    }
}
