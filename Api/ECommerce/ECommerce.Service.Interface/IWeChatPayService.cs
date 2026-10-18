using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;

namespace ECommerce.Service.Interface
{
    /// <summary>
    /// 微信支付编排服务（JSAPI 下单 / 回调落库 / 开发模拟）。
    /// 真实模式 Enabled=true：回调验签 + AES-GCM 解密后复用 OrderService.PayCallbackAsync 落库；
    /// 开发模拟模式 Enabled=false：本地加密回放，无需商户号即可端到端联调。
    /// </summary>
    [Provider_]
    public interface IWeChatPayService
    {
        /// <summary>JSAPI 统一下单：返回前端 wx.requestPayment 所需参数包</summary>
        Task<Api_Response> PrepayAsync(string orderId, string openId);

        /// <summary>处理微信支付成功通知（验签 + 解密 + 落库），返回微信要求的 {code,message}</summary>
        Task<Api_Response> HandleNotifyAsync(string signature, string timestamp, string nonce, string serial, string body);

        /// <summary>开发模拟：用本地密钥加密一段支付成功通知并回放，便于无商户号环境测试</summary>
        Task<Api_Response> SimulateNotifyAsync(string orderNo, string? transactionId = null);
    }
}
