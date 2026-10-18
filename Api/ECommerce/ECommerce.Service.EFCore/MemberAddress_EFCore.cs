using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Service.EFCore
{
    /// <summary>
    /// 对 member_address 表的所有数据库操作
    /// </summary>
    [Provider_, Inject_]
    public class MemberAddress_EFCore : Base_EFCore<MemberAddress>
    {
        public MemberAddress_EFCore(I_Repository<MemberAddress> repository) : base(repository)
        {
        }
    }
}
