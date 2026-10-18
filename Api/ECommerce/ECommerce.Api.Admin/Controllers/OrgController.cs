using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 组织架构控制器（路由前缀 /api/Org）
    /// 提供部门树、角色、菜单树以及部门角色列表等只读查询，供前端下拉与权限分配使用
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrgController : BaseController
    {
        private readonly I_Org_Service _service;
        /// <summary>
        /// 构造函数，注入登录服务与组织架构服务
        /// </summary>
        public OrgController(I_Login_Service login_Service, I_Org_Service service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>获取部门树（含层级结构）</summary>
        [HttpGet] public async Task<IActionResult> GetDeptTree() => Ok(await _service.GetDeptTree());
        /// <summary>获取角色列表</summary>
        [HttpGet] public async Task<IActionResult> GetRoles() => Ok(await _service.GetRoles());
        /// <summary>获取菜单树</summary>
        [HttpGet] public async Task<IActionResult> GetMenuTree() => Ok(await _service.GetMenuTree());
        /// <summary>获取部门角色列表（供权限分配页使用）</summary>
        [HttpGet] public async Task<IActionResult> GetDeptRoles() => Ok(await _service.GetDeptRoles());
    }
}
