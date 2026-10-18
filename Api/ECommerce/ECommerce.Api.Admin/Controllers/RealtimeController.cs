using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 实时数据推送控制器（SSE）
    /// 通过 Server-Sent Events 把"小程序新订单 / 支付 / 取消"等事件实时推送给打开首页的管理后台。
    /// 注意：EventSource 不支持自定义请求头，因此鉴权令牌从 cookie(admin_token) 或 query(?token=) 读取，
    /// 由本控制器手动校验 JWT，不使用 [Authorize]（该特性只读 Authorization 头）。
    /// </summary>
    [Route("api/Realtime/[action]")]
    [ApiController]
    public class RealtimeController : ControllerBase
    {
        private readonly IRealtimeNotifier _notifier;
        private readonly JWT_Helper _jwt;

        public RealtimeController(IRealtimeNotifier notifier, JWT_Helper jwt)
        {
            _notifier = notifier;
            _jwt = jwt;
        }

        /// <summary>
        /// SSE 事件流端点。保持连接打开，持续推送实时事件。
        /// 用户端断开（关闭标签页 / 网络中断）时 ct 触发，方法优雅退出。
        /// </summary>
        [HttpGet]
        public async Task Stream(CancellationToken ct)
        {
            // 1. 鉴权：优先 cookie，其次 query token
            var token = Request.Cookies["admin_token"]?.ToString()
                        ?? Request.Query["token"].ToString();
            if (string.IsNullOrWhiteSpace(token) || !_jwt.TryValidateToken(token, out _))
            {
                Response.StatusCode = 401;
                await Response.WriteAsync("unauthorized");
                return;
            }

            // 2. 设置 SSE 响应头
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache, no-transform";
            Response.Headers["Connection"] = "keep-alive";
            Response.Headers["X-Accel-Buffering"] = "no"; // 禁用 nginx 缓冲，确保实时
            // 避免某些代理对 text/event-stream 做 gzip 缓冲
            Response.Headers.Remove("Content-Encoding");

            // 3. 先发一条注释帧，打开流并建立连接（浏览器 EventSource 收到首字节后才算连接成功）
            await Response.WriteAsync(": connected\n\n", ct);
            await Response.Body.FlushAsync(ct);

            // 4. 订阅并持续推送
            try
            {
                await foreach (var evt in _notifier.SubscribeAsync(ct).ConfigureAwait(false))
                {
                    var json = JsonSerializer.Serialize(evt, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    var frame = new StringBuilder();
                    frame.Append("data: ").Append(json).Append("\n\n");
                    await Response.WriteAsync(frame.ToString(), ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // 用户端断开，正常结束
            }
        }
    }
}
