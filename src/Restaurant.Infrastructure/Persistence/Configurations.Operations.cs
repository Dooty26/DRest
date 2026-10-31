using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Restaurant.Infrastructure.Persistence;

public sealed class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("orders", table => table.HasCheckConstraint("ck_orders_status", "status IN ('OPEN','BILL_REQUESTED','PAID','CANCELLED')"));
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("order_id"); builder.Property(x => x.Status).HasConversion<string>();
        builder.HasIndex(x => new { x.TableId, x.Status }).HasDatabaseName("ix_orders_table_status");
        builder.HasIndex(x => x.TableId).IsUnique().HasDatabaseName("ux_orders_one_open_per_table").HasFilter("status IN ('OPEN','BILL_REQUESTED')");
        builder.HasOne<RestaurantTableEntity>().WithMany().HasForeignKey(x => x.TableId).Restrict();
        builder.HasOne<ReservationEntity>().WithMany().HasForeignKey(x => x.ReservationId).Restrict();
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.OpenedByUserId).Restrict();
    }
}

public sealed class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetailEntity>
{
    public void Configure(EntityTypeBuilder<OrderDetailEntity> builder)
    {
        builder.ToTable("order_details", table =>
        {
            table.HasCheckConstraint("ck_order_details_qty", "qty > 0");
            table.HasCheckConstraint("ck_order_details_status", "status IN ('NEW','COOKING','READY','SERVED','CANCELLED','OUT_OF_STOCK')");
            table.HasCheckConstraint("ck_order_details_source", "source IN ('QR','STAFF')");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("detail_id"); builder.Property(x => x.Status).HasConversion<string>(); builder.Property(x => x.UnitPrice).HasPrecision(12, 0);
        builder.HasIndex(x => new { x.OrderId, x.Status }).HasDatabaseName("ix_od_order_status");
        builder.HasIndex(x => new { x.OrderId, x.IdempotencyKey }).IsUnique().HasDatabaseName("ux_od_idem").HasFilter("idempotency_key IS NOT NULL");
        builder.HasOne<OrderEntity>().WithMany().HasForeignKey(x => x.OrderId).Restrict();
        builder.HasOne<MenuItemEntity>().WithMany().HasForeignKey(x => x.ItemId).Restrict();
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.CreatedByUserId).Restrict();
    }
}

public sealed class VoucherConfiguration : IEntityTypeConfiguration<VoucherEntity>
{
    public void Configure(EntityTypeBuilder<VoucherEntity> builder)
    {
        builder.ToTable("vouchers", table =>
        {
            table.HasCheckConstraint("ck_vouchers_type", "type IN ('FIXED','PERCENT')");
            table.HasCheckConstraint("ck_vouchers_source", "source IN ('DEPOSIT_REFUND','APOLOGY','PROMO')");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("voucher_id"); builder.Property(x => x.Value).HasPrecision(12, 2); builder.HasIndex(x => x.Code).IsUnique();
        builder.HasOne<CustomerEntity>().WithMany().HasForeignKey(x => x.CustomerId).Restrict();
    }
}

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<InvoiceEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        builder.ToTable("invoices", table =>
        {
            table.HasCheckConstraint("ck_invoices_payment_method", "payment_method IS NULL OR payment_method IN ('VIETQR','CASH','CARD','TRANSFER')");
            table.HasCheckConstraint("ck_invoices_status", "status IN ('DRAFT','PAID','VOID')");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("invoice_id"); builder.Property(x => x.PaymentMethod).HasConversion<string>();
        builder.Property(x => x.Subtotal).HasPrecision(12, 0); builder.Property(x => x.DiscountAmount).HasPrecision(12, 0); builder.Property(x => x.VatAmount).HasPrecision(12, 0); builder.Property(x => x.DepositApplied).HasPrecision(12, 0); builder.Property(x => x.NetAmount).HasPrecision(12, 0); builder.Property(x => x.RefundDue).HasPrecision(12, 0); builder.Property(x => x.AmountTendered).HasPrecision(12, 0);
        builder.HasIndex(x => x.OrderId).IsUnique(); builder.HasIndex(x => x.InvoiceNo).IsUnique();
        builder.HasOne<OrderEntity>().WithMany().HasForeignKey(x => x.OrderId).Restrict(); builder.HasOne<VoucherEntity>().WithMany().HasForeignKey(x => x.VoucherId).Restrict(); builder.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.CashierId).Restrict();
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable("payments", table =>
        {
            table.HasCheckConstraint("ck_payments_type", "type IN ('DEPOSIT','REFUND','BILL')");
            table.HasCheckConstraint("ck_payments_status", "status IN ('PENDING','CONFIRMED','FAILED','REVIEW')");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("payment_id"); builder.Property(x => x.Amount).HasPrecision(12, 0); builder.Property(x => x.RawPayload).HasColumnType("jsonb");
        builder.HasIndex(x => x.ProviderTxnId).IsUnique(); builder.HasIndex(x => new { x.ContentCode, x.Status }).HasDatabaseName("ix_pay_content");
        builder.HasOne<ReservationEntity>().WithMany().HasForeignKey(x => x.ReservationId).Restrict(); builder.HasOne<InvoiceEntity>().WithMany().HasForeignKey(x => x.InvoiceId).Restrict();
    }
}

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovementEntity>
{
    public void Configure(EntityTypeBuilder<StockMovementEntity> builder)
    {
        builder.ToTable("stock_movements", table => table.HasCheckConstraint("ck_stock_movements_type", "type IN ('SALE','PURCHASE','ADJUST','WASTE')"));
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("movement_id"); builder.Property(x => x.Delta).HasPrecision(14, 3);
        builder.HasIndex(x => new { x.RefInvoiceId, x.IngredientId }).IsUnique().HasDatabaseName("ux_sm_sale").HasFilter("type = 'SALE'");
        builder.HasOne<IngredientEntity>().WithMany().HasForeignKey(x => x.IngredientId).Restrict(); builder.HasOne<InvoiceEntity>().WithMany().HasForeignKey(x => x.RefInvoiceId).Restrict(); builder.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.CreatedBy).Restrict();
    }
}

public sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLogEntity>
{
    public void Configure(EntityTypeBuilder<NotificationLogEntity> builder)
    {
        builder.ToTable("notification_logs", table =>
        {
            table.HasCheckConstraint("ck_notification_channel", "channel IN ('ZALO_ZNS','ZALO_OA','MOCK')");
            table.HasCheckConstraint("ck_notification_direction", "direction IN ('OUT','IN')");
            table.HasCheckConstraint("ck_notification_status", "status IN ('SENT','FAILED','RECEIVED')");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("log_id"); builder.HasIndex(x => new { x.Phone, x.CreatedAt }).HasDatabaseName("ix_nl_phone");
    }
}

public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSettingEntity>
{
    public void Configure(EntityTypeBuilder<AppSettingEntity> builder) { builder.ToTable("app_settings"); builder.HasKey(x => x.Key); }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.ToTable("audit_logs"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("log_id"); builder.Property(x => x.Detail).HasColumnType("jsonb"); builder.HasIndex(x => new { x.Entity, x.EntityId, x.CreatedAt }).HasDatabaseName("ix_audit_entity");
    }
}

public sealed class DailyCounterConfiguration : IEntityTypeConfiguration<DailyCounterEntity>
{
    public void Configure(EntityTypeBuilder<DailyCounterEntity> builder) { builder.ToTable("daily_counters"); builder.HasKey(x => new { x.CounterDate, x.Name }); }
}
