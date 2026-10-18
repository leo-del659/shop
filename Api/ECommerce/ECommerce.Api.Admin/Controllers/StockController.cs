using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 库存管理控制器（路由前缀 /api/Stock，需 /stock 权限）
    /// 提供库存的分页查询、详情、新增、修改、删除，以及入库 / 出库操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Permission_("/stock")]
    public class StockController : BaseController
    {
        private readonly IAdminStockService _service;
        /// <summary>
        /// 构造函数，注入登录服务与库存服务
        /// </summary>
        public StockController(I_Login_Service login_Service, IAdminStockService service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>获取库存分页列表</summary>
        [HttpGet] public async Task<IActionResult> GetList(int page = 1, int size = 10, string? keyword = null)
            => Ok(await _service.GetList(page, size, keyword));

        /// <summary>获取库存详情</summary>
        [HttpGet] public async Task<IActionResult> Get(string id) => Ok(await _service.Get(id));

        /// <summary>新增库存记录</summary>
        [HttpPost] public async Task<IActionResult> Add(Stock t) => Ok(await _service.Add(t));

        /// <summary>修改库存记录</summary>
        [HttpPost] public async Task<IActionResult> Update(Stock t) => Ok(await _service.Update(t));

        /// <summary>删除库存记录</summary>
        [HttpGet] public async Task<IActionResult> Remove(string id) => Ok(await _service.Remove(id));

        /// <summary>入库</summary>
        [HttpPost] public async Task<IActionResult> StockIn(StockIoDto dto)
            => Ok(await _service.StockIn(dto.GoodsId, dto.Warehouse, dto.Num, dto.Remark));

        /// <summary>出库</summary>
        [HttpPost] public async Task<IActionResult> StockOut(StockIoDto dto)
            => Ok(await _service.StockOut(dto.GoodsId, dto.Warehouse, dto.Num, dto.Remark));

        /// <summary>
        /// 获取所有已使用过的仓库名（distinct 排序），用于"仓库"字段下拉数据源
        /// </summary>
        [HttpGet] public async Task<IActionResult> GetWarehouseList()
            => Ok(await _service.GetWarehouseList());
    }

    /// <summary>
    /// 出入库请求参数（前端以 JSON body 提交，复杂类型由模型绑定自动从 body 读取）
    /// </summary>
    public class StockIoDto
    {
        public string? GoodsId { get; set; }
        public string? Warehouse { get; set; }
        public int Num { get; set; }
        public string? Remark { get; set; }
    }
}
