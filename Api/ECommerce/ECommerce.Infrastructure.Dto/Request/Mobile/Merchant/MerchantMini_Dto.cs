using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.Dto.Request.Mobile.Merchant
{
    /// <summary>账号密码登录请求（account 为手机号）</summary>
    public class MerchantLoginDto
    {
        public string? Account { get; set; }
        public string? Password { get; set; }
    }

    /// <summary>快速注册请求：手机号 + 短信验证码 + 密码</summary>
    public class MerchantRegisterDto
    {
        /// <summary>手机号（同时作为登录账号 account）</summary>
        public string? Phone { get; set; }
        /// <summary>短信验证码</summary>
        public string? Code { get; set; }
        /// <summary>登录密码（不少于 6 位）</summary>
        public string? Password { get; set; }
    }

    /// <summary>修改密码</summary>
    public class MerchantChangePwdDto
    {
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    /// <summary>新增/编辑商品。costPrice 对应 merchant_goods.supply_price</summary>
    public class MerchantGoodsSaveDto
    {
        public string? Name { get; set; }
        public string? CategoryId { get; set; }
        public decimal? Price { get; set; }
        public decimal? MarketPrice { get; set; }
        public int? Stock { get; set; }
        public string? Unit { get; set; }
        public string? Spec { get; set; }
        public string? Description { get; set; }
        public string? CoverImg { get; set; }
        public string? ImgList { get; set; }
        public int? IsOnSale { get; set; }
        public decimal? CostPrice { get; set; }
    }

    /// <summary>更新商户资料</summary>
    public class MerchantProfileDto
    {
        public string? MerchantName { get; set; }
        /// <summary>商户头像图片地址（/api/file/{guid}）</summary>
        public string? Avatar { get; set; }
        public string? Contact { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }

    /// <summary>银行账户</summary>
    public class MerchantBankDto
    {
        public string? Id { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public string? AccountName { get; set; }
    }

    /// <summary>资质</summary>
    public class MerchantQualificationDto
    {
        public string? QualType { get; set; }
        public string? QualNo { get; set; }
        public string? FileUrl { get; set; }
        public string? ExpireDate { get; set; }
    }

    /// <summary>补货</summary>
    public class MerchantRestockDto
    {
        public int Qty { get; set; }
    }

    /// <summary>补货（带收货地址 / 备注）：下单时一并写入数据库，便于补货后商户按地址发货</summary>
    public class MerchantRestockWithAddressDto : MerchantRestockDto
    {
        public string? ReceiverName { get; set; }
        public string? ReceiverMobile { get; set; }
        public string? Address { get; set; }
        public string? Remark { get; set; }
    }

    /// <summary>取消补货订单</summary>
    public class MerchantCancelBillDto
    {
        public string? Reason { get; set; }
    }

    /// <summary>订单操作</summary>
    public class MerchantOrderActionDto
    {
        public string? Action { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>提现申请</summary>
    public class MerchantWithdrawDto
    {
        public decimal Amount { get; set; }
        public string? Remark { get; set; }
    }
}
