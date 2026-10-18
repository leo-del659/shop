using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request
{
    /// <summary>
    /// 分页的内容
    /// </summary>
    public class Base_Request_Dto
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int? page { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int? limit { get; set; }

        /// <summary>
        /// 查询条件
        /// </summary>
        public string? key { get; set; }

        /// <summary>
        /// 单个获取时传递ID参数
        /// </summary>
        public string? id { get; set; }


        /// <summary>
        /// id列表
        /// </summary>
        public List<string?>? ids { get; set; }


    }
}
