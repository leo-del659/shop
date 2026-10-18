using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商品规格表
    /// </summary>
    public partial class GoodsSpec
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 商品ID(good.id)
        /// </summary>
        public string GoodsId { get; set; } = null!;
        /// <summary>
        /// 规格名称 如：500g/份
        /// </summary>
        public string? SpecName { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
