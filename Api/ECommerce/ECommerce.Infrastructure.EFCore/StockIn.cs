using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 入库记录
    /// </summary>
    public partial class StockIn
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 商品ID
        /// </summary>
        public string? GoodsId { get; set; }
        /// <summary>
        /// 入库数量
        /// </summary>
        public int? InNum { get; set; }
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
