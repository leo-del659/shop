using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 订单履约状态机。
    /// 数值与数据库 clientorders.order_status 一致，JSON 默认序列化为数字，前端按数字契约使用。
    /// 状态流转：待接单(1) → 已接单(2) → 已发货(3) → 已完成(4)；
    /// 退款链路：退款中(5) → 已退款(6) 或 已驳回(7)；取消：已取消(9)。
    /// </summary>
    public enum OrderStatus
    {
        PendingPayment = 0,  // 待付款（未发起支付前的初始/过渡态）
        PendingAccept = 1,   // 待接单（已支付，等待商户接单）
        Accepted = 2,        // 已接单 / 备货中
        Shipped = 3,         // 已发货 / 配送中
        Completed = 4,       // 已完成 / 已签收
        Refunding = 5,       // 退款中
        Refunded = 6,        // 已退款
        Rejected = 7,        // 已驳回
        PendingShip = 8,     // 待发货（扩展待处理态）
        Cancelled = 9        // 已取消 / 已作废
    }

    /// <summary>
    /// 支付状态。0 未支付，1 已支付。与 OrderStatus 分离：支付成功只置本字段。
    /// </summary>
    public enum PayStatus
    {
        Unpaid = 0,
        Paid = 1,
        PartialPaid = 2  // 部分付（账单历史字段保留）
    }
}
