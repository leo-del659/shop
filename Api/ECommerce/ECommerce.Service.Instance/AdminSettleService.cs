using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class AdminSettleService : Base_Service, IAdminSettleService
    {
        private readonly SettleRecord_EFCore _settle_EFCore;
        private readonly SettlePay_EFCore _pay_EFCore;
        private readonly SettleRefund_EFCore _refund_EFCore;

        public AdminSettleService(SettleRecord_EFCore settle_EFCore, SettlePay_EFCore pay_EFCore, SettleRefund_EFCore refund_EFCore)
        {
            _settle_EFCore = settle_EFCore;
            _pay_EFCore = pay_EFCore;
            _refund_EFCore = refund_EFCore;
        }

        public Task<Api_Response> GetList(int page, int size, string? keyword)
        {
            int total;
            var list = _settle_EFCore
                .Query(out total, page, size, false, s => s.CreateTime,
                    s => string.IsNullOrEmpty(keyword) || (s.SettleNo != null && s.SettleNo.Contains(keyword)))
                .ToList();
            return Task.FromResult(Ok("ok", new { total, list }));
        }

        public Task<Api_Response> Get(string id)
        {
            var t = _settle_EFCore.Find(id);
            return Task.FromResult(t == null ? Fail("结算单不存在") : Ok("ok", t));
        }

        public Task<Api_Response> Add(SettleRecord t)
        {
            t.Id = Config.NewSequentialGuid();
            // 结算单号自动生成：SET + yyMMddHHmmssfff + 6位随机
            if (string.IsNullOrWhiteSpace(t.SettleNo))
                t.SettleNo = $"SET{System.DateTime.Now:yyMMddHHmmssfff}{new System.Random().Next(100000, 999999)}";
            t.Status = t.Status ?? 0;   // 0 待结算
            t.CreateTime = System.DateTime.Now;
            _settle_EFCore.Add(t);
            return Task.FromResult(Get_Result(_settle_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Update(SettleRecord t)
        {
            var old = _settle_EFCore.Find(t.Id);
            if (old == null) return Task.FromResult(Fail("结算单不存在"));
            old.ClientId = t.ClientId;
            old.SettleNo = t.SettleNo;
            old.SettleAmount = t.SettleAmount;
            old.SettleType = t.SettleType;
            old.Status = t.Status;
            _settle_EFCore.Update(old);
            return Task.FromResult(Get_Result(_settle_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Remove(string id)
        {
            var t = _settle_EFCore.Find(id);
            if (t == null) return Task.FromResult(Fail("结算单不存在"));
            _settle_EFCore.Delete(t);
            return Task.FromResult(Get_Result(_settle_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Pay(string settleId, string payNo, decimal payAmount)
        {
            var settle = _settle_EFCore.Find(settleId);
            if (settle == null) return Task.FromResult(Fail("结算单不存在"));
            _pay_EFCore.Add(new SettlePay
            {
                Id = Config.NewSequentialGuid(),
                SettleId = settleId,
                PayNo = payNo,
                PayAmount = payAmount,
                PayStatus = PayStatus.Paid,
                CreateTime = System.DateTime.Now
            });
            settle.Status = 1;
            _settle_EFCore.Update(settle);
            return Task.FromResult(Get_Result(_settle_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Refund(string settleId, string refundNo, decimal refundAmount)
        {
            var settle = _settle_EFCore.Find(settleId);
            if (settle == null) return Task.FromResult(Fail("结算单不存在"));
            _refund_EFCore.Add(new SettleRefund
            {
                Id = Config.NewSequentialGuid(),
                SettleId = settleId,
                RefundNo = refundNo,
                RefundAmount = refundAmount,
                RefundStatus = 1,
                CreateTime = System.DateTime.Now
            });
            settle.Status = 2;
            _settle_EFCore.Update(settle);
            return Task.FromResult(Get_Result(_settle_EFCore.SaveChanges()));
        }

        /// <summary>
        /// 自动生成结算单号：JS{yyyyMMdd}-{seq:00}
        /// 同日现存最大序号 + 1，格式与 DB 现存（JS20260729-01）严格一致
        /// </summary>
        public Task<Api_Response> GenerateSettleNo()
        {
            var now = System.DateTime.Now;
            string dayStamp = now.ToString("yyyyMMdd");
            string prefix = $"JS{dayStamp}-";
            int maxSeq = 0;
            var exist = _settle_EFCore
                .Query(s => s.SettleNo != null && s.SettleNo.StartsWith(prefix))
                .Select(s => s.SettleNo!)
                .ToList();
            foreach (var no in exist)
            {
                // 截 "JS20260804-XX" 的尾部序号
                var tail = no.Substring(prefix.Length);
                if (int.TryParse(tail, out var n) && n > maxSeq) maxSeq = n;
            }
            int seq = maxSeq + 1;
            string settleNo = $"{prefix}{seq.ToString("D2")}";
            return Task.FromResult(Ok("ok", new { settleNo }));
        }
    }
}
