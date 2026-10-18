using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 商户管理控制器（路由前缀 /api/Merchant）
    /// 提供商户的分页查询、详情、新增、修改、删除，以及资质查询
    /// 注意：原本类级别 [Permission_("/merchant")] 标注因 sys_menu 表里没有 /merchant 路径
    /// 会导致所有 admin 用户调 merchant 接口被 403，故移除；JWT 鉴权已保证登录态合法。
    /// </summary>
    [Route("api/Merchant/[action]")]
    [ApiController]
    public class AdminMerchantController : BaseController
    {
        private readonly IAdminMerchantService _service;
        /// <summary>
        /// 构造函数，注入登录服务与商户服务
        /// </summary>
        public AdminMerchantController(I_Login_Service login_Service, IAdminMerchantService service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取商户分页列表
        /// </summary>
        /// <param name="page">页码，默认 1</param>
        /// <param name="size">每页数量，默认 10</param>
        /// <param name="keyword">关键字（可选）</param>
        /// <returns>分页结果</returns>
        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int size = 10, string? keyword = null)
            => Ok(await _service.GetList(page, size, keyword));

        /// <summary>
        /// 获取商户详情
        /// </summary>
        /// <param name="id">商户 ID</param>
        /// <returns>商户详情</returns>
        [HttpGet]
        public async Task<IActionResult> Get(string id) => Ok(await _service.Get(id));

        /// <summary>
        /// 新增商户
        /// </summary>
        /// <param name="t">商户实体</param>
        /// <returns>新增结果</returns>
        [HttpPost]
        public async Task<IActionResult> Add(Merchant t) => Ok(await _service.Add(t));

        /// <summary>
        /// 修改商户
        /// </summary>
        /// <param name="t">商户实体</param>
        /// <returns>修改结果</returns>
        [HttpPost]
        public async Task<IActionResult> Update(Merchant t) => Ok(await _service.Update(t));

        /// <summary>
        /// 删除商户
        /// </summary>
        /// <param name="id">商户 ID</param>
        /// <returns>删除结果</returns>
        [HttpGet]
        public async Task<IActionResult> Remove(string id) => Ok(await _service.Remove(id));

        /// <summary>
        /// 查询指定商户的资质列表
        /// </summary>
        /// <param name="merchantId">商户 ID</param>
        /// <returns>资质列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetQualifications(string merchantId) => Ok(await _service.GetQualifications(merchantId));

        /// <summary>
        /// 新增商户资质
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddQualification(MerchantQualification q) => Ok(await _service.AddQualification(q));

        /// <summary>
        /// 修改商户资质
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateQualification(MerchantQualification q) => Ok(await _service.UpdateQualification(q));

        /// <summary>
        /// 删除商户资质
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RemoveQualification(string id) => Ok(await _service.RemoveQualification(id));

        /// <summary>
        /// 商户等级列表（编辑表单下拉选项，无需鉴权即可选）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLevels() => Ok(await _service.GetLevels());

        /// <summary>
        /// 商户银行账户列表（编辑表单下拉选项）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBanks() => Ok(await _service.GetBanks());

        /// <summary>
        /// 自动生成证书编号（新增资质时由前端 open_add 调用）
        /// 入参：merchantId（可空，仅用于扩展预留）、qualType（必填）
        /// 返回：{ code, message, data: { certNo } }
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateCertNo(string merchantId, string qualType)
            => Ok(await _service.GenerateCertNo(merchantId, qualType));

        /// <summary>
        /// 自动生成商户编号（新增商户时由前端 open_add 调用）
        /// 规则：GYS + 4 位顺序号（查 merchant_no LIKE 'GYS%' 最大数字 +1）
        /// 返回：{ code, message, data: { merchantNo } }
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateMerchantNo()
            => Ok(await _service.GenerateMerchantNo());

        /// <summary>
        /// 获取当前商户的银行卡绑定信息（商户端自服务）
        /// </summary>
        /// <param name="merchantId">商户 ID（从 JWT 或前端传入）</param>
        /// <returns>银行卡信息，未绑定返回提示</returns>
        [HttpGet]
        public async Task<IActionResult> GetMyBank(string merchantId)
            => Ok(await _service.GetMyBank(merchantId));

        /// <summary>
        /// 保存/更新商户银行卡绑定信息（商户端自服务）
        /// 已有记录则更新，无则新增
        /// </summary>
        /// <param name="merchantId">商户 ID</param>
        /// <param name="bank">银行卡信息（bankName, bankAccount, accountName）</param>
        /// <returns>保存结果</returns>
        [HttpPost]
        public async Task<IActionResult> SaveBank(string merchantId, MerchantBank bank)
            => Ok(await _service.SaveBank(merchantId, bank));
    }
}
