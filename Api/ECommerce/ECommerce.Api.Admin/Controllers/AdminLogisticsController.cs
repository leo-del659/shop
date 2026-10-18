using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 物流公司管理控制器（路由前缀 /api/Logistics）
    /// 提供物流公司的分页查询、详情、新增、修改与删除等后台管理功能
    /// </summary>
    [Route("api/Logistics/[action]")]
    [ApiController]
    public class AdminLogisticsController : BaseController
    {
        private readonly IAdminLogisticsService _service;
        /// <summary>
        /// 构造函数，注入登录服务与物流公司服务
        /// </summary>
        public AdminLogisticsController(I_Login_Service login_Service, IAdminLogisticsService service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取物流公司分页列表
        /// </summary>
        /// <param name="page">页码，默认 1</param>
        /// <param name="size">每页数量，默认 10</param>
        /// <param name="keyword">关键字（可选）</param>
        /// <returns>分页结果</returns>
        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int size = 10, string? keyword = null)
            => Ok(await _service.GetList(page, size, keyword));

        /// <summary>
        /// 获取物流公司详情
        /// </summary>
        /// <param name="id">物流公司 ID</param>
        /// <returns>物流公司详情</returns>
        [HttpGet]
        public async Task<IActionResult> Get(string id) => Ok(await _service.Get(id));

        /// <summary>
        /// 新增物流公司
        /// </summary>
        /// <param name="t">物流公司实体</param>
        /// <returns>新增结果</returns>
        [HttpPost]
        public async Task<IActionResult> Add(LogisticsCompany t) => Ok(await _service.Add(t));

        /// <summary>
        /// 修改物流公司
        /// </summary>
        /// <param name="t">物流公司实体</param>
        /// <returns>修改结果</returns>
        [HttpPost]
        public async Task<IActionResult> Update(LogisticsCompany t) => Ok(await _service.Update(t));

        /// <summary>
        /// 删除物流公司
        /// </summary>
        /// <param name="id">物流公司 ID</param>
        /// <returns>删除结果</returns>
        [HttpGet]
        public async Task<IActionResult> Remove(string id) => Ok(await _service.Remove(id));

        /// <summary>
        /// 预览下一个物流编码（WL + 4 位数字，不写入数据库）
        /// </summary>
        /// <returns>下一个可用编码</returns>
        [HttpGet]
        public async Task<IActionResult> PreviewNextLogisticsCode() => Ok(await _service.PreviewNextLogisticsCode());

        /// <summary>
        /// 将库内所有物流公司编码一次性重排为统一的 WL + 4 位数字格式
        /// </summary>
        /// <returns>受影响条数</returns>
        [HttpPost]
        public async Task<IActionResult> NormalizeCodes() => Ok(await _service.NormalizeCodes());
    }
}
