using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 买卖家聊天消息（用户端小程序 ↔ 商户端小程序）。
    /// 一条消息即一行；会话由 (client_id, merchant_id) 二元组确定，
    /// sender_type 区分发送方（1 买家 / 2 商户），两端各自的已读状态独立记录，
    /// 用于会话列表未读角标与进入会话后的自动已读。
    /// </summary>
    public partial class ChatMessage
    {
        /// <summary>主键（顺序 GUID）</summary>
        public string Id { get; set; } = null!;
        /// <summary>买家用户ID（sys_users.u_id，与 client_order.client_id 同源）</summary>
        public string? ClientId { get; set; }
        /// <summary>商户ID（merchant.id，如 sp_0001）</summary>
        public string? MerchantId { get; set; }
        /// <summary>发送方类型：1 买家（消费者） 2 商户</summary>
        public int SenderType { get; set; }
        /// <summary>消息文本内容</summary>
        public string? Content { get; set; }
        /// <summary>买家是否已读：0 未读 1 已读（对端为商户的消息由买家拉取时置 1）</summary>
        public int ClientRead { get; set; }
        /// <summary>商户是否已读：0 未读 1 已读（对端为买家的消息由商户拉取时置 1）</summary>
        public int MerchantRead { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
