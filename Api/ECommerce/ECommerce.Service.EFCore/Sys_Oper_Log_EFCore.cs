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
    /// 
    /// </summary>
    [Provider_,Inject_]
    public class Sys_Oper_Log_EFCore : Base_EFCore<SysOperLog>
    {
        public Sys_Oper_Log_EFCore(I_Repository<SysOperLog> repository) : base(repository)
        {
        }
    }
}
