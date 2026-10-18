using System.Text.Json.Serialization;

namespace ECommerce.Infrastructure.Pay
{
    /// <summary>JSAPI 统一下单请求（业务侧组装，金额单位：元）</summary>
    public class JsapiPrepayRequest
    {
        /// <summary>商户订单号（建议用订单 OrderNo，全局唯一）</summary>
        public string OutTradeNo { get; set; } = "";

        /// <summary>商品/订单描述</summary>
        public string Description { get; set; } = "";

        /// <summary>支付金额（元）</summary>
        public decimal Amount { get; set; }

        /// <summary>付款人微信 openid（小程序侧通过 wx.login 获取后传入）</summary>
        public string PayerOpenId { get; set; } = "";

        /// <summary>附加数据，原样回传（如 merchantId/clientId）</summary>
        public string? Attach { get; set; }
    }

    /// <summary>JSAPI 统一下单返回：prepay_id + 前端 wx.requestPayment 所需的支付参数</summary>
    public class JsapiPrepayResult
    {
        /// <summary>微信预支付交易会话标识</summary>
        public string PrepayId { get; set; } = "";

        /// <summary>JSAPI 调起支付所需的参数包（timeStamp/nonceStr/package/paySign 等）</summary>
        public WeChatPayJsapiPackage PayPackage { get; set; } = new();

        /// <summary>是否开发模拟模式（前端可据此跳过真实 wx.requestPayment 或走沙箱）</summary>
        public bool IsMock { get; set; }
    }

    /// <summary>wx.requestPayment 参数（小程序支付）</summary>
    public class WeChatPayJsapiPackage
    {
        public string AppId { get; set; } = "";
        public string TimeStamp { get; set; } = "";
        public string NonceStr { get; set; } = "";
        public string Package { get; set; } = "";
        public string SignType { get; set; } = "RSA";
        public string PaySign { get; set; } = "";
    }

    /// <summary>微信支付成功通知解密后的核心字段</summary>
    public class WeChatPayNotification
    {
        [JsonPropertyName("out_trade_no")] public string OutTradeNo { get; set; } = "";
        [JsonPropertyName("transaction_id")] public string TransactionId { get; set; } = "";
        [JsonPropertyName("trade_state")] public string TradeState { get; set; } = "";
        [JsonPropertyName("trade_state_desc")] public string TradeStateDesc { get; set; } = "";
        [JsonPropertyName("success_time")] public string SuccessTime { get; set; } = "";
        [JsonPropertyName("amount")] public AmountInfo? Amount { get; set; }

        public class AmountInfo
        {
            [JsonPropertyName("total")] public int Total { get; set; }
            [JsonPropertyName("currency")] public string? Currency { get; set; }
        }
    }

    /// <summary>商家转账到银行卡请求（金额单位：元）</summary>
    public class TransferToBankRequest
    {
        /// <summary>商户转账单号（全局唯一）</summary>
        public string OutTransferNo { get; set; } = "";

        /// <summary>收款方银行卡号</summary>
        public string BankCardNo { get; set; } = "";

        /// <summary>收款方姓名（开户名）</summary>
        public string AccountName { get; set; } = "";

        /// <summary>开户行（如 招商银行）或标准银行编码</summary>
        public string BankName { get; set; } = "";

        /// <summary>转账金额（元）</summary>
        public decimal Amount { get; set; }

        /// <summary>转账备注/用途</summary>
        public string? Remark { get; set; }
    }

    /// <summary>转账到银行卡结果</summary>
    public class TransferToBankResult
    {
        /// <summary>是否成功受理/完成</summary>
        public bool Success { get; set; }

        /// <summary>微信转账单号（transfer_no）</summary>
        public string? TransferNo { get; set; }

        /// <summary>失败原因（Success=false 时）</summary>
        public string? FailReason { get; set; }

        /// <summary>是否开发模拟模式</summary>
        public bool IsMock { get; set; }
    }
}
