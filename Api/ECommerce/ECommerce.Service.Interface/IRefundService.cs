using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Order;
using ECommerce.Infrastructure.Dto.Response;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IRefundService
    {
        /// <summary>
        /// 消费者申请退款/退货退款
        /// </summary>
        Task<Api_Response> ApplyAsync(string userId, ApplyRefundRequestDto dto);

        /// <summary>
        /// 我的退款列表
        /// </summary>
        Task<Api_Response> GetMyRefundsAsync(string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// 退款详情
        /// </summary>
        Task<Api_Response> GetDetailAsync(string userId, string id);

        /// <summary>
        /// 后台审核（同意/驳回）
        /// </summary>
        Task<Api_Response> ReviewAsync(string adminId, RefundReviewRequestDto dto);

        /// <summary>
        /// 后台退款列表
        /// </summary>
        Task<Api_Response> GetAdminListAsync(int page = 1, int pageSize = 20, int? status = null);

        /// <summary>
        /// 后台退款详情（不限制 userId）
        /// </summary>
        Task<Api_Response> GetAdminDetailAsync(string id);
    }
}
