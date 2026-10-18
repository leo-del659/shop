using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.Infrastructure.MQ
{
    /// <summary>
    /// 订单事件消费者宿主基类（HostedService）。
    ///
    /// 生命周期：
    ///   1. Broker 可用 → 声明拓扑 + BasicConsume，收到消息后逐条：反序列化 → 新建 DI Scope →
    ///      调用 HandleAsync → 成功 BasicAck；重试 3 次仍失败 → BasicNack(requeue:false) 进死信队列。
    ///   2. Broker 不可用/掉线 → 自动切换「进程内降级总线」，同套 HandleAsync 逻辑照常执行；
    ///      每 2 秒探测一次连通性，恢复后自动切回 RabbitMQ（消息在队列中等待，不丢失）。
    ///
    /// 子类只需实现：QueueName / SubscribedEventTypes / HandleAsync。
    /// </summary>
    public abstract class OrderEventConsumerHost : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>重试次数（含首次）。最终失败进死信队列。</summary>
        private const int MaxAttempts = 3;

        /// <summary>本消费者负责的队列名（见 <see cref="OrderEventQueues"/>）</summary>
        protected abstract string QueueName { get; }

        /// <summary>本消费者关注的事件类型（降级总线按此过滤；RabbitMQ 侧由队列绑定保证）</summary>
        protected abstract IReadOnlyCollection<string> SubscribedEventTypes { get; }

        /// <summary>
        /// 业务处理。每次调用都在全新的 DI Scope 中（services 为 Scope 的 ServiceProvider），
        /// 可安全解析 Scoped 的 DbContext / 业务服务。抛出异常即触发重试。
        /// </summary>
        protected abstract Task HandleAsync(OrderEventMessage message, IServiceProvider services, CancellationToken ct);

        private readonly IRabbitMQConnection _connection;
        private readonly RabbitMQOptions _options;
        private readonly InProcessOrderEventBus _fallbackBus;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;

        protected OrderEventConsumerHost(
            IRabbitMQConnection connection,
            IOptions<RabbitMQOptions> options,
            InProcessOrderEventBus fallbackBus,
            IServiceScopeFactory scopeFactory,
            ILogger logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _fallbackBus = fallbackBus ?? throw new ArgumentNullException(nameof(fallbackBus));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[{Queue}] 订单事件消费者启动", QueueName);

            while (!stoppingToken.IsCancellationRequested)
            {
                IModel? channel = null;
                try
                {
                    channel = _connection.TryCreateModel();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[{Queue}] 创建 RabbitMQ Channel 异常，转入降级链路", QueueName);
                }

                if (channel != null)
                {
                    var dropped = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                    if (!channel.IsOpen)
                    {
                        // 连接刚断开：释放后走降级链路
                        SafeClose(channel);
                        channel = null;
                    }
                    else
                    {
                        channel.ModelShutdown += (_, e) => dropped.TrySetResult(true);

                        try
                        {
                            channel.BasicQos(prefetchSize: 0, prefetchCount: _options.Prefetch, global: false);
                            var consumer = new AsyncEventingBasicConsumer(channel);
                            consumer.Received += async (_, ea) => await OnRabbitMessageAsync(channel, ea);
                            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

                            _logger.LogInformation("[{Queue}] 已接入 RabbitMQ，开始消费（prefetch={Prefetch}）",
                                QueueName, _options.Prefetch);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[{Queue}] 注册消费者失败，转入降级链路", QueueName);
                            SafeClose(channel);
                            channel = null;
                        }
                    }

                    if (channel != null)
                    {
                        // 等待掉线或进程停止
                        await WaitAnyAsync(dropped.Task, stoppingToken);
                        SafeClose(channel);
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogWarning("[{Queue}] 连接断开，2 秒后重连", QueueName);
                            await DelayAsync(TimeSpan.FromSeconds(2), stoppingToken);
                        }
                        continue;
                    }
                }

                // —— 降级链路：进程内总线 ——
                _logger.LogWarning("[{Queue}] RabbitMQ 不可用，降级为进程内总线（每 2 秒探测恢复）", QueueName);
                await ConsumeFromFallbackBusAsync(stoppingToken);
            }

            _logger.LogInformation("[{Queue}] 订单事件消费者已停止", QueueName);
        }

        /// <summary>
        /// 降级链路：订阅进程内总线，直到 Broker 恢复或进程停止。
        /// </summary>
        private async Task ConsumeFromFallbackBusAsync(CancellationToken ct)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            // 每 2 秒探测一次；Broker 恢复即退出降级，切回 RabbitMQ
            using var probe = new Timer(_ =>
            {
                try { if (_connection.IsConnected) linked.Cancel(); } catch { /* 忽略 */ }
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));

            try
            {
                await foreach (var msg in _fallbackBus.Subscribe(linked.Token))
                {
                    if (!SubscribedEventTypes.Contains(msg.Type)) continue;
                    await ProcessWithRetryAsync(msg, linked.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常退出路径：ct 取消（进程停止）或探测到 Broker 恢复
            }
        }

        private async Task OnRabbitMessageAsync(IModel channel, BasicDeliverEventArgs ea)
        {
            OrderEventMessage? msg = null;
            try
            {
                msg = JsonSerializer.Deserialize<OrderEventMessage>(ea.Body.ToArray(), JsonOptions);
            }
            catch (Exception ex)
            {
                // 反序列化失败 = poison message：直接进死信队列，不占用重试额度
                _logger.LogError(ex, "[{Queue}] 消息反序列化失败（deliveryTag={Tag}），转入死信队列", QueueName, ea.DeliveryTag);
                SafeNack(channel, ea.DeliveryTag);
                return;
            }

            if (msg == null || string.IsNullOrWhiteSpace(msg.Type))
            {
                SafeNack(channel, ea.DeliveryTag);
                return;
            }

            if (!SubscribedEventTypes.Contains(msg.Type))
            {
                // 绑定保证不该发生；防御性 ack，避免堆积
                SafeAck(channel, ea.DeliveryTag);
                return;
            }

            var ok = await ProcessWithRetryAsync(msg, CancellationToken.None);
            if (ok) SafeAck(channel, ea.DeliveryTag);
            else SafeNack(channel, ea.DeliveryTag);
        }

        /// <summary>
        /// 带重试的处理：每次尝试都新建 DI Scope；3 次均失败返回 false（由调用方 ack/nack）。
        /// </summary>
        private async Task<bool> ProcessWithRetryAsync(OrderEventMessage msg, CancellationToken ct)
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await HandleAsync(msg, scope.ServiceProvider, ct);
                    return true;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    return false; // 进程停止，不再重试
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{Queue}] 处理订单事件 [{Type}] 第 {Attempt}/{Max} 次失败：orderNo={OrderNo}",
                        QueueName, msg.Type, attempt, MaxAttempts, msg.OrderNo);
                    if (attempt < MaxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt), ct); // 1s、2s 退避
                    }
                }
            }

            _logger.LogError("[{Queue}] 订单事件 [{Type}] 重试 {Max} 次后仍失败，转入死信队列：orderNo={OrderNo}",
                QueueName, msg.Type, MaxAttempts, msg.OrderNo);
            return false;
        }

        private static async Task WaitAnyAsync(Task task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await using (ct.Register(() => tcs.TrySetResult(true)))
            {
                if (task.IsCompleted) return;
                await Task.WhenAny(task, tcs.Task);
            }
        }

        private static async Task DelayAsync(TimeSpan delay, CancellationToken ct)
        {
            try { await Task.Delay(delay, ct); } catch (OperationCanceledException) { /* 停止 */ }
        }

        private void SafeAck(IModel channel, ulong deliveryTag)
        {
            try { channel.BasicAck(deliveryTag, multiple: false); }
            catch (Exception ex) { _logger.LogWarning(ex, "[{Queue}] BasicAck 失败（连接已断开，消息将由 Broker 重投）", QueueName); }
        }

        private void SafeNack(IModel channel, ulong deliveryTag)
        {
            try { channel.BasicNack(deliveryTag, multiple: false, requeue: false); }
            catch (Exception ex) { _logger.LogWarning(ex, "[{Queue}] BasicNack 失败（连接已断开，消息将由 Broker 重投）", QueueName); }
        }

        private static void SafeClose(IModel? channel)
        {
            if (channel == null) return;
            try { if (channel.IsOpen) channel.Close(); } catch { /* 忽略 */ }
            try { channel.Dispose(); } catch { /* 忽略 */ }
        }
    }
}
