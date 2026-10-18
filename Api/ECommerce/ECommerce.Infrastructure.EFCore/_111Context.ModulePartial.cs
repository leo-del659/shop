using System;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.EFCore;

namespace ECommerce.Infrastructure.EFCore
{
	public partial class _111Context
	{
		partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
		{		

			// 商户账单 / 商户账单（结算管理下新增）

	modelBuilder.Entity<ClientLogin>(entity =>
			{
			entity.ToTable("client_login");

			entity.HasComment("用户端登录账号（消费者手机号验证码登录）");

			entity.HasIndex(e => e.Account, "uk_client_login_account")
				.IsUnique();

			entity.HasIndex(e => e.ClientId, "idx_client_login_client");

			entity.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

		entity.Property(e => e.ClientId)
			.HasMaxLength(50)
			.HasColumnName("client_id")
			.HasComment("消费者用户ID（sys_users.u_id）");

			entity.Property(e => e.Account)
				.HasMaxLength(50)
				.HasColumnName("account")
				.HasComment("登录账号（手机号）");

			entity.Property(e => e.Name)
				.HasMaxLength(50)
				.HasColumnName("name")
				.HasComment("用户名称");

			entity.Property(e => e.Avatar)
				.HasMaxLength(500)
				.HasColumnName("avatar")
				.HasComment("头像地址");

			entity.Property(e => e.CreateTime)
				.HasColumnType("datetime")
				.HasColumnName("create_time");
		});

		modelBuilder.Entity<MerchantWithdraw>(entity =>
		{
			entity.ToTable("merchant_withdraw");

			entity.HasComment("商户提现记录");

			entity.HasIndex(e => e.MerchantId, "idx_sw_merchant");

			entity.Property(e => e.Id)
				.HasColumnName("id")
				.ValueGeneratedOnAdd();

			entity.Property(e => e.MerchantId)
				.HasMaxLength(50)
				.HasColumnName("merchant_id")
				.HasComment("供货商户ID");

				entity.Property(e => e.Amount)
					.HasPrecision(18, 2)
					.HasColumnName("amount")
					.HasComment("提现金额");

				entity.Property(e => e.BankName)
					.HasMaxLength(100)
					.HasColumnName("bank_name")
					.HasComment("开户行");

				entity.Property(e => e.BankAccount)
					.HasMaxLength(100)
					.HasColumnName("bank_account")
					.HasComment("银行账号");

				entity.Property(e => e.AccountName)
					.HasMaxLength(100)
					.HasColumnName("account_name")
					.HasComment("开户名");

				entity.Property(e => e.Remark)
					.HasMaxLength(500)
					.HasColumnName("remark")
					.HasComment("备注");

				entity.Property(e => e.Status)
					.HasColumnName("status")
					.HasDefaultValueSql("'1'")
					.HasComment("1待处理 2已打款");

				entity.Property(e => e.OutTransferNo)
					.HasMaxLength(64)
					.HasColumnName("out_transfer_no")
					.HasComment("商户转账单号（本地生成，全局唯一）");

				entity.Property(e => e.TransferNo)
					.HasMaxLength(64)
					.HasColumnName("transfer_no")
					.HasComment("微信转账单号");

				entity.Property(e => e.TransferStatus)
					.HasColumnName("transfer_status")
					.HasDefaultValueSql("'0'")
					.HasComment("0未发起 1受理中 2成功 3失败");

				entity.Property(e => e.FailReason)
					.HasMaxLength(500)
					.HasColumnName("fail_reason")
					.HasComment("转账失败原因");

				entity.Property(e => e.TransferTime)
					.HasColumnType("datetime")
					.HasColumnName("transfer_time")
					.HasComment("转账完成时间");

				entity.Property(e => e.CreateTime)
					.HasColumnType("datetime")
					.HasColumnName("create_time");
			});

			modelBuilder.Entity<MerchantBill>(entity =>
			{
				entity.ToTable("merchant_bill");

				entity.HasComment("商户账单（应付）");

			entity.HasIndex(e => e.BillNo, "uk_merchant_bill_no")
				.IsUnique();

			entity.HasIndex(e => e.MerchantId, "idx_merchant_bill_merchant");

				entity.Property(e => e.Id)
					.HasMaxLength(50)
					.HasColumnName("id");

			entity.Property(e => e.BillNo)
				.HasMaxLength(64)
				.HasColumnName("bill_no")
				.HasComment("账单编号 GYS{yyyyMMdd}-NN");

			entity.Property(e => e.OrderNo)
				.HasMaxLength(64)
				.HasColumnName("order_no")
				.HasComment("关联订单编号");

		entity.Property(e => e.MerchantId)
			.HasMaxLength(50)
			.HasColumnName("merchant_id")
			.HasComment("供货商户ID");

		entity.Property(e => e.ClientId)
			.HasMaxLength(50)
			.HasColumnName("client_id")
			.HasComment("买家用户ID（sys_users.u_id）");

			entity.Property(e => e.GoodsId)
				.HasMaxLength(50)
				.HasColumnName("goods_id")
				.HasComment("商品ID");

			entity.Property(e => e.GoodsName)
				.HasMaxLength(200)
				.HasColumnName("goods_name")
				.HasComment("商品名称");

			entity.Property(e => e.BuyNum)
				.HasColumnName("buy_num")
				.HasDefaultValueSql("'0'")
				.HasComment("购入数量");

			entity.Property(e => e.Amount)
				.HasPrecision(18, 2)
				.HasColumnName("amount")
				.HasComment("支付金额");

			entity.Property(e => e.OrderStatus)
				.HasColumnName("order_status")
				.HasDefaultValueSql("'0'")
				.HasComment("0待付款 1已付款 2已发货 3已完成 4已取消 5退款中 6已退款 7已驳回");

			entity.Property(e => e.BillType)
				.HasColumnName("bill_type")
				.HasDefaultValueSql("'1'")
				.HasComment("1货款 2服务费 3运费 4其他");

			entity.Property(e => e.PeriodStart)
				.HasColumnType("datetime")
				.HasColumnName("period_start")
				.HasComment("计费起期");

			entity.Property(e => e.PeriodEnd)
				.HasColumnType("datetime")
				.HasColumnName("period_end")
				.HasComment("计费止期");

				entity.Property(e => e.PaidAmount)
					.HasPrecision(18, 2)
					.HasColumnName("paid_amount")
					.HasComment("已付金额");

				entity.Property(e => e.PayStatus)
					.HasColumnName("pay_status")
					.HasDefaultValueSql("'0'")
					.HasComment("0待付 1已付 2部分付");

				entity.Property(e => e.PayType)
					.HasColumnName("pay_type")
					.HasComment("1微信 2支付宝 3银行卡 4线下");

				entity.Property(e => e.PayNo)
					.HasMaxLength(64)
					.HasColumnName("pay_no")
					.HasComment("支付流水号");

				entity.Property(e => e.PayTime)
					.HasColumnType("datetime")
					.HasColumnName("pay_time")
					.HasComment("付款时间");

				entity.Property(e => e.Remark)
					.HasMaxLength(500)
					.HasColumnName("remark")
					.HasComment("备注");

				entity.Property(e => e.CreateTime)
					.HasColumnType("datetime")
					.HasColumnName("create_time");

		entity.Property(e => e.UpdateTime)
			.HasColumnType("datetime")
			.HasColumnName("update_time");
		});

		// 订单消息事件日志（RabbitMQ 消费幂等与审计）
		modelBuilder.Entity<OrderEventLog>(entity =>
		{
			entity.ToTable("order_event_log");

			entity.HasComment("订单消息事件日志（MQ消费幂等与审计）");

			entity.HasIndex(e => e.BizKey, "uk_biz_key")
				.IsUnique();

			entity.HasIndex(e => e.OrderId, "idx_event_order");

			entity.Property(e => e.Id)
				.HasMaxLength(50)
				.HasColumnName("id");

			entity.Property(e => e.EventId)
				.HasMaxLength(64)
				.HasColumnName("event_id")
				.HasComment("消息事件ID，与RabbitMQ消息对账");

			entity.Property(e => e.EventType)
				.HasMaxLength(64)
				.HasColumnName("event_type")
				.HasComment("事件类型");

			entity.Property(e => e.OrderId)
				.HasMaxLength(50)
				.HasColumnName("order_id");

			entity.Property(e => e.OrderNo)
				.HasMaxLength(64)
				.HasColumnName("order_no");

			entity.Property(e => e.BizKey)
				.HasMaxLength(128)
				.HasColumnName("biz_key")
				.HasComment("业务幂等键，如 settle:{orderId}");

			entity.Property(e => e.Status)
				.HasColumnName("status")
				.HasDefaultValue(0)
				.HasComment("0处理中 1已处理 2失败");

			entity.Property(e => e.Remark)
				.HasMaxLength(500)
				.HasColumnName("remark");

			entity.Property(e => e.CreateTime)
				.HasColumnType("datetime")
				.HasColumnName("create_time");

			entity.Property(e => e.UpdateTime)
				.HasColumnType("datetime")
				.HasColumnName("update_time");
		});
	}
}
}
