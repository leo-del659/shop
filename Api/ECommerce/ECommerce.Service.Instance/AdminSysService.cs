using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class AdminSysService : Base_Service, IAdminSysService
    {
        private readonly SysDepartment_EFCore _dept_EFCore;
        private readonly SysRole_EFCore _role_EFCore;
        private readonly SysMenu_EFCore _menu_EFCore;
        private readonly SysDeptRole_EFCore _deptRole_EFCore;
        private readonly SysDeptRoleMenu_EFCore _deptRoleMenu_EFCore;
        private readonly SysDict_EFCore _dict_EFCore;
        private readonly SysConfig_EFCore _config_EFCore;
        private readonly SysNotice_EFCore _notice_EFCore;

        public AdminSysService(
            SysDepartment_EFCore dept_EFCore,
            SysRole_EFCore role_EFCore,
            SysMenu_EFCore menu_EFCore,
            SysDeptRole_EFCore deptRole_EFCore,
            SysDeptRoleMenu_EFCore deptRoleMenu_EFCore,
            SysDict_EFCore dict_EFCore,
            SysConfig_EFCore config_EFCore,
            SysNotice_EFCore notice_EFCore)
        {
            _dept_EFCore = dept_EFCore;
            _role_EFCore = role_EFCore;
            _menu_EFCore = menu_EFCore;
            _deptRole_EFCore = deptRole_EFCore;
            _deptRoleMenu_EFCore = deptRoleMenu_EFCore;
            _dict_EFCore = dict_EFCore;
            _config_EFCore = config_EFCore;
            _notice_EFCore = notice_EFCore;
        }

        public Task<Api_Response> GetDepts() => Task.FromResult(Ok("ok", _dept_EFCore.Query().OrderBy(d => d.Sort).ToList()));
        public Task<Api_Response> SaveDept(SysDepartment t)
        {
            if (string.IsNullOrEmpty(t.Id)) { t.Id = Config.NewSequentialGuid(); t.CreateTime = System.DateTime.Now; _dept_EFCore.Add(t); }
            else { var o = _dept_EFCore.Find(t.Id); if (o == null) return Task.FromResult(Fail("部门不存在")); o.DeptName = t.DeptName; o.ParentId = t.ParentId; o.Sort = t.Sort; o.IsBan = t.IsBan; _dept_EFCore.Update(o); }
            return Task.FromResult(Get_Result(_dept_EFCore.SaveChanges()));
        }
        public Task<Api_Response> RemoveDept(string id) { var t = _dept_EFCore.Find(id); if (t == null) return Task.FromResult(Fail("部门不存在")); _dept_EFCore.Delete(t); return Task.FromResult(Get_Result(_dept_EFCore.SaveChanges())); }

        public Task<Api_Response> GetRoles() => Task.FromResult(Ok("ok", _role_EFCore.Query().OrderBy(r => r.Sort).ToList()));
        public Task<Api_Response> SaveRole(SysRole t)
        {
            if (string.IsNullOrEmpty(t.Id)) { t.Id = Config.NewSequentialGuid(); t.CreateTime = System.DateTime.Now; _role_EFCore.Add(t); }
            else { var o = _role_EFCore.Find(t.Id); if (o == null) return Task.FromResult(Fail("角色不存在")); o.RoleName = t.RoleName; o.RoleKey = t.RoleKey; o.Sort = t.Sort; o.Remark = t.Remark; o.IsBan = t.IsBan; _role_EFCore.Update(o); }
            return Task.FromResult(Get_Result(_role_EFCore.SaveChanges()));
        }
        public Task<Api_Response> RemoveRole(string id) { var t = _role_EFCore.Find(id); if (t == null) return Task.FromResult(Fail("角色不存在")); _role_EFCore.Delete(t); return Task.FromResult(Get_Result(_role_EFCore.SaveChanges())); }

        public Task<Api_Response> GetMenus() => Task.FromResult(Ok("ok", _menu_EFCore.Query().OrderBy(m => m.Sort).ToList()));
        public Task<Api_Response> SaveMenu(SysMenu t)
        {
            if (string.IsNullOrEmpty(t.Id)) { t.Id = Config.NewSequentialGuid(); t.CreateTime = System.DateTime.Now; _menu_EFCore.Add(t); }
            else { var o = _menu_EFCore.Find(t.Id); if (o == null) return Task.FromResult(Fail("菜单不存在")); o.ParentId = t.ParentId; o.MenuName = t.MenuName; o.Path = t.Path ?? o.Path; o.Icon = t.Icon ?? o.Icon; o.IsMenu = t.IsMenu; o.Sort = t.Sort; /* 禁用开关已在管理页移除，保留原值避免误操作 */ _menu_EFCore.Update(o); }
            return Task.FromResult(Get_Result(_menu_EFCore.SaveChanges()));
        }
        public Task<Api_Response> RemoveMenu(string id) { var t = _menu_EFCore.Find(id); if (t == null) return Task.FromResult(Fail("菜单不存在")); _menu_EFCore.Delete(t); return Task.FromResult(Get_Result(_menu_EFCore.SaveChanges())); }

        public Task<Api_Response> SaveDeptRole(SysDeptRole t)
        {
            if (string.IsNullOrEmpty(t.Id)) { t.Id = Config.NewSequentialGuid(); t.CreateTime = System.DateTime.Now; _deptRole_EFCore.Add(t); }
            else { var o = _deptRole_EFCore.Find(t.Id); if (o == null) return Task.FromResult(Fail("绑定不存在")); o.DepartmentId = t.DepartmentId; o.RoleId = t.RoleId; o.IsBan = t.IsBan; _deptRole_EFCore.Update(o); }
            return Task.FromResult(Get_Result(_deptRole_EFCore.SaveChanges()));
        }

        public Task<Api_Response> BindDeptRoleMenus(string deptRoleId, List<string> menuIds)
        {
            var existing = _deptRoleMenu_EFCore.Query(m => m.DeptRoleId == deptRoleId).ToList();
            foreach (var e in existing) _deptRoleMenu_EFCore.Delete(e);
            if (menuIds != null)
            {
                foreach (var mid in menuIds)
                {
                    _deptRoleMenu_EFCore.Add(new SysDeptRoleMenu
                    {
                        Id = Config.NewSequentialGuid(),
                        DeptRoleId = deptRoleId,
                        MenuId = mid,
                        CreateTime = System.DateTime.Now
                    });
                }
            }
            return Task.FromResult(Get_Result(_deptRoleMenu_EFCore.SaveChanges()));
        }

        public Task<Api_Response> GetDeptRoleMenus(string deptRoleId)
        {
            var ids = _deptRoleMenu_EFCore.Query(m => m.DeptRoleId == deptRoleId)
                .Select(m => m.MenuId)
                .Where(id => id != null)
                .ToList();
            return Task.FromResult(Ok("ok", ids));
        }

        public Task<Api_Response> GetDicts() => Task.FromResult(Ok("ok", _dict_EFCore.Query().OrderBy(d => d.Sort).ToList()));
        public Task<Api_Response> GetConfigs() => Task.FromResult(Ok("ok", _config_EFCore.Query().ToList()));
        public Task<Api_Response> SaveConfig(SysConfig t)
        {
            if (string.IsNullOrEmpty(t.Id)) { t.Id = Config.NewSequentialGuid(); t.CreateTime = System.DateTime.Now; _config_EFCore.Add(t); }
            else { var o = _config_EFCore.Find(t.Id); if (o == null) return Task.FromResult(Fail("配置不存在")); o.ConfigKey = t.ConfigKey; o.ConfigValue = t.ConfigValue; o.Remark = t.Remark; _config_EFCore.Update(o); }
            return Task.FromResult(Get_Result(_config_EFCore.SaveChanges()));
        }
        public Task<Api_Response> GetNotices() => Task.FromResult(Ok("ok", _notice_EFCore.Query().OrderByDescending(n => n.CreateTime).ToList()));
        public Task<Api_Response> SaveNotice(SysNotice t)
        {
            if (string.IsNullOrEmpty(t.Id)) { t.Id = Config.NewSequentialGuid(); t.CreateTime = System.DateTime.Now; _notice_EFCore.Add(t); }
            else { var o = _notice_EFCore.Find(t.Id); if (o == null) return Task.FromResult(Fail("公告不存在")); o.Title = t.Title; o.Content = t.Content; o.Type = t.Type; o.IsBan = t.IsBan; _notice_EFCore.Update(o); }
            return Task.FromResult(Get_Result(_notice_EFCore.SaveChanges()));
        }
        public Task<Api_Response> RemoveNotice(string id)
        {
            var t = _notice_EFCore.Find(id);
            if (t == null) return Task.FromResult(Fail("公告不存在"));
            _notice_EFCore.Delete(t);
            return Task.FromResult(Get_Result(_notice_EFCore.SaveChanges()));
        }
    }
}
