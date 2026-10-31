using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Restaurant.Contracts;
using Restaurant.Core;

namespace Restaurant.Infrastructure.Persistence;

public sealed class DbSeeder(RestaurantDbContext db, IClock clock, IConfiguration configuration, ILogger<DbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        await SeedRolesAsync(cancellationToken);
        await SeedSettingsAsync(now, cancellationToken);
        await SeedAreasAndTablesAsync(now, cancellationToken);
        await SeedMenuAndInventoryAsync(cancellationToken);
        await SeedUsersAsync(now, cancellationToken);
        await SeedCustomerAsync(now, cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var existing = await db.Roles.Select(role => role.Code).ToListAsync(cancellationToken);
        foreach (var role in Enum.GetValues<Role>().Except(existing)) db.Roles.Add(new RoleEntity { Code = role, Name = role switch { Role.MANAGER => "Quản lý", Role.CASHIER => "Thu ngân", Role.WAITER => "Phục vụ", _ => "Bếp/Bar" } });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSettingsAsync(DateTime now, CancellationToken cancellationToken)
    {
        Dictionary<string, string> settings = new(StringComparer.Ordinal)
        {
            ["Booking.StayMinutes"] = "90", ["Booking.CleaningMinutes"] = "15", ["Booking.SlotStepMinutes"] = "30",
            ["Booking.OpenTime"] = "10:00", ["Booking.LastSeatingTime"] = "21:30", ["Booking.MinLeadMinutes"] = "60", ["Booking.MaxAdvanceDays"] = "30",
            ["Deposit.Amount"] = "200000", ["Deposit.HoldMinutes"] = "15", ["Cancel.FullRefundHoursBefore"] = "2", ["NoShow.AutoMarkMinutes"] = "30",
            ["WalkIn.WarnIfReservationWithinMinutes"] = "45", ["Overstay.WarnBeforeMinutes"] = "30", ["Overstay.ApologyDiscountPercent"] = "10",
            ["Switch.HoldMinutes"] = "10", ["Switch.EscalationWindowMinutes"] = "10", ["Switch.LookaheadHours"] = "3", ["Switch.MaxAttemptsPerVacancy"] = "3",
            ["Ai.Model"] = "", ["Ai.TimeoutSeconds"] = "8", ["Stock.AllowNegative"] = "true", ["Billing.VatPercent"] = "0",
            ["Tables.QrBaseUrl"] = "http://localhost:5173", ["Storage.MaxImageBytes"] = "2097152", ["Brand.LogoUrl"] = ""
        };
        var existing = await db.AppSettings.Select(setting => setting.Key).ToHashSetAsync(cancellationToken);
        foreach (var setting in settings.Where(setting => !existing.Contains(setting.Key))) db.AppSettings.Add(new AppSettingEntity { Key = setting.Key, Value = setting.Value, UpdatedAt = now });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAreasAndTablesAsync(DateTime now, CancellationToken cancellationToken)
    {
        if (!await db.Areas.AnyAsync(cancellationToken))
        {
            db.Areas.AddRange(
                new AreaEntity { Name = "Tầng 1", FloorNo = 1, SortOrder = 1 },
                new AreaEntity { Name = "Tầng 2", FloorNo = 2, SortOrder = 2 },
                new AreaEntity { Name = "Tầng 3", FloorNo = 3, SortOrder = 3 },
                new AreaEntity { Name = "Ngoài trời", FloorNo = 0, SortOrder = 4 });
            await db.SaveChangesAsync(cancellationToken);
        }
        if (await db.Tables.AnyAsync(cancellationToken)) return;
        var areas = await db.Areas.ToDictionaryAsync(area => area.Name, cancellationToken);
        var specifications = new[]
        {
            ("T101", "Bàn 101", "Tầng 1", 2, 0, ViewType.STANDARD), ("T102", "Bàn 102", "Tầng 1", 2, 0, ViewType.STANDARD),
            ("T103", "Bàn 103", "Tầng 1", 4, 1, ViewType.STANDARD), ("T104", "Bàn 104", "Tầng 1", 4, 1, ViewType.STANDARD),
            ("T105", "Bàn 105", "Tầng 1", 4, 1, ViewType.STANDARD), ("T106", "Bàn 106", "Tầng 1", 6, 2, ViewType.STANDARD),
            ("T107", "Bàn 107", "Tầng 1", 6, 2, ViewType.STANDARD), ("T108", "Bàn 108", "Tầng 1", 8, 2, ViewType.STANDARD),
            ("T201", "Bàn 201", "Tầng 2", 4, 1, ViewType.WINDOW), ("T202", "Bàn 202", "Tầng 2", 4, 1, ViewType.WINDOW),
            ("T203", "Bàn 203", "Tầng 2", 2, 0, ViewType.WINDOW), ("T204", "Bàn 204", "Tầng 2", 6, 2, ViewType.WINDOW),
            ("T205", "Bàn 205", "Tầng 2", 4, 1, ViewType.WINDOW), ("T206", "Bàn 206", "Tầng 2", 8, 2, ViewType.WINDOW),
            ("T301", "Bàn 301", "Tầng 3", 4, 1, ViewType.BALCONY), ("T302", "Bàn 302", "Tầng 3", 4, 1, ViewType.BALCONY),
            ("T303", "Bàn 303", "Tầng 3", 6, 2, ViewType.BALCONY), ("T304", "Bàn 304", "Tầng 3", 8, 2, ViewType.BALCONY),
            ("V01", "Phòng VIP 01", "Tầng 3", 10, 2, ViewType.VIP), ("V02", "Phòng VIP 02", "Tầng 3", 12, 2, ViewType.VIP),
            ("O01", "Bàn ngoài trời 01", "Ngoài trời", 4, 1, ViewType.OUTDOOR), ("O02", "Bàn ngoài trời 02", "Ngoài trời", 6, 2, ViewType.OUTDOOR)
        };
        foreach (var (code, name, areaName, seats, extra, view) in specifications)
        {
            var index = db.Tables.Local.Count(table => table.AreaId == areas[areaName].Id);
            db.Tables.Add(new RestaurantTableEntity { AreaId = areas[areaName].Id, Code = code, Name = name, Seats = seats, ExtraChairsMax = extra, ViewType = view, StatusSince = now, PosX = 40 + (index % 4) * 120, PosY = 40 + (index / 4) * 110, QrSecret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)) });
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedMenuAndInventoryAsync(CancellationToken cancellationToken)
    {
        if (!await db.Categories.AnyAsync(cancellationToken))
        {
            db.Categories.AddRange(new CategoryEntity { Name = "Khai vị", SortOrder = 1 }, new CategoryEntity { Name = "Món chính", SortOrder = 2 }, new CategoryEntity { Name = "Đồ uống", SortOrder = 3 }, new CategoryEntity { Name = "Tráng miệng", SortOrder = 4 });
            await db.SaveChangesAsync(cancellationToken);
        }
        if (!await db.MenuItems.AnyAsync(cancellationToken))
        {
            var categories = await db.Categories.ToDictionaryAsync(category => category.Name, cancellationToken);
            var items = new[]
            {
                ("Gỏi cuốn tôm thịt", 69000m, "Khai vị"), ("Súp bí đỏ", 59000m, "Khai vị"), ("Salad cá ngừ", 89000m, "Khai vị"), ("Khoai tây chiên", 65000m, "Khai vị"),
                ("Bò bít tết", 189000m, "Món chính"), ("Cá hồi áp chảo", 219000m, "Món chính"), ("Gà nướng thảo mộc", 159000m, "Món chính"), ("Sườn nướng BBQ", 179000m, "Món chính"),
                ("Mì Ý bò bằm", 139000m, "Món chính"), ("Cơm chiên hải sản", 129000m, "Món chính"), ("Pizza phô mai", 149000m, "Món chính"), ("Burger bò", 139000m, "Món chính"),
                ("Cà phê sữa", 45000m, "Đồ uống"), ("Trà đào", 55000m, "Đồ uống"), ("Nước cam", 55000m, "Đồ uống"), ("Soda chanh", 49000m, "Đồ uống"),
                ("Tiramisu", 79000m, "Tráng miệng"), ("Kem vani", 59000m, "Tráng miệng"), ("Chè khúc bạch", 59000m, "Tráng miệng"), ("Trái cây theo mùa", 69000m, "Tráng miệng")
            };
            var sort = 0; foreach (var (name, price, category) in items) db.MenuItems.Add(new MenuItemEntity { CategoryId = categories[category].Id, Name = name, Price = price, Description = $"{name} được chế biến tại nhà hàng", SortOrder = ++sort });
            await db.SaveChangesAsync(cancellationToken);
        }
        if (!await db.Ingredients.AnyAsync(cancellationToken))
        {
            db.Ingredients.AddRange(
                new IngredientEntity { Name = "Thịt bò", BaseUnit = "g", DisplayUnit = "kg", DisplayFactor = 1000, StockQty = 10000, MinStock = 2000 },
                new IngredientEntity { Name = "Khoai tây", BaseUnit = "g", DisplayUnit = "kg", DisplayFactor = 1000, StockQty = 8000, MinStock = 1500 },
                new IngredientEntity { Name = "Cá hồi", BaseUnit = "g", DisplayUnit = "kg", DisplayFactor = 1000, StockQty = 6000, MinStock = 1200 },
                new IngredientEntity { Name = "Thịt gà", BaseUnit = "g", DisplayUnit = "kg", DisplayFactor = 1000, StockQty = 9000, MinStock = 1800 },
                new IngredientEntity { Name = "Phô mai", BaseUnit = "g", DisplayUnit = "kg", DisplayFactor = 1000, StockQty = 3000, MinStock = 500 },
                new IngredientEntity { Name = "Sữa tươi", BaseUnit = "ml", DisplayUnit = "l", DisplayFactor = 1000, StockQty = 12000, MinStock = 2000 },
                new IngredientEntity { Name = "Cà phê hạt", BaseUnit = "g", DisplayUnit = "kg", DisplayFactor = 1000, StockQty = 4000, MinStock = 800 },
                new IngredientEntity { Name = "Trứng gà", BaseUnit = "pcs", DisplayUnit = "quả", DisplayFactor = 1, StockQty = 120, MinStock = 24 },
                new IngredientEntity { Name = "Cam", BaseUnit = "pcs", DisplayUnit = "quả", DisplayFactor = 1, StockQty = 80, MinStock = 20 },
                new IngredientEntity { Name = "Nước ngọt", BaseUnit = "pcs", DisplayUnit = "lon", DisplayFactor = 1, StockQty = 96, MinStock = 24 });
            await db.SaveChangesAsync(cancellationToken);
        }
        if (await db.Recipes.AnyAsync(cancellationToken)) return;
        var menu = await db.MenuItems.ToDictionaryAsync(item => item.Name, cancellationToken); var ingredients = await db.Ingredients.ToDictionaryAsync(item => item.Name, cancellationToken);
        (string Item, string Ingredient, decimal Qty)[] recipes =
        [
            ("Bò bít tết", "Thịt bò", 200), ("Bò bít tết", "Khoai tây", 50), ("Cá hồi áp chảo", "Cá hồi", 220),
            ("Gà nướng thảo mộc", "Thịt gà", 250), ("Pizza phô mai", "Phô mai", 120), ("Cà phê sữa", "Cà phê hạt", 20),
            ("Cà phê sữa", "Sữa tươi", 50), ("Nước cam", "Cam", 3), ("Tiramisu", "Trứng gà", 1), ("Tiramisu", "Sữa tươi", 40),
            ("Burger bò", "Thịt bò", 150), ("Mì Ý bò bằm", "Thịt bò", 120)
        ];
        foreach (var recipe in recipes) db.Recipes.Add(new RecipeEntity { ItemId = menu[recipe.Item].Id, IngredientId = ingredients[recipe.Ingredient].Id, Qty = recipe.Qty });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(DateTime now, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(cancellationToken)) return;
        var password = configuration["Seed:DemoPassword"];
        if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Thiếu Seed:DemoPassword. Đặt bằng user-secrets hoặc biến môi trường trước khi seed.");
        logger.LogWarning("Đang tạo tài khoản demo. Hãy đổi mật khẩu ngay sau lần đăng nhập đầu tiên.");
        var roles = await db.Roles.ToDictionaryAsync(role => role.Code, cancellationToken);
        foreach (var role in Enum.GetValues<Role>()) db.Users.Add(new UserEntity { RoleId = roles[role].Id, Username = role.ToString().ToLowerInvariant(), PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 11), FullName = roles[role].Name, CreatedAt = now });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCustomerAsync(DateTime now, CancellationToken cancellationToken)
    {
        if (!await db.Customers.AnyAsync(customer => customer.Phone == "0987654321", cancellationToken))
        {
            db.Customers.Add(new CustomerEntity { FullName = "Anh Long", Phone = "0987654321", ZaloUserId = "zalo-demo-long", IsVip = true, CreatedAt = now });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
