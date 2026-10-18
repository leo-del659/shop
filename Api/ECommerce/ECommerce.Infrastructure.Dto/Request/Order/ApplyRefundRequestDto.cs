using System;

namespace ECommerce.Infrastructure.Dto.Request.Order
{
    /// <summary>
    /// 消费者申请退款/退货退款
    /// </summary>
    public class ApplyRefundRequestDto
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// 退款类型 1仅退款 2退货退款（默认 2）
        /// </summary>
        public int RefundType { get; set; } = 2;

        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// 退款原因
        /// </summary>
        public string? RefundReason { get; set; }

        /// <summary>
        /// 退货数量（退货退款时有效）
        /// </summary>
        public int? RefundNum { get; set; }

        /// <summary>
        /// 指定退款商品ID（可空，空表示整单退款）
        /// </summary>
        public string? GoodsId { get; set; }
    }
}
