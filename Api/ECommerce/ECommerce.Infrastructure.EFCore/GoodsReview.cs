using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商品用户评价表
    /// </summary>
    public partial class GoodsReview
    {
        public string Id { get; set; } = null!;
        public string GoodsId { get; set; } = null!;
        public string? GoodsName { get; set; }
        public string? SkuId { get; set; }
        public string? SkuName { get; set; }
        public string? MemberId { get; set; }
        public string? MemberName { get; set; }
        /// <summary>评价人头像（评价快照，避免用户换头像后历史评价头像变化）</summary>
        public string? Avatar { get; set; }
        public string? OrderId { get; set; }
        public int Rating { get; set; }
        public string? Content { get; set; }
        public string? Images { get; set; }
        public string? Tags { get; set; }
        /// <summary>对商家评价(独立文本,可空)</summary>
        public string? ClientReview { get; set; }
        /// <summary>是否匿名(1 是 0 否,默认 0)</summary>
        public byte? IsAnonymous { get; set; }
        public int? Status { get; set; }
        public string? Reply { get; set; }
        public DateTime? ReplyTime { get; set; }
        public DateTime? CreateTime { get; set; }
        public string? RejectReason { get; set; }
    }
}
