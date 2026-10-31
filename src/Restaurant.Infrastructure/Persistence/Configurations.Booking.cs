using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Restaurant.Infrastructure.Persistence;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<ReservationEntity>
{
    public void Configure(EntityTypeBuilder<ReservationEntity> builder)
    {
        builder.ToTable("reservations", table =>
        {
            table.HasCheckConstraint("ck_reservations_occupancy", "occupancy_minutes > 0");
            table.HasCheckConstraint("ck_reservations_party", "party_size > 0");
            table.HasCheckConstraint("ck_reservations_status", "status IN ('PENDING_DEPOSIT','CONFIRMED','CHECKED_IN','COMPLETED','CANCELLED','NO_SHOW','EXPIRED')");
            table.HasCheckConstraint("ck_reservations_deposit_status", "deposit_status IN ('NONE','PENDING','PAID','REFUNDED','FORFEITED','CONVERTED_TO_VOUCHER','APPLIED')");
            table.HasCheckConstraint("ck_reservations_source", "source IN ('WEB','STAFF')");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("reservation_id");
        builder.Property(x => x.ViewType).HasConversion<string>(); builder.Property(x => x.Status).HasConversion<string>(); builder.Property(x => x.DepositStatus).HasConversion<string>();
        builder.Property(x => x.DepositAmount).HasPrecision(12, 0); builder.Property(x => x.Allergies).HasColumnType("jsonb");
        builder.Property(x => x.Version).IsRowVersion();
        builder.HasIndex(x => x.Code).IsUnique(); builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => new { x.TableId, x.StartTime }).HasDatabaseName("ix_res_table_start");
        builder.HasIndex(x => new { x.Status, x.DepositExpiresAt }).HasDatabaseName("ix_res_status_expires").HasFilter("status = 'PENDING_DEPOSIT'");
        builder.HasIndex(x => x.GuestPhone).HasDatabaseName("ix_res_phone");
        builder.HasOne<CustomerEntity>().WithMany().HasForeignKey(x => x.CustomerId).Restrict();
        builder.HasOne<RestaurantTableEntity>().WithMany().HasForeignKey(x => x.TableId).Restrict();
    }
}

public sealed class ReservationWishlistConfiguration : IEntityTypeConfiguration<ReservationWishlistEntity>
{
    public void Configure(EntityTypeBuilder<ReservationWishlistEntity> builder)
    {
        builder.ToTable("reservation_wishlists"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("wishlist_id");
        builder.Property(x => x.TargetViewType).HasConversion<string>(); builder.HasIndex(x => x.ReservationId).IsUnique();
        builder.HasIndex(x => x.TargetViewType).HasDatabaseName("ix_wl_active_view").HasFilter("is_active");
        builder.HasOne<ReservationEntity>().WithMany().HasForeignKey(x => x.ReservationId).Restrict();
        builder.HasOne<RestaurantTableEntity>().WithMany().HasForeignKey(x => x.TargetTableId).Restrict();
    }
}

public sealed class TableSwitchRequestConfiguration : IEntityTypeConfiguration<TableSwitchRequestEntity>
{
    public void Configure(EntityTypeBuilder<TableSwitchRequestEntity> builder)
    {
        builder.ToTable("table_switch_requests", table => table.HasCheckConstraint("ck_switch_status", "status IN ('PENDING_CUSTOMER','ACCEPTED','DECLINED','ESCALATED','STAFF_ACCEPTED','STAFF_KEPT_OLD','STAFF_NO_ANSWER','AUTO_RELEASED')"));
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("request_id"); builder.Property(x => x.ReservationId).HasColumnName("res_id"); builder.Property(x => x.Status).HasConversion<string>();
        builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("ix_sw_pending").HasFilter("status = 'PENDING_CUSTOMER'");
        builder.HasIndex(x => x.EscalationExpiresAt).HasDatabaseName("ix_sw_escalated").HasFilter("status = 'ESCALATED'");
        builder.HasIndex(x => x.ToTableId).IsUnique().HasDatabaseName("ux_sw_one_open_per_table").HasFilter("status IN ('PENDING_CUSTOMER','ESCALATED')");
        builder.HasIndex(x => x.ReservationId).IsUnique().HasDatabaseName("ux_sw_one_open_per_res").HasFilter("status IN ('PENDING_CUSTOMER','ESCALATED')");
        builder.HasOne<ReservationEntity>().WithMany().HasForeignKey(x => x.ReservationId).Restrict();
        builder.HasOne<RestaurantTableEntity>().WithMany().HasForeignKey(x => x.FromTableId).Restrict();
        builder.HasOne<RestaurantTableEntity>().WithMany().HasForeignKey(x => x.ToTableId).Restrict();
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.ResolvedByUserId).Restrict();
    }
}

public sealed class SwitchRequestEventConfiguration : IEntityTypeConfiguration<SwitchRequestEventEntity>
{
    public void Configure(EntityTypeBuilder<SwitchRequestEventEntity> builder)
    {
        builder.ToTable("switch_request_events"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("event_id");
        builder.HasOne<TableSwitchRequestEntity>().WithMany().HasForeignKey(x => x.RequestId).Restrict();
    }
}

public sealed class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntryEntity>
{
    public void Configure(EntityTypeBuilder<WaitlistEntryEntity> builder)
    {
        builder.ToTable("waitlist_entries", table =>
        {
            table.HasCheckConstraint("ck_waitlist_party", "party_size > 0");
            table.HasCheckConstraint("ck_waitlist_status", "status IN ('WAITING','NOTIFIED','SEATED','CANCELLED')");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("entry_id"); builder.Property(x => x.Status).HasConversion<string>();
        builder.HasIndex(x => new { x.Status, x.CreatedAt }).HasDatabaseName("ix_wait_status");
        builder.HasOne<RestaurantTableEntity>().WithMany().HasForeignKey(x => x.TableId).Restrict();
    }
}

public sealed class TableMergeConfiguration : IEntityTypeConfiguration<TableMergeEntity>
{
    public void Configure(EntityTypeBuilder<TableMergeEntity> builder)
    {
        builder.ToTable("table_merges"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("merge_id");
        builder.HasOne<OrderEntity>().WithMany().HasForeignKey(x => x.OrderId).Restrict();
        builder.HasOne<RestaurantTableEntity>().WithMany().HasForeignKey(x => x.PrimaryTableId).Restrict();
        builder.HasOne<RestaurantTableEntity>().WithMany().HasForeignKey(x => x.MergedTableId).Restrict();
    }
}
