using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using ECommerce.Service.Interface.WebSockets;

namespace ECommerce.Api.Admin.WebSockets
{
    /// <summary>
    /// 聊天 WebSocket 中间件：仅处理 /ws/chat 路径。
    /// query 携带 token（鉴权）、role（client|merchant，本端角色）、peerId（对端ID）。
    /// 鉴权通过后按本端身份注册在线连接，循环接收文本帧（{type:'msg',content}），
    /// 调用聊天服务落库后由其向会话两端实时广播。
    /// </summary>
    public class ChatWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ChatWebSocketManager _manager;

        public ChatWebSocketMiddleware(RequestDelegate next, ChatWebSocketManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/ws/chat") || !context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            // 服务按需解析（不放在 InvokeAsync 参数里）：
            // 方法参数是「每请求」注入，容器里缺任何一个服务都会让全部 HTTP 接口 500；
            // 放进 WS 分支后，普通业务请求与聊天服务彻底解耦。
            var jwt = context.RequestServices.GetRequiredService<JWT_Helper>();
            var chatService = context.RequestServices.GetRequiredService<IChatService>();

            // 鉴权：query 取 token 并验签（JWT_Helper 已校验签名/过期）
            var token = context.Request.Query["token"].ToString();
            if (string.IsNullOrWhiteSpace(token) || !jwt.TryValidateToken(token, out var selfId) || string.IsNullOrWhiteSpace(selfId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("unauthorized");
                return;
            }

            var role = context.Request.Query["role"].ToString();      // client | merchant（来自 token 的 role 声明，下方复核）
            var peerId = context.Request.Query["peerId"].ToString(); // 对端ID
            if (string.IsNullOrWhiteSpace(peerId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("bad request");
                return;
            }

            // 复核角色：从已验签的 token 负载读取 role 声明，避免客户端伪造 role
            var tokenRole = ParseClaim(token, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role") ?? ParseClaim(token, "role");
            if (tokenRole != "user" && tokenRole != "merchant")
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("forbidden");
                return;
            }
            // 规范：user→client，merchant→merchant
            var resolvedRole = tokenRole == "merchant" ? "merchant" : "client";

            WebSocket ws = await context.WebSockets.AcceptWebSocketAsync();
            _manager.Add(resolvedRole, selfId, ws);

            var clientId = resolvedRole == "client" ? selfId : peerId;
            var merchantId = resolvedRole == "merchant" ? selfId : peerId;
            var selfSide = resolvedRole == "client" ? 1 : 2;

            try
            {
                var buffer = new byte[8 * 1024];
                while (ws.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    using var ms = new MemoryStream();
                    do
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Close) break;
                        ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    ms.Position = 0;
                    var frame = Encoding.UTF8.GetString(ms.ToArray());
                    await HandleFrameAsync(frame, selfSide, clientId, merchantId, chatService);
                }
            }
            catch
            {
                // 读取异常（对端异常断开）忽略，finally 统一清理
            }
            finally
            {
                _manager.Remove(resolvedRole, selfId, ws);
                if (ws.State != WebSocketState.Closed && ws.State != WebSocketState.Aborted)
                {
                    try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); } catch { }
                }
            }
        }

        /// <summary>处理单条文本帧：仅支持 {type:'msg', content}</summary>
        private async Task HandleFrameAsync(string frame, int selfSide, string clientId, string merchantId, IChatService chatService)
        {
            if (string.IsNullOrWhiteSpace(frame)) return;
            ChatSocketFrame? parsed;
            try { parsed = JsonSerializer.Deserialize<ChatSocketFrame>(frame); } catch { return; }
            if (parsed == null || parsed.type != "msg") return;

            var content = (parsed.content ?? "").Trim();
            if (string.IsNullOrWhiteSpace(content)) return;

            // 落库 + 实时推送：ClientSend / MerchantSend 内部已通过 ChatWebSocketManager
            // 把消息广播给会话两端（买家 + 商户）的在线连接，发送者自身也会收到回推（前端按 id 去重），
            // 此处切勿再次广播，否则同一条消息会被推两遍。
            if (selfSide == 1)
                await chatService.ClientSend(clientId, merchantId, content);
            else
                await chatService.MerchantSend(merchantId, clientId, content);
        }

        /// <summary>无需第三方库，解析 JWT 中段（payload）取指定声明（token 已由 TryValidateToken 验签，可信任）</summary>
        private static string? ParseClaim(string token, string claim)
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return null;
            try
            {
                var padded = parts[1].Replace('-', '+').Replace('_', '/');
                while (padded.Length % 4 != 0) padded += '=';
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty(claim, out var el))
                    return el.GetString();
            }
            catch { }
            return null;
        }
    }

    /// <summary>聊天 WebSocket 文本帧</summary>
    public class ChatSocketFrame
    {
        public string? type { get; set; }
        public string? content { get; set; }
    }
}
