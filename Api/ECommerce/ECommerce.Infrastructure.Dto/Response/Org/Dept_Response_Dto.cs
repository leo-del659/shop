using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.Dto.Response.Org
{
    public class Dept_Response_Dto
    {
        public string? id { get; set; }
        public string? parent_id { get; set; }
        public string? name { get; set; }
        public int? sort { get; set; }
        public int? isBan { get; set; }
    }

    public class Role_Response_Dto
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? key { get; set; }
        public int? level { get; set; }
        public string? remark { get; set; }
    }

    public class Menu_Response_Dto
    {
        public string? id { get; set; }
        public string? parent_id { get; set; }
        public string? name { get; set; }
        public string? path { get; set; }
        public string? icon { get; set; }
        public int? isMenu { get; set; }
        public int? sort { get; set; }
        public List<Menu_Response_Dto>? children { get; set; }
    }

    public class User_Response_Dto
    {
        public string? id { get; set; }
        public string? account { get; set; }
        public string? name { get; set; }
        public string? dept { get; set; }
        public string? role { get; set; }
        public int? isBan { get; set; }
    }
}
