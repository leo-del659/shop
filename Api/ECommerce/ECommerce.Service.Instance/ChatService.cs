using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 买卖家聊天服务实现（用户端小程序 ↔ 商户端小程序）。
    /// 会话键为 (clientId, merchantId)；两端未读状态独立记录，
    /// 拉取消息时把对端发来的消息置为已读。
    /// </summary>
    [Inject_]
    public class ChatService : Base_Service, IChatService
    {
        /// <summary>发送方类型：买家</summary>
        private const int SenderClient = 1;
        /// <summary>发送方类型：商户</summary>
        private const int SenderMerchant = 2;

        private readonly _111Context _db;
        private readonly ECommerce.Service.Interface.WebSockets.ChatWebSocketManager _wsManager;

        public ChatService(_111Context db, ECommerce.Service.Interface.WebSockets.ChatWebSocketManager wsManager)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _wsManager = wsManager ?? throw new ArgumentNullException(nameof(wsManager));
        }

        #region 买家侧

        public async Task<Api_Response> ClientConversations(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return Fail("用户未登录");

            var rows = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.ClientId == clientId)
                .GroupBy(m => m.MerchantId)
                .Select(g => new
                {
                    MerchantId = g.Key,
                    LastTime = g.Max(x => x.CreateTime)
                })
                .ToListAsync();

            if (rows.Count == 0)
                return Ok("ok", new List<object>());

            var merchantIds = rows.Select(r => r.MerchantId!).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var merchants = await _db.Merchants.AsNoTracking()
                .Where(s => merchantIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s);

            // 每个会话取最后一条消息与未读数
            var result = new List<object>();
            foreach (var row in rows.OrderByDescending(r => r.LastTime))
            {
                var mid = row.MerchantId ?? string.Empty;
                var last = await _db.ChatMessages.AsNoTracking()
                    .Where(m => m.ClientId == clientId && m.MerchantId == mid)
                    .OrderByDescending(m => m.CreateTime)
                    .FirstOrDefaultAsync();
                var unread = await _db.ChatMessages.AsNoTracking()
                    .CountAsync(m => m.ClientId == clientId && m.MerchantId == mid
                        && m.SenderType == SenderMerchant && m.ClientRead == 0);

                merchants.TryGetValue(mid, out var merchant);
                result.Add(new
                {
                    peerId = mid,
                    peerName = merchant?.MerchantName ?? "商家",
                    peerAvatar = merchant?.Avatar ?? string.Empty,
                    lastContent = last?.Content ?? string.Empty,
                    lastTime = FormatTime(last?.CreateTime),
                    unread
                });
            }

            return Ok("ok", result);
        }

        public async Task<Api_Response> ClientMessages(string clientId, string merchantId, string? after, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return Fail("用户未登录");
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商家不存在");

            var merchant = await _db.Merchants.AsNoTracking().FirstOrDefaultAsync(s => s.Id == merchantId);
            if (merchant == null)
                return Fail("商家不存在");

            var list = await QueryMessagesAsync(clientId, merchantId, after, pageSize);

            // 买家拉取即已读：把商户发来的未读消息置为已读
            await MarkReadAsync(clientId, merchantId, readByClient: true);

            return Ok("ok", new
            {
                peer = new
                {
                    peerId = merchant.Id,
                    peerName = merchant.MerchantName ?? "商家",
                    peerAvatar = merchant.Avatar ?? string.Empty
                },
                messages = list
            });
        }

        public async Task<Api_Response> ClientSend(string clientId, string merchantId, string content)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return Fail("用户未登录");
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商家不存在");

            var trimmed = (content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                return Fail("消息内容不能为空");
            if (trimmed.Length > 500)
                return Fail("消息内容过长");

            var merchant = await _db.Merchants.AsNoTracking().FirstOrDefaultAsync(s => s.Id == merchantId);
            if (merchant == null)
                return Fail("商家不存在");

            var msg = new ChatMessage
            {
                Id = Config.NewSequentialGuid(),
                ClientId = clientId,
                MerchantId = merchantId,
                SenderType = SenderClient,
                Content = trimmed,
                ClientRead = 1,
                MerchantRead = 0,
                CreateTime = DateTime.Now
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            // 实时推送给会话两端（买家 + 商户）的在线 WebSocket 连接。
            // 两端收到的 self 含义相反（"是否本人发出"），因此分别构造负载。
            _ = _wsManager.BroadcastAsync(
                clientId,
                merchantId,
                new { type = "msg", message = ToMessageDto(msg, selfSide: SenderClient) },
                new { type = "msg", message = ToMessageDto(msg, selfSide: SenderMerchant) });

            return Ok("ok", ToMessageDto(msg, selfSide: SenderClient));
        }

        public async Task<Api_Response> MerchantBrief(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商家不存在");

            var merchant = await _db.Merchants.AsNoTracking().FirstOrDefaultAsync(s => s.Id == merchantId);
            if (merchant == null)
                return Fail("商家不存在");

            return Ok("ok", new
            {
                peerId = merchant.Id,
                peerName = merchant.MerchantName ?? "商家",
                peerAvatar = merchant.Avatar ?? string.Empty
            });
        }

        #endregion

        #region 商户侧

        public async Task<Api_Response> MerchantConversations(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商户未登录");

            var rows = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.MerchantId == merchantId)
                .GroupBy(m => m.ClientId)
                .Select(g => new
                {
                    ClientId = g.Key,
                    LastTime = g.Max(x => x.CreateTime)
                })
                .ToListAsync();

            if (rows.Count == 0)
                return Ok("ok", new List<object>());

            var clientIds = rows.Select(r => r.ClientId!).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            // 买家展示名优先取 client_login（含用户名称/头像），缺失时回退 sys_users 账号
            var clientLogins = await _db.ClientLogins.AsNoTracking()
                .Where(c => c.ClientId != null && clientIds.Contains(c.ClientId))
                .ToListAsync();
            var sysUsers = await _db.SysUsers.AsNoTracking()
                .Where(u => clientIds.Contains(u.UId))
                .ToDictionaryAsync(u => u.UId, u => u);

            var result = new List<object>();
            foreach (var row in rows.OrderByDescending(r => r.LastTime))
            {
                var cid = row.ClientId ?? string.Empty;
                var last = await _db.ChatMessages.AsNoTracking()
                    .Where(m => m.ClientId == cid && m.MerchantId == merchantId)
                    .OrderByDescending(m => m.CreateTime)
                    .FirstOrDefaultAsync();
                var unread = await _db.ChatMessages.AsNoTracking()
                    .CountAsync(m => m.ClientId == cid && m.MerchantId == merchantId
                        && m.SenderType == SenderClient && m.MerchantRead == 0);

                var login = clientLogins.FirstOrDefault(c => c.ClientId == cid);
                sysUsers.TryGetValue(cid, out var user);
                var name = !string.IsNullOrWhiteSpace(login?.Name) ? login!.Name
                    : user?.UName ?? user?.UAccount ?? "买家";

                result.Add(new
                {
                    peerId = cid,
                    peerName = name,
                    peerAvatar = login?.Avatar ?? string.Empty,
                    lastContent = last?.Content ?? string.Empty,
                    lastTime = FormatTime(last?.CreateTime),
                    unread
                });
            }

            return Ok("ok", result);
        }

        public async Task<Api_Response> MerchantMessages(string merchantId, string clientId, string? after, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商户未登录");
            if (string.IsNullOrWhiteSpace(clientId))
                return Fail("买家不存在");

            var list = await QueryMessagesAsync(clientId, merchantId, after, pageSize);

            // 商户拉取即已读：把买家发来的未读消息置为已读
            await MarkReadAsync(clientId, merchantId, readByClient: false);

            // 买家展示信息
            var login = await _db.ClientLogins.AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
            var user = await _db.SysUsers.AsNoTracking().FirstOrDefaultAsync(u => u.UId == clientId);
            var name = !string.IsNullOrWhiteSpace(login?.Name) ? login!.Name
                : user?.UName ?? user?.UAccount ?? "买家";

            return Ok("ok", new
            {
                peer = new
                {
                    peerId = clientId,
                    peerName = name,
                    peerAvatar = login?.Avatar ?? string.Empty
                },
                messages = list
            });
        }

        public async Task<Api_Response> MerchantSend(string merchantId, string clientId, string content)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return Fail("商户未登录");
            if (string.IsNullOrWhiteSpace(clientId))
                return Fail("买家不存在");

            var trimmed = (content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                return Fail("消息内容不能为空");
            if (trimmed.Length > 500)
                return Fail("消息内容过长");

            var msg = new ChatMessage
            {
                Id = Config.NewSequentialGuid(),
                ClientId = clientId,
                MerchantId = merchantId,
                SenderType = SenderMerchant,
                Content = trimmed,
                ClientRead = 0,
                MerchantRead = 1,
                CreateTime = DateTime.Now
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            // 实时推送给会话两端（买家 + 商户）的在线 WebSocket 连接。
            // 两端收到的 self 含义相反（"是否本人发出"），因此分别构造负载。
            _ = _wsManager.BroadcastAsync(
                clientId,
                merchantId,
                new { type = "msg", message = ToMessageDto(msg, selfSide: SenderClient) },
                new { type = "msg", message = ToMessageDto(msg, selfSide: SenderMerchant) });

            return Ok("ok", ToMessageDto(msg, selfSide: SenderMerchant));
        }

        #endregion

        #region 私有辅助

        /// <summary>
        /// 拉取某会话消息：after 非空时返回该时间之后的增量（升序），
        /// 否则返回最近 pageSize 条（升序返回，前端可直接渲染）。
        /// </summary>
        private async Task<List<object>> QueryMessagesAsync(string clientId, string merchantId, string? after, int pageSize)
        {
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _db.ChatMessages.AsNoTracking()
                .Where(m => m.ClientId == clientId && m.MerchantId == merchantId);

            var afterTime = ParseTime(after);
            List<ChatMessage> rows;
            if (afterTime.HasValue)
            {
                rows = await query
                    .Where(m => m.CreateTime > afterTime.Value)
                    .OrderBy(m => m.CreateTime)
                    .ToListAsync();
            }
            else
            {
                var desc = await query
                    .OrderByDescending(m => m.CreateTime)
                    .Take(pageSize)
                    .ToListAsync();
                rows = desc.AsEnumerable().Reverse().ToList();
            }

            return rows.Select(m => ToMessageDto(m, selfSide: -1)).ToList();
        }

        /// <summary>把对端发来的未读消息置为已读</summary>
        private async Task MarkReadAsync(string clientId, string merchantId, bool readByClient)
        {
            List<ChatMessage> unread;
            if (readByClient)
            {
                unread = await _db.ChatMessages
                    .Where(m => m.ClientId == clientId && m.MerchantId == merchantId
                        && m.SenderType == SenderMerchant && m.ClientRead == 0)
                    .ToListAsync();
                foreach (var m in unread) m.ClientRead = 1;
            }
            else
            {
                unread = await _db.ChatMessages
                    .Where(m => m.ClientId == clientId && m.MerchantId == merchantId
                        && m.SenderType == SenderClient && m.MerchantRead == 0)
                    .ToListAsync();
                foreach (var m in unread) m.MerchantRead = 1;
            }
            if (unread.Count > 0)
                await _db.SaveChangesAsync();
        }

        /// <summary>消息转前端结构（selfSide 传入当前查看方以便前端判断左右气泡；-1 表示由前端按 senderType 判断）</summary>
        private object ToMessageDto(ChatMessage m, int selfSide)
        {
            return new
            {
                id = m.Id,
                clientId = m.ClientId,
                merchantId = m.MerchantId,
                senderType = m.SenderType,
                content = m.Content ?? string.Empty,
                createTime = FormatTime(m.CreateTime),
                self = selfSide > 0 ? m.SenderType == selfSide : (bool?)null
            };
        }

        /// <summary>时间统一格式化为 yyyy-MM-dd HH:mm:ss（前端自行裁剪展示）</summary>
        private static string FormatTime(DateTime? time)
        {
            if (!time.HasValue) return string.Empty;
            return time.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>解析前端传入的时间字符串（兼容 yyyy-MM-dd HH:mm:ss / ISO）</summary>
        private static DateTime? ParseTime(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (DateTime.TryParse(text, out var t)) return t;
            return null;
        }

        #endregion
    }
}
