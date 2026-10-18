using ECommerce.Infrastructure.Dto.Request.Order;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 小程序订单运费控制器
    /// 路由前缀：/api/mini-program/freight
    /// - POST  calc        按订单参数计算并返回运费金额
    /// - POST  pay         运费支付处理
    /// - GET   {orderId}   查询指定订单的运费明细
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/mini-program/freight")]
    public class FreightController : ControllerBase
    {
        private readonly IFreightService _freightService;

        public FreightController(IFreightService freightService)
        {
            _freightService = freightService;
        }

        /// <summary>
        /// 计算运费。
        /// 传 orderId 时按订单算并落库；传 items 时为下单前预览（不落库）。
        /// </summary>
        [HttpPost("calc")]
        public async Task<IActionResult> Calc([FromBody] FreightCalcRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var result = await _freightService.CalcAsync(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// 运费支付：提交即完成，记录支付状态与支付时间。
        /// </summary>
        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromBody] FreightPayRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var result = await _freightService.PayAsync(userId, GetCurrentUserMobile(), dto);
            return Ok(result);
        }

        /// <summary>
        /// 查询指定订单的运费明细（含计费方式、运费金额、生效时间、支付状态）。
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetByOrder(string orderId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var result = await _freightService.GetByOrderAsync(userId, GetCurrentUserMobile(), orderId);
            return Ok(result);
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst("unique_name")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
        }

        private string? GetCurrentUserMobile()
        {
            return User.FindFirst("mobile")?.Value;
        }
    }
}
