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
    public class AdminLogisticsService : Base_Service, IAdminLogisticsService
    {
        private readonly LogisticsCompany_EFCore _efCore;

        public AdminLogisticsService(LogisticsCompany_EFCore efCore)
        {
            _efCore = efCore;
        }

        public Task<Api_Response> GetList(int page, int size, string? keyword)
        {
            int total;
            var list = _efCore
                .Query(out total, page, size, false, s => s.CreateTime,
                    s => string.IsNullOrEmpty(keyword) || (s.CompanyName != null && s.CompanyName.Contains(keyword)))
                .ToList();
            return Task.FromResult(Ok("ok", new { total, list }));
        }

        public Task<Api_Response> Get(string id)
        {
            var t = _efCore.Find(id);
            return Task.FromResult(t == null ? Fail("物流公司不存在") : Ok("ok", t));
        }

        public Task<Api_Response> Add(LogisticsCompany t)
        {
            t.Id = Config.NewSequentialGuid();
            t.CreateTime = System.DateTime.Now;
            // 自动生成物流编码 WL + 4 位序号（按已存在最大编号 +1）
            // 仅当调用方未显式传入 Code 时生成
            if (string.IsNullOrEmpty(t.Code))
            {
                t.Code = BuildNextLogisticsCode();
            }
            _efCore.Add(t);
            return Task.FromResult(Get_Result(_efCore.SaveChanges()));
        }

        /// <summary>
        /// 自动生成下一个物流编码：取当前数据库所有物流公司的最大编号 +1，
        /// 不足 4 位前面补 0（例：WL0001、WL0002 ... WL0123）
        /// 解析失败的非数字编码不计入，按"当前总条数 + 1"兜底
        /// </summary>
        private string BuildNextLogisticsCode()
        {
            const string prefix = "WL";
            int maxNo = 0;
            try
            {
                var all = _efCore.Query().ToList();
                foreach (var c in all)
                {
                    var code = c?.Code;
                    if (string.IsNullOrEmpty(code)) continue;
                    // 只处理以 WL 开头的编码，跳过非标准编码
                    if (!code.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) continue;
                    var digits = new string(code.Skip(prefix.Length).ToArray());
                    if (int.TryParse(digits, out var n) && n > maxNo) maxNo = n;
                }
                // 兜底：若全是空/非数字编码，则按总数 + 1 开始
                if (maxNo == 0) maxNo = all.Count;
            }
            catch { maxNo = 0; }
            maxNo += 1;
            return prefix + maxNo.ToString().PadLeft(4, '0');
        }

        /// <summary>
        /// 预览下一个将被分配的物流编码（不写入数据库，供前端预览）
        /// </summary>
        public Task<Api_Response> PreviewNextLogisticsCode()
        {
            return Task.FromResult(Ok("ok", new { code = BuildNextLogisticsCode() }));
        }

        /// <summary>
        /// 一次性将库内所有物流公司编码重排为统一的 WL + 4 位数字格式。
        /// 按创建时间升序从 WL0001 依次编号，覆盖任何非 WL 开头的旧编码。
        /// 采用两遍写入：先写到不会与已有编码冲突的临时编码（TMP + 序号），
        /// 再统一写入 WL 编码，规避 Code 唯一索引在更新过程中的瞬时冲突。
        /// </summary>
        public Task<Api_Response> NormalizeCodes()
        {
            var all = _efCore.Query().OrderBy(s => s.CreateTime).ThenBy(s => s.Id).ToList();
            if (all.Count == 0) return Task.FromResult(Ok("ok", new { count = 0 }));

            // 第一遍：临时编码（不会与任何已有 WL 编码或彼此冲突）
            int idx = 0;
            foreach (var c in all)
            {
                idx += 1;
                c.Code = "TMP" + idx.ToString().PadLeft(6, '0');
            }
            foreach (var c in all) _efCore.Update(c);
            _efCore.SaveChanges();

            // 第二遍：统一的 WL + 4 位数字编码
            idx = 0;
            foreach (var c in all)
            {
                idx += 1;
                c.Code = "WL" + idx.ToString().PadLeft(4, '0');
            }
            foreach (var c in all) _efCore.Update(c);
            var result = _efCore.SaveChanges();

            return Task.FromResult(Ok("ok", new { count = idx }));
        }

        public Task<Api_Response> Update(LogisticsCompany t)
        {
            var old = _efCore.Find(t.Id);
            if (old == null) return Task.FromResult(Fail("物流公司不存在"));
            old.CompanyName = t.CompanyName;
            old.Code = t.Code;
            old.Phone = t.Phone;
            old.IsBan = t.IsBan;
            _efCore.Update(old);
            return Task.FromResult(Get_Result(_efCore.SaveChanges()));
        }

        public Task<Api_Response> Remove(string id)
        {
            var t = _efCore.Find(id);
            if (t == null) return Task.FromResult(Fail("物流公司不存在"));
            _efCore.Delete(t);
            return Task.FromResult(Get_Result(_efCore.SaveChanges()));
        }
    }
}
