using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IAdminLogisticsService
    {
        Task<Api_Response> GetList(int page, int size, string? keyword);
        Task<Api_Response> Get(string id);
        Task<Api_Response> Add(LogisticsCompany t);
        Task<Api_Response> Update(LogisticsCompany t);
        Task<Api_Response> Remove(string id);
        /// <summary>
        /// 预览下一个将被分配的物流编码（不写入数据库，供前端预览）
        /// 编码规则：WL + 4 位数字（如 WL0001、WL0002）
        /// </summary>
        Task<Api_Response> PreviewNextLogisticsCode();
        /// <summary>
        /// 一次性将库内所有物流公司的编码重排为统一的 WL + 4 位数字格式
        /// （WL0001、WL0002 …），按创建时间升序编号。用于清理历史非标准编码。
        /// </summary>
        Task<Api_Response> NormalizeCodes();
    }
}
