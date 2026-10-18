using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户账单（应向商户支付的账单，如货款/服务费/运费等）
    /// </summary>
    public partial class MerchantBill
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 账单编号 GYS{yyyyMMdd}-NN
        /// </summary>
        public string? BillNo { get; set; }
        /// <summary>
        /// 关联订单编号
        /// </summary>
        public string? OrderNo { get; set; }
        /// <summary>
        /// 供货商户ID（merchant 表）
        /// </summary>
        public string? MerchantId { get; set; }
        /// <summary>
        /// 买家用户ID（sys_users.u_id）
        /// </summary>
        public string? ClientId { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public string? GoodsId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string? GoodsName { get; set; }
        /// <summary>
        /// 购入数量
        /// </summary>
        public int? BuyNum { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal? Amount { get; set; }
        /// <summary>
        /// 订单履约状态（枚举 OrderStatus，与 clientorders.order_status 同义）
        /// </summary>
        public OrderStatus? OrderStatus { get; set; }
        /// <summary>
        /// 账单类型 1货款 2服务费 3运费 4其他（历史字段保留）
        /// </summary>
        public int? BillType { get; set; }
        /// <summary>
        /// 计费起期（历史字段保留）
        /// </summary>
        public DateTime? PeriodStart { get; set; }
        /// <summary>
        /// 计费止期（历史字段保留）
        /// </summary>
        public DateTime? PeriodEnd { get; set; }
        /// <summary>
        /// 已付金额（历史字段保留）
        /// </summary>
        public decimal? PaidAmount { get; set; }
        /// <summary>
        /// 付款状态（枚举 PayStatus：0未支付 1已支付）
        /// </summary>
        public PayStatus? PayStatus { get; set; }
        /// <summary>
        /// 支付方式 1微信 2支付宝 3银行卡 4线下（历史字段保留）
        /// </summary>
        public int? PayType { get; set; }
        /// <summary>
        /// 支付流水号（历史字段保留）
        /// </summary>
        public string? PayNo { get; set; }
        /// <summary>
        /// 付款时间（历史字段保留）
        /// </summary>
        public DateTime? PayTime { get; set; }
        /// <summary>
        /// 备注（历史字段保留）
        /// </summary>
        public string? Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
