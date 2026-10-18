using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 消费者收货地址表
    /// </summary>
    public partial class MemberAddress
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 所属消费者标识（登录凭据code，与订单client_id一致）
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 收件人
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 收件手机号
        /// </summary>
        public string? Mobile { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string? Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }
        /// <summary>
        /// 区/县
        /// </summary>
        public string? District { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string? DetailAddr { get; set; }
        /// <summary>
        /// 0普通 1默认地址
        /// </summary>
        public int? IsDefault { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
