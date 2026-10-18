using ECommerce.Infrastructure.Dto.Request.Goods;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 商品评价（移动端 /api/mini-program）
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/mini-program")]
    public class MiniReviewController : ControllerBase
    {
        private readonly IGoodsReviewService _service;

        public MiniReviewController(IGoodsReviewService service)
        {
            _service = service;
        }

        private string? GetMemberId()
            => User.FindFirst("unique_name")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;

        /// <summary>提交评价</summary>
        [HttpPost("reviews")]
        public async Task<IActionResult> SubmitReview([FromBody] GoodsReviewCreateDto dto)
        {
            var memberId = GetMemberId();
            if (string.IsNullOrWhiteSpace(memberId)) return Unauthorized();
            var result = await _service.Submit(dto, memberId);
            return Ok(result);
        }

        /// <summary>商品详情页评价列表（公开，支持档位/标签筛选）</summary>
        [AllowAnonymous]
        [HttpGet("reviews/goods/{goodsId}")]
        public async Task<IActionResult> GetGoodsReviews(string goodsId, int page = 1, int size = 20,
            string? tier = null, string? tag = null)
        {
            var result = await _service.GetByGoods(goodsId, page, size, tier, tag);
            return Ok(result);
        }

        /// <summary>查询某订单已评价的商品（防止重复评价）</summary>
        [HttpGet("reviews/order/{orderId}")]
        public async Task<IActionResult> GetOrderReviewed(string orderId)
        {
            var memberId = GetMemberId();
            if (string.IsNullOrWhiteSpace(memberId)) return Unauthorized();
            var result = await _service.GetOrderReviewed(orderId, memberId);
            return Ok(result);
        }
    }
}
