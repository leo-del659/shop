using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Order;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 订单管理控制器（路由前缀 /api/Order，需 /order 权限）
    /// 提供订单的分页查询、详情、新增、修改、删除，以及付款、发货、退款、退货等履约操作
    /// </summary>
    [Route("api/Order/[action]")]
    [ApiController]
    [Permission_("/order")]
    public class AdminOrderController : BaseController
    {
        private readonly IAdminOrderService _service;
        private readonly IRefundService _refundService;
        /// <summary>
        /// 构造函数，注入登录服务与订单服务
        /// </summary>
        public AdminOrderController(I_Login_Service login_Service, IAdminOrderService service, IRefundService refundService) : base(login_Service)
        {
            _service = service;
            _refundService = refundService;
        }

        /// <summary>
        /// 获取订单分页列表（支持关键字、状态、商户筛选）
        /// </summary>
        /// <param name="page">页码，默认 1</param>
        /// <param name="size">每页数量，默认 10</param>
        /// <param name="keyword">关键字（可选）</param>
        /// <param name="status">订单状态（可选）</param>
        /// <param name="clientId">商户ID（可选）</param>
        /// <returns>分页结果</returns>
        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int size = 10, string? keyword = null, int? status = null, string? clientId = null)
            => Ok(await _service.GetList(page, size, keyword, status, clientId));

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="id">订单 ID</param>
        /// <returns>订单详情</returns>
        [HttpGet]
        public async Task<IActionResult> Get(string id) => Ok(await _service.Get(id));

        /// <summary>
        /// 新增订单
        /// </summary>
        /// <param name="t">订单实体</param>
        /// <returns>新增结果</returns>
        [HttpPost]
        public async Task<IActionResult> Add(ClientOrder t) => Ok(await _service.Add(t));

        /// <summary>
        /// 修改订单
        /// </summary>
        /// <param name="t">订单实体</param>
        /// <returns>修改结果</returns>
        [HttpPost]
        public async Task<IActionResult> Update(ClientOrder t) => Ok(await _service.Update(t));

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="id">订单 ID</param>
        /// <returns>删除结果</returns>
        [HttpGet]
        public async Task<IActionResult> Remove(string id) => Ok(await _service.Remove(id));

        /// <summary>
        /// 订单付款
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <param name="payNo">支付流水号</param>
        /// <param name="payAmount">支付金额</param>
        /// <param name="payType">支付方式，默认 1</param>
        /// <returns>付款结果</returns>
        [HttpPost]
        public async Task<IActionResult> Pay(PayOrderDto dto)
            => Ok(await _service.Pay(dto.OrderId, dto.PayNo, dto.PayAmount, dto.PayType ?? 1));

        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <param name="logisticsId">物流公司 ID</param>
        /// <param name="trackingNo">快递单号</param>
        /// <returns>发货结果</returns>
        [HttpPost]
        public async Task<IActionResult> Delivery(DeliveryOrderDto dto)
            => Ok(await _service.Delivery(dto.OrderId, dto.LogisticsId, dto.TrackingNo));

        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <param name="refundNo">退款流水号</param>
        /// <param name="refundAmount">退款金额</param>
        /// <param name="reason">退款原因</param>
        /// <returns>退款结果</returns>
        [HttpPost]
        public async Task<IActionResult> Refund(RefundOrderDto dto)
            => Ok(await _service.Refund(dto.OrderId, dto.RefundNo, dto.RefundAmount, dto.Reason));

        /// <summary>
        /// 订单退货
        /// </summary>
        /// <param name="orderId">订单 ID</param>
        /// <param name="returnNo">退货单号</param>
        /// <param name="returnNum">退货数量</param>
        /// <param name="reason">退货原因</param>
        /// <returns>退货结果</returns>
        [HttpPost]
        public async Task<IActionResult> Return(ReturnOrderDto dto)
            => Ok(await _service.Return(dto.OrderId, dto.ReturnNo, dto.ReturnNum, dto.Reason));

        /// <summary>
        /// 退款/退货审核（同意或驳回）
        /// </summary>
        /// <param name="dto">审核参数（Id / Approve / Remark）</param>
        /// <returns>审核结果</returns>
        [HttpPost]
        public async Task<IActionResult> RefundReview(RefundReviewRequestDto dto)
        {
            var admin = Get_Curr_User();
            if (admin == null) return Unauthorized();
            var result = await _refundService.ReviewAsync(admin.UId, dto);
            return Ok(result);
        }

        /// <summary>
        /// 退款列表（后台）
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="size">每页数量</param>
        /// <param name="status">退款状态（可选）：0待审核 1已退款 2驳回</param>
        /// <returns>分页结果</returns>
        [HttpGet]
        public async Task<IActionResult> RefundList(int page = 1, int size = 20, int? status = null)
            => Ok(await _refundService.GetAdminListAsync(page, size, status));

        /// <summary>
        /// 退款详情（后台，管理员可看任意退款单）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RefundDetail(string id)
            => Ok(await _refundService.GetAdminDetailAsync(id));

        /// <summary>
        /// 自动生成订单号（前端 open_add 调用）
        /// 格式：{yyMMdd}{4位当日序号}，例如 2608120001
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateOrderNo() => Ok(await _service.GenerateOrderNo());

        /// <summary>
        /// 管理首页多维销售/财务分析看板
        /// 返回销售周趋势、分类销售占比、退款趋势、商户销售排行、财务概览
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard() => Ok(await _service.Dashboard());
    }

    /// <summary>订单支付请求（复杂类型，由模型绑定从 JSON body 读取）</summary>
    public class PayOrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string PayNo { get; set; } = string.Empty;
        public decimal PayAmount { get; set; }
        public int? PayType { get; set; }
    }
    /// <summary>订单发货请求</summary>
    public class DeliveryOrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string LogisticsId { get; set; } = string.Empty;
        public string TrackingNo { get; set; } = string.Empty;
    }
    /// <summary>订单退款请求</summary>
    public class RefundOrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string RefundNo { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
    /// <summary>订单退货请求</summary>
    public class ReturnOrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string ReturnNo { get; set; } = string.Empty;
        public int ReturnNum { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
