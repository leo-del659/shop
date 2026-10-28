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
        /// 规格组名称 如：规格/重量/包装
        /// </summary>
        public string? SpecName { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
