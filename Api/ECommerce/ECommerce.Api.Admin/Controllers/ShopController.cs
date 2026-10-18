using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 小程序商城控制器（面向消费者，只读浏览）
    /// 提供首页分类商品、商品列表与商品详情等接口
    /// </summary>
    [ApiController]
    [Route("api/shop")]
    public class ShopController : ControllerBase
    {
        private readonly IGoodsService _goodsService;
        private readonly Good_EFCore _goodEFCore;
        private readonly GoodsCategory_EFCore _categoryEFCore;
        private readonly GoodsSku_EFCore _skuEFCore;
        private readonly ClientOrder_EFCore _clientOrderEFCore;

        /// <summary>
        /// 构造函数，注入商品服务与各商品相关仓储
        /// </summary>
        public ShopController(
            IGoodsService goodsService,
            Good_EFCore goodEFCore,
            GoodsCategory_EFCore categoryEFCore,
            GoodsSku_EFCore skuEFCore,
            ClientOrder_EFCore clientOrderEFCore)
        {
            _goodsService = goodsService;
            _goodEFCore = goodEFCore;
            _categoryEFCore = categoryEFCore;
            _skuEFCore = skuEFCore;
            _clientOrderEFCore = clientOrderEFCore;
        }

        /// <summary>
        /// 获取小程序首页数据：分类树（每个分类附带若干商品）
        /// </summary>
        /// <returns>分类树（含商品）</returns>
        [HttpGet("home")]
        public async Task<IActionResult> GetHome()
        {
            // 树状分类 + 显示全部商品：每分类最多取 1000 件（实际远小于此），includeAllLevels=true 使各级节点都带自身商品
            var data = await _goodsService.GetCategoryTreeWithGoodsAsync(null, 1000, true);
            return Ok(new Api_Response { code = Api_Code.ok, message = "ok", data = data });
        }

        /// <summary>
        /// 获取某个分类（含其子分类）的商品树
        /// 用于小程序「分类详情页」：树状卡片 + 顶部筛选
        /// 不传 categoryId 时返回完整分类树
        /// </summary>
        [HttpGet("cattree")]
        public async Task<IActionResult> GetCategoryTree(string? categoryId)
        {
            var data = await _goodsService.GetCategoryTreeWithGoodsAsync(categoryId, 1000, true);
            return Ok(new Api_Response { code = Api_Code.ok, message = "ok", data = data });
        }

        /// <summary>
        /// 商品列表（仅返回已上架商品）
        /// 支持按分类、关键词分页查询
        /// </summary>
        /// <param name="categoryId">分类 ID（可选）</param>
        /// <param name="keyword">关键词（按商品名称模糊匹配，可选）</param>
        /// <param name="page">页码，默认 1</param>
        /// <param name="size">每页数量，默认 10</param>
        /// <returns>商品分页列表</returns>
        [HttpGet("goods")]
        public IActionResult GetGoodsList(string? categoryId, string? keyword, string? sort, int page = 1, int size = 10)
        {
            Expression<Func<Good, bool>> where = g =>
                g.IsSale == 1
                && (string.IsNullOrEmpty(categoryId) || g.CategoryId == categoryId)
                && (string.IsNullOrEmpty(keyword) || (g.GoodsName != null && g.GoodsName.Contains(keyword)));

            int total;
            List<Good> pageEntities;

            // 排序：price_asc 价格升序 / price_desc 价格降序 / 其他 综合（创建时间倒序）
            if (sort == "price_asc")
            {
                pageEntities = _goodEFCore.Query(out total, page, size, true, g => g.Price, where).ToList();
            }
            else if (sort == "price_desc")
            {
                pageEntities = _goodEFCore.Query(out total, page, size, false, g => g.Price, where).ToList();
            }
            else
            {
                pageEntities = _goodEFCore.Query(out total, page, size, false, g => g.CreateTime, where).ToList();
            }

            // 预聚合本批商品的销量
            var ids = pageEntities.Select(e => e.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var soldMap = ComputeSoldCounts(ids);

            var list = pageEntities.Select(g => new
            {
                g.Id,
                g.CategoryId,
                g.GoodsName,
                g.Price,
                g.SalePrice,
                g.Stock,
                g.CoverImg,
                g.Unit,
                g.ImgList,
                SoldCount = soldMap.TryGetValue(g.Id, out var n) ? n : 0
            }).ToList();

            return Ok(new Api_Response
            {
                code = Api_Code.ok,
                message = "ok",
                data = new { total, list }
            });
        }

        /// <summary>
        /// 预聚合指定商品 ID 集合的累计已售数量（已支付且未取消/未作废订单中 buyNum 累加）。
        /// </summary>
        private Dictionary<string, int> ComputeSoldCounts(HashSet<string> goodsIds)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (goodsIds == null || goodsIds.Count == 0) return result;
            try
            {
                var orders = _clientOrderEFCore
                    .Query(o => o.PayStatus == PayStatus.Paid && o.OrderStatus != OrderStatus.Cancelled && (o.IsBan == null || o.IsBan != 1))
                    .Select(o => new { o.Id, o.ItemsJson })
                    .ToList();
                foreach (var row in orders)
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
            catch { /* 销量聚合异常降级为 0 */ }
            return result;
        }

        private static object MapGood(Good g)
        {
            return new
            {
                g.Id,
                g.CategoryId,
                g.GoodsName,
                g.Price,
                g.SalePrice,
                g.Stock,
                g.CoverImg,
                g.Unit,
                g.ImgList,
                SoldCount = 0
            };
        }

        /// <summary>
        /// 商品详情
        /// 返回商品主信息、图片列表、SKU 与规格
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品详情</returns>
        [HttpGet("goods/{id}")]
        public IActionResult GetGoodsDetail(string id)
        {
            var good = _goodEFCore.Find(id);
            if (good == null)
            {
                return Ok(new Api_Response { code = Api_Code.fail, message = "商品不存在" });
            }

            // 多图统一从 good.img_list（逗号分隔）解析，不再依赖 goods_image 表
            var images = (good.ImgList ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(url => new { imgUrl = url.Trim() })
                .ToList();
            var skus = _skuEFCore.Query(s => s.GoodsId == id).ToList();
            // 规格表 goods_spec 未创建：规格直接由 goods_sku 合成（spec_id 作为规格 id，sku_name 作为规格名）
            var specs = skus
                .Where(s => !string.IsNullOrEmpty(s.SpecId))
                .GroupBy(s => s.SpecId)
                .Select(g => new GoodsSpec
                {
                    Id = g.Key,
                    GoodsId = id,
                    SpecName = g.First().SkuName
                })
                .ToList();

            // 累计已售数量：统计「已支付(PayStatus = PayStatus.Paid)且未取消/未作废」订单中该商品的购买总件数。
            // 明细内联在 client_order.items_json，逐单解析并累加 goodsId 匹配项的 buyNum。
            // 说明：当前为详情接口按需统计；若后续列表页也要展示，建议改为预聚合/缓存。
            var soldCount = 0;
            try
            {
                var orders = _clientOrderEFCore
                    .Query(o => o.PayStatus == PayStatus.Paid && o.OrderStatus != OrderStatus.Cancelled && (o.IsBan == null || o.IsBan != 1))
                    .ToList();
                foreach (var order in orders)
                {
                    foreach (var it in order.ParseItems())
                    {
                        if (it.GoodsId == id)
                            soldCount += it.BuyNum ?? 0;
                    }
                }
            }
            catch
            {
                // 销量统计异常不影响商品详情主流程
            }
            good.SoldCount = soldCount;

            return Ok(new Api_Response
            {
                code = Api_Code.ok,
                message = "ok",
                data = new { good, images, skus, specs }
            });
        }
    }
}
