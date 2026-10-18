using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 出库记录
    /// </summary>
    public partial class StockOut
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 商品ID
        /// </summary>
        public string? GoodsId { get; set; }
        /// <summary>
        /// 出库数量
        /// </summary>
        public int? OutNum { get; set; }
        /// <summary>
        /// 批次号
        /// </summary>
        public string? BatchNo { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string? Operator { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
