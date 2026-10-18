using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    /// <summary>
    /// 排行榜服务。当前提供「商户销售排行榜」，
    /// 统计口径与 GoodsService 的商品销量统计保持一致。
    /// </summary>
    [Provider_]
    public interface IAdminRankService
    {
        /// <summary>
        /// 商户销售排行榜：按「已支付(pay_status=1) + 未取消(order_status!=9) + 未作废(is_ban!=1)」
        /// 的订单汇总各商户的订单数与销售额，按销售额降序取 Top N。
        /// 0 单商户同样返回（订单数/销售额为 0），便于看到全量商户。
        /// </summary>
        Task<Api_Response> MerchantRank(int top = 10);
    }
}
