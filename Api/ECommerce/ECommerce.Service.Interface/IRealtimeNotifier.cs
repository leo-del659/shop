using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    /// <summary>
    /// 实时事件负载：服务端推送给前端的单一事件
    /// </summary>
    public class RealtimeEvent
    {
        /// <summary>事件类型，如 order.created / order.paid / order.updated</summary>
        public string Type { get; set; } = "";
        /// <summary>可选的业务负载（订单号、金额等），可为 null</summary>
        public object? Payload { get; set; }
        /// <summary>事件产生时间戳（UTC 毫秒），便于前端排查</summary>
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// 进程内实时通知器（内存广播）
    /// 用于把"小程序产生新订单 / 支付 / 取消"等事件实时推送给所有打开首页的管理后台。
    /// 设计为单例（Singleton）：跨请求共享同一组订阅者。
    /// </summary>
    public interface IRealtimeNotifier
    {
        /// <summary>
        /// 订阅事件流。返回的异步枚举在用户端断开（ct 触发）或连接被移除时结束。
        /// 调用方应在 finally 中确保本订阅从全局集合中移除（由实现内部保证）。
        /// </summary>
        IAsyncEnumerable<RealtimeEvent> SubscribeAsync(CancellationToken ct);

        /// <summary>
        /// 向全部当前订阅者广播一条事件。
        /// </summary>
        Task PublishAsync(string type, object? payload, CancellationToken ct = default);
    }
}
