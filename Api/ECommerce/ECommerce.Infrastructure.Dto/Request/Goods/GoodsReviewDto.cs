namespace ECommerce.Infrastructure.Dto.Request.Goods
{
    /// <summary>
    /// 提交评价
    /// </summary>
    public class GoodsReviewCreateDto
    {
        public string? goods_id { get; set; }
        public string? sku_id { get; set; }
        public string? order_id { get; set; }
        public string? member_name { get; set; }
        public int rating { get; set; }
        public string? content { get; set; }
        public string? images { get; set; }
        public string? tags { get; set; }
        /// <summary>对商家评价(可选)</summary>
        public string? client_review { get; set; }
        /// <summary>是否匿名(0 否 1 是,默认 0)</summary>
        public byte? is_anonymous { get; set; }
    }

    /// <summary>
    /// 后台评价列表查询
    /// </summary>
    public class GoodsReviewQueryDto
    {
        public string? goods_id { get; set; }
        /// <summary>评分档位：good 好评 / mid 中评 / bad 差评</summary>
        public string? tier { get; set; }
        /// <summary>标签精确匹配，如：口感好</summary>
        public string? tag { get; set; }
        public string? keyword { get; set; }
        public int? status { get; set; }
    }

    public class GoodsReviewAuditDto
    {
        public string? id { get; set; }
        /// <summary>1通过 2驳回</summary>
        public int? status { get; set; }
        /// <summary>驳回原因（status=2 时必填）</summary>
        public string? reject_reason { get; set; }
    }

    public class GoodsReviewReplyDto
    {
        public string? id { get; set; }
        public string? reply { get; set; }
    }
}
