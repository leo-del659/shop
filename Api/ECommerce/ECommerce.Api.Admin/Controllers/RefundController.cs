using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Order;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 小程序退款/退货退款控制器
    /// 消费者发起退款申请、查询我的退款列表与详情
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/mini-program/refunds")]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        /// <summary>
        /// 申请退款/退货退款
        /// </summary>
        [Idempotent_]
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyRefundRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
            var result = await _refundService.ApplyAsync(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// 我的退款列表
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> MyList([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
            var result = await _refundService.GetMyRefundsAsync(userId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// 退款详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
            var result = await _refundService.GetDetailAsync(userId, id);
            return Ok(result);
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst("unique_name")?.Value
                ?? User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        }
    }
}
