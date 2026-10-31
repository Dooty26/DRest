using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Restaurant.Infrastructure.Persistence;

internal static class ConfigurationHelpers
{
    public static PropertyBuilder<int> Identity(this PropertyBuilder<int> property, string name) => property.HasColumnName(name).UseIdentityByDefaultColumn();
    public static PropertyBuilder<long> Identity(this PropertyBuilder<long> property, string name) => property.HasColumnName(name).UseIdentityByDefaultColumn();
    public static ReferenceCollectionBuilder<TPrincipal, TDependent> Restrict<TPrincipal, TDependent>(this ReferenceCollectionBuilder<TPrincipal, TDependent> relationship)
        where TPrincipal : class where TDependent : class => relationship.OnDelete(DeleteBehavior.Restrict);
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("roles", table => table.HasCheckConstraint("ck_roles_code", "code IN ('MANAGER','CASHIER','WAITER','KITCHEN')"));
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("role_id");
        builder.Property(x => x.Code).HasConversion<string>(); builder.HasIndex(x => x.Code).IsUnique();
    }
}

public sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("user_id");
        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasOne<RoleEntity>().WithMany().HasForeignKey(x => x.RoleId).Restrict();
    }
}

public sealed class AreaConfiguration : IEntityTypeConfiguration<AreaEntity>
{
    public void Configure(EntityTypeBuilder<AreaEntity> builder)
    {
        builder.ToTable("areas"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("area_id");
    }
}

public sealed class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTableEntity>
{
    public void Configure(EntityTypeBuilder<RestaurantTableEntity> builder)
    {
        builder.ToTable("tables", table =>
        {
            table.HasCheckConstraint("ck_tables_seats", "seats > 0");
            table.HasCheckConstraint("ck_tables_view_type", "view_type IN ('STANDARD','WINDOW','BALCONY','VIP','OUTDOOR')");
            table.HasCheckConstraint("ck_tables_status", "status IN ('AVAILABLE','OCCUPIED','CLEANING','HELD')");
            table.HasCheckConstraint("ck_tables_shape", "shape IN ('RECT','ROUND')");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("table_id");
        builder.Property(x => x.ViewType).HasConversion<string>(); builder.Property(x => x.Status).HasConversion<string>();
        builder.HasIndex(x => x.Code).IsUnique(); builder.HasIndex(x => new { x.AreaId, x.IsActive }).HasDatabaseName("ix_tables_area");
        builder.HasOne<AreaEntity>().WithMany().HasForeignKey(x => x.AreaId).Restrict();
    }
}

public sealed class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("customers"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("customer_id");
        builder.HasIndex(x => x.Phone).IsUnique();
    }
}

public sealed class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("categories"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("category_id");
    }
}

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItemEntity>
{
    public void Configure(EntityTypeBuilder<MenuItemEntity> builder)
    {
        builder.ToTable("menu_items", table => table.HasCheckConstraint("ck_menu_items_price", "price >= 0"));
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("item_id"); builder.Property(x => x.Price).HasPrecision(12, 0);
        builder.HasOne<CategoryEntity>().WithMany().HasForeignKey(x => x.CategoryId).Restrict();
    }
}

public sealed class IngredientConfiguration : IEntityTypeConfiguration<IngredientEntity>
{
    public void Configure(EntityTypeBuilder<IngredientEntity> builder)
    {
        builder.ToTable("ingredients", table => table.HasCheckConstraint("ck_ingredients_base_unit", "base_unit IN ('g','ml','pcs')"));
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).Identity("ingredient_id"); builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.DisplayFactor).HasPrecision(10, 3); builder.Property(x => x.StockQty).HasPrecision(14, 3); builder.Property(x => x.MinStock).HasPrecision(14, 3);
    }
}

public sealed class RecipeConfiguration : IEntityTypeConfiguration<RecipeEntity>
{
    public void Configure(EntityTypeBuilder<RecipeEntity> builder)
    {
        builder.ToTable("recipes", table => table.HasCheckConstraint("ck_recipes_qty", "qty > 0"));
        builder.HasKey(x => new { x.ItemId, x.IngredientId }); builder.Property(x => x.Qty).HasPrecision(14, 3);
        builder.HasOne<MenuItemEntity>().WithMany().HasForeignKey(x => x.ItemId).Restrict();
        builder.HasOne<IngredientEntity>().WithMany().HasForeignKey(x => x.IngredientId).Restrict();
    }
}
