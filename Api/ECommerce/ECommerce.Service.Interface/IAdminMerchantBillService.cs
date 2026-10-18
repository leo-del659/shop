using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IAdminMerchantBillService
    {
        Task<Api_Response> GetList(int page, int size, string? keyword, string? merchantId = null, string? clientId = null);
        Task<Api_Response> Get(string id);
        Task<Api_Response> Add(MerchantBill t);
        Task<Api_Response> Update(MerchantBill t);
        Task<Api_Response> Remove(string id);
        /// <summary>
        /// 商户账单付款登记（支持部分付，自动更新已付金额与状态）
        /// </summary>
        Task<Api_Response> Pay(string billId, string payNo, decimal payAmount, int payType);
        /// <summary>
        /// 自动生成账单编号：GYS{yyyyMMdd}-NN
        /// </summary>
        Task<Api_Response> GenerateBillNo();
    }
}
