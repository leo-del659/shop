using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Login;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{

    [Provider_]
    public interface I_Log_Service
    {
        /// <summary>
        /// 添加行为日志
        /// </summary>
        /// <param name="sysOperLog"></param>
        /// <returns></returns>
        public Api_Response Add_Action(SysOperLog sysOperLog);


    }
}
