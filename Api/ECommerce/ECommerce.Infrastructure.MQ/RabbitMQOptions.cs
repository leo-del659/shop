using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.MQ
{
    /// <summary>
    /// RabbitMQ 连接与订单消息拓扑配置。
    /// 对应 appsettings.json 的 "RabbitMQ" 段；密码等敏感项可用环境变量
    /// COMMUNITY_RABBITMQ_PASSWORD 覆盖（与 Redis 密码的管理方式一致，不落源码）。
    /// </summary>
    public class RabbitMQOptions
    {
        /// <summary>总开关。false 时发布/消费全部走进程内降级总线（无需 Broker 也能跑通全链路）。</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Broker 主机名</summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>AMQP 端口（默认 5672；管理界面 15672）</summary>
        public int Port { get; set; } = 5672;

        /// <summary>用户名</summary>
        public string UserName { get; set; } = "guest";

        /// <summary>密码（可用 COMMUNITY_RABBITMQ_PASSWORD 环境变量覆盖）</summary>
        public string Password { get; set; } = "guest";

        /// <summary>虚拟主机</summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>订单事件主交换机（topic，持久化）</summary>
        public string Exchange { get; set; } = "community.order";

        /// <summary>死信交换机（fanout，持久化）：工作队列消费失败 nack 后的最终归宿</summary>
        public string DeadLetterExchange { get; set; } = "community.order.dlx";

        /// <summary>单消费者预取条数（QoS）：避免预警/结算高峰把消息全部预取到单个实例</summary>
        public ushort Prefetch { get; set; } = 20;

        /// <summary>下单后超过该分钟数仍未支付，由延迟队列触发自动关单（默认 30 分钟）</summary>
        public int AutoCancelDelayMinutes { get; set; } = 30;

        /// <summary>库存预警阈值：下单后商品可购买库存 ≤ 该值时生成站内预警（默认 10）</summary>
        public int LowStockThreshold { get; set; } = 10;
    }
}
