using System.Collections.Generic;

namespace ECommerce.Infrastructure.Dto.Request.Order
{
    public class CreateOrderRequestDto
    {
        public string? Remark { get; set; }

        public string? DeliveryDate { get; set; }

        /// <summary>
        /// 收货人姓名（来自收货地址）
        /// </summary>
        public string? ReceiverName { get; set; }

        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string? ReceiverMobile { get; set; }

        /// <summary>
        /// 收货详细地址
        /// </summary>
        public string? Address { get; set; }

        public List<CreateOrderItemRequestDto> Items { get; set; } = new List<CreateOrderItemRequestDto>();
    }

    public class CreateOrderItemRequestDto
    {
        public string? GoodsId { get; set; }

        /// <summary>
        /// 选中的 SKU 规格 ID（可空；为空时按下单商品的基准售价计算）
        /// </summary>
        public string? SkuId { get; set; }

        public int BuyNum { get; set; }
    }
}