using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Api.Admin.Controllers
{

    /// <summary>
    /// 后台控制器基类
    /// 提供统一的路由基类（api/[controller]/[action]）与当前登录用户获取能力，所有后台管理控制器均继承自此类
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// 登录服务实例，用于校验登录态与获取当前用户
        /// </summary>
        protected readonly I_Login_Service _login_Service;

        /// <summary>
        /// 构造函数，注入登录服务
        /// </summary>
        /// <param name="login_Service">登录服务</param>
        public BaseController(I_Login_Service login_Service)
        {
            _login_Service = login_Service;

        }

        /// <summary>
        /// 获取当前登陆用户
        /// 从登录态 Claim 中解析用户标识，并校验登录有效性后返回用户实体
        /// </summary>
        /// <returns>当前登录用户；未登录或校验失败返回 null</returns>
        protected SysUser? Get_Curr_User()
        {
            var claim = User.Claims
           .FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (claim == null)
            {
                return null;
            }
            var code = claim.Value;
            var user = _login_Service.Check_Login(code);
            if (user == null)
            {
                return null;
            }
            return user;
        }
    }
}
