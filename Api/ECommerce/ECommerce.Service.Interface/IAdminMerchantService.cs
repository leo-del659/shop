using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IAdminMerchantService
    {
        Task<Api_Response> GetList(int page, int size, string? keyword);
        Task<Api_Response> Get(string id);
        Task<Api_Response> Add(Merchant t);
        Task<Api_Response> Update(Merchant t);
        Task<Api_Response> Remove(string id);
        /// <summary>
        /// 商户资质列表
        /// </summary>
        Task<Api_Response> GetQualifications(string merchantId);
        /// <summary>
        /// 新增商户资质
        /// </summary>
        Task<Api_Response> AddQualification(MerchantQualification q);
        /// <summary>
        /// 修改商户资质
        /// </summary>
        Task<Api_Response> UpdateQualification(MerchantQualification q);
        /// <summary>
        /// 删除商户资质
        /// </summary>
        Task<Api_Response> RemoveQualification(string id);

        /// <summary>
        /// 商户等级列表（编辑表单下拉选项）
        /// </summary>
        Task<Api_Response> GetLevels();

        /// <summary>
        /// 商户银行账户列表（编辑表单下拉选项）
        /// </summary>
        Task<Api_Response> GetBanks();

        /// <summary>
        /// 生成证书编号（新增资质时自动调用）
        /// 规则：{qualType 首字母 2-3 字符大写}{yyyyMMddHHmmss}，同 qualType 计数 +1 后缀
        /// </summary>
        Task<Api_Response> GenerateCertNo(string merchantId, string qualType);

        /// <summary>
        /// 生成商户编号（新增商户时由前端 open_add 自动调用）
        /// 规则：'GYS' + 4 位顺序号（查 merchant_no LIKE 'GYS%' 的最大数字 +1）
        /// </summary>
        Task<Api_Response> GenerateMerchantNo();

        /// <summary>
        /// 获取指定商户的银行卡绑定信息（商户端自服务）
        /// </summary>
        Task<Api_Response> GetMyBank(string merchantId);

        /// <summary>
        /// 保存/更新商户银行卡绑定信息（商户端自服务，有则更新无则新增）
        /// </summary>
        Task<Api_Response> SaveBank(string merchantId, MerchantBank bank);
    }
}
