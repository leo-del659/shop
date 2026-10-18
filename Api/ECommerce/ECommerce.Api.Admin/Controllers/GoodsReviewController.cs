using ECommerce.Infrastructure.Dto.Request.Goods;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 商品评价管理（后台）
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GoodsReviewController : BaseController
    {
        private readonly IGoodsReviewService _service;

        public GoodsReviewController(I_Login_Service login_Service, IGoodsReviewService service) : base(login_Service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int size = 10, string? keyword = null,
            string? tier = null, string? tag = null, int? status = null, string? goodsId = null)
            => Ok(await _service.GetList(page, size, keyword, tier, tag, status, goodsId));

        [HttpPost]
        public async Task<IActionResult> Audit(GoodsReviewAuditDto dto) => Ok(await _service.Audit(dto));

        [HttpPost]
        public async Task<IActionResult> Reply(GoodsReviewReplyDto dto) => Ok(await _service.Reply(dto));

        [HttpGet]
        public async Task<IActionResult> Remove(string id) => Ok(await _service.Remove(id));
    }
}
