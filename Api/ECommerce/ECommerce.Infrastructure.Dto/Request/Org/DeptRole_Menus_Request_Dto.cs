using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Org
{
    public class DeptRole_Menus_Request_Dto
    {
        //部门角色 对应的ID

        //权限ID


        /// <summary>
        /// 部门角色对应的ID列表
        /// </summary>
        public List<string>? deptrole_ids { get; set; }

        /// <summary>
        /// 菜单ID列表
        /// </summary>
        public List<string>? menu_ids { get; set; }

    }
}
