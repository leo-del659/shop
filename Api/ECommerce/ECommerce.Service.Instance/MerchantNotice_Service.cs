using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 商户通知服务实现：库存预警自动生成 + 未读数统计。
    /// 触发时机：商户进入消息中心时扫描一次库存，低库存商品自动生成一条通知；
    /// 同���商品在库存没有变化前不会重复生成（用 stock_snapshot 去重）。
    /// </summary>
    [Inject_]
    public class MerchantNotice_Service : Base_Service, IMerchantNoticeService
    {
        /// <summary>通知类型：库存预警</summary>
        private const int NoticeTypeStock = 1;

        /// <summary>默认库存预警阈值（可配置：Merchant:StockWarningThreshold）</summary>
        private const int DefaultStockThreshold = 5;

        private readonly _111Context _db;
        private readonly IConfiguration _config;

        public MerchantNotice_Service(_111Context db, IConfiguration config)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _config = config;
        }

        /// <summary>生成库存预警并返回通知列表（进入消息中心时调用）</summary>
        public async Task<Api_Response> MerchantNotices(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商户未登录");

            await GenerateStockWarningsAsync(merchantId);

            var list = await _db.MerchantNotices.AsNoTracking()
                .Where(n => n.MerchantId == merchantId)
                .OrderByDescending(n => n.CreateTime)
                .Take(20)
                .ToListAsync();

            var unread = await _db.MerchantNotices.AsNoTracking()
                .CountAsync(n => n.MerchantId == merchantId && n.IsRead == 0);

            return Ok("ok", new
            {
                notices = list.Select(n => new
                {
                    id = n.Id,
                    noticeType = n.NoticeType,
                    typeText = NoticeTypeText(n.NoticeType),
                    title = n.Title ?? string.Empty,
                    content = n.Content ?? string.Empty,
                    relatedId = n.RelatedId ?? string.Empty,
                    stock = n.StockSnapshot,
                    isRead = n.IsRead,
                    createTime = n.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                }).ToList(),
                unread
            });
        }

        /// <summary>全部标记已读</summary>
        public async Task<Api_Response> ReadAll(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商户未登录");

            var unread = await _db.MerchantNotices
                .Where(n => n.MerchantId == merchantId && n.IsRead == 0)
                .ToListAsync();
            foreach (var n in unread) n.IsRead = 1;
            if (unread.Count > 0)
                await _db.SaveChangesAsync();

            return Ok("ok", new { updated = unread.Count });
        }

        /// <summary>未读数汇总：聊天未读 + 通知未读（首页消息按钮红点）</summary>
        public async Task<Api_Response> UnreadCount(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商户未登录");

            var chat = await _db.ChatMessages.AsNoTracking()
                .CountAsync(m => m.MerchantId == merchantId && m.MerchantRead == 0 && m.SenderType == 1);
            var notice = await _db.MerchantNotices.AsNoTracking()
                .CountAsync(n => n.MerchantId == merchantId && n.IsRead == 0);

            return Ok("ok", new { chat, notice, total = chat + notice });
        }

        #region 私有辅助

        /// <summary>
        /// 扫描该商户在售商品，把库存低于阈值的商品自动生成库存预警通知。
        /// 去重规则：同一商品在库存数值未变化前不重复生成（避免每次进入消息中心刷屏）。
        /// </summary>
        private async Task GenerateStockWarningsAsync(string merchantId)
        {
            var threshold = StockThreshold();

            var lowGoods = await (from mg in _db.MerchantGood.AsNoTracking()
                                  join g in _db.Goods.AsNoTracking() on mg.GoodsId equals g.Id
                                  where mg.MerchantId == merchantId && g.Stock <= threshold
                                  select new { g.Id, g.GoodsName, g.Stock })
                                .Distinct()
                                .ToListAsync();
            if (lowGoods.Count == 0) return;

            var relatedIds = lowGoods.Select(x => x.Id).ToList();
            var existed = await _db.MerchantNotices.AsNoTracking()
                .Where(n => n.MerchantId == merchantId
                            && n.NoticeType == NoticeTypeStock
                            && n.RelatedId != null
                            && relatedIds.Contains(n.RelatedId))
                .ToListAsync();

            // 每个商品取最新一条预警做快照对比
            var latest = existed
                .GroupBy(n => n.RelatedId)
                .Select(g => new
                {
                    RelatedId = g.Key!,
                    Snapshot = g.OrderByDescending(n => n.CreateTime).First().StockSnapshot
                })
                .ToDictionary(x => x.RelatedId, x => x.Snapshot);

            var now = DateTime.Now;
            var added = new System.Collections.Generic.List<MerchantNotice>();
            foreach (var item in lowGoods)
            {
                // 库存没变化 → 已有预警仍然有效，不再重复生成
                if (latest.TryGetValue(item.Id, out var snapshot) && snapshot == item.Stock)
                    continue;

                added.Add(new MerchantNotice
                {
                    Id = Config.NewSequentialGuid(),
                    MerchantId = merchantId,
                    NoticeType = NoticeTypeStock,
                    Title = item.Stock <= 0 ? "商品已售罄" : "库存预警",
                    Content = $"商品《{item.GoodsName}》当前库存仅剩 {item.Stock} 件，请及时补货。",
                    RelatedId = item.Id,
                    StockSnapshot = item.Stock,
                    IsRead = 0,
                    CreateTime = now
                });
            }

            if (added.Count > 0)
            {
                _db.MerchantNotices.AddRange(added);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>库存预警阈值：配置项 Merchant:StockWarningThreshold，缺省 5</summary>
        private int StockThreshold()
        {
            var raw = _config?["Merchant:StockWarningThreshold"];
            if (int.TryParse(raw, out var v) && v >= 0) return v;
            return DefaultStockThreshold;
        }

        /// <summary>通知类型文案</summary>
        private static string NoticeTypeText(int type)
        {
            return type switch
            {
                1 => "库存预警",
                2 => "订单提醒",
                _ => "通知",
            };
        }

        #endregion
    }
}
