using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.MQ
{
    /// <summary>
    /// 订单事件消息信封。业务字段全部平铺到顶层，消费端无需解析嵌套负载即可完成
    /// 结算 / 关单 / 预警判断；Data 承载少量补充信息（如拒单原因、预警商品快照）。
    /// </summary>
    public class OrderEventMessage
    {
        /// <summary>消息唯一 ID（发布时生成，用于链路追踪与排障）</summary>
        public string EventId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>事件类型（= routing key，见 <see cref="OrderEventTypes"/>）</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>事件发生时间（UTC 毫秒时间戳）</summary>
        public long OccurredAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// 发布实例标识（进程级 GUID）。
        /// 实时广播消费者据此跳过「本实例已直接推送过 SSE」的事件，避免单实例重复推送。
        /// </summary>
        public string Origin { get; set; } = string.Empty;

        /// <summary>订单 ID</summary>
        public string? OrderId { get; set; }

        /// <summary>订单编号</summary>
        public string? OrderNo { get; set; }

        /// <summary>下单商户/买家 ID</summary>
        public string? ClientId { get; set; }

        /// <summary>供货商户 ID</summary>
        public string? MerchantId { get; set; }

        /// <summary>订单总金额</summary>
        public double? TotalAmount { get; set; }

        /// <summary>补充负载（匿名对象序列化；反序列化后为 JsonElement）</summary>
        public object? Data { get; set; }
    }
}
