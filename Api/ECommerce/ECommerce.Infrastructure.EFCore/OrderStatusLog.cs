using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 订单状态日志
    /// </summary>
    public partial class OrderStatusLog
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 订单ID
        /// </summary>
        public string? OrderId { get; set; }
        /// <summary>
        /// 订单履约状态（枚举 OrderStatus）
        /// </summary>
        public OrderStatus? OrderStatus { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string? Operator { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
