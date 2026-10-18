using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Goods;
using ECommerce.Infrastructure.Dto.Response;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IGoodsReviewService
    {
        /// <summary>移动端提交评价（memberId 由服务端从登录态解析）</summary>
        Task<Api_Response> Submit(GoodsReviewCreateDto dto, string memberId);

        /// <summary>后台评价列表（支持 关键词/评分档位/标签/状态/商品 过滤）</summary>
        Task<Api_Response> GetList(int page, int size, string? keyword, string? tier, string? tag, int? status, string? goodsId);

        /// <summary>移动端商品详情页评价列表（仅通过状态 + 档位/标签过滤）</summary>
        Task<Api_Response> GetByGoods(string goodsId, int page, int size, string? tier, string? tag);

        /// <summary>移动端：查询某订单下已评价的商品（用于防止重复评价）</summary>
        Task<Api_Response> GetOrderReviewed(string orderId, string memberId);

        /// <summary>后台审核（1通过 2驳回）</summary>
        Task<Api_Response> Audit(GoodsReviewAuditDto dto);

        /// <summary>后台商家回复</summary>
        Task<Api_Response> Reply(GoodsReviewReplyDto dto);

        /// <summary>后台删除评价</summary>
        Task<Api_Response> Remove(string id);
    }
}
