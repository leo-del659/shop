using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Request.Member
{
    /// <summary>
    /// 收货地址保存（新增/更新）请求
    /// </summary>
    public class MemberAddress_Request_Dto
    {
        /// <summary>
        /// 地址主键（更新时必填，新增时忽略）
        /// </summary>
        public string? Id { get; set; }

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
        /// 是否设为默认地址：1 设默认，0 或不传取消默认（仅当该用户已有默认地址时）
        /// </summary>
        public int? IsDefault { get; set; }
    }

    /// <summary>
    /// 通用 ID 请求体（用于删除、设为默认等只需主键的操作）
    /// </summary>
    public class IdRequestDto
    {
        public string? Id { get; set; }
    }

    /// <summary>
    /// 收货地址响应
    /// </summary>
    public class MemberAddress_Response_Dto
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? DetailAddr { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public int? IsDefault { get; set; }
    }
}
