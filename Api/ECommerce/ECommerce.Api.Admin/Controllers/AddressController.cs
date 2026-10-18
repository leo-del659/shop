using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Member;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 消费者收货地址控制器
    /// 提供收货地址的列表、详情、新增/更新、删除与设为默认能力，按当前登录消费者隔离数据
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class AddressController : ControllerBase
    {
        private readonly IMemberAddressService _service;

        /// <summary>
        /// 构造函数，注入收货地址服务
        /// </summary>
        /// <param name="service">收货地址服务实例</param>
        public AddressController(IMemberAddressService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取当前消费者的收货地址列表（默认地址排最前）
        /// </summary>
        /// <returns>收货地址列表</returns>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }
            var result = await _service.GetListAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// 获取单个收货地址详情
        /// </summary>
        /// <param name="id">地址主键</param>
        /// <returns>地址详情</returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }
            var result = await _service.GetAsync(userId, id);
            return Ok(result);
        }

        /// <summary>
        /// 新增或更新收货地址
        /// </summary>
        /// <param name="dto">地址保存参数</param>
        /// <returns>保存结果</returns>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MemberAddress_Request_Dto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }
            var result = await _service.SaveAsync(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="dto">包含地址主键的请求体</param>
        /// <returns>删除结果</returns>
        [HttpPost]
        public async Task<IActionResult> Remove([FromBody] IdRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }
            var result = await _service.RemoveAsync(userId, dto.Id);
            return Ok(result);
        }

        /// <summary>
        /// 设为默认地址
        /// </summary>
        /// <param name="dto">包含地址主键的请求体</param>
        /// <returns>设置结果</returns>
        [HttpPost]
        public async Task<IActionResult> SetDefault([FromBody] IdRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }
            var result = await _service.SetDefaultAsync(userId, dto.Id);
            return Ok(result);
        }

        /// <summary>
        /// 从当前登录用户上下文中获取用户标识（即登录凭据 code，与订单 client_id 一致）
        /// </summary>
        private string? GetCurrentUserId()
        {
            return User.FindFirst("unique_name")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
