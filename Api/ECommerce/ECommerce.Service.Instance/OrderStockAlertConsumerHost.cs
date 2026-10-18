using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.MQ;
using ECommerce.Infrastructure.Tools;
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
    /// 库存预警消费者：监听 order.created，下单成功后检查订单商品的可购买库存，
    /// 低于阈值（RabbitMQ:LowStockThreshold，默认 10）时向 sys_notice 写入一条站内预警，
    /// 供货/仓管可在管理后台及时补货。
    /// 幂等：biz_key=stock-alert:{orderId}，一个订单只预警一次（一条通知列出全部低库存商品）。
    /// </summary>
    public class OrderStockAlertConsumerHost : OrderEventConsumerHost
    {
        private readonly RabbitMQOptions _options;
        private readonly ILogger<OrderStockAlertConsumerHost> _logger;

        public OrderStockAlertConsumerHost(
            IRabbitMQConnection connection,
            IOptions<RabbitMQOptions> options,
            InProcessOrderEventBus fallbackBus,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderStockAlertConsumerHost> logger)
            : base(connection, options, fallbackBus, scopeFactory, logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        protected override string QueueName => OrderEventQueues.StockAlert;

        protected override IReadOnlyCollection<string> SubscribedEventTypes
            => new[] { OrderEventTypes.Created };

        protected override async Task HandleAsync(OrderEventMessage message, IServiceProvider services, CancellationToken ct)
        {
            var orderId = message.OrderId;
            if (string.IsNullOrWhiteSpace(orderId)) return;

            var db = services.GetRequiredService<_111Context>();

            var (acquired, log) = await OrderEventIdempotency.TryAcquireAsync(
                db, message.EventId, message.Type, orderId, message.OrderNo, $"stock-alert:{orderId}");
            if (!acquired) return;

            try
            {
                var order = await db.ClientOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId, ct);

                var items = order?.ParseItems() ?? new List<ClientOrderItemDto>();
                var lowStock = new List<string>();

                foreach (var it in items)
                {
                    if (string.IsNullOrWhiteSpace(it.GoodsId)) continue;

                    var goods = await db.Goods
                        .AsNoTracking()
                        .FirstOrDefaultAsync(g => g.Id == it.GoodsId, ct);
                    if (goods == null) continue;

                    var stock = goods.Stock;
                    if (stock <= _options.LowStockThreshold)
                    {
                        lowStock.Add($"{goods.GoodsName}（剩余 {stock} 件）");
                    }
                }

                if (lowStock.Count > 0)
                {
                    // type=2 表示预警类通知（type=1 为公告，沿用 sys_notice 现有惯例）
                    db.SysNotices.Add(new SysNotice
                    {
                        Id = Config.NewSequentialGuid(),
                        Title = "库存预警",
                        Content = $"订单 {message.OrderNo} 下单后，以下商品库存已低于预警值（{_options.LowStockThreshold} 件）：{string.Join("；", lowStock)}，请及时补货。",
                        Type = 2,
                        IsBan = 0,
                        CreateTime = DateTime.Now
                    });
                    await db.SaveChangesAsync(ct);

                    _logger.LogInformation("库存预警已生成：orderNo={OrderNo} 低库存商品 {Count} 种", message.OrderNo, lowStock.Count);
                }

                await OrderEventIdempotency.MarkDoneAsync(db, log,
                    lowStock.Count > 0 ? $"{lowStock.Count} 个商品低库存" : "库存充足，无预警");
            }
            catch (Exception ex)
            {
                await OrderEventIdempotency.MarkFailedAsync(db, log, ex.Message.Length > 400 ? ex.Message[..400] : ex.Message);
                throw;
            }
        }
    }
}
