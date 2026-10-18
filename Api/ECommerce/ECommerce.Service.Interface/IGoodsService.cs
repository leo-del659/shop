using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto;
using ECommerce.Infrastructure.Dto.Response.Order;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IGoodsService
    {
        /// <summary>
        /// 获取分类树并为每个分类附带商品（按更新时间降序，每分类限制数量）
        /// </summary>
        Task<List<CategoryWithGoodsDto>> GetCategoryTreeWithGoodsAsync(string? rootCategoryId = null, int productsPerCategory = 10, bool includeAllLevels = false);

        /// <summary>
        /// 提交订单
        /// </summary>
        Task<bool> SubmitOrderAsync(string remark, DateTime deliveryDate, List<OrderItemDto> items);

        /// <summary>
        /// 获取订单
        /// </summary>
        Task<OrderDto> GetOrderAsync(string orderId);
    }
}
