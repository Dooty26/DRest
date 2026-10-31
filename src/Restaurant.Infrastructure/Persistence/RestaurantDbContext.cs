using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Restaurant.Infrastructure.Persistence;

public sealed class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<AreaEntity> Areas => Set<AreaEntity>();
    public DbSet<RestaurantTableEntity> Tables => Set<RestaurantTableEntity>();
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<MenuItemEntity> MenuItems => Set<MenuItemEntity>();
    public DbSet<IngredientEntity> Ingredients => Set<IngredientEntity>();
    public DbSet<RecipeEntity> Recipes => Set<RecipeEntity>();
    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();
    public DbSet<ReservationWishlistEntity> ReservationWishlists => Set<ReservationWishlistEntity>();
    public DbSet<TableSwitchRequestEntity> TableSwitchRequests => Set<TableSwitchRequestEntity>();
    public DbSet<SwitchRequestEventEntity> SwitchRequestEvents => Set<SwitchRequestEventEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderDetailEntity> OrderDetails => Set<OrderDetailEntity>();
    public DbSet<VoucherEntity> Vouchers => Set<VoucherEntity>();
    public DbSet<InvoiceEntity> Invoices => Set<InvoiceEntity>();
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
    public DbSet<StockMovementEntity> StockMovements => Set<StockMovementEntity>();
    public DbSet<WaitlistEntryEntity> WaitlistEntries => Set<WaitlistEntryEntity>();
    public DbSet<TableMergeEntity> TableMerges => Set<TableMergeEntity>();
    public DbSet<NotificationLogEntity> NotificationLogs => Set<NotificationLogEntity>();
    public DbSet<AppSettingEntity> AppSettings => Set<AppSettingEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();
    public DbSet<DailyCounterEntity> DailyCounters => Set<DailyCounterEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RestaurantDbContext).Assembly);
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.ClrType == typeof(DateTime)) property.SetValueConverter(new UtcDateTimeConverter());
            if (property.ClrType == typeof(DateTime?)) property.SetValueConverter(new NullableUtcDateTimeConverter());
        }
    }

    private static DateTime RequireUtc(DateTime value) => value.Kind == DateTimeKind.Utc
        ? value
        : throw new InvalidOperationException($"DateTime ghi vào PostgreSQL phải có Kind=Utc, nhận {value.Kind}.");

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(value => RequireUtc(value), value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private sealed class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(value => value.HasValue ? RequireUtc(value.Value) : value, value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);
}
