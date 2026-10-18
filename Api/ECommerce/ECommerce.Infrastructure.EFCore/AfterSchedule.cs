using System;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// 定时任务
    /// </summary>
    public partial class AfterSchedule
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 任务名称
        /// </summary>
        public string? JobName { get; set; }
        /// <summary>
        /// cron表达式
        /// </summary>
        public string? Cron { get; set; }
        /// <summary>
        /// 任务组
        /// </summary>
        public string? JobGroup { get; set; }
        /// <summary>
        /// 0暂停 1运行
        /// </summary>
        public int? Status { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
