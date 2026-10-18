using ECommerce.Infrastructure.Dto.Request.Login;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 短信控制器（路由前缀 /api/Sms）
    /// 提供后台短信验证码发送能力，用于管理员登录、绑定手机等场景
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SmsController : BaseController
    {
        private readonly I_Login_Service _service;
        /// <summary>
        /// 构造函数，注入登录服务（短信发送能力）
        /// </summary>
        public SmsController(I_Login_Service login_Service, I_Login_Service service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="dto">发送验证码请求（手机号等）</param>
        /// <returns>发送结果</returns>
        [HttpPost]
        public async Task<IActionResult> Send(Send_Code_Request_Dto dto)
        {
            // 从请求上下文中获取用户端 IP
            dto.ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return Ok(await _service.Send_Sms_Code(dto));
        }
    }
}
