using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 商户结算控制器（路由前缀 /api/Settle，需 /settle 权限）
    /// 提供结算记录的分页查询、详情、新增、修改、删除，以及结算付款与退款
    /// </summary>
    [Route("api/Settle/[action]")]
    [ApiController]
    [Permission_("/settle")]
    public class AdminSettleController : BaseController
    {
        private readonly IAdminSettleService _service;
        /// <summary>
        /// 构造函数，注入登录服务与结算服务
        /// </summary>
        public AdminSettleController(I_Login_Service login_Service, IAdminSettleService service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取结算记录分页列表
        /// </summary>
        /// <param name="page">页码，默认 1</param>
        /// <param name="size">每页数量，默认 10</param>
        /// <param name="keyword">关键字（可选）</param>
        /// <returns>分页结果</returns>
        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int size = 10, string? keyword = null)
            => Ok(await _service.GetList(page, size, keyword));

        /// <summary>
        /// 获取结算记录详情
        /// </summary>
        /// <param name="id">结算记录 ID</param>
        /// <returns>结算记录详情</returns>
        [HttpGet]
        public async Task<IActionResult> Get(string id) => Ok(await _service.Get(id));

        /// <summary>
        /// 新增结算记录
        /// </summary>
        /// <param name="t">结算记录实体</param>
        /// <returns>新增结果</returns>
        [HttpPost]
        public async Task<IActionResult> Add(SettleRecord t) => Ok(await _service.Add(t));

        /// <summary>
        /// 修改结算记录
        /// </summary>
        /// <param name="t">结算记录实体</param>
        /// <returns>修改结果</returns>
        [HttpPost]
        public async Task<IActionResult> Update(SettleRecord t) => Ok(await _service.Update(t));

        /// <summary>
        /// 删除结算记录
        /// </summary>
        /// <param name="id">结算记录 ID</param>
        /// <returns>删除结果</returns>
        [HttpGet]
        public async Task<IActionResult> Remove(string id) => Ok(await _service.Remove(id));

        /// <summary>
        /// 结算付款
        /// </summary>
        /// <param name="settleId">结算记录 ID</param>
        /// <param name="payNo">支付流水号</param>
        /// <param name="payAmount">支付金额</param>
        /// <returns>付款结果</returns>
        [HttpPost]
        public async Task<IActionResult> Pay(SettlePayDto dto) => Ok(await _service.Pay(dto.SettleId, dto.PayNo, dto.PayAmount));

        /// <summary>
        /// 结算退款
        /// </summary>
        /// <param name="settleId">结算记录 ID</param>
        /// <param name="refundNo">退款流水号</param>
        /// <param name="refundAmount">退款金额</param>
        /// <returns>退款结果</returns>
        [HttpPost]
        public async Task<IActionResult> Refund(SettleRefundDto dto) => Ok(await _service.Refund(dto.SettleId, dto.RefundNo, dto.RefundAmount));

        /// <summary>
        /// 自动生成结算单号（前端 open_add 调用）
        /// 格式：JS{yyyyMMdd}-{2位序号}，与 DB 现存数据严格一致
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateSettleNo() => Ok(await _service.GenerateSettleNo());
    }

    /// <summary>结算付款请求（复杂类型，由模型绑定从 JSON body 读取）</summary>
    public class SettlePayDto
    {
        public string SettleId { get; set; } = string.Empty;
        public string PayNo { get; set; } = string.Empty;
        public decimal PayAmount { get; set; }
    }
    /// <summary>结算退款请求</summary>
    public class SettleRefundDto
    {
        public string SettleId { get; set; } = string.Empty;
        public string RefundNo { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
    }
}
