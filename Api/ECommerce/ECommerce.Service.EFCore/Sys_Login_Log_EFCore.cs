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
    /// SysLoginLog 表的 所有数据库操作
    /// </summary>
    [Provider_,Inject_]
    public class Sys_Login_Log_EFCore : Base_EFCore<SysLoginLog>
    {
        public Sys_Login_Log_EFCore(I_Repository<SysLoginLog> repository) : base(repository)
        {
        }
    }
}
