using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Member;
using ECommerce.Infrastructure.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface IMemberAddressService
    {
        /// <summary>
        /// 获取当前消费者的收货地址列表（默认地址排在最前）
        /// </summary>
        Task<Api_Response> GetListAsync(string userId);

        /// <summary>
        /// 获取单个收货地址
        /// </summary>
        Task<Api_Response> GetAsync(string userId, string id);

        /// <summary>
        /// 新增或更新收货地址
        /// </summary>
        Task<Api_Response> SaveAsync(string userId, MemberAddress_Request_Dto dto);

        /// <summary>
        /// 删除收货地址
        /// </summary>
        Task<Api_Response> RemoveAsync(string userId, string id);

        /// <summary>
        /// 设为默认地址
        /// </summary>
        Task<Api_Response> SetDefaultAsync(string userId, string id);
    }
}
