using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.Dto.Response.Org;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class Org_Service : Base_Service, I_Org_Service
    {
        private readonly SysDepartment_EFCore _dept_EFCore;
        private readonly SysRole_EFCore _role_EFCore;
        private readonly SysMenu_EFCore _menu_EFCore;
        private readonly SysDeptRole_EFCore _deptRole_EFCore;

        public Org_Service(SysDepartment_EFCore dept_EFCore, SysRole_EFCore role_EFCore, SysMenu_EFCore menu_EFCore, SysDeptRole_EFCore deptRole_EFCore)
        {
            _dept_EFCore = dept_EFCore;
            _role_EFCore = role_EFCore;
            _menu_EFCore = menu_EFCore;
            _deptRole_EFCore = deptRole_EFCore;
        }

        public Task<Api_Response> GetDeptTree()
        {
            var all = _dept_EFCore.Query().OrderBy(d => d.Sort).ToList();
            var dict = all.ToDictionary(d => d.Id);
            var nodes = all.Where(d => string.IsNullOrEmpty(d.ParentId) || !dict.ContainsKey(d.ParentId))
                .Select(d => ToNode(d, all)).ToList();
            return Task.FromResult(Ok("ok", nodes));
        }

        private object ToNode(SysDepartment d, List<SysDepartment> all)
        {
            var children = all.Where(c => c.ParentId == d.Id).OrderBy(c => c.Sort)
                .Select(c => ToNode(c, all)).ToList();
            return new
            {
                id = d.Id,
                parent_id = d.ParentId,
                name = d.DeptName,
                sort = d.Sort,
                isBan = d.IsBan,
                children
            };
        }

        public Task<Api_Response> GetRoles() => Task.FromResult(Ok("ok", _role_EFCore.Query().OrderBy(r => r.Sort).ToList()));

        public Task<Api_Response> GetMenuTree()
        {
            var all = _menu_EFCore.Query().OrderBy(m => m.Sort).ToList();
            var dict = all.ToDictionary(m => m.Id);
            var nodes = all.Where(m => string.IsNullOrEmpty(m.ParentId) || !dict.ContainsKey(m.ParentId))
                .Select(m => ToMenuNode(m, all)).ToList();
            return Task.FromResult(Ok("ok", nodes));
        }

        private Menu_Response_Dto ToMenuNode(SysMenu m, List<SysMenu> all)
        {
            return new Menu_Response_Dto
            {
                id = m.Id,
                parent_id = m.ParentId,
                name = m.MenuName,
                path = m.Path,
                icon = m.Icon,
                isMenu = m.IsMenu,
                sort = m.Sort,
                children = all.Where(c => c.ParentId == m.Id).OrderBy(c => c.Sort)
                    .Select(c => ToMenuNode(c, all)).ToList()
            };
        }

        public Task<Api_Response> GetDeptRoles() => Task.FromResult(Ok("ok", _deptRole_EFCore.Query().ToList()));
    }
}
