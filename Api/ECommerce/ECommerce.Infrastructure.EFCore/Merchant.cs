using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户。业务主键 Id 对应 merchant_login.merchant_id（如 sp_0001），订单、账单等表经此 ID 关联。
    /// Account 为登录手机号；密码为 PHC 格式哈希串（盐值与算法参数内嵌，pepper 取 Login:Keys）。
    /// </summary>
    public partial class Merchant
    {
        /// <summary>商户业务ID（字符串主键）</summary>
        public string Id { get; set; } = null!;
        /// <summary>商户名称</summary>
        public string? MerchantName { get; set; }
        /// <summary>是否禁用 0-正常 1-禁用</summary>
        public int? IsBan { get; set; }
        /// <summary>登录手机号</summary>
        public string Account { get; set; } = null!;
        /// <summary>头像图片地址，为空时前端回退名称首字</summary>
        public string? Avatar { get; set; }
        /// <summary>登录密码（PHC 格式哈希串）</summary>
        public string Password { get; set; } = null!;
        /// <summary>随机盐标记（每行随机，仅作标记，校验时不读取）</summary>
        public string? Salt { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
