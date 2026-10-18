using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IAdminStockService
    {
        Task<Api_Response> GetList(int page, int size, string? keyword);
        Task<Api_Response> Get(string id);
        Task<Api_Response> Add(Stock t);
        Task<Api_Response> Update(Stock t);
        Task<Api_Response> Remove(string id);
        /// <summary>
        /// 入库：按 商品ID+仓库 累加库存，并写一条入库记录
        /// </summary>
        Task<Api_Response> StockIn(string goodsId, string? warehouse, int num, string? remark);
        /// <summary>
        /// 出库：扣减可用库存（总库存-锁定），并写一条出库记录
        /// </summary>
        Task<Api_Response> StockOut(string goodsId, string? warehouse, int num, string? remark);
        /// <summary>
        /// 获取所有已使用过的仓库名（distinct），用于前端"仓库"字段下拉数据源
        /// </summary>
        Task<Api_Response> GetWarehouseList();
    }
}
