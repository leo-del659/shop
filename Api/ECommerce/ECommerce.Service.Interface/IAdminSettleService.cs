using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IAdminSettleService
    {
        Task<Api_Response> GetList(int page, int size, string? keyword);
        Task<Api_Response> Get(string id);
        Task<Api_Response> Add(SettleRecord t);
        Task<Api_Response> Update(SettleRecord t);
        Task<Api_Response> Remove(string id);
        /// <summary>
        /// 结算支付
        /// </summary>
        Task<Api_Response> Pay(string settleId, string payNo, decimal payAmount);
        /// <summary>
        /// 结算退款
        /// </summary>
        Task<Api_Response> Refund(string settleId, string refundNo, decimal refundAmount);
        /// <summary>
        /// 自动生成结算单号（前端 open_add 调用）：JS{yyyyMMdd}-{2位序号}
        /// 同日同 prefix 的现存最大序号 + 1，空表回 01
        /// </summary>
        Task<Api_Response> GenerateSettleNo();
    }
}
