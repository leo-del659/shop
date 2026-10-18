using ECommerce.Infrastructure.MQ;
using ECommerce.Service.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 实时广播消费者：把订单事件广播到本实例的 SSE 订阅者（管理后台首页看板）。
    ///
    /// 为什么需要它：OrderService 等入口在写库成功后已「直接」推送过本进程的 SSE，
    /// 单实例部署下前端已能实时刷新；但多实例部署时，订单可能由实例 A 处理，
    /// 而打开看板的浏览器连的是实例 B——B 的进程内通知器收不到 A 的事件。
    /// 该消费者消费 RabbitMQ 广播队列中的跨实例事件，补齐这一环。
    /// 去重：消息信封带 Origin（发布实例标识），本实例发布的事件直接跳过，避免重复推送。
    /// </summary>
    public class OrderRealtimeFanoutConsumerHost : OrderEventConsumerHost
    {
        private readonly IOrderEventPublisher _publisher;
        private readonly IRealtimeNotifier _notifier;
        private readonly ILogger<OrderRealtimeFanoutConsumerHost> _logger;

        public OrderRealtimeFanoutConsumerHost(
            IRabbitMQConnection connection,
            IOptions<RabbitMQOptions> options,
            InProcessOrderEventBus fallbackBus,
            IServiceScopeFactory scopeFactory,
            IOrderEventPublisher publisher,
            IRealtimeNotifier notifier,
            ILogger<OrderRealtimeFanoutConsumerHost> logger)
            : base(connection, options, fallbackBus, scopeFactory, logger)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _logger = logger;
        }

        protected override string QueueName => OrderEventQueues.Realtime;

        protected override IReadOnlyCollection<string> SubscribedEventTypes
            => new[]
            {
                OrderEventTypes.Created,
                OrderEventTypes.Paid,
                OrderEventTypes.Cancelled,
                OrderEventTypes.Accepted,
                OrderEventTypes.Rejected,
                OrderEventTypes.Shipped,
                OrderEventTypes.Completed,
                OrderEventTypes.Deleted
            };

        protected override async Task HandleAsync(OrderEventMessage message, IServiceProvider services, CancellationToken ct)
        {
            // 本实例自己发布的事件：OrderService 等处已直接推送 SSE，跳过防重复
            if (string.Equals(message.Origin, _publisher.InstanceId, StringComparison.Ordinal))
            {
                return;
            }

            await _notifier.PublishAsync(message.Type, new
            {
                orderId = message.OrderId,
                orderNo = message.OrderNo,
                totalAmount = message.TotalAmount,
                merchantId = message.MerchantId,
                eventId = message.EventId,
                occurredAt = message.OccurredAt
            }, ct);
        }
    }
}
