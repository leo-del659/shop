using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 排行榜控制器（路由前缀 /api/Rank）
    /// 提供后台运营看板所需的各类排行榜数据。
    /// 说明：原「供应商排行榜」已随 供应商→商户 重命名统一为商户销售排行榜。
    /// </summary>
    [Route("api/Rank/[action]")]
    [ApiController]
    public class RankController : BaseController
    {
        private readonly IAdminRankService _service;

        /// <summary>
        /// 构造函数，注入登录服务与排行榜服务
        /// </summary>
        public RankController(I_Login_Service login_Service, IAdminRankService service) : base(login_Service)
        {
            _service = service;
        }

        /// <summary>
        /// 商户销售排行榜
        /// 口径：已支付(pay_status=1) + 未取消(order_status!=9) + 未作废(is_ban!=1)，
        /// 按销售额降序取 Top N；0 单商户同样返回（订单数/销售额为 0）。
        /// </summary>
        /// <param name="top">取前 N 名，默认 10，最大 200</param>
        [HttpGet]
        public async Task<IActionResult> MerchantRank(int top = 10)
            => Ok(await _service.MerchantRank(top));
    }
}
