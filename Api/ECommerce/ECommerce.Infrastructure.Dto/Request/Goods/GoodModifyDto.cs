using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.Dto.Request.Goods
{
    /// <summary>
    /// 商品修改 DTO
    /// </summary>
    public class GoodModifyDto
    {
        public string? id { get; set; }
        public string? category_id { get; set; }
        public string? goods_name { get; set; }
        public decimal? price { get; set; }
        public decimal? sale_price { get; set; }
        public int? stock { get; set; }
        public string? cover_img { get; set; }
        public string? img_list { get; set; }
        /// <summary>
        /// 商品图片地址列表（与 good.img_list 逗号分隔互为来源，后端写库时统一合并到 img_list）
        /// </summary>
        public List<string>? img_urls { get; set; }
        public string? unit { get; set; }
        /// <summary>
        /// SKU 列表（规格 + 价格 + 库存）
        /// </summary>
        public List<GoodSkuDto>? skus { get; set; }
    }
}
