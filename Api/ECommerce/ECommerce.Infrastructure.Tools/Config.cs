using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Tools
{
    public static class Config
    {

        /// <summary>
        /// 
        /// </summary>
        public static string NewSequentialGuid()
        {
           

            return Guid.NewGuid().ToString().Replace("-","");
        }

    }
}
