using System;
using System.Collections.Generic;
using ECommerce.Infrastructure.EFCore;

namespace ECommerce.Infrastructure.Dto.Response.Order
{
    public class OrderResponseDto
    {
        public string OrderId { get; set; } = null!;

        public string OrderNo { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public OrderStatus? OrderStatus { get; set; }

        /// <summary>
        /// 支付状态：0 未支付 / 1 已支付。与 OrderStatus 区分——
        /// 支付成功只置 PayStatus=1，订单履约状态(OrderStatus)仍保持待接单(1)，
        /// 待商户端「接单」后再推进，避免与商户端状态机冲突。
        /// </summary>
        public PayStatus? PayStatus { get; set; }

        public string? Remark { get; set; }

        public string? ReceiverName { get; set; }

        public string? ReceiverMobile { get; set; }

        public string? Address { get; set; }

        /// <summary>
        /// 付款时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? ShipTime { get; set; }

        /// <summary>
        /// 成交/完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 售卖商户ID：用户购买商品绑定在哪个商户购买的
        /// </summary>
        public string? SellClientId { get; set; }

        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderSummaryDto
    {
        public string Id { get; set; } = null!;

        public string OrderNo { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public OrderStatus? OrderStatus { get; set; }

        /// <summary>
        /// 支付状态：0 未支付 / 1 已支付。与 OrderStatus 区分——
        /// 支付成功只置 PayStatus=1，订单履约状态(OrderStatus)仍保持待接单(1)，
        /// 待商户端「接单」后再推进，避免与商户端状态机冲突。
        /// </summary>
        public PayStatus? PayStatus { get; set; }

        public string? Remark { get; set; }

        public string? ReceiverName { get; set; }

        public string? ReceiverMobile { get; set; }

        public string? Address { get; set; }

        /// <summary>
        /// 售卖商户ID：用户购买商品绑定在哪个商户购买的（来自 good.client_id）
        /// </summary>
        public string? SellClientId { get; set; }

        /// <summary>
        /// 售卖商户名称（便于前端直接展示，无需二次查询）
        /// </summary>
        public string? SellClientName { get; set; }

        /// <summary>
        /// 所属商户ID（下单时根据商品反解写入 client_order.merchant_id）
        /// </summary>
        public string? MerchantId { get; set; }

        /// <summary>
        /// 所属商户名称（便于前端直接展示，无需二次查询）
        /// </summary>
        public string? MerchantName { get; set; }

        /// <summary>
        /// 付款时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? ShipTime { get; set; }

        /// <summary>
        /// 成交/完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        public DateTime CreateTime { get; set; }

        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public string GoodsId { get; set; } = null!;

        public string? SkuId { get; set; }

        public string? GoodsName { get; set; }

        public string? GoodsImg { get; set; }

        public int BuyNum { get; set; }

        public decimal BuyPrice { get; set; }

        public decimal SubAmount { get; set; }

        /// <summary>
        /// 该商品所属售卖商户ID（从 good.client_id 冗余到明细）
        /// </summary>
        public string? SellClientId { get; set; }
    }
}