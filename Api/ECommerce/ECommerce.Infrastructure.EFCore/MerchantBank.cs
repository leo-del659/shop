using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户银行账户
    /// </summary>
    public partial class MerchantBank
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 商户ID
        /// </summary>
        public string? MerchantId { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        public string? BankName { get; set; }
        /// <summary>
        /// 银行账号
        /// </summary>
        public string? BankAccount { get; set; }
        /// <summary>
        /// 开户名
        /// </summary>
        public string? AccountName { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
