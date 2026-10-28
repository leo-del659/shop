using ECommerce.Infrastructure.Dto.Request.Mobile.Merchant;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 商户端（微信小程序）控制器
    /// 统一前缀 /api/merchant，与已有 LoginController(api/login) 区分。
    /// 登录接口匿名；其余接口需携带 JWT Bearer（unique_name 声明为 merchant_id）。
    /// </summary>
    [ApiController]
    [Authorize(Roles = "merchant")]
    [Route("api/merchant")]
    public class MerchantMiniController : ControllerBase
    {
        private readonly IMerchantMiniService _service;
        private readonly IChatService _chatService;
        private readonly IMerchantNoticeService _noticeService;

        public MerchantMiniController(IMerchantMiniService service, IChatService chatService, IMerchantNoticeService noticeService)
        {
            _service = service;
            _chatService = chatService;
            _noticeService = noticeService;
        }

        private string? GetMerchantId()
        {
            return User.FindFirst("unique_name")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
        }

        private string? GetRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value;
        }


        /// <summary>账号密码登录（account 为手机号）</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] MerchantLoginDto dto)
        {
            return Ok(_service.Login(dto.Account ?? "", dto.Password ?? ""));
        }

        /// <summary>快速注册：手机号 + 短信验证码 + 密码</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] MerchantRegisterDto dto)
        {
            return Ok(_service.Register(dto.Phone ?? "", dto.Code ?? "", dto.Password ?? ""));
        }

        /// <summary>获取当前商户信息</summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetMe(id));
        }

        /// <summary>个人中心资料 + 统计</summary>
        [HttpGet("profile")]
        [Authorize]
        public IActionResult Profile()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetProfile(id));
        }

        /// <summary>更新商户资料</summary>
        [HttpPost("profile")]
        [Authorize]
        public IActionResult UpdateProfile([FromBody] MerchantProfileDto dto)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.UpdateProfile(id, dto));
        }

        /// <summary>修改密码</summary>
        [HttpPost("change-password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] MerchantChangePwdDto dto)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.ChangePassword(id, dto.OldPassword ?? "", dto.NewPassword ?? ""));
        }

        /// <summary>首页数据</summary>
        [HttpGet("home")]
        [Authorize]
        public IActionResult Home()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetHome(id));
        }

        /// <summary>商户余额（累计已结算、累计已提现、可结算余额 + 最新同步状态）</summary>
        [HttpGet("balance")]
        [Authorize]
        public IActionResult Balance()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetBalance(id));
        }

        /// <summary>触发余额同步（重算已完成订单并写同步日志，返回同步状态）</summary>
        [HttpPost("balance/sync")]
        [Authorize]
        public IActionResult SyncBalance()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.SyncBalance(id));
        }

        #region 商品

        /// <summary>商品列表</summary>
        [HttpGet("goods")]
        [Authorize]
        public IActionResult Goods([FromQuery] string filter = "all", [FromQuery] string category = "")
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetGoods(id, filter, category));
        }

        /// <summary>商品详情</summary>
        [HttpGet("goods/{id}")]
        [Authorize]
        public IActionResult GoodsDetail(string id)
        {
            var merchantId = GetMerchantId();
            if (string.IsNullOrWhiteSpace(merchantId)) return Unauthorized();
            return Ok(_service.GetGoodsDetail(merchantId, id));
        }

        /// <summary>商品规格（多规格 SKU 编辑页用）</summary>
        [HttpGet("goods/{id}/specs")]
        [Authorize]
        public IActionResult GoodsSpecs(string id)
        {
            var merchantId = GetMerchantId();
            if (string.IsNullOrWhiteSpace(merchantId)) return Unauthorized();
            return Ok(_service.GetGoodsSpecs(merchantId, id));
        }

        /// <summary>新增商品</summary>
        [HttpPost("goods")]
        [Authorize]
        public IActionResult AddGoods([FromBody] MerchantGoodsSaveDto dto)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.AddGoods(id, dto));
        }

        /// <summary>编辑商品</summary>
        [HttpPost("goods/{id}")]
        [Authorize]
        public IActionResult UpdateGoods(string id, [FromBody] MerchantGoodsSaveDto dto)
        {
            var merchantId = GetMerchantId();
            if (string.IsNullOrWhiteSpace(merchantId)) return Unauthorized();
            return Ok(_service.UpdateGoods(merchantId, id, dto));
        }

        /// <summary>上下架切换</summary>
        [HttpPost("goods/{id}/toggle")]
        [Authorize]
        public IActionResult ToggleGoods(string id)
        {
            var merchantId = GetMerchantId();
            if (string.IsNullOrWhiteSpace(merchantId)) return Unauthorized();
            return Ok(_service.ToggleGoods(merchantId, id));
        }

        /// <summary>补货</summary>
        [HttpPost("goods/{id}/restock")]
        [Authorize]
        public IActionResult RestockGoods(string id, [FromBody] MerchantRestockDto dto)
        {
            var merchantId = GetMerchantId();
            if (string.IsNullOrWhiteSpace(merchantId)) return Unauthorized();
            return Ok(_service.RestockGoods(merchantId, id, dto.Qty));
        }

        #endregion

        #region 订单

        /// <summary>订单列表（支持状态 + 日期范围筛选）</summary>
        [HttpGet("orders")]
        [Authorize]
        public IActionResult Orders([FromQuery] string status = "all", [FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetOrders(id, status, startDate, endDate));
        }

        /// <summary>商户账单列表（对齐前端管理后台 /settle/merchantbill 数据源）</summary>
        [HttpGet("bills")]
        [Authorize]
        public IActionResult Bills([FromQuery] string status = "all")
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetBills(id, status));
        }

        /// <summary>商户账单详情（含物流时间线）</summary>
        [HttpGet("bills/{id}")]
        [Authorize]
        public IActionResult BillDetail(string id)
        {
            var sid = GetMerchantId();
            if (string.IsNullOrWhiteSpace(sid)) return Unauthorized();
            return Ok(_service.GetBillDetail(sid, id));
        }

        /// <summary>所有商户订单列表（跨商户，支持状态 + 日期范围筛选）</summary>
        [HttpGet("orders/all")]
        [Authorize]
        public IActionResult OrdersAll([FromQuery] string status = "all", [FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetAllOrders(status, startDate, endDate));
        }

        /// <summary>订单详情</summary>
        [HttpGet("orders/{id}")]
        [Authorize]
        public IActionResult OrderDetail(string id)
        {
            var merchantId = GetMerchantId();
            if (string.IsNullOrWhiteSpace(merchantId)) return Unauthorized();
            return Ok(_service.GetOrderDetail(merchantId, id));
        }

        /// <summary>订单操作（接单/拒绝/发货/完成）</summary>
        [HttpPost("orders/{id}/action")]
        [Authorize]
        public IActionResult OrderAction(string id, [FromBody] MerchantOrderActionDto dto)
        {
            var merchantId = GetMerchantId();
            if (string.IsNullOrWhiteSpace(merchantId)) return Unauthorized();
            return Ok(_service.OrderAction(merchantId, id, dto.Action ?? "", dto.Reason));
        }

        #endregion

        #region 对账 / 提现

        /// <summary>对账数据（支持月份 + 日期范围筛选）</summary>
        [HttpGet("reconcile")]
        [Authorize]
        public IActionResult Reconcile([FromQuery] string month = "", [FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetReconcile(id, month, startDate, endDate));
        }

        /// <summary>提现记录（支持状态 + 时间范围筛选）</summary>
        [HttpGet("withdrawals")]
        [Authorize]
        public IActionResult Withdrawals([FromQuery] int? status = null, [FromQuery] string? startTime = null, [FromQuery] string? endTime = null)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetWithdrawals(id, status, startTime, endTime));
        }

        /// <summary>申请提现（发起微信转账到银行卡）</summary>
        [HttpPost("withdraw")]
        [Authorize]
        public async Task<IActionResult> Withdraw([FromBody] MerchantWithdrawDto dto)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(await _service.AddWithdraw(id, dto.Amount, dto.Remark ?? ""));
        }

        #endregion

        #region 银行 / 资质

        /// <summary>银行账户</summary>
        [HttpGet("bank")]
        [Authorize]
        public IActionResult Bank()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetBank(id));
        }

        /// <summary>更新银行账户</summary>
        [HttpPost("bank")]
        [Authorize]
        public IActionResult UpdateBank([FromBody] MerchantBankDto dto)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.UpdateBank(id, dto));
        }

        /// <summary>资质列表</summary>
        [HttpGet("qualification")]
        [Authorize]
        public IActionResult Qualifications()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.GetQualifications(id));
        }

        /// <summary>新增资质</summary>
        [HttpPost("qualification")]
        [Authorize]
        public IActionResult AddQualification([FromBody] MerchantQualificationDto dto)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(_service.AddQualification(id, dto));
        }

        #endregion

        #region 聊天（与用户端小程序消息互通）

        /// <summary>会话列表：聊过的买家（含最后一条消息与未读数）</summary>
        [HttpGet("chat/conversations")]
        public async Task<IActionResult> ChatConversations()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(await _chatService.MerchantConversations(id));
        }

        /// <summary>
        /// 拉取与某买家的消息（最近 50 条，升序返回）
        /// after：传最后一条消息的 createTime 做增量轮询，只返回该时间之后的新消息
        /// </summary>
        [HttpGet("chat/clients/{clientId}/messages")]
        public async Task<IActionResult> ChatMessages(string clientId, [FromQuery] string? after, [FromQuery] int pageSize = 50)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(await _chatService.MerchantMessages(id, clientId, after, pageSize));
        }

        /// <summary>发送消息给买家</summary>
        [HttpPost("chat/clients/{clientId}/messages")]
        public async Task<IActionResult> ChatSend(string clientId, [FromBody] MerchantChatSendDto dto)
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(await _chatService.MerchantSend(id, clientId, dto?.Content));
        }

        /// <summary>
        /// 未读数汇总（首页消息按钮红点）：聊天未读 + 通知未读
        /// </summary>
        [HttpGet("chat/unread")]
        public async Task<IActionResult> ChatUnread()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(await _noticeService.UnreadCount(id));
        }

        #endregion

        #region 商户通知（库存预警等系统消息）

        /// <summary>
        /// 通知列表：进入消息中心时调用——先扫描库存自动生成库存预警，再返回通知列表
        /// </summary>
        [HttpGet("notices")]
        public async Task<IActionResult> Notices()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(await _noticeService.MerchantNotices(id));
        }

        /// <summary>把该商户所有未读通知标记为已读</summary>
        [HttpPost("notices/read")]
        public async Task<IActionResult> ReadNotices()
        {
            var id = GetMerchantId();
            if (string.IsNullOrWhiteSpace(id)) return Unauthorized();
            return Ok(await _noticeService.ReadAll(id));
        }

        #endregion

    }
}

/// <summary>商户端发送消息请求体</summary>
public class MerchantChatSendDto
{
    /// <summary>消息文本内容</summary>
    public string? Content { get; set; }
}
