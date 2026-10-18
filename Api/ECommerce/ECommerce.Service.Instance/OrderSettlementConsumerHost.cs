using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.MQ;
using ECommerce.Infrastructure.Tools;
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
    /// 结算消费者：监听 order.completed，订单完成后自动生成结算单（settle_record）。
    /// 对应平台业务规则「订单完成自动生成结算单」——此前该动作依赖人工在后台创建，
    /// 现由 MQ 异步化：下单/履约主链路零额外耗时，结算失败也不影响订单状态。
    /// 幂等：order_event_log 的 biz_key=settle:{orderId} 保证同一订单只生成一张结算单。
    /// </summary>
    public class OrderSettlementConsumerHost : OrderEventConsumerHost
    {
        private readonly ILogger<OrderSettlementConsumerHost> _logger;

        public OrderSettlementConsumerHost(
            IRabbitMQConnection connection,
            IOptions<RabbitMQOptions> options,
            InProcessOrderEventBus fallbackBus,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderSettlementConsumerHost> logger)
            : base(connection, options, fallbackBus, scopeFactory, logger)
        {
            _logger = logger;
        }

        protected override string QueueName => OrderEventQueues.Settlement;

        protected override IReadOnlyCollection<string> SubscribedEventTypes
            => new[] { OrderEventTypes.Completed };

        protected override async Task HandleAsync(OrderEventMessage message, IServiceProvider services, CancellationToken ct)
        {
            var orderId = message.OrderId;
            if (string.IsNullOrWhiteSpace(orderId)) return; // 无订单ID：直接 ACK

            var db = services.GetRequiredService<_111Context>();

            // 1. 幂等占位（重复投递时直接跳过）
            var (acquired, log) = await OrderEventIdempotency.TryAcquireAsync(
                db, message.EventId, message.Type, orderId, message.OrderNo, $"settle:{orderId}");
            if (!acquired)
            {
                _logger.LogDebug("订单 {OrderNo} 结算单已生成过，跳过重复投递", message.OrderNo);
                return;
            }

            try
            {
                // 2. 校验订单确实已完成（防止乱序/伪造消息）
                var order = await db.ClientOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId, ct);

                if (order == null || (order.OrderStatus ?? OrderStatus.PendingPayment) != OrderStatus.Completed)
                {
                    await OrderEventIdempotency.MarkDoneAsync(db, log, "订单不存在或未完成，跳过结算");
                    return;
                }

                // 3. 生成结算单（编号规则与 AdminSettleService.GenerateSettleNo 一致：JS{yyyyMMdd}-{seq:D2}）
                var settleNo = await GenerateSettleNoAsync(db, ct);
                var settle = new SettleRecord
                {
                    Id = Config.NewSequentialGuid(),
                    ClientId = order.ClientId,
                    SettleNo = settleNo,
                    SettleAmount = (decimal?)order.TotalAmount,
                    SettleType = 1,  // 线上
                    Status = 0,      // 待结算
                    CreateTime = DateTime.Now
                };

                db.SettleRecords.Add(settle);
                await db.SaveChangesAsync(ct);

                await OrderEventIdempotency.MarkDoneAsync(db, log, $"结算单 {settleNo}");

                _logger.LogInformation(
                    "订单完成自动结算：orderNo={OrderNo} settleNo={SettleNo} amount={Amount} client={ClientId}",
                    order.OrderNo, settleNo, settle.SettleAmount, settle.ClientId);
            }
            catch (Exception ex)
            {
                // 标记失败并抛出：交给宿主重试；3 次仍失败进死信队列，人工介入
                await OrderEventIdempotency.MarkFailedAsync(db, log, ex.Message.Length > 400 ? ex.Message[..400] : ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 结算单号：JS{yyyyMMdd}-{seq:00}，同日最大序号 + 1（与后台 GenerateSettleNo 保持同一格式）。
        /// 并发下若撞唯一键，宿主重试时会读到最新序号自愈。
        /// </summary>
        private static async Task<string> GenerateSettleNoAsync(_111Context db, CancellationToken ct)
        {
            var now = DateTime.Now;
            var prefix = $"JS{now:yyyyMMdd}-";
            var exist = await db.SettleRecords
                .Where(s => s.SettleNo != null && s.SettleNo.StartsWith(prefix))
                .Select(s => s.SettleNo!)
                .ToListAsync(ct);

            int maxSeq = 0;
            foreach (var no in exist)
            {
                var tail = no.Length > prefix.Length ? no[prefix.Length..] : string.Empty;
                if (int.TryParse(tail, out var n) && n > maxSeq) maxSeq = n;
            }
            return $"{prefix}{(maxSeq + 1).ToString("D2")}";
        }
    }
}
