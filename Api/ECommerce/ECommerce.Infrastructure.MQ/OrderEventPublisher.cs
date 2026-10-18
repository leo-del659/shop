using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.Infrastructure.MQ
{
    /// <summary>
    /// 订单事件发布器。
    /// 规则：每条消息只走一条链路——Broker 连通时发 RabbitMQ（持久化），
    /// 否则写进程内降级总线；发布异常同样降级，保证业务代码永不会因 MQ 故障报错。
    /// </summary>
    public interface IOrderEventPublisher
    {
        /// <summary>实例标识（与消息信封 Origin 一致，用于消费端去重本实例的 SSE 直推）</summary>
        string InstanceId { get; }

        /// <summary>发布订单事件（routing key = 事件类型）</summary>
        Task PublishAsync(OrderEventMessage message);

        /// <summary>
        /// 发布延迟事件：Broker 连通时投递到延迟关单队列（per-message TTL，到期死信回主交换机）；
        /// 降级模式下用进程内定时器延时后写入降级总线。
        /// </summary>
        Task PublishDelayedAsync(OrderEventMessage message, TimeSpan delay);
    }

    public class OrderEventPublisher : IOrderEventPublisher
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IRabbitMQConnection _connection;
        private readonly RabbitMQOptions _options;
        private readonly InProcessOrderEventBus _fallbackBus;
        private readonly ILogger<OrderEventPublisher> _logger;

        public OrderEventPublisher(
            IRabbitMQConnection connection,
            IOptions<RabbitMQOptions> options,
            InProcessOrderEventBus fallbackBus,
            ILogger<OrderEventPublisher> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _fallbackBus = fallbackBus ?? throw new ArgumentNullException(nameof(fallbackBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string InstanceId { get; } = Guid.NewGuid().ToString("N").Substring(0, 8);

        public async Task PublishAsync(OrderEventMessage message)
        {
            if (message == null) return;
            if (string.IsNullOrWhiteSpace(message.Origin)) message.Origin = InstanceId;

            var channel = _connection.TryCreateModel();
            if (channel == null)
            {
                await _fallbackBus.PublishAsync(message);
                return;
            }

            try
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
                var props = channel.CreateBasicProperties();
                props.DeliveryMode = 2; // 持久化
                props.MessageId = message.EventId;
                props.Type = message.Type;
                props.AppId = "community.order";
                props.Timestamp = new AmqpTimestamp(message.OccurredAt);

                channel.BasicPublish(_options.Exchange, message.Type, props, body);
            }
            catch (Exception ex)
            {
                // 连接在发布瞬间断开等：降级进程内总线，保证链路不断
                _logger.LogWarning(ex, "订单事件 [{Type}] 投递 RabbitMQ 失败，降级进程内总线", message.Type);
                await _fallbackBus.PublishAsync(message);
            }
            finally
            {
                try { channel.Close(); } catch { /* 已断开时关闭报错，忽略 */ }
                channel.Dispose();
            }
        }

        public Task PublishDelayedAsync(OrderEventMessage message, TimeSpan delay)
        {
            if (message == null) return Task.CompletedTask;
            if (string.IsNullOrWhiteSpace(message.Origin)) message.Origin = InstanceId;

            var channel = _connection.TryCreateModel();
            if (channel == null)
            {
                // 降级：进程内定时器延时后写入总线
                _fallbackBus.PublishDelayed(message, delay);
                return Task.CompletedTask;
            }

            try
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
                var props = channel.CreateBasicProperties();
                props.DeliveryMode = 2;
                props.MessageId = message.EventId;
                props.Type = message.Type;
                props.AppId = "community.order";
                props.Timestamp = new AmqpTimestamp(message.OccurredAt);
                // per-message TTL（毫秒字符串）：到期后由延迟队列的 DLX 配置死信回主交换机
                props.Expiration = ((long)delay.TotalMilliseconds).ToString();

                channel.BasicPublish(_options.Exchange, OrderEventTypes.AutoCancelDelay, props, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "延迟关单消息 [{OrderNo}] 投递 RabbitMQ 失败，降级进程内定时器", message.OrderNo);
                _fallbackBus.PublishDelayed(message, delay);
            }
            finally
            {
                try { channel.Close(); } catch { /* 忽略 */ }
                channel.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}
