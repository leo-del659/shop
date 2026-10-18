using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商品多级分类表
    /// </summary>
    public partial class GoodsCategory
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 父分类ID，一级分类为空
        /// </summary>
        public string? ParentId { get; set; }
        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; } = null!;
        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 0启用 1禁用
        /// </summary>
        public int? IsBan { get; set; }
        /// <summary>
        /// 分类图标
        /// </summary>
        public string? Icon { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
