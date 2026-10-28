using System;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// merchant_notice 实体注册（商户通知：库存预警等）。
    /// 注意：OnModelCreatingPartial 只能有一个实现，实体映射配置在
    /// _111Context.ModulePartial.cs，本文件仅补充 DbSet。
    /// </summary>
    public partial class _111Context
    {
        public virtual DbSet<MerchantNotice> MerchantNotices { get; set; } = null!;
    }
}
