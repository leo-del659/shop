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
        /// SKU名称（规格值文本，如：500g/盒）
        /// </summary>
        public string? SkuName { get; set; }
        /// <summary>
        /// SKU编码（商家自定义货号）
        /// </summary>
        public string? SkuCode { get; set; }
        /// <summary>
        /// SKU价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// SKU库存
        /// </summary>
        public int Stock { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
