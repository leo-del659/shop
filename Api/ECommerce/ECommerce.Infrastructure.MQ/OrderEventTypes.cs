using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.MQ
{
    /// <summary>
    /// 订单事件类型常量。取值同时作为 topic 交换机的 routing key，
    /// 并沿用既有 SSE 事件名（order.created 等），保证前端无需适配。
    /// </summary>
    public static class OrderEventTypes
    {
        /// <summary>下单成功（已锁定库存）</summary>
        public const string Created = "order.created";
        /// <summary>支付成功</summary>
        public const string Paid = "order.paid";
        /// <summary>订单取消（用户取消 / 超时自动关单 / 商户拒单）</summary>
        public const string Cancelled = "order.cancelled";
        /// <summary>商户接单（1 → 2 接单备货中）</summary>
        public const string Accepted = "order.accepted";
        /// <summary>商户拒单（1 → 9，带原因）</summary>
        public const string Rejected = "order.rejected";
        /// <summary>商户发货（2 → 3 配送中）</summary>
        public const string Shipped = "order.shipped";
        /// <summary>订单完成（买家确认收货 / 商户完成，3 → 4）——触发自动结算</summary>
        public const string Completed = "order.completed";
        /// <summary>买家删除订单（逻辑删除）</summary>
        public const string Deleted = "order.deleted";
        /// <summary>
        /// 延迟关单：下单时投递到「延迟关单队列」（TTL = AutoCancelDelayMinutes），
        /// 到期经死信交换机重新投递到 order.auto-cancel，由消费者核对订单仍未支付后执行关单。
        /// </summary>
        public const string AutoCancel = "order.auto-cancel";
        /// <summary>延迟关单投递专用的 routing key（仅延迟关单队列绑定）</summary>
        public const string AutoCancelDelay = "order.auto-cancel.delay";
    }

    /// <summary>
    /// 订单消息队列名常量（均持久化）。
    /// </summary>
    public static class OrderEventQueues
    {
        /// <summary>结算队列：order.completed → 自动生成结算单</summary>
        public const string Settlement = "community.order.settlement";
        /// <summary>库存预警队列：order.created → 低库存商品站内预警</summary>
        public const string StockAlert = "community.order.stock-alert";
        /// <summary>实时广播队列：订单事件 → 跨实例 SSE 广播（管理后台看板）</summary>
        public const string Realtime = "community.order.realtime";
        /// <summary>自动关单队列：order.auto-cancel → 超时未支付关单</summary>
        public const string AutoCancel = "community.order.auto-cancel";
        /// <summary>延迟关单队列：TTL 到期死信到主交换机（routing key = order.auto-cancel）</summary>
        public const string AutoCancelDelay = "community.order.auto-cancel.delay";
        /// <summary>死信队列：工作队列消费失败（重试后仍失败）消息的最终归宿，供运维排查</summary>
        public const string DeadLetter = "community.order.dlx";
    }
}
