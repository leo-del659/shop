using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 微信支付控制器
    /// - prepay：小程序发起支付时调用，返回 wx.requestPayment 参数包
    /// - notify：微信支付成功回调（匿名，微信服务器调用），验签+解密后落库
    /// - simulate/notify：开发模拟（无商户号环境联调），本地加密回放
    /// </summary>
    [ApiController]
    [Route("api/pay")]
    public class WeChatPayController : ControllerBase
    {
        private readonly IWeChatPayService _payService;

        public WeChatPayController(IWeChatPayService payService)
        {
            _payService = payService;
        }

        /// <summary>JSAPI 预下单：返回前端调起支付所需参数</summary>
        [HttpPost("prepay")]
        [Authorize]
        public async Task<IActionResult> Prepay([FromBody] WeChatPrepayDto dto)
        {
            var result = await _payService.PrepayAsync(dto?.OrderId ?? "", dto?.OpenId ?? "");
            return Ok(result);
        }

        /// <summary>微信支付成功通知（真实回调入口，匿名）</summary>
        [HttpPost("notify")]
        [AllowAnonymous]
        public async Task<IActionResult> Notify()
        {
            var signature = Request.Headers["Wechatpay-Signature"].ToString();
            var timestamp = Request.Headers["Wechatpay-Timestamp"].ToString();
            var nonce = Request.Headers["Wechatpay-Nonce"].ToString();
            var serial = Request.Headers["Wechatpay-Serial"].ToString();

            string body;
            using (var reader = new System.IO.StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            var result = await _payService.HandleNotifyAsync(signature, timestamp, nonce, serial, body);
            if (result.code == Api_Code.ok)
            {
                return Ok(new { code = "SUCCESS", message = "成功" });
            }
            return Ok(new { code = "FAIL", message = result.message });
        }

        /// <summary>开发模拟：用本地密钥加密一段支付成功通知并回放（无需商户号）</summary>
        [HttpPost("simulate/notify")]
        [AllowAnonymous]
        public async Task<IActionResult> SimulateNotify([FromBody] SimulateNotifyDto dto)
        {
            var result = await _payService.SimulateNotifyAsync(dto?.OrderNo ?? "", dto?.TransactionId);
            return Ok(result);
        }
    }

    public class WeChatPrepayDto
    {
        public string OrderId { get; set; } = "";
        public string OpenId { get; set; } = "";
    }

    public class SimulateNotifyDto
    {
        public string OrderNo { get; set; } = "";
        public string? TransactionId { get; set; }
    }
}
