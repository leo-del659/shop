using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Login;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class Log_Service : Base_Service, I_Log_Service
    {
        private readonly Sys_Oper_Log_EFCore _sys_Oper_Log_EFCore;

        public Log_Service(Sys_Oper_Log_EFCore sys_Oper_Log_EFCore)
        {
            _sys_Oper_Log_EFCore = sys_Oper_Log_EFCore;

        }

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="sysOperLog"></param>
        /// <returns></returns>
        public Api_Response Add_Action(SysOperLog sysOperLog)
        {

            _sys_Oper_Log_EFCore.Add(sysOperLog);

           var result= _sys_Oper_Log_EFCore.SaveChanges();
            return Get_Result(result);
        }
    }
}
