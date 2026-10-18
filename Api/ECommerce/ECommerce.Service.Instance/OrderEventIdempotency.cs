using ECommerce.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 订单事件消费幂等助手。
    /// 基于 order_event_log 的 uk_biz_key 唯一索引：
    ///   - 首次处理：插入 status=0 的记录后继续执行业务；
    ///   - 重复投递 / 重试：已存在 status=1 的记录 → 直接跳过（保证「一订单一结算单」）；
    ///   - 上次失败（status=2）：允许再次处理（由业务自身的状态校验兜底幂等）。
    /// </summary>
    public static class OrderEventIdempotency
    {
        /// <summary>
        /// 尝试占用业务幂等键。
        /// 返回 (acquired, log)：acquired=true 表示调用方应继续处理；false 表示已处理过，直接 ACK。
        /// </summary>
        public static async Task<(bool acquired, OrderEventLog? log)> TryAcquireAsync(
            _111Context db,
            string eventId,
            string eventType,
            string? orderId,
            string? orderNo,
            string bizKey)
        {
            var existing = await db.OrderEventLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BizKey == bizKey);

            if (existing != null)
            {
                // 已处理完成 → 跳过；处理中/失败 → 允许再次处理
                return (existing.Status != 1, existing);
            }

            var log = new OrderEventLog
            {
                Id = ECommerce.Infrastructure.Tools.Config.NewSequentialGuid(),
                EventId = eventId,
                EventType = eventType,
                OrderId = orderId,
                OrderNo = orderNo,
                BizKey = bizKey,
                Status = 0,
                CreateTime = DateTime.Now
            };

            try
            {
                db.OrderEventLogs.Add(log);
                await db.SaveChangesAsync();
                return (true, log);
            }
            catch (DbUpdateException)
            {
                // 并发抢占：另一消费者刚插入同一 biz_key。解除跟踪后按已存在处理
                db.Entry(log).State = EntityState.Detached;
                var raced = await db.OrderEventLogs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.BizKey == bizKey);
                if (raced == null) throw; // 不是唯一键冲突（如连接故障），抛出交给上层重试
                return (raced.Status != 1, raced);
            }
        }

        /// <summary>标记处理完成</summary>
        public static async Task MarkDoneAsync(_111Context db, OrderEventLog? log, string? remark = null)
        {
            if (log == null) return;
            // EF Core 6 无 ExecuteUpdate，按跟踪实体更新
            var tracked = await db.OrderEventLogs.FirstOrDefaultAsync(x => x.Id == log.Id);
            if (tracked == null) return;
            tracked.Status = 1;
            tracked.UpdateTime = DateTime.Now;
            tracked.Remark = remark;
            await db.SaveChangesAsync();
        }

        /// <summary>标记处理失败（不抛出：由上层决定重试或 ACK）</summary>
        public static async Task MarkFailedAsync(_111Context db, OrderEventLog? log, string? remark = null)
        {
            if (log == null) return;
            var tracked = await db.OrderEventLogs.FirstOrDefaultAsync(x => x.Id == log.Id);
            if (tracked == null) return;
            tracked.Status = 2;
            tracked.UpdateTime = DateTime.Now;
            tracked.Remark = remark;
            await db.SaveChangesAsync();
        }
    }
}
