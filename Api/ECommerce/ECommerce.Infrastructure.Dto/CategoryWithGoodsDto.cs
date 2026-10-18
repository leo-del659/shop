using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.Dto
{
    public class CategoryWithGoodsDto
    {
        public string Id { get; set; } = null!;
        public string? ParentId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? Icon { get; set; }
        public int? Sort { get; set; }
        public int? IsBan { get; set; }
        public DateTime? CreateTime { get; set; }

        public List<CategoryWithGoodsDto> Children { get; set; } = new List<CategoryWithGoodsDto>();
        public List<GoodsDto> Goods { get; set; } = new List<GoodsDto>();
    }
}