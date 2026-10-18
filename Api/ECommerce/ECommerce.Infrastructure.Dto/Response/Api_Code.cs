using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Dto.Response
{
    [Serializable]
    public enum Api_Code
    {
        /// <summary>
        /// 代表操作成功
        /// </summary>
        ok = 0,
        /// <summary>
        /// 代表操作失败
        /// </summary>
        fail = -1,
        /// <summary>
        /// 代表操作异常
        /// </summary>
        error = -500

    }
}
