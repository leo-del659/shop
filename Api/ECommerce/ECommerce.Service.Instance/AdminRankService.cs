using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 排行榜服务实现。
    /// 商户(卖家)销售排行口径与 GoodsService.ComputeSoldCountsAsync 完全一致：
    /// 已支付(pay_status=1) + 未取消(order_status!=9) + 未作废(is_ban!=1)。
    /// 订单表的 MerchantId 即「供货商户ID」（seller 维度），MerchantGood.MerchantId 同理。
    /// </summary>
    [Inject_]
    public class AdminRankService : Base_Service, IAdminRankService
    {
        private readonly Merchant_EFCore _merchant_EFCore;
        private readonly MerchantGood_EFCore _merchantGood_EFCore;
        private readonly ClientOrder_EFCore _clientOrder_EFCore;
        private readonly MerchantLevel_EFCore _level_EFCore;

        public AdminRankService(
            Merchant_EFCore merchant_EFCore,
            MerchantGood_EFCore merchantGood_EFCore,
            ClientOrder_EFCore clientOrder_EFCore,
            MerchantLevel_EFCore level_EFCore)
        {
            _merchant_EFCore = merchant_EFCore;
            _merchantGood_EFCore = merchantGood_EFCore;
            _clientOrder_EFCore = clientOrder_EFCore;
            _level_EFCore = level_EFCore;
        }

        public Task<Api_Response> MerchantRank(int top = 10)
        {
            if (top <= 0) top = 10;
            if (top > 200) top = 200;

            try
            {
                // 1) 取出有效订单的「商户ID + 金额」（口径同商品销量统计）
                var validOrders = _clientOrder_EFCore
                    .Query(o => o.PayStatus == PayStatus.Paid && o.OrderStatus != OrderStatus.Cancelled && (o.IsBan == null || o.IsBan != 1))
                    .Select(o => new { o.MerchantId, o.TotalAmount })
                    .ToList();

                // 2) 按供货商户聚合：订单数 / 销售额
                var statMap = validOrders
                    .Where(o => !string.IsNullOrEmpty(o.MerchantId))
                    .GroupBy(o => o.MerchantId!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            OrderCount = g.Count(),
                            TotalAmount = Math.Round(g.Sum(x => x.TotalAmount ?? 0d), 2)
                        },
                        StringComparer.OrdinalIgnoreCase);

                // 3) 各商户在售商品数（merchant_good 关联）
                var goodsCountMap = _merchantGood_EFCore
                    .Query(sg => sg.MerchantId != null)
                    .Select(sg => sg.MerchantId!)
                    .ToList()
                    .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

                // 4) 商户等级 ID → 名称（列表展示用）
                var levelMap = _level_EFCore
                    .Query()
                    .Select(l => new { l.Id, l.LevelName })
                    .ToList()
                    .ToDictionary(x => x.Id, x => x.LevelName ?? string.Empty, StringComparer.OrdinalIgnoreCase);

                // 5) 全量商户（含 0 单），按销售额降序 → 订单数降序 → 名称升序
                var list = _merchant_EFCore
                    .Query()
                    .ToList()
                    .Select(m =>
                    {
                        statMap.TryGetValue(m.Id ?? string.Empty, out var st);
                        goodsCountMap.TryGetValue(m.Id ?? string.Empty, out var gc);
                        // 等级名称：无条件调用 TryGetValue，保证 out 变量一定被赋值
                        levelMap.TryGetValue(m.Id ?? string.Empty, out var levelName);
                        return new
                        {
                            merchantId = m.Id,
                            merchantName = m.MerchantName,
                            merchantNo = m.Id,
                            levelName = levelName,
                            orderCount = st?.OrderCount ?? 0,
                            totalAmount = st?.TotalAmount ?? 0d,
                            goodsCount = gc
                        };
                    })
                    .OrderByDescending(x => x.totalAmount)
                    .ThenByDescending(x => x.orderCount)
                    .ThenBy(x => x.merchantName ?? string.Empty)
                    .Take(top)
                    .ToList();

                return Task.FromResult(Ok("ok", new { total = list.Count, list }));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Fail("排行榜统计失败：" + ex.Message));
            }
        }
    }
}
