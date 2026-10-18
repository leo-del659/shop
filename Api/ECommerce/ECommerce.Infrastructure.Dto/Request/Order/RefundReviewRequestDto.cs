using System;

namespace ECommerce.Infrastructure.Dto.Request.Order
{
    /// <summary>
    /// 后台审核退款/退货
    /// </summary>
    public class RefundReviewRequestDto
    {
        /// <summary>
        /// 退款单ID
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 是否同意 true=同意(退款/退货) false=驳回
        /// </summary>
        public bool Approve { get; set; }

        /// <summary>
        /// 审核备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
