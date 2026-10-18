using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.Dto.Request.Goods
{
    /// <summary>
    /// 商品新增 DTO（后台录入，进入审核流程）
    /// </summary>
    public class GoodCreateDto
    {
        public string? category_id { get; set; }
        public string? goods_name { get; set; }
        public decimal? price { get; set; }
        public decimal? sale_price { get; set; }
        public int? stock { get; set; }
        public string? cover_img { get; set; }
        public string? img_list { get; set; }
        public string? unit { get; set; }
        /// <summary>
        /// 所属商户 id（商品挂在该商户名下）
        /// </summary>
        public string? merchant_id { get; set; }
        /// <summary>
        /// 商品图片地址列表
        /// </summary>
        public List<string>? img_urls { get; set; }
        /// <summary>
        /// SKU 列表（规格 + 价格 + 库存）
        /// </summary>
        public List<GoodSkuDto>? skus { get; set; }
    }

    public class GoodSkuDto
    {
        public string? spec_name { get; set; }
        public string? sku_name { get; set; }
        public decimal? price { get; set; }
        public int? stock { get; set; }
    }
}
