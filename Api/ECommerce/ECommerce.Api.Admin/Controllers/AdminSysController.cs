using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Org;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 系统管理控制器（路由前缀 /api/Sys，需 /sys 权限）
    /// 提供部门、角色、菜单、部门角色权限绑定等复杂 RBAC 管理，以及数据字典、系统配置与通知管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Permission_("/sys")]
    public class AdminSysController : BaseController
    {
        private readonly IAdminSysService _service;
        /// <summary>
        /// 构造函数，注入登录服务与系统管理服务
        /// </summary>
        public AdminSysController(I_Login_Service login_Service, IAdminSysService service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>获取部门列表</summary>
        [HttpGet] public async Task<IActionResult> GetDepts() => Ok(await _service.GetDepts());
        /// <summary>保存（新增 / 更新）部门</summary>
        /// <param name="t">部门实体</param>
        [HttpPost] public async Task<IActionResult> SaveDept(SysDepartment t) => Ok(await _service.SaveDept(t));
        /// <summary>删除部门</summary>
        /// <param name="id">部门 ID</param>
        [HttpGet] public async Task<IActionResult> RemoveDept(string id) => Ok(await _service.RemoveDept(id));

        /// <summary>获取角色列表</summary>
        [HttpGet] public async Task<IActionResult> GetRoles() => Ok(await _service.GetRoles());
        /// <summary>保存（新增 / 更新）角色</summary>
        /// <param name="t">角色实体</param>
        [HttpPost] public async Task<IActionResult> SaveRole(SysRole t) => Ok(await _service.SaveRole(t));
        /// <summary>删除角色</summary>
        /// <param name="id">角色 ID</param>
        [HttpGet] public async Task<IActionResult> RemoveRole(string id) => Ok(await _service.RemoveRole(id));

        /// <summary>获取菜单列表</summary>
        [HttpGet] public async Task<IActionResult> GetMenus() => Ok(await _service.GetMenus());
        /// <summary>保存（新增 / 更新）菜单</summary>
        /// <param name="t">菜单实体</param>
        [HttpPost] public async Task<IActionResult> SaveMenu(SysMenu t) => Ok(await _service.SaveMenu(t));
        /// <summary>删除菜单</summary>
        /// <param name="id">菜单 ID</param>
        [HttpGet] public async Task<IActionResult> RemoveMenu(string id) => Ok(await _service.RemoveMenu(id));

        /// <summary>保存部门角色绑定（新增 / 更新）</summary>
        /// <param name="t">部门角色实体</param>
        [HttpPost] public async Task<IActionResult> SaveDeptRole(SysDeptRole t) => Ok(await _service.SaveDeptRole(t));
        /// <summary>为部门角色绑定菜单权限（复杂 RBAC 核心：部门角色 → 可访问菜单）</summary>
        /// <param name="dto">包含部门角色 ID 与菜单 ID 列表的请求</param>
        [HttpPost] public async Task<IActionResult> BindDeptRoleMenus(DeptRole_Menus_Request_Dto dto) => Ok(await _service.BindDeptRoleMenus(dto.deptrole_ids != null && dto.deptrole_ids.Count > 0 ? dto.deptrole_ids[0] : null, dto.menu_ids));
        /// <summary>获取部门角色已授权菜单 ID 列表</summary>
        /// <param name="deptRoleId">部门角色 ID</param>
        [HttpGet] public async Task<IActionResult> GetDeptRoleMenus(string deptRoleId) => Ok(await _service.GetDeptRoleMenus(deptRoleId));

        /// <summary>获取数据字典列表</summary>
        [HttpGet] public async Task<IActionResult> GetDicts() => Ok(await _service.GetDicts());
        /// <summary>获取系统配置列表</summary>
        [HttpGet] public async Task<IActionResult> GetConfigs() => Ok(await _service.GetConfigs());
        /// <summary>保存系统配置</summary>
        /// <param name="t">系统配置实体</param>
        [HttpPost] public async Task<IActionResult> SaveConfig(SysConfig t) => Ok(await _service.SaveConfig(t));
        /// <summary>获取系统通知列表</summary>
        [HttpGet] public async Task<IActionResult> GetNotices() => Ok(await _service.GetNotices());
        /// <summary>保存系统通知</summary>
        /// <param name="t">系统通知实体</param>
        [HttpPost] public async Task<IActionResult> SaveNotice(SysNotice t) => Ok(await _service.SaveNotice(t));
        /// <summary>删除系统通知</summary>
        /// <param name="id">通知 ID</param>
        [HttpGet] public async Task<IActionResult> RemoveNotice(string id) => Ok(await _service.RemoveNotice(id));
    }
}
