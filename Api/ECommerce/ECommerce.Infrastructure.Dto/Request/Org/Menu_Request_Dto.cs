using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Org
{
    /// <summary>
    /// 菜单
    /// </summary>
    public class Menu_Request_Dto:Base_Request_Dto
    {
        /// <summary>
        /// 父级菜单ID
        /// </summary>
        public string? parent_id { get; set; }
        /// <summary>
        /// 菜单名字
        /// </summary>
        public string? name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? path { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? icon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? isMenu { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? sort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? remark { get; set; }

    }
}
