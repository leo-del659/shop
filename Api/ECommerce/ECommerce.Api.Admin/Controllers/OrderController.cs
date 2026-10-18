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
    /// 小程序订单控制器
    /// 负责处理订单创建、查询、详情、取消及支付回调等业务
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/mini-program")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        /// <summary>
        /// 构造函数，注入订单服务
        /// </summary>
        /// <param name="orderService">订单服务实例</param>
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="dto">创建订单请求参数</param>
        /// <returns>订单创建结果</returns>
        [Idempotent_]
        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var result = await _orderService.CreateOrderAsync(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// 获取当前用户订单列表
        /// </summary>
        /// <param name="status">订单状态筛选</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>订单分页列表</returns>
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] int? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var mobile = GetCurrentUserMobile();
            var result = await _orderService.GetOrdersAsync(userId, mobile, status, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <returns>订单详情信息</returns>
        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetOrderDetail(string orderId)
        {
            // 获取当前登录用户 ID
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var mobile = GetCurrentUserMobile();
            var result = await _orderService.GetOrderDetailAsync(userId, orderId, mobile);
            return Ok(result);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <param name="dto">取消订单请求参数</param>
        /// <returns>取消结果</returns>
        [HttpPost("orders/{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(string orderId, [FromBody] CancelOrderRequestDto? dto)
        {
            // 获取当前登录用户 ID
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var mobile = GetCurrentUserMobile();
            var result = await _orderService.CancelOrderAsync(userId, orderId, mobile, dto?.Reason);
            return Ok(result);
        }

        /// <summary>
        /// 买家删除订单（逻辑删除）
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("orders/{orderId}")]
        public async Task<IActionResult> DeleteOrder(string orderId)
        {
            // 获取当前登录用户 ID
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var mobile = GetCurrentUserMobile();
            var result = await _orderService.DeleteOrderAsync(userId, orderId, mobile);
            return Ok(result);
        }

        /// <summary>
        /// 支付回调接口
        /// 允许匿名访问，供第三方支付平台回调
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <param name="dto">支付回调参数</param>
        /// <returns>回调处理结果</returns>
        [AllowAnonymous]
        [HttpPost("orders/{orderId}/pay-callback")]
        public async Task<IActionResult> PayCallback(string orderId, [FromBody] PayCallbackRequestDto? dto)
        {
            var result = await _orderService.PayCallbackAsync(orderId, dto);
            return Ok(result);
        }

        /// <summary>
        /// 商家发货：sell_client_id 列已删除，订单不再保存售卖商户，
        /// 本接口当前直接返回失败（发货请走商户端 /api/merchant/orders/{id}/action）。
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <returns>发货结果</returns>
        [HttpPost("orders/{orderId}/ship")]
        public async Task<IActionResult> ShipOrder(string orderId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var mobile = GetCurrentUserMobile();
            var result = await _orderService.ShipOrderAsync(userId, orderId, mobile);
            return Ok(result);
        }

        /// <summary>
        /// 买家确认收货：将已发货订单标记为已完成
        /// 对应小程序端「确认收货」按钮
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <returns>确认结果</returns>
        [HttpPost("orders/{orderId}/receive")]
        public async Task<IActionResult> ConfirmReceipt(string orderId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var mobile = GetCurrentUserMobile();
            var result = await _orderService.ConfirmReceiptAsync(userId, orderId, mobile);
            return Ok(result);
        }

        /// <summary>
        /// 从当前登录用户上下文中获取用户 ID
        /// </summary>
        /// <returns>用户 ID</returns>
        private string? GetCurrentUserId()
        {
            return User.FindFirst("unique_name")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// 从当前登录用户上下文中获取手机号（JWT 的 mobile 声明）。
        /// 用于订单查询的「同手机号下所有历史 u_id」兜底，应对用户体系重构造成的旧订单 owner 脱节。
        /// </summary>
        private string? GetCurrentUserMobile()
        {
            return User.FindFirst("mobile")?.Value;
        }
    }
}