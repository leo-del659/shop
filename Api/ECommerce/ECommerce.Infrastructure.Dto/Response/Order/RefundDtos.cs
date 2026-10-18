using System;
using System.Collections.Generic;
using ECommerce.Infrastructure.EFCore;

namespace ECommerce.Infrastructure.Dto.Response.Order
{
    /// <summary>
    /// 退款状态文案
    /// </summary>
    public static class RefundStatusText
    {
        public const string Pending = "待审核";
        public const string Refunded = "已退款";
        public const string Rejected = "已驳回";

        public static string Of(int? status) => status switch
        {
            0 => Pending,
            1 => Refunded,
            2 => Rejected,
            _ => "未知"
        };
    }

    /// <summary>
    /// 退款类型文案
    /// </summary>
    public static class RefundTypeText
    {
        public const string OnlyRefund = "仅退款";
        public const string RefundAndReturn = "退货退款";

        public static string Of(int? type) => type switch
        {
            1 => OnlyRefund,
            2 => RefundAndReturn,
            _ => "退货退款"
        };
    }

    /// <summary>
    /// 我的退款列表项
    /// </summary>
    public class RefundListItemDto
    {
        public string Id { get; set; } = null!;
        public string? RefundNo { get; set; }
        public string? OrderId { get; set; }
        public string? OrderNo { get; set; }
        public decimal RefundAmount { get; set; }
        public int RefundType { get; set; }
        public string? RefundTypeText { get; set; }
        public int RefundStatus { get; set; }
        public string? RefundStatusText { get; set; }
        public string? RefundReason { get; set; }
        public string? GoodsName { get; set; }
        public string? GoodsImg { get; set; }
        public DateTime? CreateTime { get; set; }
    }

    /// <summary>
    /// 退款详情（含订单与退货信息）
    /// </summary>
    public class RefundDetailDto
    {
        public string Id { get; set; } = null!;
        public string? RefundNo { get; set; }
        public string? OrderId { get; set; }
        public string? OrderNo { get; set; }
        public decimal RefundAmount { get; set; }
        public int RefundType { get; set; }
        public string? RefundTypeText { get; set; }
        public int RefundStatus { get; set; }
        public string? RefundStatusText { get; set; }
        public string? RefundReason { get; set; }
        public int? RefundNum { get; set; }
        public string? GoodsName { get; set; }
        public string? GoodsImg { get; set; }
        public string? HandleRemark { get; set; }
        public DateTime? HandleTime { get; set; }
        public DateTime? CreateTime { get; set; }

        // 订单概要
        public decimal OrderTotal { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public string? OrderStatusText { get; set; }
        public List<RefundOrderItemDto> Items { get; set; } = new();

        // 退货信息（仅退货退款）
        public RefundReturnDto? Return { get; set; }
    }

    public class RefundOrderItemDto
    {
        public string? GoodsId { get; set; }
        public string? GoodsName { get; set; }
        public string? GoodsImg { get; set; }
        public int BuyNum { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SubAmount { get; set; }
    }

    public class RefundReturnDto
    {
        public string? ReturnNo { get; set; }
        public int? ReturnNum { get; set; }
        public int? ReturnStatus { get; set; }
        public string? ReturnStatusText { get; set; }
        public string? ReturnReason { get; set; }
    }
}
