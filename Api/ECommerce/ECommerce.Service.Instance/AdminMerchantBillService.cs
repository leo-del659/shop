using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class AdminMerchantBillService : Base_Service, IAdminMerchantBillService
    {
        private readonly MerchantBill_EFCore _bill_EFCore;

        public AdminMerchantBillService(MerchantBill_EFCore bill_EFCore)
        {
            _bill_EFCore = bill_EFCore;
        }

        public Task<Api_Response> GetList(int page, int size, string? keyword, string? merchantId = null, string? clientId = null)
        {
            int total;
            var rows = _bill_EFCore
                .Query(out total, page, size, false, r => r.CreateTime,
                    r => (string.IsNullOrEmpty(keyword) || (r.BillNo != null && r.BillNo.Contains(keyword)))
                      && (string.IsNullOrEmpty(merchantId) || r.MerchantId == merchantId)
                      && (string.IsNullOrEmpty(clientId) || r.ClientId == clientId))
                .ToList();

            var list = rows.Select(r => new
            {
                id = r.Id,
                billNo = r.BillNo,
                merchantId = r.MerchantId,
                clientId = r.ClientId,
                goodsId = r.GoodsId,
                goodsName = r.GoodsName,
                buyNum = r.BuyNum,
                amount = r.Amount,
                orderStatus = r.OrderStatus,
                createTime = r.CreateTime,
                updateTime = r.UpdateTime
            }).ToList();

            return Task.FromResult(Ok("ok", new { total, list }));
        }

        public Task<Api_Response> Get(string id)
        {
            var t = _bill_EFCore.Find(id);
            return Task.FromResult(t == null ? Fail("账单不存在") : Ok("ok", t));
        }

        public Task<Api_Response> Add(MerchantBill t)
        {
            t.Id = Config.NewSequentialGuid();
            if (string.IsNullOrWhiteSpace(t.BillNo))
                t.BillNo = GenerateBillNoInner();
            t.ClientId = t.ClientId ?? string.Empty;
            t.GoodsId = t.GoodsId ?? string.Empty;
            t.GoodsName = t.GoodsName ?? string.Empty;
            t.BuyNum = t.BuyNum ?? 0;
            t.Amount = t.Amount ?? 0;
            t.OrderStatus = t.OrderStatus ?? OrderStatus.PendingPayment;
            t.CreateTime = System.DateTime.Now;
            t.UpdateTime = System.DateTime.Now;
            _bill_EFCore.Add(t);
            return Task.FromResult(Get_Result(_bill_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Update(MerchantBill t)
        {
            var old = _bill_EFCore.Find(t.Id);
            if (old == null) return Task.FromResult(Fail("账单不存在"));
            old.MerchantId = t.MerchantId;
            old.ClientId = t.ClientId;
            old.GoodsId = t.GoodsId;
            old.GoodsName = t.GoodsName;
            old.BuyNum = t.BuyNum;
            old.Amount = t.Amount;
            old.OrderStatus = t.OrderStatus;
            old.UpdateTime = System.DateTime.Now;
            _bill_EFCore.Update(old);
            return Task.FromResult(Get_Result(_bill_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Remove(string id)
        {
            var t = _bill_EFCore.Find(id);
            if (t == null) return Task.FromResult(Fail("账单不存在"));
            _bill_EFCore.Delete(t);
            return Task.FromResult(Get_Result(_bill_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Pay(string billId, string payNo, decimal payAmount, int payType)
        {
            var bill = _bill_EFCore.Find(billId);
            if (bill == null) return Task.FromResult(Fail("账单不存在"));
            var paid = (bill.PaidAmount ?? 0) + payAmount;
            bill.PaidAmount = paid;
            bill.PayNo = payNo;
            bill.PayType = payType;
            bill.PayTime = System.DateTime.Now;
            var total = bill.Amount ?? 0;
            bill.PayStatus = total <= 0 ? PayStatus.Unpaid : (paid >= total ? PayStatus.Paid : PayStatus.PartialPaid);
            bill.UpdateTime = System.DateTime.Now;
            _bill_EFCore.Update(bill);
            return Task.FromResult(Get_Result(_bill_EFCore.SaveChanges()));
        }

        public Task<Api_Response> GenerateBillNo() => Task.FromResult(Ok("ok", new { billNo = GenerateBillNoInner() }));

        private string GenerateBillNoInner()
        {
            var now = System.DateTime.Now;
            string dayStamp = now.ToString("yyyyMMdd");
            string prefix = $"GYS{dayStamp}-";
            int maxSeq = 0;
            var exist = _bill_EFCore
                .Query(r => r.BillNo != null && r.BillNo.StartsWith(prefix))
                .Select(r => r.BillNo!)
                .ToList();
            foreach (var no in exist)
            {
                var tail = no.Substring(prefix.Length);
                if (int.TryParse(tail, out var n) && n > maxSeq) maxSeq = n;
            }
            return $"{prefix}{(maxSeq + 1).ToString("D2")}";
        }
    }
}
