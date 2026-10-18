using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 商户账单控制器（路由前缀 /api/MerchantBill，需 /settle/merchantbill 权限，挂在结算管理菜单下）
    /// </summary>
    [Route("api/MerchantBill/[action]")]
    [ApiController]
    [Permission_("/settle/merchantbill")]
    public class AdminMerchantBillController : BaseController
    {
        private readonly IAdminMerchantBillService _service;
        public AdminMerchantBillController(I_Login_Service login_Service, IAdminMerchantBillService service) : base(login_Service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int size = 10, string? keyword = null, string? merchantId = null, string? clientId = null)
            => Ok(await _service.GetList(page, size, keyword, merchantId, clientId));

        [HttpGet]
        public async Task<IActionResult> Get(string id) => Ok(await _service.Get(id));

        [HttpPost]
        public async Task<IActionResult> Add(MerchantBill t) => Ok(await _service.Add(t));

        [HttpPost]
        public async Task<IActionResult> Update(MerchantBill t) => Ok(await _service.Update(t));

        [HttpGet]
        public async Task<IActionResult> Remove(string id) => Ok(await _service.Remove(id));

        [HttpPost]
        public async Task<IActionResult> Pay(MerchantBillPayDto dto) => Ok(await _service.Pay(dto.BillId, dto.PayNo, dto.PayAmount, dto.PayType));

        [HttpGet]
        public async Task<IActionResult> GenerateBillNo() => Ok(await _service.GenerateBillNo());
    }

    /// <summary>商户账单付款请求</summary>
    public class MerchantBillPayDto
    {
        public string BillId { get; set; } = string.Empty;
        public string PayNo { get; set; } = string.Empty;
        public decimal PayAmount { get; set; }
        public int PayType { get; set; } = 1;
    }
}
