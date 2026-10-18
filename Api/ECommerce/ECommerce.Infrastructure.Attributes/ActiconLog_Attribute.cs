using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Attributes
{
    /// <summary>
    /// 行为日志的标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ActiconLog_Attribute:Attribute
    {
        /// <summary>
        /// 分类
        /// </summary>
        public string? group { get; set; }

        /// <summary>
        /// 具体行为
        /// </summary>
        public string? action { get; set; }
    }
}
