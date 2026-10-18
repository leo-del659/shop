using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Order;
using ECommerce.Infrastructure.Dto.Response;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IOrderService
    {
        Task<Api_Response> CreateOrderAsync(string userId, CreateOrderRequestDto dto);

        Task<Api_Response> GetOrdersAsync(string userId, string? mobile = null, int? status = null, int page = 1, int pageSize = 10);

        Task<Api_Response> GetOrderDetailAsync(string userId, string orderId, string? mobile = null);

        Task<Api_Response> CancelOrderAsync(string userId, string orderId, string? mobile = null, string? reason = null);

        /// <summary>
        /// 买家删除订单：逻辑删除（IsBan=1），仅买家本人可操作。
        /// </summary>
        Task<Api_Response> DeleteOrderAsync(string userId, string orderId, string? mobile = null);

        Task<Api_Response> PayCallbackAsync(string orderId, PayCallbackRequestDto? dto = null, string? transactionId = null);

        /// <summary>
        /// 商家发货：原为「将已支付(待发货)订单标记为已发货(待收货)，仅售卖商户可操作」。
        /// 注：client_order.sell_client_id 列已删除，订单不再保存售卖商户，
        /// 原越权校验失去依据，故实现改为直接返回失败；发货请走商户端
        /// /api/merchant/orders/{id}/action（action=ship）。
        /// </summary>
        Task<Api_Response> ShipOrderAsync(string userId, string orderId, string? mobile = null);

        /// <summary>
        /// 买家确认收货：将已发货订单标记为已完成
        /// 仅买家(ClientId)可操作
        /// </summary>
        Task<Api_Response> ConfirmReceiptAsync(string userId, string orderId, string? mobile = null);

        /// <summary>
        /// B 方案：历史订单/地址 owner 迁移。
        /// 同一手机号在 sys_users 中只有一个规范账号（u_account UNIQUE），但历史数据里
        /// member_address.user_id / client_order.client_id 可能仍指向重构前的旧 UId（孤儿）。
        /// 本方法把这些孤儿引用统一迁移到当前规范 UId，避免数据库长期留孤儿；
        /// 迁移完成后 A 方案的 mobile 兜底仅作为安全网。幂等：无可迁移数据则返回 0。
        /// </summary>
        Task<int> ConsolidateLegacyOwnersByMobileAsync(string mobile);
    }
}