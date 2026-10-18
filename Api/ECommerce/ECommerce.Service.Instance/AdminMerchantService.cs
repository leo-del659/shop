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
    public class AdminMerchantService : Base_Service, IAdminMerchantService
    {
        private readonly Merchant_EFCore _merchant_EFCore;
        private readonly MerchantQualification_EFCore _qual_EFCore;
        private readonly MerchantLevel_EFCore _level_EFCore;
        private readonly MerchantBank_EFCore _bank_EFCore;

        public AdminMerchantService(
            Merchant_EFCore merchant_EFCore,
            MerchantQualification_EFCore qual_EFCore,
            MerchantLevel_EFCore level_EFCore,
            MerchantBank_EFCore bank_EFCore)
        {
            _merchant_EFCore = merchant_EFCore;
            _qual_EFCore = qual_EFCore;
            _level_EFCore = level_EFCore;
            _bank_EFCore = bank_EFCore;
        }

        public Task<Api_Response> GetList(int page, int size, string? keyword)
        {
            int total;
            var list = _merchant_EFCore
                .Query(out total, page, size, false, s => s.CreateTime,
                    s => string.IsNullOrEmpty(keyword) || (s.MerchantName != null && s.MerchantName.Contains(keyword)))
                .ToList();
            return Task.FromResult(Ok("ok", new { total, list }));
        }

        public Task<Api_Response> Get(string id)
        {
            var t = _merchant_EFCore.Find(id);
            return Task.FromResult(t == null ? Fail("商户不存在") : Ok("ok", t));
        }

        public Task<Api_Response> Add(Merchant t)
        {
            t.Id = Config.NewSequentialGuid();
            t.CreateTime = System.DateTime.Now;
            _merchant_EFCore.Add(t);
            return Task.FromResult(Get_Result(_merchant_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Update(Merchant t)
        {
            var old = _merchant_EFCore.Find(t.Id);
            if (old == null) return Task.FromResult(Fail("商户不存在"));
            old.MerchantName = t.MerchantName;
            old.IsBan = t.IsBan;
            _merchant_EFCore.Update(old);
            return Task.FromResult(Get_Result(_merchant_EFCore.SaveChanges()));
        }

        public Task<Api_Response> Remove(string id)
        {
            var t = _merchant_EFCore.Find(id);
            if (t == null) return Task.FromResult(Fail("商户不存在"));
            _merchant_EFCore.Delete(t);
            return Task.FromResult(Get_Result(_merchant_EFCore.SaveChanges()));
        }

        public Task<Api_Response> GetQualifications(string merchantId)
        {
            var list = _qual_EFCore.Query(q => q.MerchantId == merchantId).ToList();
            return Task.FromResult(Ok("ok", list));
        }

        public Task<Api_Response> AddQualification(MerchantQualification q)
        {
            q.Id = Config.NewSequentialGuid();
            q.CreateTime = System.DateTime.Now;
            _qual_EFCore.Add(q);
            return Task.FromResult(Get_Result(_qual_EFCore.SaveChanges()));
        }

        public Task<Api_Response> UpdateQualification(MerchantQualification q)
        {
            var old = _qual_EFCore.Find(q.Id);
            if (old == null) return Task.FromResult(Fail("资质不存在"));
            old.QualType = q.QualType;
            old.QualNo = q.QualNo;
            old.FileUrl = q.FileUrl;
            old.ExpireDate = q.ExpireDate;
            old.IsBan = q.IsBan;
            _qual_EFCore.Update(old);
            return Task.FromResult(Get_Result(_qual_EFCore.SaveChanges()));
        }

        public Task<Api_Response> RemoveQualification(string id)
        {
            var t = _qual_EFCore.Find(id);
            if (t == null) return Task.FromResult(Fail("资质不存在"));
            _qual_EFCore.Delete(t);
            return Task.FromResult(Get_Result(_qual_EFCore.SaveChanges()));
        }

        public Task<Api_Response> GetLevels()
        {
            // 按 Sort 升序返回等级列表；Sort 为 null 时通过 ?? 0 统一兜底
            var list = _level_EFCore.Query<int>(true, s => s.Sort ?? 0).ToList();
            return Task.FromResult(Ok("ok", list));
        }

        public Task<Api_Response> GetBanks()
        {
            // 银行账户列表按创建时间倒序，编辑时挑最近开户；用户也可在搜索框过滤
            var list = _bank_EFCore.Query<DateTime>(false, s => s.CreateTime ?? DateTime.MinValue).ToList();
            return Task.FromResult(Ok("ok", list));
        }

        /// <summary>
        /// 生成证书编号：{qualType 首字母 2-3 字符大写}{yyyyMMddHHmmss}
        /// 毫秒级时间戳保证唯一；同一天同 qualType 用户连续点新增会得到不同编号
        /// </summary>
        public Task<Api_Response> GenerateCertNo(string merchantId, string qualType)
        {
            if (string.IsNullOrWhiteSpace(qualType)) return Task.FromResult(Fail("资质类型不能为空"));

            string prefix = ExtractPrefix(qualType);
            var now = System.DateTime.Now;
            string certNo = $"{prefix}{now:yyyyMMddHHmmss}";

            // 极端情况：毫秒相同且 DB 中已存在该 certNo 时，再 +1 自增
            while (_qual_EFCore.Query(q => q.QualNo == certNo).Any())
            {
                now = now.AddMilliseconds(1);
                certNo = $"{prefix}{now:yyyyMMddHHmmss}";
            }

            return Task.FromResult(Ok("ok", new { certNo }));
        }

        /// <summary>
        /// 提取资质类型前缀：取中英文混合的前 2-3 个字符大写
        /// - 纯英文：取大写首字母连续段，如 "ISO 9001" → "ISO"，"SC" → "SC"
        /// - 含中文：取前 2 个汉字（编码上无拼音，做最简字符截断）
        /// </summary>
        private static string ExtractPrefix(string s)
        {
            s = (s ?? "").Trim();
            if (string.IsNullOrEmpty(s)) return "QT";

            // 仅含 ASCII 字母数字
            var asciiLetters = new string(s.Where(char.IsLetterOrDigit).ToArray());
            if (!string.IsNullOrEmpty(asciiLetters) && asciiLetters.All(ch => ch < 128))
            {
                return asciiLetters.ToUpperInvariant().Length > 3
                    ? asciiLetters.ToUpperInvariant().Substring(0, 3)
                    : asciiLetters.ToUpperInvariant();
            }

            // 含中文：取前 2 个非空白字符
            var compact = s.Replace(" ", "");
            return compact.Length >= 2 ? compact.Substring(0, 2) : compact;
        }

        /// <summary>
        /// 生成下一个商户业务ID sp_0001、sp_0002…
        /// 实现：拉所有 id LIKE 'sp_%'，解析末尾数字取最大 +1；空表 → 1
        /// while 自旋兜底防极端并发重复
        /// </summary>
        public Task<Api_Response> GenerateMerchantNo()
        {
            const string prefix = "sp_";
            const int width = 4;

            int maxSeq = 0;
            var existing = _merchant_EFCore
                .Query(s => s.Id != null && s.Id.StartsWith(prefix))
                .Select(s => s.Id!)
                .ToList();

            foreach (var no in existing)
            {
                // 跳过 sp_ 这种纯前缀
                if (no.Length <= prefix.Length) continue;
                var tail = no.Substring(prefix.Length);
                if (int.TryParse(tail, out var n) && n > maxSeq) maxSeq = n;
            }

            int next = maxSeq + 1;
            string merchantNo = $"{prefix}{next.ToString().PadLeft(width, '0')}";

            // 防极端并发：DB 已存在则 +1
            while (_merchant_EFCore.Query(s => s.Id == merchantNo).Any())
            {
                next++;
                merchantNo = $"{prefix}{next.ToString().PadLeft(width, '0')}";
            }

            return Task.FromResult(Ok("ok", new { merchantNo }));
        }

        public Task<Api_Response> GetMyBank(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
            {
                return Task.FromResult(Fail("商户ID不能为空"));
            }
            var bank = _bank_EFCore.Query(b => b.MerchantId == merchantId).FirstOrDefault();
            return Task.FromResult(bank == null ? Fail("未绑定银行卡") : Ok("ok", bank));
        }

        public Task<Api_Response> SaveBank(string merchantId, MerchantBank bank)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
            {
                return Task.FromResult(Fail("商户ID不能为空"));
            }
            if (string.IsNullOrWhiteSpace(bank.BankName) ||
                string.IsNullOrWhiteSpace(bank.BankAccount) ||
                string.IsNullOrWhiteSpace(bank.AccountName))
            {
                return Task.FromResult(Fail("请填写完整的银行卡信息"));
            }
            var existing = _bank_EFCore.Query(b => b.MerchantId == merchantId).FirstOrDefault();
            if (existing != null)
            {
                // 更新
                existing.BankName = bank.BankName.Trim();
                existing.BankAccount = bank.BankAccount.Trim();
                existing.AccountName = bank.AccountName.Trim();
                _bank_EFCore.Update(existing);
            }
            else
            {
                // 新增
                bank.Id = Config.NewSequentialGuid();
                bank.MerchantId = merchantId;
                bank.CreateTime = DateTime.Now;
                _bank_EFCore.Add(bank);
            }
            return Task.FromResult(Get_Result(_bank_EFCore.SaveChanges(), "保存成功"));
        }
    }
}
