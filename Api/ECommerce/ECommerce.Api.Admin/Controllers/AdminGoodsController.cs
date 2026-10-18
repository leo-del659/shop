using ECommerce.Infrastructure.Dto.Request.Goods;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 商品管理控制器（路由前缀 /api/Goods）
    /// 提供商品的分页查询、详情获取、新增、修改、删除，以及提交审核与审核处理等后台管理功能
    /// </summary>
    [Route("api/Goods/[action]")]
    [ApiController]
    public class AdminGoodsController : BaseController
    {
        private readonly IAdminGoodsService _service;
        /// <summary>
        /// 构造函数，注入登录服务与商品服务
        /// </summary>
        public AdminGoodsController(I_Login_Service login_Service, IAdminGoodsService service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取商品分页列表
        /// </summary>
        /// <param name="page">页码，默认 1</param>
        /// <param name="size">每页数量，默认 10</param>
        /// <param name="keyword">商品名称关键字（可选）</param>
        /// <param name="categoryId">商品分类ID（可选）</param>
        /// <param name="merchantId">商户ID（可选），传入后仅返回该商户可供应的商品</param>
        /// <returns>包含 total 与 list 的分页结果</returns>
        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int size = 10, string? keyword = null, string? categoryId = null, string? merchantId = null)
            => Ok(await _service.GetList(page, size, keyword, null, categoryId, merchantId));

        /// <summary>
        /// 获取单个商品详情（含 SKU 规格列表）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品详情</returns>
        [HttpGet]
        public async Task<IActionResult> Get(string id) => Ok(await _service.Get(id));

        /// <summary>
        /// 新增商品（进入审核流程）
        /// </summary>
        /// <param name="dto">商品创建参数</param>
        /// <returns>新增结果</returns>
        [HttpPost]
        public async Task<IActionResult> Add(GoodCreateDto dto) => Ok(await _service.Add(dto));

        /// <summary>
        /// 修改商品（同步更新 SKU 规格）
        /// </summary>
        /// <param name="dto">商品修改参数</param>
        /// <returns>修改结果</returns>
        [HttpPost]
        public async Task<IActionResult> Update(GoodModifyDto dto) => Ok(await _service.Update(dto));

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>删除结果</returns>
        [HttpGet]
        public async Task<IActionResult> Remove(string id) => Ok(await _service.Remove(id));

        /// <summary>
        /// 提交商品审核
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>提交结果</returns>
        [HttpGet]
        public async Task<IActionResult> SubmitAudit(string id) => Ok(await _service.SubmitAudit(id));

        /// <summary>
        /// 商品审核处理（通过 / 驳回）
        /// </summary>
        /// <param name="dto">审核处理参数</param>
        /// <returns>审核结果</returns>
        [HttpPost]
        public async Task<IActionResult> Audit(AuditHandleDto dto) => Ok(await _service.Audit(dto));

        /// <summary>
        /// 设置商品上架/下架状态
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="isSale">1 上架 / 0 下架</param>
        /// <returns>操作结果</returns>
        [HttpGet]
        public async Task<IActionResult> SetSale(string id, int isSale) => Ok(await _service.SetSale(id, isSale));
    }
}
