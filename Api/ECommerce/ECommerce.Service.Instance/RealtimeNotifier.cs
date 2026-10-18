using ECommerce.Service.Interface;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 进程内实时通知器实现。
    /// 每个订阅者拥有独立的无界 Channel；Publish 时向所有订阅者 Channel 写入同一事件，
    /// 从而达成"一对多广播"。订阅者数量等于当前打开首页的管理后台数量，通常很小。
    /// 注意：本类不标记 [Inject_] / [Provider_]，由 Program.cs 手动注册为 Singleton。
    /// </summary>
    public class RealtimeNotifier : IRealtimeNotifier
    {
        // 订阅者集合：id -> 独立的事件 Channel
        private readonly ConcurrentDictionary<Guid, Channel<RealtimeEvent>> _subscribers = new();

        public async IAsyncEnumerable<RealtimeEvent> SubscribeAsync([EnumeratorCancellation] CancellationToken ct)
        {
            var id = Guid.NewGuid();
            // 单订阅者无界 Channel：本连接独占读取，互不干扰
            var channel = Channel.CreateUnbounded<RealtimeEvent>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            _subscribers[id] = channel;

            try
            {
                await foreach (var evt in channel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
                {
                    yield return evt;
                }
            }
            finally
            {
                // 用户端断开或枚举结束：务必移除，避免 Channel 泄漏
                _subscribers.TryRemove(id, out _);
            }
        }

        public async Task PublishAsync(string type, object? payload, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(type)) return;

            var evt = new RealtimeEvent
            {
                Type = type,
                Payload = payload,
                Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // 快照当前订阅者，逐个写入；某订阅者写入失败（如已断开但未及时清理）直接跳过，不影响其他用户端
            foreach (var kvp in _subscribers)
            {
                try
                {
                    await kvp.Value.Writer.WriteAsync(evt, ct).ConfigureAwait(false);
                }
                catch
                {
                    // 写入异常（用户端已断开）：移除该订阅者
                    _subscribers.TryRemove(kvp.Key, out _);
                }
            }
        }
    }
}
