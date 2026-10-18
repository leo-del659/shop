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
    /// 对 sms_code 表的所有数据库操作
    /// </summary>
    [Provider_, Inject_]
    public class Sms_Code_EFCore : Base_EFCore<SmsCode>
    {
        public Sms_Code_EFCore(I_Repository<SmsCode> repository) : base(repository)
        {
        }
    }
}
