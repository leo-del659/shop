using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 用户采购订单主表
    /// 注：sell_client_id / settle_type / settle_channel / settle_batch_id /
    /// delivery_date 已于 2026-09-15 从数据库删除；
    /// receiver_name / receiver_mobile 后因订单详情需展示收货信息于 2026-09-15 重新加回，
    /// 并新增 pay_time / ship_time / complete_time 三个时间字段供详情页展示。
    /// client_id（原库列名 merchant_id，2026-10-17 更名）存买家 userId（sys_users.u_id）。
    /// </summary>
    public partial class ClientOrder
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 订单编号
        /// </summary>
        public string? OrderNo { get; set; }
        /// <summary>
        /// 买家用户ID（sys_users.u_id，库列名 client_id）
        /// </summary>
        public string? ClientId { get; set; }
        /// <summary>
        /// 供货商户ID（商户端订单隔离的核心字段）
        /// </summary>
        public string? MerchantId { get; set; }
        /// <summary>
        /// 订单总金额
        /// </summary>
        public double? TotalAmount { get; set; }
        /// <summary>
        /// 订单履约状态（枚举 OrderStatus：1待接单 2已接单 3已发货 4已完成 5退款中 6已退款 7已驳回 9已取消）
        /// </summary>
        public OrderStatus? OrderStatus { get; set; }
        /// <summary>
        /// 支付状态（枚举 PayStatus：0未支付 1已支付；与 OrderStatus 分离，支付成功只置本字段）
        /// </summary>
        public PayStatus? PayStatus { get; set; }
        /// <summary>
        /// 商品信息文本（单行摘要）
        /// </summary>
        public string? GoodsInfo { get; set; }
        /// <summary>
        /// 订单商品明细 JSON 数组（合并自原 client_order_item 表）。
        /// 单个元素形如 { id, goodsId, specId, goodsName, specName, goodsImg,
        /// buyPrice, buyNum, subAmount, isBan, createTime, updateTime }。
        /// 单商品订单为长度 1 的数组，多商品订单保留全部明细。
        /// </summary>
        public string? ItemsJson { get; set; }
        /// <summary>
        /// 拒绝原因
        /// </summary>
        public string? RejectReason { get; set; }
        /// <summary>
        /// 商户下单备注
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string? ReceiverName { get; set; }
        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string? ReceiverMobile { get; set; }
        /// <summary>
        /// 收货详细地址（省市区 + 详细地址整串）
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// 付款时间（PayCallback 置 payStatus=1 时写入）
        /// </summary>
        public DateTime? PayTime { get; set; }
        /// <summary>
        /// 发货时间（商户端发货 action 写入）
        /// </summary>
        public DateTime? ShipTime { get; set; }
        /// <summary>
        /// 成交/完成时间（买家确认收货或商户确认完成时写入）
        /// </summary>
        public DateTime? CompleteTime { get; set; }
        /// <summary>
        /// 是否禁用 0正常 1作废
        /// </summary>
        public int? IsBan { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
