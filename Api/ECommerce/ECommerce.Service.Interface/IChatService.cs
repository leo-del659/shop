using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;

namespace ECommerce.Service.Interface
{
    /// <summary>
    /// 买卖家聊天服务（用户端小程序 ↔ 商户端小程序）
    /// 会话以 (clientId, merchantId) 二元组确定；消息文本仅支持文字内容。
    /// </summary>
    /// <remarks>
    /// 必须标注 [Provider_]：接口层程序集由此特性被 AddScoped_Extension 扫描，
    /// 自动注册为 Scoped（实现类 ChatService 标注 [Inject_]）。
    /// 若缺少该特性，IChatService 不会进入 DI 容器，
    /// ChatWebSocketMiddleware 的每请求参数注入会抛
    /// "Unable to resolve service for type 'ECommerce.Service.Interface.IChatService'"，
    /// 导致整条管道上的所有 HTTP 接口（登录/验证码等）一律 500。
    /// </remarks>
    [Provider_]
    public interface IChatService
    {
        /// <summary>买家会话列表（聊过的商家 + 最后一条消息 + 未读数）</summary>
        Task<Api_Response> ClientConversations(string clientId);

        /// <summary>买家拉取与某商户的消息（after 传最后一条消息时间做增量轮询，pageSize 默认 50）</summary>
        Task<Api_Response> ClientMessages(string clientId, string merchantId, string? after, int pageSize);

        /// <summary>买家发送消息给商户</summary>
        Task<Api_Response> ClientSend(string clientId, string merchantId, string content);

        /// <summary>商户信息简要（名称/头像），供买家从订单进入尚无历史消息的会话时展示</summary>
        Task<Api_Response> MerchantBrief(string merchantId);

        /// <summary>商户会话列表（聊过的买家 + 最后一条消息 + 未读数）</summary>
        Task<Api_Response> MerchantConversations(string merchantId);

        /// <summary>商户拉取与某买家的消息（after 传最后一条消息时间做增量轮询，pageSize 默认 50）</summary>
        Task<Api_Response> MerchantMessages(string merchantId, string clientId, string? after, int pageSize);

        /// <summary>商户发送消息给买家</summary>
        Task<Api_Response> MerchantSend(string merchantId, string clientId, string content);
    }
}
