using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商品SKU规格表
    /// </summary>
    public partial class GoodsSku
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 商品ID(good.id)
        /// </summary>
        public string GoodsId { get; set; } = null!;
        /// <summary>
        /// 规格ID(goods_spec.id)
        /// </summary>
        public string? SpecId { get; set; }
        /// <summary>
        /// SKU名称
        /// </summary>
        public string? SkuName { get; set; }
        /// <summary>
        /// SKU价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// SKU库存
        /// </summary>
        public int Stock { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
