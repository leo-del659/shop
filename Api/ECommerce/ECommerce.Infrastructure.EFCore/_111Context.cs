using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ECommerce.Infrastructure.EFCore
{
    public partial class _111Context : DbContext
    {
        public _111Context()
        {
        }

        public _111Context(DbContextOptions<_111Context> options)
            : base(options)
        {
        }

        public virtual DbSet<AfterSchedule> AfterSchedules { get; set; } = null!;
        public virtual DbSet<AuditRecord> AuditRecords { get; set; } = null!;
        public virtual DbSet<Good> Goods { get; set; } = null!;
        public virtual DbSet<GoodsCategory> GoodsCategories { get; set; } = null!;
        public virtual DbSet<GoodsSku> GoodsSkus { get; set; } = null!;
        public virtual DbSet<GoodsSpec> GoodsSpecs { get; set; } = null!;
        public virtual DbSet<GoodsReview> GoodsReviews { get; set; } = null!;
        public virtual DbSet<LogisticsCompany> LogisticsCompanies { get; set; } = null!;
        public virtual DbSet<MemberAddress> MemberAddresses { get; set; } = null!;
        public virtual DbSet<ClientOrder> ClientOrders { get; set; } = null!;
        public virtual DbSet<OrderFreight> OrderFreights { get; set; } = null!;
        public virtual DbSet<OrderDelivery> OrderDeliveries { get; set; } = null!;
        public virtual DbSet<OrderPay> OrderPays { get; set; } = null!;
        public virtual DbSet<OrderRefund> OrderRefunds { get; set; } = null!;
        public virtual DbSet<OrderStatusLog> OrderStatusLogs { get; set; } = null!;
        public virtual DbSet<SettlePay> SettlePays { get; set; } = null!;
        public virtual DbSet<SettleRecord> SettleRecords { get; set; } = null!;
        public virtual DbSet<SettleRefund> SettleRefunds { get; set; } = null!;
        public virtual DbSet<SmsCode> SmsCodes { get; set; } = null!;
        public virtual DbSet<Stock> Stocks { get; set; } = null!;
        public virtual DbSet<StockIn> StockIns { get; set; } = null!;
        public virtual DbSet<StockOut> StockOuts { get; set; } = null!;
        public virtual DbSet<Merchant> Merchants { get; set; } = null!;
        public virtual DbSet<MerchantBank> MerchantBanks { get; set; } = null!;
        public virtual DbSet<MerchantGood> MerchantGood { get; set; } = null!;
        public virtual DbSet<MerchantLevel> MerchantLevels { get; set; } = null!;
        public virtual DbSet<MerchantQualification> MerchantQualifications { get; set; } = null!;
        public virtual DbSet<MerchantBill> MerchantBills { get; set; } = null!;
        public virtual DbSet<ClientLogin> ClientLogins { get; set; } = null!;
        public virtual DbSet<MerchantWithdraw> MerchantWithdraws { get; set; } = null!;
        public virtual DbSet<SysConfig> SysConfigs { get; set; } = null!;
        public virtual DbSet<SysDepartment> SysDepartments { get; set; } = null!;
        public virtual DbSet<SysDeptRole> SysDeptRoles { get; set; } = null!;
        public virtual DbSet<SysDeptRoleMenu> SysDeptRoleMenus { get; set; } = null!;
        public virtual DbSet<SysDict> SysDicts { get; set; } = null!;
        public virtual DbSet<SysLoginLog> SysLoginLogs { get; set; } = null!;
        public virtual DbSet<SysMenu> SysMenus { get; set; } = null!;
        public virtual DbSet<SysNotice> SysNotices { get; set; } = null!;
        public virtual DbSet<SysOperLog> SysOperLogs { get; set; } = null!;
        public virtual DbSet<SysRole> SysRoles { get; set; } = null!;
        public virtual DbSet<SysUser> SysUsers { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySql("server=localhost;user id=root;password=123456;database=111", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.46-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<AfterSchedule>(entity =>
            {
                entity.ToTable("after_schedule");

                entity.HasComment("定时任务");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.Cron)
                    .HasMaxLength(100)
                    .HasColumnName("cron")
                    .HasComment("cron表达式");

                entity.Property(e => e.JobGroup)
                    .HasMaxLength(50)
                    .HasColumnName("job_group")
                    .HasComment("任务组");

                entity.Property(e => e.JobName)
                    .HasMaxLength(100)
                    .HasColumnName("job_name")
                    .HasComment("任务名称");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0暂停 1运行");
            });

            modelBuilder.Entity<AuditRecord>(entity =>
            {
                entity.ToTable("audit_record");

                entity.HasComment("审核记录表");

                entity.HasIndex(e => new { e.BizType, e.BizId }, "idx_audit_biz");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.AuditRemark)
                    .HasMaxLength(500)
                    .HasColumnName("audit_remark")
                    .HasComment("审核意见");

                entity.Property(e => e.AuditStatus)
                    .HasColumnName("audit_status")
                    .HasComment("1通过 2驳回");

                entity.Property(e => e.Auditor)
                    .HasMaxLength(50)
                    .HasColumnName("auditor")
                    .HasComment("审核人");

                entity.Property(e => e.BizId)
                    .HasMaxLength(50)
                    .HasColumnName("biz_id")
                    .HasComment("业务主键");

                entity.Property(e => e.BizType)
                    .HasMaxLength(50)
                    .HasColumnName("biz_type")
                    .HasComment("业务类型 商品/买家/商户");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");
            });

            modelBuilder.Entity<Good>(entity =>
            {
                entity.ToTable("good");

                entity.HasComment("商品审核主表(后台)");

                entity.HasIndex(e => e.AuditStatus, "idx_good_audit");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.AuditId)
                    .HasMaxLength(50)
                    .HasColumnName("audit_id")
                    .HasComment("关联审核记录audit_record.id");

                entity.Property(e => e.AuditStatus)
                    .HasColumnName("audit_status")
                    .HasComment("0待审核 1通过 2驳回 未提交(null)");

                entity.Property(e => e.CategoryId)
                    .HasMaxLength(50)
                    .HasColumnName("category_id")
                    .HasComment("所属分类ID");

                entity.Property(e => e.CoverImg)
                    .HasMaxLength(256)
                    .HasColumnName("cover_img")
                    .HasComment("封面图");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.GoodsName)
                    .HasMaxLength(128)
                    .HasColumnName("goods_name")
                    .HasComment("商品名称");

                entity.Property(e => e.ImgList)
                    .HasMaxLength(1000)
                    .HasColumnName("img_list")
                    .HasComment("多图逗号分隔");

                entity.Property(e => e.IsSale)
                    .HasColumnName("is_sale")
                    .HasDefaultValueSql("'0'")
                    .HasComment("1上架 0下架");

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2)
                    .HasColumnName("price")
                    .HasComment("原价");

                entity.Property(e => e.SalePrice)
                    .HasPrecision(18, 2)
                    .HasColumnName("sale_price")
                    .HasComment("售卖价/团购价");

                entity.Property(e => e.Stock)
                    .HasColumnName("stock")
                    .HasComment("库存");

                entity.Property(e => e.Unit)
                    .HasMaxLength(16)
                    .HasColumnName("unit")
                    .HasComment("单位");

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("update_time");
            });

            modelBuilder.Entity<GoodsCategory>(entity =>
            {
                entity.ToTable("goods_category");

                entity.HasComment("商品多级分类表");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .HasColumnName("category_name")
                    .HasComment("分类名称");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.Icon)
                    .HasMaxLength(256)
                    .HasColumnName("icon")
                    .HasComment("分类图标");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0启用 1禁用");

                entity.Property(e => e.ParentId)
                    .HasMaxLength(50)
                    .HasColumnName("parent_id")
                    .HasComment("父分类ID，一级分类为空");

                entity.Property(e => e.Sort)
                    .HasColumnName("sort")
                    .HasDefaultValueSql("'0'")
                    .HasComment("排序");
            });

            modelBuilder.Entity<GoodsSku>(entity =>
            {
                entity.ToTable("goods_sku");

                entity.HasComment("商品SKU规格表");

                entity.HasIndex(e => e.GoodsId, "idx_sku_goods");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(50)
                    .HasColumnName("goods_id")
                    .HasComment("商品ID(good.id)");

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2)
                    .HasColumnName("price")
                    .HasComment("SKU价格");

                entity.Property(e => e.SkuName)
                    .HasMaxLength(200)
                    .HasColumnName("sku_name")
                    .HasComment("SKU名称");

                entity.Property(e => e.SpecId)
                    .HasMaxLength(50)
                    .HasColumnName("spec_id")
                    .HasComment("规格ID(goods_spec.id)");

                entity.Property(e => e.Stock)
                    .HasColumnName("stock")
                    .HasComment("SKU库存");
            });

            modelBuilder.Entity<GoodsSpec>(entity =>
            {
                entity.ToTable("goods_spec");

                entity.HasComment("商品规格表");

                entity.HasIndex(e => e.GoodsId, "idx_gs_goods");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(50)
                    .HasColumnName("goods_id")
                    .HasComment("商品ID(good.id)");

                entity.Property(e => e.SpecName)
                    .HasMaxLength(128)
                    .HasColumnName("spec_name")
                    .HasComment("规格名称 如：500g/份");
            });

            modelBuilder.Entity<GoodsReview>(entity =>
            {
                entity.ToTable("goods_review");

                entity.HasComment("商品用户评价表");

                entity.HasIndex(e => e.GoodsId, "idx_gr_goods");

                entity.HasIndex(e => e.MemberId, "idx_gr_member");

                entity.HasIndex(e => e.OrderId, "idx_gr_order");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(50)
                    .HasColumnName("goods_id")
                    .HasComment("商品ID(good.id)");

                entity.Property(e => e.GoodsName)
                    .HasMaxLength(128)
                    .HasColumnName("goods_name")
                    .HasComment("商品名称快照");

                entity.Property(e => e.SkuId)
                    .HasMaxLength(50)
                    .HasColumnName("sku_id")
                    .HasComment("规格ID(goods_sku.id)");

                entity.Property(e => e.SkuName)
                    .HasMaxLength(200)
                    .HasColumnName("sku_name")
                    .HasComment("规格名称快照");

                entity.Property(e => e.MemberId)
                    .HasMaxLength(50)
                    .HasColumnName("member_id")
                    .HasComment("评价人标识(登录凭据code)");

                entity.Property(e => e.MemberName)
                    .HasMaxLength(100)
                    .HasColumnName("member_name")
                    .HasComment("评价人昵称");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(500)
                    .HasColumnName("avatar")
                    .HasComment("评价人头像快照");

                entity.Property(e => e.OrderId)
                    .HasMaxLength(50)
                    .HasColumnName("order_id")
                    .HasComment("关联订单ID");

                entity.Property(e => e.Rating)
                    .HasColumnName("rating")
                    .HasDefaultValueSql("'5'")
                    .HasComment("评分 1-5");

                entity.Property(e => e.Content)
                    .HasMaxLength(1000)
                    .HasColumnName("content")
                    .HasComment("评价内容");

                entity.Property(e => e.Images)
                    .HasMaxLength(1000)
                    .HasColumnName("images")
                    .HasComment("评价图，逗号分隔");

                entity.Property(e => e.Tags)
                    .HasMaxLength(255)
                    .HasColumnName("tags")
                    .HasComment("标签，逗号分隔 如：口感好,包装精美");

                entity.Property(e => e.ClientReview)
                    .HasMaxLength(1000)
                    .HasColumnName("merchant_review")
                    .HasComment("对商户评价(独立文本)（库列名为历史遗留 merchant_review）");

                entity.Property(e => e.IsAnonymous)
                    .HasColumnName("is_anonymous")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否匿名:0否 1是");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0待审核 1通过 2驳回");

                entity.Property(e => e.Reply)
                    .HasMaxLength(1000)
                    .HasColumnName("reply")
                    .HasComment("商家回复");

                entity.Property(e => e.ReplyTime)
                    .HasColumnType("datetime")
                    .HasColumnName("reply_time")
                    .HasComment("回复时间");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.RejectReason)
                    .HasMaxLength(500)
                    .HasColumnName("reject_reason")
                    .HasComment("驳回原因");
            });

            modelBuilder.Entity<LogisticsCompany>(entity =>
            {
                entity.ToTable("logistics_company");

                entity.HasComment("物流公司");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .HasColumnName("code")
                    .HasComment("物流编码");

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(100)
                    .HasColumnName("company_name")
                    .HasComment("物流公司名称");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用");

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .HasColumnName("phone")
                    .HasComment("联系电话");
            });

            modelBuilder.Entity<MemberAddress>(entity =>
            {
                entity.ToTable("member_address");

                entity.HasComment("消费者收货地址表");

                entity.HasIndex(e => e.UserId, "idx_member_user_id");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .HasColumnName("city")
                    .HasComment("城市");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.DetailAddr)
                    .HasMaxLength(500)
                    .HasColumnName("detail_addr")
                    .HasComment("详细地址");

                entity.Property(e => e.District)
                    .HasMaxLength(50)
                    .HasColumnName("district")
                    .HasComment("区/县");

                entity.Property(e => e.IsDefault)
                    .HasColumnName("is_default")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0普通 1默认地址");

                entity.Property(e => e.Mobile)
                    .HasMaxLength(20)
                    .HasColumnName("mobile")
                    .HasComment("收件手机号");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name")
                    .HasComment("收件人");

                entity.Property(e => e.Province)
                    .HasMaxLength(50)
                    .HasColumnName("province")
                    .HasComment("省份");

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("update_time");

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .HasColumnName("user_id")
                    .HasComment("所属消费者标识（登录凭据code，与订单client_id一致）");
            });

            modelBuilder.Entity<OrderFreight>(entity =>
            {
                entity.ToTable("order_freight");

                entity.HasComment("订单运费单");

                entity.HasIndex(e => e.OrderId, "idx_of_order");

                entity.HasIndex(e => e.FreightNo, "uk_of_no")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.FreightNo)
                    .HasMaxLength(64)
                    .HasColumnName("freight_no")
                    .HasComment("运费单号 FR+yyMMddHHmmss+随机");

                entity.Property(e => e.OrderId)
                    .HasMaxLength(50)
                    .HasColumnName("order_id")
                    .HasComment("关联订单ID");

                entity.Property(e => e.OrderNo)
                    .HasMaxLength(64)
                    .HasColumnName("order_no")
                    .HasComment("关联订单编号");

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .HasColumnName("user_id")
                    .HasComment("下单用户ID");

                entity.Property(e => e.MerchantId)
                    .HasMaxLength(50)
                    .HasColumnName("merchant_id")
                    .HasComment("商户ID");

                entity.Property(e => e.GoodsAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName("goods_amount")
                    .HasDefaultValueSql("'0.00'")
                    .HasComment("商品金额（运费计费基数）");

                entity.Property(e => e.FreightAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName("freight_amount")
                    .HasDefaultValueSql("'0.00'")
                    .HasComment("运费金额");

                entity.Property(e => e.ItemCount)
                    .HasColumnName("item_count")
                    .HasDefaultValueSql("'0'")
                    .HasComment("计费件数");

                entity.Property(e => e.ChargeType)
                    .HasColumnName("charge_type")
                    .HasDefaultValueSql("'1'")
                    .HasComment("计费方式 1按件计费 2满额包邮");

                entity.Property(e => e.ChargeTypeText)
                    .HasMaxLength(32)
                    .HasColumnName("charge_type_text")
                    .HasComment("计费方式文案");

                entity.Property(e => e.ChargeRule)
                    .HasMaxLength(200)
                    .HasColumnName("charge_rule")
                    .HasComment("计费规则说明");

                entity.Property(e => e.FreeShipping)
                    .HasColumnName("free_shipping")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否包邮 0否 1是");

                entity.Property(e => e.TotalAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName("total_amount")
                    .HasDefaultValueSql("'0.00'")
                    .HasComment("应付合计=商品金额+运费金额");

                entity.Property(e => e.PayStatus)
                    .HasColumnName("pay_status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("运费支付状态 0未支付 1已支付");

                entity.Property(e => e.PayTime)
                    .HasColumnType("datetime")
                    .HasColumnName("pay_time")
                    .HasComment("运费支付时间");

                entity.Property(e => e.EffectiveTime)
                    .HasColumnType("datetime")
                    .HasColumnName("effective_time")
                    .HasComment("生效时间");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否作废 0正常 1作废");

                entity.Property(e => e.Remark)
                    .HasMaxLength(200)
                    .HasColumnName("remark")
                    .HasComment("备注");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("update_time");
            });

            modelBuilder.Entity<ClientOrder>(entity =>
            {
                entity.ToTable("clientorders");

                entity.HasComment("用户采购订单主表");

                entity.HasIndex(e => e.ClientId, "idx_client");

                entity.HasIndex(e => e.OrderStatus, "idx_status");

                entity.HasIndex(e => e.MerchantId, "idx_merchant");

                entity.HasIndex(e => e.OrderNo, "uk_order_no")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(500)
                    .HasColumnName("address")
                    .HasComment("收货详细地址");

                entity.Property(e => e.ReceiverName)
                    .HasMaxLength(64)
                    .HasColumnName("receiver_name")
                    .HasComment("收货人姓名");

                entity.Property(e => e.ReceiverMobile)
                    .HasMaxLength(32)
                    .HasColumnName("receiver_mobile")
                    .HasComment("收货人手机号");

                entity.Property(e => e.PayTime)
                    .HasColumnType("datetime")
                    .HasColumnName("pay_time")
                    .HasComment("付款时间");

                entity.Property(e => e.ShipTime)
                    .HasColumnType("datetime")
                    .HasColumnName("ship_time")
                    .HasComment("发货时间");

                entity.Property(e => e.CompleteTime)
                    .HasColumnType("datetime")
                    .HasColumnName("complete_time")
                    .HasComment("成交/完成时间");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用 0正常 1作废");

                entity.Property(e => e.ClientId)
                    .HasMaxLength(50)
                    .HasColumnName("client_id")
                    .HasComment("买家用户ID（sys_users.u_id）");

                entity.Property(e => e.OrderNo)
                    .HasMaxLength(64)
                    .HasColumnName("order_no")
                    .HasComment("订单编号");

                entity.Property(e => e.OrderStatus)
                    .HasColumnName("order_status")
                    .HasDefaultValueSql("'1'")
                    .HasComment("1待处理 2接单备货中 3备货完成待配送 4已签收 9拒绝");

                entity.Property(e => e.PayStatus)
                    .HasColumnName("pay_status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("支付状态 0未支付 1已支付（与 order_status 分离：支付回调只置本字段）");

                entity.Property(e => e.RejectReason)
                    .HasMaxLength(500)
                    .HasColumnName("reject_reason")
                    .HasComment("拒绝原因");

                entity.Property(e => e.Remark)
                    .HasMaxLength(500)
                    .HasColumnName("remark")
                    .HasComment("商户下单备注");

                entity.Property(e => e.GoodsInfo)
                    .HasMaxLength(500)
                    .HasColumnName("goods_info")
                    .HasComment("商品信息文本（单行摘要）");

                entity.Property(e => e.MerchantId)
                    .HasMaxLength(50)
                    .HasColumnName("merchant_id")
                    .HasComment("供货商户ID");

                entity.Property(e => e.TotalAmount)
                    .HasColumnName("total_amount")
                    .HasComment("订单总金额");

                entity.Property(e => e.ItemsJson)
                    .HasColumnName("items_json")
                    .HasComment("合并后的订单商品明细（JSON 数组，原 client_order_item 表数据内联）");

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("update_time");
            });

            modelBuilder.Entity<OrderDelivery>(entity =>
            {
                entity.ToTable("order_delivery");

                entity.HasComment("订单配送");

                entity.HasIndex(e => e.OrderId, "idx_od_order");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.DeliveryStatus)
                    .HasColumnName("delivery_status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0待发 1已发 2已签收");

                entity.Property(e => e.LogisticsId)
                    .HasMaxLength(50)
                    .HasColumnName("logistics_id")
                    .HasComment("物流公司ID");

                entity.Property(e => e.OrderId)
                    .HasMaxLength(50)
                    .HasColumnName("order_id")
                    .HasComment("订单ID");

                entity.Property(e => e.TrackingNo)
                    .HasMaxLength(64)
                    .HasColumnName("tracking_no")
                    .HasComment("运单号");
            });

            modelBuilder.Entity<OrderPay>(entity =>
            {
                entity.ToTable("order_pay");

                entity.HasComment("订单支付");

                entity.HasIndex(e => e.OrderId, "idx_op_order");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.OrderId)
                    .HasMaxLength(50)
                    .HasColumnName("order_id")
                    .HasComment("订单ID");

                entity.Property(e => e.PayAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName("pay_amount")
                    .HasComment("支付金额");

                entity.Property(e => e.PayNo)
                    .HasMaxLength(64)
                    .HasColumnName("pay_no")
                    .HasComment("支付流水号");

                entity.Property(e => e.PayStatus)
                    .HasColumnName("pay_status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0未付 1已付");

                entity.Property(e => e.PayType)
                    .HasColumnName("pay_type")
                    .HasComment("支付方式");
            });

            modelBuilder.Entity<OrderRefund>(entity =>
            {
                entity.ToTable("order_refund");

                entity.HasComment("订单退款");

                entity.HasIndex(e => e.OrderId, "idx_orf_order");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.OrderId)
                    .HasMaxLength(50)
                    .HasColumnName("order_id")
                    .HasComment("订单ID");

                entity.Property(e => e.RefundAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName("refund_amount")
                    .HasComment("退款金额");

                entity.Property(e => e.RefundNo)
                    .HasMaxLength(64)
                    .HasColumnName("refund_no")
                    .HasComment("退款单号");

                entity.Property(e => e.RefundReason)
                    .HasMaxLength(500)
                    .HasColumnName("refund_reason")
                    .HasComment("退款原因");

                entity.Property(e => e.RefundStatus)
                    .HasColumnName("refund_status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0待审核 1已退款 2驳回");

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .HasColumnName("user_id")
                    .HasComment("申请人用户ID");

                entity.Property(e => e.RefundType)
                    .HasColumnName("refund_type")
                    .HasDefaultValueSql("'2'")
                    .HasComment("1仅退款 2退货退款");

                entity.Property(e => e.RefundNum)
                    .HasColumnName("refund_num")
                    .HasComment("退货数量");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(50)
                    .HasColumnName("goods_id")
                    .HasComment("关联商品ID");

                entity.Property(e => e.GoodsName)
                    .HasMaxLength(200)
                    .HasColumnName("goods_name")
                    .HasComment("关联商品名称");

                entity.Property(e => e.GoodsImg)
                    .HasMaxLength(500)
                    .HasColumnName("goods_img")
                    .HasComment("关联商品图片");

                entity.Property(e => e.HandleRemark)
                    .HasMaxLength(500)
                    .HasColumnName("handle_remark")
                    .HasComment("审核备注");

                entity.Property(e => e.HandleTime)
                    .HasColumnType("datetime")
                    .HasColumnName("handle_time")
                    .HasComment("审核时间");

                entity.Property(e => e.HandleBy)
                    .HasMaxLength(50)
                    .HasColumnName("handle_by")
                    .HasComment("审核人ID");
            });

            modelBuilder.Entity<OrderStatusLog>(entity =>
            {
                entity.ToTable("order_status_log");

                entity.HasComment("订单状态日志");

                entity.HasIndex(e => e.OrderId, "idx_osl_order");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.Operator)
                    .HasMaxLength(50)
                    .HasColumnName("operator")
                    .HasComment("操作人");

                entity.Property(e => e.OrderId)
                    .HasMaxLength(50)
                    .HasColumnName("order_id")
                    .HasComment("订单ID");

                entity.Property(e => e.OrderStatus)
                    .HasColumnName("order_status")
                    .HasComment("订单状态");

                entity.Property(e => e.Remark)
                    .HasMaxLength(500)
                    .HasColumnName("remark")
                    .HasComment("备注");
            });


            modelBuilder.Entity<SettlePay>(entity =>
            {
                entity.ToTable("settle_pay");

                entity.HasComment("结算支付");

                entity.HasIndex(e => e.SettleId, "idx_sp_settle");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.PayAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName("pay_amount")
                    .HasComment("支付金额");

                entity.Property(e => e.PayNo)
                    .HasMaxLength(64)
                    .HasColumnName("pay_no")
                    .HasComment("支付流水号");

                entity.Property(e => e.PayStatus)
                    .HasColumnName("pay_status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0未付 1已付");

                entity.Property(e => e.SettleId)
                    .HasMaxLength(50)
                    .HasColumnName("settle_id")
                    .HasComment("结算记录ID");
            });

            modelBuilder.Entity<SettleRecord>(entity =>
            {
                entity.ToTable("settle_record");

                entity.HasComment("用户结算记录");

                entity.HasIndex(e => e.ClientId, "idx_sr_client");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.ClientId)
                    .HasMaxLength(50)
                    .HasColumnName("client_id")
                    .HasComment("买家用户ID（sys_users.u_id）");

                entity.Property(e => e.SettleAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName("settle_amount")
                    .HasComment("结算金额");

                entity.Property(e => e.SettleNo)
                    .HasMaxLength(64)
                    .HasColumnName("settle_no")
                    .HasComment("结算单号");

                entity.Property(e => e.SettleType)
                    .HasColumnName("settle_type")
                    .HasDefaultValueSql("'1'")
                    .HasComment("1线上 2线下");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0待结算 1已结算 2已退款");
            });

            modelBuilder.Entity<SettleRefund>(entity =>
            {
                entity.ToTable("settle_refund");

                entity.HasComment("结算退款");

                entity.HasIndex(e => e.SettleId, "idx_sref_settle");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.RefundAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName("refund_amount")
                    .HasComment("退款金额");

                entity.Property(e => e.RefundNo)
                    .HasMaxLength(64)
                    .HasColumnName("refund_no")
                    .HasComment("退款单号");

                entity.Property(e => e.RefundStatus)
                    .HasColumnName("refund_status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0未退 1已退");

                entity.Property(e => e.SettleId)
                    .HasMaxLength(50)
                    .HasColumnName("settle_id")
                    .HasComment("结算记录ID");
            });

            modelBuilder.Entity<SmsCode>(entity =>
            {
                entity.ToTable("sms_code");

                entity.HasComment("短信验证码表");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .HasColumnName("code")
                    .HasComment("验证码");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time")
                    .HasComment("创建时间");

                entity.Property(e => e.ExpireTime)
                    .HasColumnType("datetime")
                    .HasColumnName("expire_time")
                    .HasComment("过期时间");

                entity.Property(e => e.Ip)
                    .HasMaxLength(50)
                    .HasColumnName("ip")
                    .HasComment("发送IP");

                entity.Property(e => e.IsUsed)
                    .HasColumnName("is_used")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否已使用 0-否 1-是");

                entity.Property(e => e.Mobile)
                    .HasMaxLength(20)
                    .HasColumnName("mobile")
                    .HasComment("手机号码");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("'3'")
                    .HasComment("验证码类型 1-商户 2-商户 3-管理员");
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.ToTable("stock");

                entity.HasComment("库存表");

                entity.HasIndex(e => e.GoodsId, "idx_stock_goods");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(50)
                    .HasColumnName("goods_id")
                    .HasComment("商品ID");

                entity.Property(e => e.LockStock)
                    .HasColumnName("lock_stock")
                    .HasDefaultValueSql("'0'")
                    .HasComment("锁定库存");

                entity.Property(e => e.TotalStock)
                    .HasColumnName("total_stock")
                    .HasDefaultValueSql("'0'")
                    .HasComment("总库存");

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("update_time");

                entity.Property(e => e.Warehouse)
                    .HasMaxLength(100)
                    .HasColumnName("warehouse")
                    .HasComment("仓库");
            });

            modelBuilder.Entity<StockIn>(entity =>
            {
                entity.ToTable("stock_in");

                entity.HasComment("入库记录");

                entity.HasIndex(e => e.GoodsId, "idx_si_goods");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.BatchNo)
                    .HasMaxLength(50)
                    .HasColumnName("batch_no")
                    .HasComment("批次号");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(50)
                    .HasColumnName("goods_id")
                    .HasComment("商品ID");

                entity.Property(e => e.InNum)
                    .HasColumnName("in_num")
                    .HasDefaultValueSql("'0'")
                    .HasComment("入库数量");

                entity.Property(e => e.Operator)
                    .HasMaxLength(50)
                    .HasColumnName("operator")
                    .HasComment("操作人");
            });

            modelBuilder.Entity<StockOut>(entity =>
            {
                entity.ToTable("stock_out");

                entity.HasComment("出库记录");

                entity.HasIndex(e => e.GoodsId, "idx_so_goods");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.BatchNo)
                    .HasMaxLength(50)
                    .HasColumnName("batch_no")
                    .HasComment("批次号");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(50)
                    .HasColumnName("goods_id")
                    .HasComment("商品ID");

                entity.Property(e => e.Operator)
                    .HasMaxLength(50)
                    .HasColumnName("operator")
                    .HasComment("操作人");

                entity.Property(e => e.OutNum)
                    .HasColumnName("out_num")
                    .HasDefaultValueSql("'0'")
                    .HasComment("出库数量");
            });

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.ToTable("merchant_login");

                entity.HasComment("商户表（account 为登录手机号）");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id, "uk_merchant")
                    .IsUnique();

                // 手机号唯一，防止重复注册
                entity.HasIndex(e => e.Account, "uk_merchant_account")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("merchant_id")
                    .HasComment("商户业务ID");

                entity.Property(e => e.MerchantName)
                    .HasMaxLength(200)
                    .HasColumnName("merchant_name")
                    .HasComment("商户名称");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(500)
                    .HasColumnName("avatar")
                    .HasComment("头像图片地址");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用 0-正常 1-禁用");

                entity.Property(e => e.Account)
                    .HasMaxLength(100)
                    .HasColumnName("account")
                    .HasComment("登录手机号");

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .HasColumnName("password")
                    .HasComment("登录密码（PHC 格式哈希串）");

                entity.Property(e => e.Salt)
                    .HasMaxLength(20)
                    .HasColumnName("salt")
                    .HasComment("随机盐标记（仅作标记，校验时不读取）");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");
            });

            modelBuilder.Entity<MerchantBank>(entity =>
            {
                entity.ToTable("merchant_bank");

                entity.HasComment("商户银行账户");

                entity.HasIndex(e => e.MerchantId, "idx_sb_merchant");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.AccountName)
                    .HasMaxLength(100)
                    .HasColumnName("account_name")
                    .HasComment("开户名");

                entity.Property(e => e.BankAccount)
                    .HasMaxLength(50)
                    .HasColumnName("bank_account")
                    .HasComment("银行账号");

                entity.Property(e => e.BankName)
                    .HasMaxLength(100)
                    .HasColumnName("bank_name")
                    .HasComment("开户行");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.MerchantId)
                    .HasMaxLength(50)
                    .HasColumnName("merchant_id")
                    .HasComment("商户ID");
            });

            modelBuilder.Entity<MerchantGood>(entity =>
            {
                entity.ToTable("merchant_good");

                entity.HasComment("商户商品");

                entity.HasIndex(e => e.MerchantId, "idx_sg_merchant");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(50)
                    .HasColumnName("goods_id")
                    .HasComment("商品ID(good.id)");

                entity.Property(e => e.MerchantId)
                    .HasMaxLength(50)
                    .HasColumnName("merchant_id")
                    .HasComment("商户ID");

                entity.Property(e => e.SupplyPrice)
                    .HasPrecision(18, 2)
                    .HasColumnName("supply_price")
                    .HasComment("供货价");
            });

            modelBuilder.Entity<MerchantLevel>(entity =>
            {
                entity.ToTable("merchant_level");

                entity.HasComment("商户等级");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.LevelName)
                    .HasMaxLength(50)
                    .HasColumnName("level_name")
                    .HasComment("等级名称");

                entity.Property(e => e.Remark)
                    .HasMaxLength(500)
                    .HasColumnName("remark")
                    .HasComment("备注");

                entity.Property(e => e.Sort)
                    .HasColumnName("sort")
                    .HasDefaultValueSql("'0'")
                    .HasComment("排序");
            });

            modelBuilder.Entity<MerchantQualification>(entity =>
            {
                entity.ToTable("merchant_qualification");

                entity.HasComment("商户资质");

                entity.HasIndex(e => e.MerchantId, "idx_sq_merchant");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.ExpireDate)
                    .HasMaxLength(32)
                    .HasColumnName("expire_date")
                    .HasComment("到期日期");

                entity.Property(e => e.FileUrl)
                    .HasMaxLength(256)
                    .HasColumnName("file_url")
                    .HasComment("资质文件");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否作废");

                entity.Property(e => e.QualNo)
                    .HasMaxLength(100)
                    .HasColumnName("qual_no")
                    .HasComment("资质编号");

                entity.Property(e => e.QualType)
                    .HasMaxLength(100)
                    .HasColumnName("qual_type")
                    .HasComment("资质类型");

                entity.Property(e => e.MerchantId)
                    .HasMaxLength(50)
                    .HasColumnName("merchant_id")
                    .HasComment("商户ID");
            });

            modelBuilder.Entity<SysConfig>(entity =>
            {
                entity.ToTable("sys_config");

                entity.HasComment("系统参数配置");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.ConfigKey)
                    .HasMaxLength(100)
                    .HasColumnName("config_key")
                    .HasComment("配置键");

                entity.Property(e => e.ConfigValue)
                    .HasMaxLength(500)
                    .HasColumnName("config_value")
                    .HasComment("配置值");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.Remark)
                    .HasMaxLength(500)
                    .HasColumnName("remark")
                    .HasComment("备注");
            });

            modelBuilder.Entity<SysDepartment>(entity =>
            {
                entity.ToTable("sys_department");

                entity.HasComment("系统部门");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.DeptName)
                    .HasMaxLength(100)
                    .HasColumnName("dept_name")
                    .HasComment("部门名称");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用");

                entity.Property(e => e.ParentId)
                    .HasMaxLength(50)
                    .HasColumnName("parent_id")
                    .HasComment("上级部门");

                entity.Property(e => e.Sort)
                    .HasColumnName("sort")
                    .HasDefaultValueSql("'0'")
                    .HasComment("排序");
            });

            modelBuilder.Entity<SysDeptRole>(entity =>
            {
                entity.ToTable("sys_dept_role");

                entity.HasComment("部门角色绑定");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.DepartmentId)
                    .HasMaxLength(50)
                    .HasColumnName("department_id")
                    .HasComment("部门ID");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用");

                entity.Property(e => e.RoleId)
                    .HasMaxLength(50)
                    .HasColumnName("role_id")
                    .HasComment("角色ID");
            });

            modelBuilder.Entity<SysDeptRoleMenu>(entity =>
            {
                entity.ToTable("sys_dept_role_menu");

                entity.HasComment("部门角色菜单权限");

                entity.HasIndex(e => e.DeptRoleId, "idx_drm_dr");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.DeptRoleId)
                    .HasMaxLength(50)
                    .HasColumnName("dept_role_id")
                    .HasComment("部门角色ID");

                entity.Property(e => e.MenuId)
                    .HasMaxLength(50)
                    .HasColumnName("menu_id")
                    .HasComment("菜单ID");
            });

            modelBuilder.Entity<SysDict>(entity =>
            {
                entity.ToTable("sys_dict");

                entity.HasComment("系统字典");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.DictLabel)
                    .HasMaxLength(100)
                    .HasColumnName("dict_label")
                    .HasComment("标签");

                entity.Property(e => e.DictType)
                    .HasMaxLength(100)
                    .HasColumnName("dict_type")
                    .HasComment("字典类型");

                entity.Property(e => e.DictValue)
                    .HasMaxLength(100)
                    .HasColumnName("dict_value")
                    .HasComment("值");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用");

                entity.Property(e => e.Sort)
                    .HasColumnName("sort")
                    .HasDefaultValueSql("'0'")
                    .HasComment("排序");
            });

            modelBuilder.Entity<SysLoginLog>(entity =>
            {
                entity.ToTable("sys_login_log");

                entity.HasComment("系统登录日志表");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.Account)
                    .HasMaxLength(50)
                    .HasColumnName("account")
                    .HasComment("登录账号(冗余)");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .HasColumnName("code")
                    .HasComment("登陆的凭据");

                entity.Property(e => e.LoginAddress)
                    .HasMaxLength(200)
                    .HasColumnName("login_address")
                    .HasComment("登录地点");

                entity.Property(e => e.LoginIp)
                    .HasMaxLength(50)
                    .HasColumnName("login_ip")
                    .HasComment("登录IP地址");

                entity.Property(e => e.LoginTime)
                    .HasColumnType("datetime")
                    .HasColumnName("login_time")
                    .HasComment("登录时间");

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .HasColumnName("user_id")
                    .HasComment("关联用户ID(sys_users.u_id)");
            });

            modelBuilder.Entity<SysMenu>(entity =>
            {
                entity.ToTable("sys_menu");

                entity.HasComment("系统菜单");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.Icon)
                    .HasMaxLength(500)
                    .HasColumnName("icon")
                    .HasComment("图标，支持图片 URL");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用");

                entity.Property(e => e.IsMenu)
                    .HasColumnName("is_menu")
                    .HasDefaultValueSql("'1'")
                    .HasComment("1菜单 0按钮");

                entity.Property(e => e.MenuName)
                    .HasMaxLength(100)
                    .HasColumnName("menu_name")
                    .HasComment("菜单名称");

                entity.Property(e => e.ParentId)
                    .HasMaxLength(50)
                    .HasColumnName("parent_id")
                    .HasComment("父菜单");

                entity.Property(e => e.Path)
                    .HasMaxLength(200)
                    .HasColumnName("path")
                    .HasComment("路由");

                entity.Property(e => e.Sort)
                    .HasColumnName("sort")
                    .HasDefaultValueSql("'0'")
                    .HasComment("排序");
            });

            modelBuilder.Entity<SysNotice>(entity =>
            {
                entity.ToTable("sys_notice");

                entity.HasComment("系统通知公告");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content")
                    .HasComment("内容");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用");

                entity.Property(e => e.Title)
                    .HasMaxLength(200)
                    .HasColumnName("title")
                    .HasComment("标题");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("'0'")
                    .HasComment("类型");
            });

            modelBuilder.Entity<SysOperLog>(entity =>
            {
                entity.ToTable("sys_oper_log");

                entity.HasComment("系统操作日志表");

                entity.HasIndex(e => e.BusinessType, "idx_business_type");

                entity.HasIndex(e => e.OperTime, "idx_oper_time");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.BusinessType)
                    .HasMaxLength(100)
                    .HasColumnName("business_type")
                    .HasComment("业务类型");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasComment("记录入库时间");

                entity.Property(e => e.OperAction)
                    .HasMaxLength(200)
                    .HasColumnName("oper_action")
                    .HasComment("操作行为描述");

                entity.Property(e => e.OperTime)
                    .HasColumnType("datetime")
                    .HasColumnName("oper_time")
                    .HasComment("操作时间");

                entity.Property(e => e.RequestMethod)
                    .HasMaxLength(20)
                    .HasColumnName("request_method")
                    .HasComment("请求方式");

                entity.Property(e => e.RequestParams)
                    .HasColumnName("request_params")
                    .HasComment("请求参数");

                entity.Property(e => e.RequestUrl)
                    .HasMaxLength(500)
                    .HasColumnName("request_url")
                    .HasComment("请求URL");

                entity.Property(e => e.Result)
                    .HasMaxLength(50)
                    .HasColumnName("result")
                    .HasComment("结果");
            });

            modelBuilder.Entity<SysRole>(entity =>
            {
                entity.ToTable("sys_role");

                entity.HasComment("系统角色");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("create_time");

                entity.Property(e => e.IsBan)
                    .HasColumnName("is_ban")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用");

                entity.Property(e => e.Remark)
                    .HasMaxLength(500)
                    .HasColumnName("remark")
                    .HasComment("备注");

                entity.Property(e => e.RoleKey)
                    .HasMaxLength(100)
                    .HasColumnName("role_key")
                    .HasComment("角色权限标识");

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .HasColumnName("role_name")
                    .HasComment("角色名称");

                entity.Property(e => e.Sort)
                    .HasColumnName("sort")
                    .HasDefaultValueSql("'0'")
                    .HasComment("排序");
            });

            modelBuilder.Entity<SysUser>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.ToTable("sys_users");

                entity.HasComment("系统用户表");

                entity.HasIndex(e => e.Dept, "fk_user_dept_id");

                entity.HasIndex(e => e.Role, "fk_user_role_id");

                entity.HasIndex(e => e.UAccount, "uk_account")
                    .IsUnique();

                entity.Property(e => e.UId)
                    .HasMaxLength(50)
                    .HasColumnName("u_id")
                    .HasComment("主键ID");

                entity.Property(e => e.Dept)
                    .HasMaxLength(50)
                    .HasColumnName("dept")
                    .HasComment("部门ID");

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .HasColumnName("role")
                    .HasComment("角色ID");

                entity.Property(e => e.UAccount)
                    .HasMaxLength(50)
                    .HasColumnName("u_account")
                    .HasComment("登录账号，唯一");

                entity.Property(e => e.UCreateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("u_create_time")
                    .HasComment("创建时间");

                entity.Property(e => e.UIsBan)
                    .HasColumnName("u_isBan")
                    .HasDefaultValueSql("'0'")
                    .HasComment("是否禁用 0正常 1禁用");

                entity.Property(e => e.UName)
                    .HasMaxLength(100)
                    .HasColumnName("u_name")
                    .HasComment("名字");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(500)
                    .HasColumnName("avatar")
                    .HasComment("头像地址");

                entity.Property(e => e.UPassword)
                    .HasMaxLength(255)
                    .HasColumnName("u_password")
                    .HasComment("加密后的密码（Argon2id PHC 哈希串）");

                entity.Property(e => e.USalt)
                    .HasMaxLength(64)
                    .HasColumnName("u_salt")
                    .HasComment("加密盐值");
            });

            // ClientLogin 映射见 _111Context.ModulePartial.cs（client_login 表，覆盖本节同名映射）

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
