using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class AdminStockService : Base_Service, IAdminStockService
    {
        private readonly Stock_EFCore _stock_EFCore;
        private readonly StockIn_EFCore _stockIn_EFCore;
        private readonly StockOut_EFCore _stockOut_EFCore;
        private readonly Good_EFCore _good_EFCore;
        private readonly GoodsSku_EFCore _sku_EFCore;
        private readonly _111Context _db;

        public AdminStockService(Stock_EFCore stock_EFCore, StockIn_EFCore stockIn_EFCore, StockOut_EFCore stockOut_EFCore, Good_EFCore good_EFCore, GoodsSku_EFCore sku_EFCore, _111Context db)
        {
            _stock_EFCore = stock_EFCore;
            _stockIn_EFCore = stockIn_EFCore;
            _stockOut_EFCore = stockOut_EFCore;
            _good_EFCore = good_EFCore;
            _sku_EFCore = sku_EFCore;
            _db = db;
        }

        public Task<Api_Response> GetList(int page, int size, string? keyword)
        {
            // 按 stock 记录维度展示：每条库存记录一行，便于删除后消失、新增后多出一行
            var query = _db.Stocks.AsNoTracking()
                .Join(_db.Goods.AsNoTracking(),
                    s => s.GoodsId,
                    g => g.Id,
                    (s, g) => new { Stock = s, GoodsName = g.GoodsName })
                .Where(x => string.IsNullOrWhiteSpace(keyword) || (x.GoodsName != null && x.GoodsName.Contains(keyword)));

            var total = query.Count();
            var list = query
                .OrderByDescending(x => x.Stock.CreateTime)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new
                {
                    id = x.Stock.Id,
                    goodsId = x.Stock.GoodsId,
                    goodsName = x.GoodsName,
                    warehouse = string.IsNullOrWhiteSpace(x.Stock.Warehouse) ? "-" : x.Stock.Warehouse,
                    totalStock = x.Stock.TotalStock,
                    lockStock = x.Stock.LockStock,
                    createTime = x.Stock.CreateTime
                })
                .ToList();

            return Task.FromResult(Ok("ok", new { total, list }));
        }

        public Task<Api_Response> Get(string id)
        {
            var t = _db.Stocks.Find(id);
            return Task.FromResult(t == null ? Fail("库存记录不存在") : Ok("ok", t));
        }

        public Task<Api_Response> Add(Stock t)
        {
            var good = _db.Goods.Find(t.GoodsId);
            if (good == null) return Task.FromResult(Fail("商品不存在"));

            // 同一商品 + 仓库已存在则禁止重复添加，避免覆盖/混淆旧数据
            var exists = _db.Stocks.Any(s => s.GoodsId == t.GoodsId && s.Warehouse == t.Warehouse);
            if (exists) return Task.FromResult(Fail("该商品在该仓库已有库存记录，请勿重复添加"));

            t.Id = Config.NewSequentialGuid();
            t.CreateTime = DateTime.Now;

            _db.Stocks.Add(t);
            // 以仓库库存记录为基准回写商品/SKU 库存，保证三处一致
            SyncGoodStockFromRecords(t.GoodsId);
            return Task.FromResult(Get_Result(_db.SaveChanges()));
        }

        public Task<Api_Response> Update(Stock t)
        {
            var old = _db.Stocks.Find(t.Id);
            if (old == null) return Task.FromResult(Fail("库存记录不存在"));

            var good = _db.Goods.Find(old.GoodsId);
            if (good == null) return Task.FromResult(Fail("商品不存在"));

            // 如果变更了商品或仓库，需要校验目标组合是否已存在（排除自身）
            if ((old.GoodsId != t.GoodsId || old.Warehouse != t.Warehouse) &&
                _db.Stocks.Any(s => s.Id != t.Id && s.GoodsId == t.GoodsId && s.Warehouse == t.Warehouse))
                return Task.FromResult(Fail("该商品在该仓库已有库存记录，请勿重复添加"));

            var oldGoodsId = old.GoodsId;

            old.GoodsId = t.GoodsId;
            old.Warehouse = t.Warehouse;
            old.TotalStock = t.TotalStock;
            old.LockStock = t.LockStock;
            old.UpdateTime = DateTime.Now;

            _db.Stocks.Update(old);
            // 以仓库库存记录为基准回写商品/SKU 库存；若商品变更，旧商品也同步
            SyncGoodStockFromRecords(t.GoodsId);
            if (oldGoodsId != t.GoodsId)
                SyncGoodStockFromRecords(oldGoodsId);
            return Task.FromResult(Get_Result(_db.SaveChanges()));
        }

        public Task<Api_Response> Remove(string id)
        {
            var old = _db.Stocks.Find(id);
            if (old == null) return Task.FromResult(Fail("库存记录不存在"));

            var goodsId = old.GoodsId;
            _db.Stocks.Remove(old);
            // 以仓库库存记录为基准回写商品/SKU 库存
            SyncGoodStockFromRecords(goodsId);
            return Task.FromResult(Get_Result(_db.SaveChanges()));
        }

        /// <summary>
        /// 根据仓库库存记录回写商品库存。单 SKU 商品时同时回写 SKU 库存，保证三处一致。
        /// 优先从当前上下文已跟踪的 stock 实体计算，避免 SaveChanges 前读库拿到旧值。
        /// </summary>
        private void SyncGoodStockFromRecords(string goodsId)
        {
            var good = _db.Goods.Find(goodsId);
            if (good == null) return;

            // 先取本地跟踪的库存记录（含新增/修改），排除已标记删除的
            var localStocks = _db.Stocks.Local
                .Where(s => s.GoodsId == goodsId && _db.Entry(s).State != EntityState.Deleted)
                .ToList();

            var available = localStocks.Any()
                ? localStocks.Sum(s => (s.TotalStock ?? 0) - (s.LockStock ?? 0))
                : _db.Stocks
                    .Where(s => s.GoodsId == goodsId)
                    .Sum(s => (s.TotalStock ?? 0) - (s.LockStock ?? 0));

            good.Stock = available;
            good.UpdateTime = DateTime.Now;
            _db.Goods.Update(good);

            // 单 SKU 商品时，把 SKU 库存也同步为仓库可用库存
            var skus = _db.GoodsSkus.Where(s => s.GoodsId == goodsId).ToList();
            if (skus.Count == 1)
            {
                skus[0].Stock = available;
                _db.GoodsSkus.Update(skus[0]);
            }
        }

        public Task<Api_Response> StockIn(string goodsId, string? warehouse, int num, string? remark)
        {
            var good = _db.Goods.Find(goodsId);
            if (good == null) return Task.FromResult(Fail("商品不存在"));

            var stock = _db.Stocks
                .FirstOrDefault(s => s.GoodsId == goodsId && (warehouse == null || s.Warehouse == warehouse));
            if (stock == null)
            {
                stock = new Stock
                {
                    Id = Config.NewSequentialGuid(),
                    GoodsId = goodsId,
                    Warehouse = warehouse,
                    TotalStock = 0,
                    LockStock = 0,
                    CreateTime = DateTime.Now
                };
                _db.Stocks.Add(stock);
            }

            stock.TotalStock = (stock.TotalStock ?? 0) + num;
            stock.UpdateTime = DateTime.Now;

            // 以仓库库存记录为基准回写商品/SKU 库存
            SyncGoodStockFromRecords(goodsId);

            _db.StockIns.Add(new StockIn
            {
                Id = Config.NewSequentialGuid(),
                GoodsId = goodsId,
                InNum = num,
                BatchNo = remark,
                CreateTime = DateTime.Now
            });

            return Task.FromResult(Get_Result(_db.SaveChanges()));
        }

        public Task<Api_Response> StockOut(string goodsId, string? warehouse, int num, string? remark)
        {
            var good = _db.Goods.Find(goodsId);
            if (good == null) return Task.FromResult(Fail("商品不存在"));

            var stock = _db.Stocks
                .FirstOrDefault(s => s.GoodsId == goodsId && (warehouse == null || s.Warehouse == warehouse));
            if (stock == null) return Task.FromResult(Fail("未找到对应库存记录"));
            if ((stock.TotalStock ?? 0) - (stock.LockStock ?? 0) < num) return Task.FromResult(Fail("可用库存不足"));

            stock.TotalStock = (stock.TotalStock ?? 0) - num;
            stock.UpdateTime = DateTime.Now;

            // 以仓库库存记录为基准回写商品/SKU 库存
            SyncGoodStockFromRecords(goodsId);

            _db.StockOuts.Add(new StockOut
            {
                Id = Config.NewSequentialGuid(),
                GoodsId = goodsId,
                OutNum = num,
                BatchNo = remark,
                CreateTime = DateTime.Now
            });

            return Task.FromResult(Get_Result(_db.SaveChanges()));
        }

        public Task<Api_Response> GetWarehouseList()
        {
            var list = _stock_EFCore
                .Query(s => s.Warehouse != null && s.Warehouse != "")
                .Select(s => s.Warehouse!)
                .Distinct()
                .OrderBy(w => w)
                .ToList();
            return Task.FromResult(Ok("ok", list));
        }
    }
}
