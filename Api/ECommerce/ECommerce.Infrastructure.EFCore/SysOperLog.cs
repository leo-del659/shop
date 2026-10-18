using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 系统操作日志表
    /// </summary>
    public partial class SysOperLog
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime? OperTime { get; set; }
        /// <summary>
        /// 请求URL
        /// </summary>
        public string? RequestUrl { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string? BusinessType { get; set; }
        /// <summary>
        /// 操作行为描述
        /// </summary>
        public string? OperAction { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public string? RequestMethod { get; set; }
        /// <summary>
        /// 请求参数
        /// </summary>
        public string? RequestParams { get; set; }
        /// <summary>
        /// 记录入库时间
        /// </summary>
        public DateTime? CreateAt { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        public string? Result { get; set; }
    }
}
