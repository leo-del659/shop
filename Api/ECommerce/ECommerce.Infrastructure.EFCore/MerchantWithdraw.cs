using System;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户提现记录
    /// </summary>
    public partial class MerchantWithdraw
    {
        public long Id { get; set; }
        /// <summary>供货商户ID（merchant 表）</summary>
        public string MerchantId { get; set; } = null!;
        /// <summary>提现金额</summary>
        public decimal Amount { get; set; }
        /// <summary>开户行</summary>
        public string? BankName { get; set; }
        /// <summary>银行账号</summary>
        public string? BankAccount { get; set; }
        /// <summary>开户名</summary>
        public string? AccountName { get; set; }
        /// <summary>备注</summary>
        public string? Remark { get; set; }
        /// <summary>1待处理 2已打款</summary>
        public int Status { get; set; }
        /// <summary>商户转账单号（本地生成，全局唯一，用于幂等与对账）</summary>
        public string? OutTransferNo { get; set; }
        /// <summary>微信转账单号（transfer_no），由微信返回</summary>
        public string? TransferNo { get; set; }
        /// <summary>微信转账状态 0未发起 1受理中 2成功 3失败</summary>
        public int TransferStatus { get; set; }
        /// <summary>转账失败原因（TransferStatus=3 时）</summary>
        public string? FailReason { get; set; }
        /// <summary>转账完成时间</summary>
        public DateTime? TransferTime { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
