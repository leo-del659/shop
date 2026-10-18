using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Login;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 登录控制器
    /// 负责后台登录、登录状态检查、当前用户信息与菜单权限查询
    /// </summary>
    [ApiController]
    public class LoginController : BaseController
    {
        private readonly JWT_Helper _jWT_Helper;

        /// <summary>
        /// 构造函数，注入登录服务与 JWT 帮助类
        /// </summary>
        /// <param name="login_Service">登录服务</param>
        /// <param name="jWT_Helper">JWT 工具类</param>
        public LoginController(I_Login_Service login_Service, JWT_Helper jWT_Helper) : base(login_Service)
        {
            _jWT_Helper = jWT_Helper;
        }

        /// <summary>
        /// 获取单个用户信息
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <returns>用户信息结果</returns>
        [Authorize(Roles = "admin")]
        [HttpGet]
        [ActiconLog_(action = "查询单个用户", group = "用户管理")]
        public IActionResult Get_Single_User(string id)
        {
            var result = _login_Service.Get_User_ByID(id);
            return Ok(result);
        }

        /// <summary>
        /// 管理员登录
        /// </summary>
        /// <param name="dto">登录请求参数</param>
        /// <returns>登录结果</returns>
        [ActiconLog_(action = "管理员登陆", group = "用户管理")]
        [HttpPost]
        public async Task<IActionResult> Login_Admin(Login_Request_Dto dto)
        {
            dto.code = Config.NewSequentialGuid();

            var result = await _login_Service.Login_Admin(dto);

            if (result.code == Infrastructure.Dto.Response.Api_Code.ok)
            {
                string token = _jWT_Helper.GenerateToken(dto.code, "admin");
                result.data = token;
            }

            return Ok(result);
        }

        /// <summary>
        /// 检查当前用户是否已登录
        /// </summary>
        /// <returns>登录状态结果</returns>
        [HttpGet]
        [Authorize]
        public IActionResult Check_Login()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (claim == null)
            {
                return Unauthorized();
            }

            var code = claim.Value;
            var user = _login_Service.Check_Login(code);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new Api_Response
            {
                code = Api_Code.ok,
                message = "ok",
                data = new
                {
                    account = user.UAccount,
                    // 优先用 sys_users.u_name（真名字）；为空时回退账号，避免出现空名
                    name = string.IsNullOrWhiteSpace(user.UName) ? user.UAccount : user.UName
                }
            });
        }

        /// <summary>
        /// 获取当前登录用户基本信息
        /// </summary>
        /// <returns>当前用户信息</returns>
        [HttpGet]
        [Authorize]
        public IActionResult Get_Current_User()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (claim == null)
            {
                return Unauthorized();
            }

            var result = _login_Service.Get_Current_User(claim.Value);
            return Ok(result);
        }

        /// <summary>
        /// 更新消费者个人信息（昵称 / 头像）
        /// 需登录；从当前 JWT 凭证解析出用户后写入 sys_users
        /// </summary>
        [HttpPost]
        [Authorize]
        public IActionResult Update_Profile([FromBody] Update_Profile_Dto dto)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)
                        ?? User.Claims.FirstOrDefault(c => c.Type == "unique_name");
            if (claim == null)
            {
                return Unauthorized();
            }

            var result = _login_Service.Update_Profile(claim.Value, dto?.name, dto?.avatar);
            return Ok(result);
        }

        /// <summary>
        /// 获取当前登录用户可见的菜单权限树（复杂 RBAC）
        /// </summary>
        /// <returns>当前用户可见的菜单权限树（含父子层级）</returns>
        /// <summary>
        /// 消费者（小程序）手机号 + 短信验证码登录
        /// 调用登录服务校验验证码，通过后为该登录凭据签发 JWT（角色 user）
        /// </summary>
        /// <param name="dto">手机号与短信验证码</param>
        /// <returns>登录结果，成功时返回 token 与登录凭据 code</returns>
        [HttpPost]
        public async Task<IActionResult> Login(Sms_Login_Request_Dto dto)
        {
            var result = await _login_Service.Login_By_Sms(dto);
            if (result.code != Api_Code.ok)
            {
                return Ok(result);
            }

            // result.data 为登录凭据 code，作为 JWT 的主题标识
            string credential = result.data?.ToString() ?? string.Empty;
            string token = _jWT_Helper.GenerateToken(credential, "user", dto.mobile);

            return Ok(new Api_Response
            {
                code = Api_Code.ok,
                message = "ok",
                data = new { token, code = credential }
            });
        }

        /// <summary>
        /// 消费者（小程序）账号注册
        /// 校验短信验证码并创建账号，注册成功后自动登录（为该登录凭据签发 JWT，角色 user）
        /// </summary>
        /// <param name="dto">手机号与短信验证码</param>
        /// <returns>注册结果，成功时返回 token 与登录凭据 code</returns>
        [HttpPost]
        public async Task<IActionResult> Register(Register_Request_Dto dto)
        {
            var result = await _login_Service.Register(dto);
            if (result.code != Api_Code.ok)
            {
                return Ok(result);
            }

            // result.data 为登录凭据 code，作为 JWT 的主题标识，注册后直接登录
            string credential = result.data?.ToString() ?? string.Empty;
            string token = _jWT_Helper.GenerateToken(credential, "user", dto.mobile);

            return Ok(new Api_Response
            {
                code = Api_Code.ok,
                message = "注册成功",
                data = new { token, code = credential }
            });
        }

        /// <summary>
        /// 获取当前登录消费者的用户资料（client_login：账号 / 用户名称 / 头像）
        /// 需登录；从 JWT 解析用户身份（消费者登录时主题为 sys_users.UId）
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult Client_Profile()
        {
            var claim = User.FindFirst(ClaimTypes.Name)
                        ?? User.FindFirst("unique_name");
            if (claim == null)
            {
                return Unauthorized();
            }
            var mobile = User.FindFirst("mobile")?.Value;
            var result = _login_Service.Get_Client_Profile(claim.Value, mobile);
            return Ok(result);
        }

        /// <summary>
        /// 更新消费者用户资料（用户名称 / 头像，写入 client_login）
        /// 需登录；name 为 null/空白不修改，avatar 传 null 不修改、显式传 "" 清除头像
        /// </summary>
        [HttpPost]
        [Authorize]
        public IActionResult Update_Client_Profile([FromBody] Update_Profile_Dto dto)
        {
            var claim = User.FindFirst(ClaimTypes.Name)
                        ?? User.FindFirst("unique_name");
            if (claim == null)
            {
                return Unauthorized();
            }
            var mobile = User.FindFirst("mobile")?.Value;
            var result = _login_Service.Update_Client_Profile(claim.Value, mobile, dto?.name, dto?.avatar);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get_Menus()
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (claim == null)
            {
                return Unauthorized();
            }
            var result = _login_Service.Get_Current_Menus(claim.Value);
            return Ok(result);
        }

        /// <summary>
        /// 修改登录密码（适用于管理员和商户端）
        /// 需登录；从 JWT 解析用户身份，校验原密码后更新为新密码
        /// </summary>
        /// <param name="dto">原密码和新密码</param>
        /// <returns>修改结果</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Change_Password(Change_Password_Dto dto)
        {
            if (dto?.oldPassword == null || dto?.newPassword == null)
            {
                return Ok(new Api_Response { code = Api_Code.fail, message = "原密码和新密码不能为空" });
            }
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)
                        ?? User.Claims.FirstOrDefault(c => c.Type == "unique_name");
            if (claim == null)
            {
                return Unauthorized();
            }
            var result = await _login_Service.Change_Password(claim.Value, dto.oldPassword, dto.newPassword);
            return Ok(result);
        }



    }
}
