using System;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.EFCore
{
	/// <summary>
	/// order_event_log 实体注册（RabbitMQ 消费幂等表）。
	/// 注意：OnModelCreatingPartial 只能有一个实现，实体映射配置已合并进
	/// _111Context.ModulePartial.cs，本文件仅补充 DbSet。
	/// </summary>
	public partial class _111Context
	{
		public virtual DbSet<OrderEventLog> OrderEventLogs { get; set; } = null!;
	}
}
