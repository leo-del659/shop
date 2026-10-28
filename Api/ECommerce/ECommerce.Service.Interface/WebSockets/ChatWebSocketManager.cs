using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ECommerce.Service.Interface.WebSockets
{
    /// <summary>
    /// 聊天 WebSocket 连接管理器（单例，置于共享接口层以避免项目循环依赖）：
    /// 按「角色:用户ID」分组维护在线连接，用于把一条新消息实时推送给会话两端（买家 + 商户）的在线连接。
    /// </summary>
    public class ChatWebSocketManager
    {
        private readonly ConcurrentDictionary<string, HashSet<WebSocket>> _groups = new();
        private readonly object _gate = new();

        private static string GroupKey(string role, string id) => $"{role}:{id}";

        /// <summary>注册一条在线连接（本端角色 + 用户ID）</summary>
        public void Add(string role, string id, WebSocket ws)
        {
            var key = GroupKey(role, id);
            lock (_gate)
            {
                if (!_groups.TryGetValue(key, out var set))
                {
                    set = new HashSet<WebSocket>();
                    _groups[key] = set;
                }
                set.Add(ws);
            }
        }

        /// <summary>移除一条离线连接</summary>
        public void Remove(string role, string id, WebSocket ws)
        {
            var key = GroupKey(role, id);
            lock (_gate)
            {
                if (_groups.TryGetValue(key, out var set))
                {
                    set.Remove(ws);
                    if (set.Count == 0) _groups.TryRemove(key, out _);
                }
            }
        }

        /// <summary>向会话两端（clientId / merchantId）的在线连接实时推送 JSON 负载</summary>
        public Task BroadcastAsync(string? clientId, string? merchantId, object payload)
        {
            return BroadcastAsync(clientId, merchantId, payload, payload);
        }

        /// <summary>
        /// 向会话两端推送：买家连接收到 clientPayload，商户连接收到 merchantPayload。
        /// 用于「self（是否本人发出）」这类相对字段——同一消息对两端含义相反，必须分别构造。
        /// </summary>
        public async Task BroadcastAsync(string? clientId, string? merchantId, object clientPayload, object merchantPayload)
        {
            var targets = new List<(WebSocket Ws, byte[] Bytes)>();
            if (!string.IsNullOrWhiteSpace(clientId))
                Collect("client", clientId!, clientPayload, targets);
            if (!string.IsNullOrWhiteSpace(merchantId))
                Collect("merchant", merchantId!, merchantPayload, targets);
            foreach (var (ws, bytes) in targets)
            {
                if (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        // 单条推送失败忽略，不影响其他连接
                    }
                }
            }
        }

        private void Collect(string role, string id, object payload, List<(WebSocket Ws, byte[] Bytes)> outList)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            var key = GroupKey(role, id);
            lock (_gate)
            {
                if (_groups.TryGetValue(key, out var set))
                    outList.AddRange(set.Where(w => w.State == WebSocketState.Open).Select(w => (w, bytes)));
            }
        }
    }
}
