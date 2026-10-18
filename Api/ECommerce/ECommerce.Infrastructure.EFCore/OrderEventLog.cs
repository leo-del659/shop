using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 订单消息事件日志（RabbitMQ 消费幂等与审计）。
    /// 每个经 MQ 处理的业务动作（如自动结算、库存预警）先在本表落一行 biz_key 唯一记录：
    /// 重复投递/重试时依据 uk_biz_key 判重，保证「同一订单只结算一次、只预警一次」。
    /// </summary>
    public partial class OrderEventLog
    {
        /// <summary>主键（顺序 GUID）</summary>
        public string Id { get; set; } = null!;
        /// <summary>
        /// 消息事件 ID（<see cref="ECommerce.Infrastructure.MQ.OrderEventMessage.EventId"/>），
        /// 用于与 RabbitMQ 消息对账排障
        /// </summary>
        public string EventId { get; set; } = null!;
        /// <summary>事件类型（order.completed / order.created 等）</summary>
        public string EventType { get; set; } = null!;
        /// <summary>订单 ID</summary>
        public string? OrderId { get; set; }
        /// <summary>订单编号</summary>
        public string? OrderNo { get; set; }
        /// <summary>业务幂等键（如 settle:{orderId}、stock-alert:{orderId}）</summary>
        public string? BizKey { get; set; }
        /// <summary>0处理中 1已处理 2失败</summary>
        public int Status { get; set; }
        /// <summary>处理备注（失败原因 / 生成的结算单号等）</summary>
        public string? Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
