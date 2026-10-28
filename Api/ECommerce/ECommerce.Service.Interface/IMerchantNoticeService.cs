using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    /// <summary>
    /// 商户通知服务（库存预警等系统消息）。
    /// 通知与买卖家聊天（IChatService）分离：没有买家参与方，由系统产生。
    /// </summary>
    /// <remarks>
    /// 必须标注 [Provider_]：接口层程序集由此特性被 AddScoped_Extension 扫描，
    /// 自动注册为 Scoped（实现类标注 [Inject_]）。漏标会导致 DI 解析失败。
    /// </remarks>
    [Provider_]
    public interface IMerchantNoticeService
    {
        /// <summary>
        /// 生成并返回商户通知列表（进入消息中心时调用：先扫描库存生成预警，再返回列表）
        /// </summary>
        Task<Api_Response> MerchantNotices(string merchantId);

        /// <summary>把该商户的所有未读通知标记为已读</summary>
        Task<Api_Response> ReadAll(string merchantId);

        /// <summary>未读数汇总：聊天未读 + 通知未读（供首页消息按钮红点）</summary>
        Task<Api_Response> UnreadCount(string merchantId);
    }
}
