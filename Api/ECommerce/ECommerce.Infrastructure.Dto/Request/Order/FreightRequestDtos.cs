using System.Collections.Generic;

namespace ECommerce.Infrastructure.Dto.Request.Order
{
    /// <summary>
    /// 运费计算请求
    /// 两种用法（二选一，优先 orderId）：
    /// 1) 传 orderId：按已有订单的商品明细与金额计算（同时落库/更新运费单）；
    /// 2) 传 items：下单前的预览计算，不落库（用于「确认订单」页展示运费）。
    /// </summary>
    public class FreightCalcRequestDto
    {
        /// <summary>
        /// 关联订单ID（已有订单时必传）
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// 收货详细地址（用于后续按区域计费的扩展，当前仅记录）
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 商品明细（下单前预览计算时传）
        /// </summary>
        public List<FreightCalcItemDto> Items { get; set; } = new List<FreightCalcItemDto>();
    }

    /// <summary>
    /// 运费计算-商品明细项
    /// </summary>
    public class FreightCalcItemDto
    {
        public string? GoodsId { get; set; }

        /// <summary>
        /// 选中的 SKU 规格ID（可空，为空时按商品售价计算）
        /// </summary>
        public string? SkuId { get; set; }

        /// <summary>
        /// 购买数量（按件计费的件数口径）
        /// </summary>
        public int BuyNum { get; set; }
    }

    /// <summary>
    /// 运费支付请求
    /// </summary>
    public class FreightPayRequestDto
    {
        /// <summary>
        /// 关联订单ID（必传）
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// 运费单ID（可空；不传时按 orderId 定位或新建运费单）
        /// </summary>
        public string? FreightId { get; set; }

        /// <summary>
        /// 支付渠道：1 微信（默认）/ 2 支付宝
        /// </summary>
        public int? PayChannel { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
