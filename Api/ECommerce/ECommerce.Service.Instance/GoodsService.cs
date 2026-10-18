using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto;
using ECommerce.Infrastructure.Dto.Response.Order;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class GoodsService : IGoodsService
    {
        private readonly _111Context _db;

        public GoodsService(_111Context db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<List<CategoryWithGoodsDto>> GetCategoryTreeWithGoodsAsync(string? rootCategoryId = null, int productsPerCategory = 10, bool includeAllLevels = false)
        {
            if (productsPerCategory <= 0) productsPerCategory = 10;

            var allCategories = await _db.GoodsCategories
                .AsNoTracking()
                .OrderBy(c => c.Sort)
                .ThenBy(c => c.CreateTime)
                .ToListAsync()
                .ConfigureAwait(false);

            var dict = new Dictionary<string, CategoryWithGoodsDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in allCategories)
            {
                var node = new CategoryWithGoodsDto
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    CategoryName = c.CategoryName,
                    Icon = c.Icon,
                    Sort = c.Sort,
                    IsBan = c.IsBan,
                    CreateTime = c.CreateTime
                };
                dict[c.Id] = node;
            }

            var roots = new List<CategoryWithGoodsDto>();
            foreach (var node in dict.Values)
            {
                if (!string.IsNullOrWhiteSpace(node.ParentId) && dict.TryGetValue(node.ParentId!, out var parent))
                {
                    parent.Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            HashSet<string> includedCategoryIds;
            if (string.IsNullOrWhiteSpace(rootCategoryId))
            {
                includedCategoryIds = dict.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                if (!dict.ContainsKey(rootCategoryId))
                {
                    return new List<CategoryWithGoodsDto>();
                }

                includedCategoryIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CollectSubtreeIds(dict[rootCategoryId], includedCategoryIds);
            }

            var goodsEntities = await _db.Goods
                .AsNoTracking()
                .Where(g => includedCategoryIds.Contains(g.CategoryId) && g.IsSale == 1)
                .OrderByDescending(g => g.UpdateTime)
                .ToListAsync()
                .ConfigureAwait(false);

            // 累计已售数量（一次性预聚合，避免 N×M 重复解析）
            // 仅取与本次返回商品相关的 ID；统计有效订单（已签收且未作废）
            var includedGoodsIds = goodsEntities.Select(g => g.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var soldCountMap = await ComputeSoldCountsAsync(includedGoodsIds).ConfigureAwait(false);

            var goodsByCat = goodsEntities
                .GroupBy(g => g.CategoryId)
                .ToDictionary(g => g.Key, g => g.Take(productsPerCategory)
                    .Select(e => new GoodsDto
                    {
                        Id = e.Id,
                        CategoryId = e.CategoryId,
                        GoodsName = e.GoodsName,
                        Price = e.Price,
                        SalePrice = e.SalePrice,
                        Stock = e.Stock,
                        CoverImg = e.CoverImg,
                        Unit = e.Unit,
                        IsSale = e.IsSale,
                        UpdateTime = e.UpdateTime,
                        SoldCount = soldCountMap.TryGetValue(e.Id, out var n) ? n : 0
                    })
                    .ToList()
                , StringComparer.OrdinalIgnoreCase);

            foreach (var node in dict.Values)
            {
                if (goodsByCat.TryGetValue(node.Id, out var goods))
                {
                    if (includeAllLevels || node.Children == null || node.Children.Count == 0)
                    {
                        node.Goods = goods;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(rootCategoryId))
            {
                return roots;
            }

            return new List<CategoryWithGoodsDto> { dict[rootCategoryId] };
        }

        public Task<List<CategoryWithGoodsDto>> GetCategoryTreeWithGoodsAsync(string? rootCategoryId = null, bool includeAllLevels = false)
        {
            return GetCategoryTreeWithGoodsAsync(rootCategoryId, 10, includeAllLevels);
        }

        /// <summary>
        /// 预聚合指定商品 ID 集合的累计已售数量。
        /// 统计「已支付且未取消/未删除」订单的 items_json，在内存中解析并按 goodsId 累加 buyNum。
        /// 任一商品未命中返回 0；解析异常不影响整体。
        /// </summary>
        private async Task<Dictionary<string, int>> ComputeSoldCountsAsync(HashSet<string> goodsIds)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (goodsIds == null || goodsIds.Count == 0) return result;

            try
            {
                var rows = await _db.ClientOrders
                    .AsNoTracking()
                    .Where(o => o.PayStatus == PayStatus.Paid && o.OrderStatus != OrderStatus.Cancelled && (o.IsBan == null || o.IsBan != 1))
                    .Select(o => new { o.Id, o.ItemsJson })
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var row in rows)
                {
                    var items = ClientOrderItemsHelper.ParseItems(row.ItemsJson);
                    if (items == null) continue;
                    foreach (var it in items)
                    {
                        if (string.IsNullOrEmpty(it.GoodsId)) continue;
                        if (!goodsIds.Contains(it.GoodsId)) continue;
                        var n = it.BuyNum ?? 0;
                        if (n <= 0) continue;
                        result.TryGetValue(it.GoodsId, out var cur);
                        result[it.GoodsId] = cur + n;
                    }
                }
            }
            catch
            {
                // 销量聚合异常时返回空字典：商品仍能展示，销量降级为 0
            }
            return result;
        }


        public Task<OrderDto> GetOrderAsync(string orderId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SubmitOrderAsync(string remark, DateTime deliveryDate, List<OrderItemDto> items)
        {
            throw new NotImplementedException();
        }

        private void CollectSubtreeIds(CategoryWithGoodsDto root, HashSet<string> set)
        {
            if (set.Add(root.Id))
            {
                foreach (var child in root.Children)
                {
                    CollectSubtreeIds(child, set);
                }
            }
        }
    }
}
