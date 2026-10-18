using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Goods;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class GoodsReviewService : Base_Service, IGoodsReviewService
    {
        private readonly GoodsReview_EFCore _review_EFCore;
        private readonly _111Context _db;

        public GoodsReviewService(GoodsReview_EFCore review_EFCore, _111Context db)
        {
            _review_EFCore = review_EFCore;
            _db = db;
        }

        public async Task<Api_Response> Submit(GoodsReviewCreateDto dto, string memberId)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.goods_id))
                return Fail("商品ID不能为空");
            if (dto.rating < 1 || dto.rating > 5)
                return Fail("评分必须在 1-5 之间");

            var good = await _db.Goods.FirstOrDefaultAsync(g => g.Id == dto.goods_id);
            if (good == null) return Fail("商品不存在");

            string? skuName = null;
            if (!string.IsNullOrWhiteSpace(dto.sku_id))
            {
                var sku = await _db.GoodsSkus
                    .FirstOrDefaultAsync(s => s.Id == dto.sku_id && s.GoodsId == dto.goods_id);
                skuName = sku?.SkuName;
            }

            // 同一订单同一商品只允许评价一次
            if (!string.IsNullOrWhiteSpace(dto.order_id))
            {
                var existed = _review_EFCore
                    .Query(r => r.OrderId == dto.order_id && r.GoodsId == dto.goods_id && r.MemberId == memberId)
                    .FirstOrDefault();
                if (existed != null) return Fail("该商品已评价，无需重复提交");
            }

            // 必须已购买（订单属于当前用户且已支付/已完成）
            if (!string.IsNullOrWhiteSpace(dto.order_id))
            {
                var order = await _db.ClientOrders
                    .FirstOrDefaultAsync(o => o.Id == dto.order_id && o.ClientId == memberId);
                if (order == null) return Fail("订单不存在或不属于当前用户");
                if ((order.OrderStatus ?? OrderStatus.PendingPayment) < OrderStatus.Accepted) return Fail("订单未完成，无法评价");
            }

            // 取当前用户头像作为评价快照（匿名评价不保存头像）
            string? avatar = null;
            if (dto.is_anonymous != 1)
            {
                avatar = await _db.SysUsers
                    .Where(u => u.UId == memberId)
                    .Select(u => u.Avatar)
                    .FirstOrDefaultAsync();

                // 旧登录凭据是 sys_login_log.code，兼容按 Code 找到真实用户再取头像
                if (string.IsNullOrEmpty(avatar))
                {
                    var userId = await _db.SysLoginLogs
                        .Where(l => l.Code == memberId && !string.IsNullOrEmpty(l.UserId))
                        .OrderByDescending(l => l.LoginTime)
                        .Select(l => l.UserId)
                        .FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        avatar = await _db.SysUsers
                            .Where(u => u.UId == userId)
                            .Select(u => u.Avatar)
                            .FirstOrDefaultAsync();
                    }
                }
            }

            _review_EFCore.Add(new GoodsReview
            {
                Id = Config.NewSequentialGuid(),
                GoodsId = good.Id,
                GoodsName = good.GoodsName,
                SkuId = dto.sku_id,
                SkuName = skuName,
                MemberId = memberId,
                MemberName = dto.is_anonymous == 1 ? "匿名用户" : dto.member_name,
                Avatar = dto.is_anonymous == 1 ? null : avatar,
                OrderId = dto.order_id,
                Rating = dto.rating,
                Content = dto.content,
                Images = dto.images,
                Tags = dto.tags,
                ClientReview = dto.client_review,
                IsAnonymous = dto.is_anonymous ?? 0,
                // 默认直接展示,社区团购不做人工审核;若后续启用审核可改为 0 并由 Audit 端点流转 0→1
                Status = 1,
                CreateTime = DateTime.Now
            });

            var result = _review_EFCore.SaveChanges();
            return Get_Result(result);
        }

        public Task<Api_Response> GetList(int page, int size, string? keyword, string? tier, string? tag, int? status, string? goodsId)
        {
            // 只把 EF 能稳定翻译的简单条件放到 IQueryable，复杂过滤放到内存，避免表达式树解析异常
            var query = _review_EFCore.Query(
                r => (string.IsNullOrEmpty(goodsId) || r.GoodsId == goodsId)
                  && (!status.HasValue || r.Status == status.Value)
            );

            var listAll = query.AsEnumerable()
                .Where(r => string.IsNullOrEmpty(keyword) || (!string.IsNullOrEmpty(r.Content) && r.Content.Contains(keyword)))
                .Where(r => string.IsNullOrEmpty(tier)
                    || (tier == "good" && r.Rating >= 4)
                    || (tier == "mid" && r.Rating == 3)
                    || (tier == "bad" && r.Rating <= 2))
                .Where(r => string.IsNullOrEmpty(tag) || (!string.IsNullOrEmpty(r.Tags) && r.Tags.Contains(tag)))
                .OrderByDescending(r => r.CreateTime)
                .ToList();

            int total = listAll.Count;
            int p = page < 1 ? 1 : page;
            var list = listAll.Skip((p - 1) * size).Take(size).ToList();
            return Task.FromResult(Ok("ok", new { total, list }));
        }

        public async Task<Api_Response> GetByGoods(string goodsId, int page, int size, string? tier, string? tag)
        {
            var query = _review_EFCore.Query(r => r.GoodsId == goodsId && r.Status == 1);

            var listAll = query.AsEnumerable()
                .Where(r => string.IsNullOrEmpty(tier)
                    || (tier == "good" && r.Rating >= 4)
                    || (tier == "mid" && r.Rating == 3)
                    || (tier == "bad" && r.Rating <= 2))
                .Where(r => string.IsNullOrEmpty(tag) || (!string.IsNullOrEmpty(r.Tags) && r.Tags.Contains(tag)))
                .OrderByDescending(r => r.CreateTime)
                .ToList();

            // 评价 member_id 可能是 sys_users.u_id（新）也可能是 sys_login_log.code（旧）。
            // 这里统一解析出用户当前头像：匿名评价保持 null，非匿名优先用当前头像，没有再回退快照。
            const string DEFAULT_AVATAR = "/api/file/default-avatar.png";
            var memberIds = listAll
                .Where(r => r.IsAnonymous != 1)
                .Select(r => r.MemberId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var uidAvatars = new Dictionary<string, string?>();
            var codeToUserId = new Dictionary<string, string?>();
            var indirectAvatars = new Dictionary<string, string?>();

            if (memberIds.Any())
            {
                uidAvatars = await _db.SysUsers
                    .Where(u => memberIds.Contains(u.UId))
                    .ToDictionaryAsync(u => u.UId, u => u.Avatar);

                var unresolvedCodes = memberIds.Except(uidAvatars.Keys).ToList();
                if (unresolvedCodes.Any())
                {
                    // 拉到内存再分组，避免 EF Core 翻译 GroupBy+First 的不确定性
                    var logs = await _db.SysLoginLogs
                        .Where(l => unresolvedCodes.Contains(l.Code) && !string.IsNullOrEmpty(l.UserId))
                        .Select(l => new { l.Code, l.UserId, l.LoginTime })
                        .ToListAsync();

                    codeToUserId = logs
                        .GroupBy(l => l.Code)
                        .ToDictionary(
                            g => g.Key,
                            g => (string?)g.OrderByDescending(l => l.LoginTime).First().UserId);

                    var indirectUids = codeToUserId.Values.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                    if (indirectUids.Any())
                    {
                        indirectAvatars = await _db.SysUsers
                            .Where(u => indirectUids.Contains(u.UId))
                            .ToDictionaryAsync(u => u.UId, u => u.Avatar);
                    }
                }
            }

            string? ResolveAvatar(string? memberId)
            {
                if (string.IsNullOrEmpty(memberId)) return null;
                if (uidAvatars.TryGetValue(memberId, out var ava) && !string.IsNullOrEmpty(ava)) return ava;
                if (codeToUserId.TryGetValue(memberId, out var uid) && !string.IsNullOrEmpty(uid)
                    && indirectAvatars.TryGetValue(uid, out ava) && !string.IsNullOrEmpty(ava)) return ava;
                return null;
            }

            int total = listAll.Count;
            int p = page < 1 ? 1 : page;
            var list = listAll.Skip((p - 1) * size).Take(size)
                .Select(r => new
                {
                    r.Id,
                    r.GoodsId,
                    r.GoodsName,
                    r.MemberId,
                    r.MemberName,
                    Avatar = r.IsAnonymous == 1 ? null : (ResolveAvatar(r.MemberId) ?? r.Avatar ?? DEFAULT_AVATAR),
                    r.Rating,
                    r.Content,
                    r.Images,
                    r.Tags,
                    r.Reply,
                    r.ReplyTime,
                    r.CreateTime
                })
                .ToList();
            return Ok("ok", new { total, list });
        }

        public Task<Api_Response> GetOrderReviewed(string orderId, string memberId)
        {
            var list = _review_EFCore
                .Query(r => r.OrderId == orderId && r.MemberId == memberId)
                .Select(r => new { r.GoodsId, r.SkuId })
                .ToList();
            return Task.FromResult(Ok("ok", list));
        }

        public Task<Api_Response> Audit(GoodsReviewAuditDto dto)
        {
            var r = _review_EFCore.Find(dto.id);
            if (r == null) return Task.FromResult(Fail("评价不存在"));
            // 驳回必须填写原因
            if (dto.status == 2 && string.IsNullOrWhiteSpace(dto.reject_reason))
                return Task.FromResult(Fail("驳回评价必须填写驳回原因"));
            r.Status = dto.status;
            r.RejectReason = dto.status == 2 ? dto.reject_reason?.Trim() : null;
            _review_EFCore.Update(r);
            return Task.FromResult(Get_Result(_review_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Reply(GoodsReviewReplyDto dto)
        {
            var r = _review_EFCore.Find(dto.id);
            if (r == null) return Task.FromResult(Fail("评价不存在"));
            r.Reply = dto.reply;
            r.ReplyTime = DateTime.Now;
            if (r.Status == 0) r.Status = 1;
            _review_EFCore.Update(r);
            return Task.FromResult(Get_Result(_review_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Remove(string id)
        {
            var r = _review_EFCore.Find(id);
            if (r == null) return Task.FromResult(Fail("评价不存在"));
            _review_EFCore.Delete(r);
            return Task.FromResult(Get_Result(_review_EFCore.SaveChanges()));
        }
    }
}
