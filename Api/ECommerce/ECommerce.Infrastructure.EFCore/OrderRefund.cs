using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 订单退款
    /// </summary>
    public partial class OrderRefund
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 订单ID
        /// </summary>
        public string? OrderId { get; set; }
        /// <summary>
        /// 申请人用户ID（消费者）
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// 退款单号
        /// </summary>
        public string? RefundNo { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal? RefundAmount { get; set; }
        /// <summary>
        /// 退款原因
        /// </summary>
        public string? RefundReason { get; set; }
        /// <summary>
        /// 退款类型 1仅退款 2退货退款
        /// </summary>
        public int? RefundType { get; set; }
        /// <summary>
        /// 退货数量（仅退货退款时有意义）
        /// </summary>
        public int? RefundNum { get; set; }
        /// <summary>
        /// 关联商品ID（可按商品发起退款）
        /// </summary>
        public string? GoodsId { get; set; }
        /// <summary>
        /// 关联商品名称
        /// </summary>
        public string? GoodsName { get; set; }
        /// <summary>
        /// 关联商品图片
        /// </summary>
        public string? GoodsImg { get; set; }
        /// <summary>
        /// 0待审核 1已退款 2驳回
        /// </summary>
        public int? RefundStatus { get; set; }
        /// <summary>
        /// 审核备注
        /// </summary>
        public string? HandleRemark { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? HandleTime { get; set; }
        /// <summary>
        /// 审核人ID
        /// </summary>
        public string? HandleBy { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
