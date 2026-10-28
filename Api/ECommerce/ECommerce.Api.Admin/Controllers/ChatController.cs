using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 用户端（消费者小程序）聊天控制器
    /// 与商户端小程序消息互通：会话列表（历史商家）、消息拉取（含增量轮询）、发送消息、商家简要信息。
    /// 统一前缀 /api/mini-program/chat，与 OrderController(api/mini-program) 同域；需携带消费者 JWT Bearer。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/mini-program/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>当前登录消费者用户 ID（JWT unique_name，即 sys_users.u_id）</summary>
        private string? GetClientId()
        {
            return User.FindFirst("unique_name")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>会话列表：聊过的商家（含最后一条消息与未读数）</summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> Conversations()
        {
            var clientId = GetClientId();
            if (string.IsNullOrWhiteSpace(clientId)) return Unauthorized();
            return Ok(await _chatService.ClientConversations(clientId));
        }

        /// <summary>商家简要信息（名称/头像），供从订单直接进入尚无历史消息的会话时展示</summary>
        [HttpGet("merchants/{merchantId}")]
        public async Task<IActionResult> MerchantBrief(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(GetClientId())) return Unauthorized();
            return Ok(await _chatService.MerchantBrief(merchantId));
        }

        /// <summary>
        /// 拉取与某商家的消息（最近 50 条，升序返回）
        /// after：传最后一条消息的 createTime 做增量轮询，只返回该时间之后的新消息
        /// </summary>
        [HttpGet("merchants/{merchantId}/messages")]
        public async Task<IActionResult> Messages(string merchantId, [FromQuery] string? after, [FromQuery] int pageSize = 50)
        {
            var clientId = GetClientId();
            if (string.IsNullOrWhiteSpace(clientId)) return Unauthorized();
            return Ok(await _chatService.ClientMessages(clientId, merchantId, after, pageSize));
        }

        /// <summary>发送消息给商家</summary>
        [HttpPost("merchants/{merchantId}/messages")]
        public async Task<IActionResult> Send(string merchantId, [FromBody] ChatSendDto dto)
        {
            var clientId = GetClientId();
            if (string.IsNullOrWhiteSpace(clientId)) return Unauthorized();
            return Ok(await _chatService.ClientSend(clientId, merchantId, dto?.Content));
        }
    }

    /// <summary>发送消息请求体</summary>
    public class ChatSendDto
    {
        /// <summary>消息文本内容</summary>
        public string? Content { get; set; }
    }
}
