using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Mobile.Merchant;
using ECommerce.Infrastructure.Dto.Response;

namespace ECommerce.Service.Interface
{
    /// <summary>
    /// 商户端（微信小程序）业务服务
    /// 覆盖：账号密码登录、资料、首页、商品、订单、对账、提现、账户、资质
    /// </summary>
    [Provider_]
    public interface IMerchantMiniService
    {
        Api_Response Login(string account, string password);
        /// <summary>快速注册：手机号 + 短信验证码 + 密码，校验通过后建号（account 即手机号）</summary>
        Api_Response Register(string phone, string code, string password);
        Api_Response ChangePassword(string merchantId, string oldPassword, string newPassword);
        Api_Response GetMe(string merchantId);
        Api_Response GetProfile(string merchantId);
        Api_Response UpdateProfile(string merchantId, MerchantProfileDto dto);
        Api_Response GetHome(string merchantId);

        /// <summary>获取商户余额（累计已结算、累计已提现、可结算余额 + 最新同步状态）</summary>
        Api_Response GetBalance(string merchantId);

        /// <summary>触发余额同步（重算已完成订单并写入同步日志，返回同步状态）</summary>
        Api_Response SyncBalance(string merchantId);

        Api_Response GetGoods(string merchantId, string filter, string category = "");
        Api_Response GetGoodsDetail(string merchantId, string id);
        Api_Response AddGoods(string merchantId, MerchantGoodsSaveDto dto);
        Api_Response UpdateGoods(string merchantId, string id, MerchantGoodsSaveDto dto);
        /// <summary>查询商品规格（规格组 + SKU 明细），供商户端编辑多规格商品</summary>
        Api_Response GetGoodsSpecs(string merchantId, string goodsId);
        Api_Response ToggleGoods(string merchantId, string id);
        Api_Response RestockGoods(string merchantId, string id, int qty);

        Api_Response GetOrders(string merchantId, string status, string? startDate = null, string? endDate = null);
        /// <summary>商户账单列表（从 MerchantBill 表查询，对齐前端管理后台商户账单）</summary>
        Api_Response GetBills(string merchantId, string status);
        /// <summary>商户账单详情（含物流时间线，按订单状态动态生成）</summary>
        Api_Response GetBillDetail(string merchantId, string id);
        Api_Response GetAllOrders(string status, string? startDate = null, string? endDate = null);
        Api_Response GetOrderDetail(string merchantId, string id);
        Api_Response OrderAction(string merchantId, string id, string action, string? reason);

        Api_Response GetReconcile(string merchantId, string month, string? startDate = null, string? endDate = null);
        Api_Response GetWithdrawals(string merchantId, int? status = null, string? startTime = null, string? endTime = null);
        Task<Api_Response> AddWithdraw(string merchantId, decimal amount, string remark);

        Api_Response GetBank(string merchantId);
        Api_Response UpdateBank(string merchantId, MerchantBankDto dto);
        Api_Response GetQualifications(string merchantId);
        Api_Response AddQualification(string merchantId, MerchantQualificationDto dto);

    }
}
