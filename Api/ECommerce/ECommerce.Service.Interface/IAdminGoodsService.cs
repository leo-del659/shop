using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Goods;
using ECommerce.Infrastructure.Dto.Response;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IAdminGoodsService
    {
        Task<Api_Response> GetList(int page, int size, string? keyword, int? auditStatus = null, string? categoryId = null, string? merchantId = null);
        Task<Api_Response> Get(string id);
        Task<Api_Response> Add(GoodCreateDto dto);
        Task<Api_Response> Update(GoodModifyDto dto);
        Task<Api_Response> Remove(string id);
        /// <summary>
        /// 提交商品审核（状态置为待审核）
        /// </summary>
        Task<Api_Response> SubmitAudit(string id);
        /// <summary>
        /// 处理商品审核（通过/驳回，写入审核记录）
        /// </summary>
        Task<Api_Response> Audit(AuditHandleDto dto);
        /// <summary>
        /// 设置商品上架/下架状态
        /// </summary>
        Task<Api_Response> SetSale(string id, int isSale);
    }
}
