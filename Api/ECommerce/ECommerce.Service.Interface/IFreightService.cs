using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Order;
using ECommerce.Infrastructure.Dto.Response;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    /// <summary>
    /// 订单运费服务
    /// 负责：按订单参数计算运费、运费支付处理、按订单查询运费明细。
    /// </summary>
    [Provider_]
    public interface IFreightService
    {
        /// <summary>
        /// 计算运费。
        /// 传 orderId：按已有订单计算并落库（更新生效时间）；传 items：下单前预览计算（不落库）。
        /// </summary>
        Task<Api_Response> CalcAsync(string userId, FreightCalcRequestDto dto);

        /// <summary>
        /// 运费支付：将运费单标记为已支付并记录支付时间（幂等，已支付直接返回）。
        /// </summary>
        Task<Api_Response> PayAsync(string userId, string? mobile, FreightPayRequestDto dto);

        /// <summary>
        /// 查询指定订单的运费明细；若订单尚无运费单，则按订单当前参数实时计算返回（不落库）。
        /// </summary>
        Task<Api_Response> GetByOrderAsync(string userId, string? mobile, string orderId);
    }
}
