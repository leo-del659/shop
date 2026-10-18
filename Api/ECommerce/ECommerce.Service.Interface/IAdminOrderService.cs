using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IAdminOrderService
    {
        Task<Api_Response> GetList(int page, int size, string? keyword, int? status, string? clientId = null);
        Task<Api_Response> Get(string id);
        Task<Api_Response> Add(ClientOrder t);
        Task<Api_Response> Update(ClientOrder t);
        Task<Api_Response> Remove(string id);
        /// <summary>
        /// 订单支付
        /// </summary>
        Task<Api_Response> Pay(string orderId, string payNo, decimal payAmount, int payType);
        /// <summary>
        /// 订单配送
        /// </summary>
        Task<Api_Response> Delivery(string orderId, string logisticsId, string trackingNo);
        /// <summary>
        /// 订单退款
        /// </summary>
        Task<Api_Response> Refund(string orderId, string refundNo, decimal refundAmount, string reason);
        /// <summary>
        /// 订单退货
        /// </summary>
        Task<Api_Response> Return(string orderId, string returnNo, int returnNum, string reason);
        /// <summary>
        /// 自动生成订单号（前端 open_add 调用）：{yyMMdd}{4位当日序号}
        /// 同日现存最大序号 + 1，例如 2608120001
        /// </summary>
        Task<Api_Response> GenerateOrderNo();
        /// <summary>
        /// 管理首页多维销售/财务分析看板：销售周趋势、分类销售占比、
        /// 退款趋势、商户销售排行
        /// </summary>
        Task<Api_Response> Dashboard();
    }
}
