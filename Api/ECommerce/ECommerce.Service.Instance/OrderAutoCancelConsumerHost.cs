using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.MQ;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 自动关单消费者：监听 order.auto-cancel（下单时投递到延迟关单队列，TTL 到期经死信交换机转入）。
    /// 到期时若订单仍是「待处理且未支付」，复用 IOrderService.CancelOrderAsync 执行关单——
    /// 该路径自带库存回滚、状态置 9、SSE 通知与 order.cancelled 事件发布，无需重复实现。
    /// 已支付/已接单/已取消的订单直接 ACK 跳过（执行时核对，天然幂等）。
    /// </summary>
    public class OrderAutoCancelConsumerHost : OrderEventConsumerHost
    {
        private readonly ILogger<OrderAutoCancelConsumerHost> _logger;

        public OrderAutoCancelConsumerHost(
            IRabbitMQConnection connection,
            IOptions<RabbitMQOptions> options,
            InProcessOrderEventBus fallbackBus,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderAutoCancelConsumerHost> logger)
            : base(connection, options, fallbackBus, scopeFactory, logger)
        {
            _logger = logger;
        }

        protected override string QueueName => OrderEventQueues.AutoCancel;

        protected override IReadOnlyCollection<string> SubscribedEventTypes
            => new[] { OrderEventTypes.AutoCancel };

        protected override async Task HandleAsync(OrderEventMessage message, IServiceProvider services, CancellationToken ct)
        {
            var orderId = message.OrderId;
            if (string.IsNullOrWhiteSpace(orderId)) return;

            var db = services.GetRequiredService<_111Context>();

            var order = await db.ClientOrders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
            if (order == null) return;

            // 只有「待处理(1) + 未支付 + 未作废」才关单；其余状态说明期间已发生变化，直接放过
            if ((order.OrderStatus ?? OrderStatus.PendingPayment) != OrderStatus.PendingAccept || (order.PayStatus ?? PayStatus.Unpaid) == PayStatus.Paid || (order.IsBan ?? 0) == 1)
            {
                _logger.LogDebug("订单 {OrderNo} 状态已变化（status={Status} pay={Pay}），跳过自动关单",
                    order.OrderNo, order.OrderStatus, order.PayStatus);
                return;
            }

            var orderService = services.GetRequiredService<IOrderService>();
            var result = await orderService.CancelOrderAsync(order.ClientId!, order.Id, null, "超时未支付，系统自动取消");

            if (result.code == ECommerce.Infrastructure.Dto.Response.Api_Code.ok)
            {
                _logger.LogInformation("订单超时未支付，自动关单成功：orderNo={OrderNo}", order.OrderNo);
            }
            else
            {
                // 多为并发下用户已先行取消（状态校验失败），按最终一致性处理，不重试
                _logger.LogWarning("订单自动关单未执行：orderNo={OrderNo} reason={Reason}", order.OrderNo, result.message);
            }
        }
    }
}
