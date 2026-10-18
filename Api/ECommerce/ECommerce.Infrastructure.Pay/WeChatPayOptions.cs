namespace ECommerce.Infrastructure.Pay
{
    /// <summary>
    /// 微信支付 v3 配置（appsettings "WeChatPay" 段）。
    /// 当 Enabled=false（默认/缺省）时，所有接口走「开发模拟模式」：
    /// 不发起真实网络请求，签名与回调解密使用本地 ApiV3Key 完成，便于无商户号环境端到端联调；
    /// 配齐真实商户号 + 证书 + ApiV3Key 后将 Enabled 置为 true 即切换到真实 v3 调用。
    /// </summary>
    public class WeChatPayOptions
    {
        /// <summary>是否启用真实微信支付（false=开发模拟模式）</summary>
        public bool Enabled { get; set; }

        /// <summary>小程序/公众号 AppId</summary>
        public string? AppId { get; set; }

        /// <summary>商户号</summary>
        public string? MchId { get; set; }

        /// <summary>商户 API 证书序列号（serial_no）</summary>
        public string? MchSerialNo { get; set; }

        /// <summary>
        /// 商户 API 私钥（apiclient_key.pem 内容，PEM 格式含 -----BEGIN PRIVATE KEY-----）。
        /// 建议通过环境变量 COMMUNITY_WXPAY_PRIVATEKEY 注入，避免私钥落源码。
        /// </summary>
        public string? PrivateKey { get; set; }

        /// <summary>ApiV3 密钥（32 字节），用于回调解密与模拟模式签名</summary>
        public string? ApiV3Key { get; set; }

        /// <summary>
        /// 微信支付平台证书公钥（PEM，apiclient_cert.pem 或平台证书），用于真实回调验签。
        /// 留空时真实模式回调控验会失败（提示需配置），模拟模式忽略。
        /// </summary>
        public string? PlatformPublicKey { get; set; }

        /// <summary>支付结果通知地址（真实模式由微信回调此地址）</summary>
        public string? NotifyUrl { get; set; }

        /// <summary>微信支付网关（默认 https://api.mch.weixin.qq.com）</summary>
        public string Gateway { get; set; } = "https://api.mch.weixin.qq.com";
    }
}
