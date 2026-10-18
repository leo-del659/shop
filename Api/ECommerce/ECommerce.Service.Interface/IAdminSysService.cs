using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IAdminSysService
    {
        // 部门
        Task<Api_Response> GetDepts();
        Task<Api_Response> SaveDept(SysDepartment t);
        Task<Api_Response> RemoveDept(string id);
        // 角色
        Task<Api_Response> GetRoles();
        Task<Api_Response> SaveRole(SysRole t);
        Task<Api_Response> RemoveRole(string id);
        // 菜单
        Task<Api_Response> GetMenus();
        Task<Api_Response> SaveMenu(SysMenu t);
        Task<Api_Response> RemoveMenu(string id);
        // 部门角色绑定
        Task<Api_Response> SaveDeptRole(SysDeptRole t);
        Task<Api_Response> BindDeptRoleMenus(string deptRoleId, System.Collections.Generic.List<string> menuIds);
        Task<Api_Response> GetDeptRoleMenus(string deptRoleId);
        // 字典 / 配置 / 通知
        Task<Api_Response> GetDicts();
        Task<Api_Response> GetConfigs();
        Task<Api_Response> SaveConfig(SysConfig t);
        Task<Api_Response> GetNotices();
        Task<Api_Response> SaveNotice(SysNotice t);
        Task<Api_Response> RemoveNotice(string id);
    }
}
