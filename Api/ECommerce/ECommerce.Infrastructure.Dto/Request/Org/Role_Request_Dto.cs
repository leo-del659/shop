using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Org
{
    public class Role_Request_Dto:Base_Request_Dto
    {
        

        /// <summary>
        /// 角色名字
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// 角色Key
        /// </summary>
        public string? key { get; set; }

        /// <summary>
        /// 角色等级 越小越大
        /// </summary>
        public int? Level { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? remark { get; set; }
    }
}
