using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Org
{
    public class Dept_Request_Dto:Base_Request_Dto
    {
        

        /// <summary>
        /// 部门上级ID
        /// </summary>
        public string? parent_id { get; set; }
        /// <summary>
        /// 部门名字
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? sort { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? remark { get; set; }

       
        /// <summary>
        /// 选择部门存在哪一些角色
        /// </summary>
        public List<string?>? roles { get; set; }



    }
}
