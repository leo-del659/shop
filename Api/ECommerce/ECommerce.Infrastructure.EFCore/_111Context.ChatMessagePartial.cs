using System;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// chat_message 实体注册（买卖家聊天消息表）。
    /// 注意：OnModelCreatingPartial 只能有一个实现，实体映射配置已合并进
    /// _111Context.ModulePartial.cs，本文件仅补充 DbSet。
    /// </summary>
    public partial class _111Context
    {
        public virtual DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    }
}
