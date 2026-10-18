using System;

namespace ECommerce.Infrastructure.Dto
{
    public class GoodsDto
    {
        public string Id { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
        public string GoodsName { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal SalePrice { get; set; }
        public int Stock { get; set; }
        public string? CoverImg { get; set; }
        public string? Spec { get; set; }
        public string? Unit { get; set; }
        public int? IsSale { get; set; }
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 累计已售数量（统计已支付且未取消/未删除订单的购买总件数，非数据库字段）。
        /// </summary>
        public int SoldCount { get; set; }
    }
}