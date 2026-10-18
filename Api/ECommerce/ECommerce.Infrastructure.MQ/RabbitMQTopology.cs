using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommerce.Infrastructure.MQ
{
    /// <summary>
    /// 订单消息拓扑声明（幂等，可反复执行）。
    ///
    /// 结构：
    ///   exchange community.order (topic, durable)
    ///     ├─ order.completed          → queue community.order.settlement      （结算）
    ///     ├─ order.created            → queue community.order.stock-alert     （库存预警）
    ///     ├─ order.created/paid/...   → queue community.order.realtime        （跨实例 SSE 广播）
    ///     ├─ order.auto-cancel        → queue community.order.auto-cancel     （超时关单执行）
    ///     └─ order.auto-cancel.delay  → queue community.order.auto-cancel.delay
    ///                                      （延迟关单：消息 TTL 到期后经本队列配置的
    ///                                       x-dead-letter-exchange 死信回主交换机，
    ///                                       routing key 改写为 order.auto-cancel）
    ///   exchange community.order.dlx (fanout) → queue community.order.dlx（工作队列消费失败的最终归宿）
    ///
    /// 设计要点：
    /// 1. 全部队列持久化（durable）+ 消息持久化（DeliveryMode=2），Broker 重启不丢订单事件；
    /// 2. 每个队列的消费者可跨多实例水平竞争，同一消息只会被一个实例的一个消费者处理；
    /// 3. 消费重试后仍失败 → nack(requeue:false) → 死信交换机 → community.order.dlx 队列留证；
    /// 4. 延迟关单采用「per-message TTL + DLX」方案，关单时长可在运行时调整（见 AutoCancelDelayMinutes）。
    /// </summary>
    public class RabbitMQTopology
    {
        // 实时广播队列关注的事件（显式绑定，不含 auto-cancel：关单结果以 order.cancelled 广播）
        private static readonly string[] RealtimeRoutingKeys = new[]
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

        private readonly RabbitMQOptions _options;
        private readonly ILogger<RabbitMQTopology> _logger;

        public RabbitMQTopology(IOptions<RabbitMQOptions> options, ILogger<RabbitMQTopology> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 声明交换机、队列与绑定。幂等：重复声明同一参数无副作用；参数冲突会抛错（属于部署事故，应暴露）。
        /// </summary>
        public void EnsureTopology(IModel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);

            var exchange = _options.Exchange;
            var dlxExchange = _options.DeadLetterExchange;

            // 1. 主交换机（topic）与死信交换机（fanout）
            channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
            channel.ExchangeDeclare(dlxExchange, ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);

            // 2. 工作队列：消费失败 → 死信交换机（最终进 community.order.dlx）
            DeclareWorkQueue(channel, OrderEventQueues.Settlement, exchange, OrderEventTypes.Completed);
            DeclareWorkQueue(channel, OrderEventQueues.StockAlert, exchange, OrderEventTypes.Created);
            DeclareWorkQueue(channel, OrderEventQueues.AutoCancel, exchange, OrderEventTypes.AutoCancel);

            // 3. 实时广播队列：一个队列绑定多个 routing key
            DeclareWorkQueue(channel, OrderEventQueues.Realtime, exchange, RealtimeRoutingKeys);

            // 4. 延迟关单队列：TTL 到期后死信回主交换机，routing key 改写为 order.auto-cancel
            channel.QueueDeclare(
                queue: OrderEventQueues.AutoCancelDelay,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = exchange,
                    ["x-dead-letter-routing-key"] = OrderEventTypes.AutoCancel
                });
            channel.QueueBind(OrderEventQueues.AutoCancelDelay, exchange, OrderEventTypes.AutoCancelDelay);

            // 5. 死信队列：承接所有工作队列的 poison message
            channel.QueueDeclare(
                queue: OrderEventQueues.DeadLetter,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            channel.QueueBind(OrderEventQueues.DeadLetter, dlxExchange, string.Empty);

            _logger.LogDebug("订单消息拓扑声明完成：exchange={Exchange}，死信={Dlx}", exchange, dlxExchange);
        }

        private void DeclareWorkQueue(IModel channel, string queue, string exchange, params string[] routingKeys)
        {
            channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    // 消费重试后仍失败（nack 不重入队）→ 死信交换机 → community.order.dlx
                    ["x-dead-letter-exchange"] = _options.DeadLetterExchange
                });

            foreach (var rk in routingKeys)
            {
                channel.QueueBind(queue, exchange, rk);
            }
        }
    }
}
