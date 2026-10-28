using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户通知（库存预警等系统产生的消息）。
    /// 与 ChatMessage（买卖家聊天）分离：通知没有买家参与方，
    /// 由服务端在商户进入消息中心时扫描业务数据自动生成。
    /// </summary>
    public partial class MerchantNotice
    {
        /// <summary>主键（顺序 GUID）</summary>
        public string Id { get; set; } = null!;
        /// <summary>商户ID（merchant.merchant_id，如 sp_0001）</summary>
        public string? MerchantId { get; set; }
        /// <summary>通知类型：1 库存预警 2 订单提醒 3 其他</summary>
        public int NoticeType { get; set; }
        /// <summary>通知标题</summary>
        public string? Title { get; set; }
        /// <summary>通知内容</summary>
        public string? Content { get; set; }
        /// <summary>关联业务ID（库存预警为商品ID）</summary>
        public string? RelatedId { get; set; }
        /// <summary>预警时的库存快照：库存没变化则不重复生成预警</summary>
        public int StockSnapshot { get; set; }
        /// <summary>是否已读：0 未读 1 已读</summary>
        public int IsRead { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
