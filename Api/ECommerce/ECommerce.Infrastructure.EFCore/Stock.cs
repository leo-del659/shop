using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 库存表
    /// </summary>
    public partial class Stock
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 商品ID
        /// </summary>
        public string? GoodsId { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string? Warehouse { get; set; }
        /// <summary>
        /// 总库存
        /// </summary>
        public int? TotalStock { get; set; }
        /// <summary>
        /// 锁定库存
        /// </summary>
        public int? LockStock { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
