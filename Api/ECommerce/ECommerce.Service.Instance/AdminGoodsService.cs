using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Goods;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class AdminGoodsService : Base_Service, IAdminGoodsService
    {
        private readonly Good_EFCore _good_EFCore;
        private readonly GoodsCategory_EFCore _category_EFCore;
        private readonly AuditRecord_EFCore _audit_EFCore;
        private readonly GoodsSku_EFCore _sku_EFCore;
        private readonly MerchantGood_EFCore _merchantGoods_EFCore;
        private readonly _111Context _db;

        public AdminGoodsService(
            Good_EFCore good_EFCore,
            GoodsCategory_EFCore category_EFCore,
            AuditRecord_EFCore audit_EFCore,
            GoodsSku_EFCore sku_EFCore,
            MerchantGood_EFCore merchantGoods_EFCore,
            _111Context db)
        {
            _good_EFCore = good_EFCore;
            _category_EFCore = category_EFCore;
            _audit_EFCore = audit_EFCore;
            _sku_EFCore = sku_EFCore;
            _merchantGoods_EFCore = merchantGoods_EFCore;
            _db = db;
        }

        public Task<Api_Response> GetList(int page, int size, string? keyword, int? auditStatus = null, string? categoryId = null, string? merchantId = null)
        {
            int total;

            // 如果按商户过滤，先取出该商户在 merchant_goods 中关联的商品 ID 集合
            var allowedGoodsIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(merchantId))
            {
                allowedGoodsIds = _merchantGoods_EFCore
                    .Query(sg => sg.MerchantId == merchantId)
                    .Select(sg => sg.GoodsId)
                    .Where(id => id != null)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            var goods = _good_EFCore
                .Query(out total, page, size, false, g => g.CreateTime,
                    g => (string.IsNullOrEmpty(keyword) || (g.GoodsName != null && g.GoodsName.Contains(keyword)))
                      && (auditStatus == null || g.AuditStatus == auditStatus)
                      && (string.IsNullOrEmpty(categoryId) || g.CategoryId == categoryId)
                      && (string.IsNullOrEmpty(merchantId) || (g.Id != null && allowedGoodsIds.Contains(g.Id))))
                .ToList();

            // 商户不再挂在 good 表，改由 merchant_goods 关联查询，并带出商户名称
            var goodsIds = goods.Select(g => g.Id).ToList();
            var sgRows = _merchantGoods_EFCore
                .Query(sg => sg.GoodsId != null && goodsIds.Contains(sg.GoodsId))
                .ToList();
            var merchantIds = sgRows
                .Select(sg => sg.MerchantId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => id!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var supNameMap = _db.Merchants
                .Where(s => merchantIds.Contains(s.Id))
                .ToDictionary(s => s.Id!, s => s.MerchantName, StringComparer.OrdinalIgnoreCase);
            var merchantMap = sgRows
                .GroupBy(sg => sg.GoodsId!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var list = goods.Select(g =>
            {
                merchantMap.TryGetValue(g.Id!, out var sg);
                var supName = (sg?.MerchantId != null && supNameMap.TryGetValue(sg.MerchantId!, out var nm)) ? nm : null;
                return new
                {
                    g.Id,
                    g.CategoryId,
                    g.GoodsName,
                    g.Price,
                    g.SalePrice,
                    g.Stock,
                    g.CoverImg,
                    g.ImgList,
                    g.Unit,
                    g.AuditStatus,
                    g.AuditId,
                    g.IsSale,
                    g.CreateTime,
                    g.UpdateTime,
                    MerchantId = sg?.MerchantId,
                    MerchantName = supName
                };
            }).ToList();
            return Task.FromResult(Ok("ok", new { total, list }));
        }

        public Task<Api_Response> Get(string id)
        {
            var good = _good_EFCore.Find(id);
            if (good == null) return Task.FromResult(Fail("商品不存在"));
            var skus = _sku_EFCore.Query(s => s.GoodsId == id).ToList();
            // 规格组：优先取 goods_spec（商户端维护的规格组）；老数据回退为「按 sku.spec_id 合成」
            var specRows = _db.GoodsSpecs.Where(s => s.GoodsId == id).OrderBy(s => s.Sort).ToList();
            var specs = specRows.Count > 0
                ? specRows
                : skus
                    .Where(s => !string.IsNullOrEmpty(s.SpecId))
                    .GroupBy(s => s.SpecId)
                    .Select(g => new GoodsSpec
                    {
                        Id = g.Key,
                        GoodsId = id,
                        SpecName = g.First().SkuName
                    })
                    .ToList();
            var merchant = _merchantGoods_EFCore.Query(sg => sg.GoodsId == id).FirstOrDefault();
            var merchantName = merchant?.MerchantId != null
                ? _db.Merchants.FirstOrDefault(s => s.Id == merchant.MerchantId)?.MerchantName
                : null;
            return Task.FromResult(Ok("ok", new { good, skus, specs, merchantId = merchant?.MerchantId, merchantName }));
        }

        public Task<Api_Response> Add(GoodCreateDto dto)
        {
            var id = Config.NewSequentialGuid();
            // 统一多张封面图源：优先用 img_urls 数组，回退到 cover_img/img_list 逗号字符串
            var imgUrls = NormalizeImgUrls(dto.img_urls, dto.cover_img, dto.img_list);
            var good = new Good
            {
                Id = id,
                CategoryId = dto.category_id,
                GoodsName = dto.goods_name,
                Price = dto.price ?? 0,
                SalePrice = dto.sale_price ?? 0,
                // 方案 A：有规格时总库存强制 = 各规格库存之和（真相来源）；无规格才用前端手填值
                Stock = (dto.skus != null && dto.skus.Count > 0)
                    ? dto.skus.Sum(s => s.stock ?? 0)
                    : (dto.stock ?? 0),
                CoverImg = imgUrls.FirstOrDefault(),
                ImgList = string.Join(",", imgUrls),
                Unit = dto.unit,
                AuditStatus = null,
                IsSale = 0,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            _good_EFCore.Add(good);

            if (dto.skus != null)
            {
                foreach (var sku in dto.skus)
                {
                    var specId = Config.NewSequentialGuid();
                    // 为每个 SKU 生成独立 SpecId；规格组名由商户端维护于 goods_spec 表，读取时若该表为空则按 SpecId 合成
                    _sku_EFCore.Add(new GoodsSku
                    {
                        Id = Config.NewSequentialGuid(),
                        GoodsId = id,
                        SpecId = specId,
                        SkuName = sku.sku_name,
                        Price = sku.price ?? 0,
                        Stock = sku.stock ?? 0,
                        CreateTime = DateTime.Now
                    });
                }
            }

            // 新增商品时同步创建仓库库存记录，确保库存管理页可见且一致
            SyncStockRecordFromGood(good);
            // 同步商户-商品关联（merchant_goods），保证按商户筛选能命中该商品
            SyncMerchantGood(id, dto.merchant_id);

            // 商品/SKU/图片与仓库库存可能分属不同上下文跟踪，分别保存
            var result1 = _good_EFCore.SaveChanges();
            var result2 = _db.SaveChanges();
            return Task.FromResult(Get_Result(result1 + result2));
        }

        public Task<Api_Response> Update(GoodModifyDto dto)
        {
            var good = _good_EFCore.Find(dto.id);
            if (good == null) return Task.FromResult(Fail("商品不存在"));
            // 统一多张封面图源：优先用 img_urls 数组，回退到 cover_img/img_list 逗号字符串
            var imgUrls = NormalizeImgUrls(dto.img_urls, dto.cover_img, dto.img_list);
            good.CategoryId = dto.category_id;
            good.GoodsName = dto.goods_name;
            good.Price = dto.price ?? good.Price;
            good.SalePrice = dto.sale_price ?? good.SalePrice;
            // 库存在商品页不可更改：有多规格时仍按各规格库存之和汇总，否则保持原值（前端手填值不生效）
            good.Stock = (dto.skus != null && dto.skus.Count > 0)
                ? dto.skus.Sum(s => s.stock ?? 0)
                : good.Stock;
            good.CoverImg = imgUrls.FirstOrDefault() ?? good.CoverImg;
            good.ImgList = string.Join(",", imgUrls);
            good.Unit = dto.unit;
            good.UpdateTime = DateTime.Now;
            _good_EFCore.Update(good);

            // 同步 SKU 规格：有传入 skus 时，先删除旧的再重新写入
            if (dto.skus != null)
            {
                foreach (var old in _sku_EFCore.Query(s => s.GoodsId == dto.id).ToList())
                    _sku_EFCore.Delete(old);
                foreach (var sku in dto.skus)
                {
                    var specId = Config.NewSequentialGuid();
                    // 为每个 SKU 生成独立 SpecId；规格组名由商户端维护于 goods_spec 表，读取时若该表为空则按 SpecId 合成
                    _sku_EFCore.Add(new GoodsSku
                    {
                        Id = Config.NewSequentialGuid(),
                        GoodsId = dto.id,
                        SpecId = specId,
                        SkuName = sku.sku_name,
                        Price = sku.price ?? 0,
                        Stock = sku.stock ?? 0,
                        CreateTime = DateTime.Now
                    });
                }
            }

            // 同步仓库库存记录：让商品库存与库存管理页保持一致
            SyncStockRecordFromGood(good);

            // 商品/SKU/图片与仓库库存可能分属不同上下文跟踪，分别保存
            var result1 = _good_EFCore.SaveChanges();
            var result2 = _db.SaveChanges();
            return Task.FromResult(Get_Result(result1 + result2));
        }

        /// <summary>
        /// 商品保存时同步商户-商品关联（merchant_goods）：
        /// 先清掉该商品已有的关联，再按当前商户写一条；商户为空则只清理不写入。
        /// </summary>
        private void SyncMerchantGood(string? goodsId, string? merchantId)
        {
            if (string.IsNullOrEmpty(goodsId)) return;
            foreach (var old in _merchantGoods_EFCore.Query(sg => sg.GoodsId == goodsId).ToList())
                _merchantGoods_EFCore.Delete(old);
            if (string.IsNullOrEmpty(merchantId)) return;
            _merchantGoods_EFCore.Add(new MerchantGood
            {
                Id = Config.NewSequentialGuid(),
                MerchantId = merchantId,
                GoodsId = goodsId,
                CreateTime = DateTime.Now
            });
        }

        /// <summary>
        /// 把可能来自 img_urls/cover_img/img_list 的多张封面图统一成 URL 数组并去重、去空。
        /// </summary>
        private static List<string> NormalizeImgUrls(List<string>? imgUrls, string? coverImg, string? imgList)
        {
            var result = new List<string>();
            if (imgUrls != null)
                result.AddRange(imgUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()));
            if (result.Count == 0)
            {
                var commaSources = new[] { coverImg, imgList };
                foreach (var src in commaSources)
                {
                    if (!string.IsNullOrWhiteSpace(src))
                        result.AddRange(src.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)));
                }
            }
            return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// 商品保存时同步仓库库存记录：以商品库存为基准，让库存管理页与商品页保持一致。
        /// 多仓库时以最早一条记录吸收差额，避免误删其他仓库数据。
        /// </summary>
        private void SyncStockRecordFromGood(Good good)
        {
            if (good == null || string.IsNullOrWhiteSpace(good.Id)) return;

            var targetAvailable = good.Stock;
            var stocks = _db.Stocks.Where(s => s.GoodsId == good.Id).OrderBy(s => s.CreateTime).ToList();

            if (!stocks.Any())
            {
                // 无库存记录时，以商品库存为基准新建一条默认仓库记录
                _db.Stocks.Add(new Stock
                {
                    Id = Config.NewSequentialGuid(),
                    GoodsId = good.Id,
                    Warehouse = null,
                    TotalStock = targetAvailable,
                    LockStock = 0,
                    CreateTime = DateTime.Now
                });
                return;
            }

            var currentAvailable = stocks.Sum(s => (s.TotalStock ?? 0) - (s.LockStock ?? 0));
            var diff = targetAvailable - currentAvailable;

            if (diff != 0)
            {
                var first = stocks.First();
                var newTotal = (first.TotalStock ?? 0) + diff;
                if (newTotal < (first.LockStock ?? 0)) newTotal = first.LockStock ?? 0;
                first.TotalStock = newTotal;
                first.UpdateTime = DateTime.Now;
                _db.Stocks.Update(first);
            }

            // 单 SKU 商品时，SKU 库存也与商品库存保持一致
            var skus = _db.GoodsSkus.Where(s => s.GoodsId == good.Id).ToList();
            if (skus.Count == 1 && skus[0].Stock != targetAvailable)
            {
                skus[0].Stock = targetAvailable;
                _db.GoodsSkus.Update(skus[0]);
            }
        }

        public Task<Api_Response> Remove(string id)
        {
            var good = _good_EFCore.Find(id);
            if (good == null) return Task.FromResult(Fail("商品不存在"));
            _good_EFCore.Delete(good);
            var result = _good_EFCore.SaveChanges();
            return Task.FromResult(Get_Result(result));
        }

        public Task<Api_Response> SubmitAudit(string id)
        {
            var good = _good_EFCore.Find(id);
            if (good == null) return Task.FromResult(Fail("商品不存在"));
            // 已通过(1) 或待审核(0) 不允许重复提交；未提交(null) / 已驳回(2) 允许提交或重新提交
            if (good.AuditStatus == 0)
                return Task.FromResult(Fail("商品正在审核中，无需重复提交"));
            if (good.AuditStatus == 1)
                return Task.FromResult(Fail("商品已通过审核，无需重复提交"));
            good.AuditStatus = 0;
            good.UpdateTime = DateTime.Now;
            _good_EFCore.Update(good);
            var result = _good_EFCore.SaveChanges();
            return Task.FromResult(Get_Result(result));
        }

        public Task<Api_Response> Audit(AuditHandleDto dto)
        {
            var good = _good_EFCore.Find(dto.id);
            if (good == null) return Task.FromResult(Fail("商品不存在"));
            good.AuditStatus = dto.audit_status;
            good.UpdateTime = DateTime.Now;
            // 审核结果与上架状态联动：通过=上架、驳回=下架
            // 数据库只跟随审核流程，不允许出现"待审核+上架"或"已驳回+上架"的错乱组合
            if (dto.audit_status == 1)
                good.IsSale = 1;
            else if (dto.audit_status == 2)
                good.IsSale = 0;

            var rec = new AuditRecord
            {
                Id = Config.NewSequentialGuid(),
                BizType = dto.biz_type ?? "商品",
                BizId = dto.id,
                AuditStatus = dto.audit_status,
                AuditRemark = dto.audit_remark,
                Auditor = dto.auditor,
                CreateTime = DateTime.Now
            };
            good.AuditId = rec.Id;
            _good_EFCore.Update(good);
            _audit_EFCore.Add(rec);
            var result = _good_EFCore.SaveChanges();
            return Task.FromResult(Get_Result(result));
        }

        public Task<Api_Response> SetSale(string id, int isSale)
        {
            var good = _good_EFCore.Find(id);
            if (good == null) return Task.FromResult(Fail("商品不存在"));
            good.IsSale = isSale;
            good.UpdateTime = DateTime.Now;
            _good_EFCore.Update(good);
            var result = _good_EFCore.SaveChanges();
            return Task.FromResult(Get_Result(result));
        }
    }
}
