using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 商品审核主表(后台)
    /// </summary>
    public partial class Good
    {
        public string Id { get; set; } = null!;

        /// <summary>
        /// 累计已售数量（非数据库列，由订单统计后赋值，仅用于展示）
        /// </summary>
        [NotMapped]
        public int SoldCount { get; set; }
        /// <summary>
        /// 所属分类ID
        /// </summary>
        public string CategoryId { get; set; } = null!;
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodsName { get; set; } = null!;
        /// <summary>
        /// 原价
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 售卖价/团购价
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }
        /// <summary>
        /// 封面图
        /// </summary>
        public string? CoverImg { get; set; }
        /// <summary>
        /// 多图逗号分隔
        /// </summary>
        public string? ImgList { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string? Unit { get; set; }
        /// <summary>
        /// 0待审核 1通过 2驳回
        /// </summary>
        public int? AuditStatus { get; set; }
        /// <summary>
        /// 关联审核记录audit_record.id
        /// </summary>
        public string? AuditId { get; set; }
        /// <summary>
        /// 1上架 0下架
        /// </summary>
        public int? IsSale { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
