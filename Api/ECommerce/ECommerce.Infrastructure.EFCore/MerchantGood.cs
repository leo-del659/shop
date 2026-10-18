using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商户商品
    /// </summary>
    public partial class MerchantGood
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 商户ID
        /// </summary>
        public string? MerchantId { get; set; }
        /// <summary>
        /// 商品ID(goods_info.id)
        /// </summary>
        public string? GoodsId { get; set; }
        /// <summary>
        /// 供货价
        /// </summary>
        public decimal? SupplyPrice { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
