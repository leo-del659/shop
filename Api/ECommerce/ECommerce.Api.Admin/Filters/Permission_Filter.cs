using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Filters
{
    /// <summary>
    /// 接口/按钮级权限过滤器（复杂 RBAC 的最后一道闸）。
    /// 仅当 Action/Controller 标记了 [Permission_("key")] 时才生效，校验当前登录用户
    /// （部门 + 角色 → 部门角色 → 菜单）是否拥有该 key（对应 sys_menu.Path）。
    /// 无标记则直接放行，鉴权由 JWT 中间件与 [Authorize] 负责。
    /// </summary>
    public class Permission_Filter : IAsyncActionFilter
    {
        private readonly I_Login_Service _login_Service;
        private readonly SysDeptRole_EFCore _deptRole_EFCore;
        private readonly SysDeptRoleMenu_EFCore _deptRoleMenu_EFCore;
        private readonly SysMenu_EFCore _menu_EFCore;

        public Permission_Filter(
            I_Login_Service login_Service,
            SysDeptRole_EFCore deptRole_EFCore,
            SysDeptRoleMenu_EFCore deptRoleMenu_EFCore,
            SysMenu_EFCore menu_EFCore)
        {
            _login_Service = login_Service;
            _deptRole_EFCore = deptRole_EFCore;
            _deptRoleMenu_EFCore = deptRoleMenu_EFCore;
            _menu_EFCore = menu_EFCore;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. 是否声明了 [Permission_] 权限标识
            var attr = context.ActionDescriptor.EndpointMetadata
                .FirstOrDefault(m => m is Permission_Attribute) as Permission_Attribute;
            if (attr == null)
            {
                await next();
                return;
            }

            // 2. 取出当前登录凭据（JWT 的 unique_name / Name = 登录 code）
            var claim = context.HttpContext.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (claim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // 3. 凭据 → 当前用户（含 Dept / Role）
            var user = _login_Service.Check_Login(claim.Value);
            if (user == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // 4. 部门 + 角色 → 部门角色
            var deptRole = _deptRole_EFCore
                .Query(d => d.DepartmentId == user.Dept && d.RoleId == user.Role && d.IsBan != 1)
                .FirstOrDefault();
            if (deptRole == null)
            {
                context.Result = Forbidden(attr.Key);
                return;
            }

            // 5. 部门角色 → 菜单 → Path 集合
            var menuIds = _deptRoleMenu_EFCore
                .Query(d => d.DeptRoleId == deptRole.Id)
                .Select(d => d.MenuId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();

            var paths = _menu_EFCore
                .Query(d => menuIds.Contains(d.Id) && d.IsBan != 1)
                .Select(d => d.Path)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            // 6. 校验所需权限 key
            //    采用"模块级"匹配：拥有父模块权限即可访问其下任意子接口，拥有任一子菜单权限
            //    也可推导拥有父模块权限。这样即使用户只绑定了 /sys/menu 这类子菜单（未显式绑定
            //    /sys 父菜单），标记了 [Permission_("/sys")] 的 AdminSys 控制器接口也能正常放行。
            if (!paths.Any(p => Has_Permission(p, attr.Key)))
            {
                context.Result = Forbidden(attr.Key);
                return;
            }

            await next();
        }

        /// <summary>
        /// 判断用户拥有的菜单路径 path 是否满足所需权限 key（双向模块匹配）。
        ///   - 精确相等：path == key
        ///   - 拥有子权限推导父模块：path 是 key 的子路径（/sys/menu 满足 /sys）
        ///   - 拥有父模块推导子权限：key 是 path 的子路径（/sys 满足 /sys/menu）
        /// 只要 key 与 path 互为"自身或父子层级"，即视为有权限。
        /// </summary>
        private static bool Has_Permission(string path, string key)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(key))
                return false;

            path = path.Trim();
            key = key.Trim();

            if (string.Equals(path, key, StringComparison.OrdinalIgnoreCase))
                return true;

            // path 是 key 的子路径：/sys/menu 满足 /sys
            if (path.Length > key.Length
                && path.StartsWith(key, StringComparison.OrdinalIgnoreCase)
                && path[key.Length] == '/')
                return true;

            // key 是 path 的子路径：/sys 满足 /sys/menu（拥有父模块即拥有其下全部子权限）
            if (key.Length > path.Length
                && key.StartsWith(path, StringComparison.OrdinalIgnoreCase)
                && key[path.Length] == '/')
                return true;

            return false;
        }

        private static JsonResult Forbidden(string key)
        {
            return new JsonResult(new Api_Response
            {
                code = Api_Code.fail,
                message = "无访问权限：" + key
            })
            { StatusCode = 403 };
        }
    }
}
