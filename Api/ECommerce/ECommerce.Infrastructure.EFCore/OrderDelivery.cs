using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 订单配送
    /// </summary>
    public partial class OrderDelivery
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 订单ID
        /// </summary>
        public string? OrderId { get; set; }
        /// <summary>
        /// 物流公司ID
        /// </summary>
        public string? LogisticsId { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string? TrackingNo { get; set; }
        /// <summary>
        /// 0待发 1已发 2已签收
        /// </summary>
        public int? DeliveryStatus { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
