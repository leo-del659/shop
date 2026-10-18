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
    /// 对Sys_Users 表的 所有数据库操作
    /// </summary>
    [Provider_,Inject_]
    public class Sys_User_EFCore : Base_EFCore<SysUser>
    {
        public Sys_User_EFCore(I_Repository<SysUser> repository) : base(repository)
        {
        }
    }
}
