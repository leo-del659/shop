using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.MQ
{
    /// <summary>
    /// 进程内降级总线（单例）。
    /// 用途：RabbitMQ 未启用 / 连接断开 / 发布失败的瞬间，把订单事件改走本进程内存通道，
    /// 由同进程的消费者宿主直接处理——功能不中断，只是失去跨实例能力。
    /// 与 RealtimeNotifier 同思路：无界 Channel 一对多广播，订阅者互不干扰。
    /// </summary>
    public class InProcessOrderEventBus
    {
        private readonly Channel<OrderEventMessage> _channel =
            Channel.CreateUnbounded<OrderEventMessage>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });

        /// <summary>立即投递（降级链路用）</summary>
        public async Task PublishAsync(OrderEventMessage message)
        {
            if (message == null) return;
            await _channel.Writer.WriteAsync(message);
        }

        /// <summary>
        /// 延时投递（降级链路下的「超时未支付自动关单」）：
        /// 到点后写入总线，事件类型为 order.auto-cancel，仅自动关单消费者会处理。
        /// </summary>
        public void PublishDelayed(OrderEventMessage message, TimeSpan delay)
        {
            if (message == null) return;
            var totalMs = (long)Math.Max(0, delay.TotalMilliseconds);
            _ = Task.Run(async () =>
            {
                try
                {
                    if (totalMs > 0) await Task.Delay(TimeSpan.FromMilliseconds(totalMs));
                    await _channel.Writer.WriteAsync(message);
                }
                catch
                {
                    // 降级链路投递失败只能丢弃：主链路（RabbitMQ）恢复后新订单不会受影响，
                    // 极端情况下表现为「该笔订单不会自动关单」，由运维在管理后台手动处理。
                }
            });
        }

        /// <summary>订阅总线。ct 取消或进程退出时结束。</summary>
        public IAsyncEnumerable<OrderEventMessage> Subscribe(CancellationToken ct)
            => _channel.Reader.ReadAllAsync(ct);
    }
}
