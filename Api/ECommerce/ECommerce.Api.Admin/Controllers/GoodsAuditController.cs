using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Goods;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 商品审核控制器（路由前缀 /api/GoodsAudit，需 /goods/audit 权限）
    /// 提供待审核商品列表与审核处理（通过 / 驳回）功能
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Permission_("/goods/audit")]
    public class GoodsAuditController : BaseController
    {
        private readonly IAdminGoodsService _service;
        /// <summary>
        /// 构造函数，注入登录服务与商品服务（审核）
        /// </summary>
        public GoodsAuditController(I_Login_Service login_Service, IAdminGoodsService service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>
        /// 待审核商品列表
        /// </summary>
        /// <param name="page">页码，默认 1</param>
        /// <param name="size">每页数量，默认 10</param>
        /// <returns>待审核商品分页结果</returns>
        [HttpGet]
        public async Task<IActionResult> PendingList(int page = 1, int size = 10)
            => Ok(await _service.GetList(page, size, null, 0));

        /// <summary>
        /// 审核处理（通过 / 驳回）
        /// </summary>
        /// <param name="dto">审核处理参数</param>
        /// <returns>审核结果</returns>
        [HttpPost]
        public async Task<IActionResult> Handle(AuditHandleDto dto) => Ok(await _service.Audit(dto));
    }
}
