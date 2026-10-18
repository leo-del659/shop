using System;

namespace ECommerce.Service.Interface
{
    public class PayCallbackRequestDto
    {
        /// <summary>微信支付流水号（transaction_id）。回调落库时优先写入 order_pay.pay_no 供对账核对。</summary>
        public string? TransactionId { get; set; }
    }
}