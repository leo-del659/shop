using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Org
{
    public class User_Request_Dto:Base_Request_Dto
    {
      

        /// <summary>
        /// 账号
        /// </summary>
        public string? account { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// 部门/组织
        /// </summary>
        public string? dept { get; set; }

        /// <summary>
        /// 角色/岗位
        /// </summary>
        public string? role { get; set; }

        public int? isBan { get; set; }

    }
}
