using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Restaurant.Contracts;
using Restaurant.Core;
using Restaurant.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Restaurant.Api.Tests;

public sealed class DatabaseIntegrationTests
{
    [Fact]
    public async Task PostgreSql_schema_seed_and_guards_work_end_to_end()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var postgres = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("restaurant_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await postgres.StartAsync(cancellationToken);

        var connectionString = postgres.GetConnectionString();
        await ExecuteAsync(connectionString, "CREATE ROLE anon LOGIN PASSWORD 'anon_test'", cancellationToken);

        var options = CreateOptions(connectionString);
        await using (var db = new RestaurantDbContext(options))
        {
            await db.Database.MigrateAsync(cancellationToken);
            var configuration = new ConfigurationManager { ["Seed:DemoPassword"] = "Local-Test-Only-2026!" };
            var seeder = new DbSeeder(db, new FixedClock(), configuration, NullLogger<DbSeeder>.Instance);
            await seeder.SeedAsync(cancellationToken);
            await seeder.SeedAsync(cancellationToken);

            (await db.Roles.CountAsync(cancellationToken)).Should().Be(4);
            (await db.Areas.CountAsync(cancellationToken)).Should().Be(4);
            (await db.Tables.CountAsync(cancellationToken)).Should().Be(22);
            (await db.MenuItems.CountAsync(cancellationToken)).Should().Be(20);
            (await db.Recipes.Select(recipe => recipe.ItemId).Distinct().CountAsync(cancellationToken)).Should().BeGreaterThanOrEqualTo(8);
            (await db.Users.CountAsync(cancellationToken)).Should().Be(4);
            (await db.Customers.CountAsync(customer => customer.Phone == "0987654321", cancellationToken)).Should().Be(1);
        }

        (await ScalarAsync<long>(connectionString, "SELECT count(*) FROM pg_tables WHERE schemaname = 'public'", cancellationToken)).Should().Be(26);
        (await ScalarAsync<long>(connectionString, "SELECT count(*) FROM pg_tables WHERE schemaname = 'public' AND NOT rowsecurity", cancellationToken)).Should().Be(0);
        (await ScalarAsync<long>(connectionString, "SELECT count(*) FROM pg_tables WHERE schemaname = 'public' AND (has_table_privilege('anon', quote_ident(schemaname) || '.' || quote_ident(tablename), 'SELECT') OR has_table_privilege('anon', quote_ident(schemaname) || '.' || quote_ident(tablename), 'INSERT'))", cancellationToken)).Should().Be(0);
        (await ScalarAsync<string>(connectionString, "SELECT extnamespace::regnamespace::text FROM pg_extension WHERE extname = 'btree_gist'", cancellationToken)).Should().Be("extensions");

        await VerifyJsonAndUtcAsync(options, cancellationToken);
        await VerifyReservationExclusionAsync(options, cancellationToken);
        await VerifyPartialUniqueIndexesAsync(options, cancellationToken);
        await VerifyAnonymousRoleIsBlockedAsync(postgres, cancellationToken);
    }

    private static DbContextOptions<RestaurantDbContext> CreateOptions(string connectionString) =>
        new DbContextOptionsBuilder<RestaurantDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

    private static async Task VerifyJsonAndUtcAsync(DbContextOptions<RestaurantDbContext> options, CancellationToken cancellationToken)
    {
        long reservationId;
        await using (var db = new RestaurantDbContext(options))
        {
            var tableId = await db.Tables.Select(table => table.Id).FirstAsync(cancellationToken);
            var reservation = NewReservation("JSON-UTC", tableId, Utc(2026, 11, 1, 12), ReservationStatus.COMPLETED);
            reservation.Allergies = JsonDocument.Parse("{\"items\":[\"peanut\"]}");
            db.Reservations.Add(reservation);
            await db.SaveChangesAsync(cancellationToken);
            reservationId = reservation.Id;
        }

        await using (var db = new RestaurantDbContext(options))
        {
            var stored = await db.Reservations.SingleAsync(item => item.Id == reservationId, cancellationToken);
            stored.StartTime.Kind.Should().Be(DateTimeKind.Utc);
            stored.Allergies!.RootElement.GetProperty("items")[0].GetString().Should().Be("peanut");
        }

        await using (var db = new RestaurantDbContext(options))
        {
            db.Customers.Add(new CustomerEntity { FullName = "Invalid time", Phone = "0900000001", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified) });
            var action = async () => await db.SaveChangesAsync(cancellationToken);
            var exception = await action.Should().ThrowAsync<Exception>();
            exception.Which.ToString().Should().Contain("Kind=Utc");
        }
    }

    private static async Task VerifyReservationExclusionAsync(DbContextOptions<RestaurantDbContext> options, CancellationToken cancellationToken)
    {
        int tableId;
        await using (var db = new RestaurantDbContext(options))
        {
            tableId = await db.Tables.Select(table => table.Id).FirstAsync(cancellationToken);
            db.Reservations.Add(NewReservation("BASE-1900", tableId, Utc(2026, 11, 2, 12), ReservationStatus.CONFIRMED));
            await db.SaveChangesAsync(cancellationToken);
        }

        await SaveReservationAsync(options, NewReservation("EDGE-1700", tableId, Utc(2026, 11, 2, 10), ReservationStatus.CONFIRMED), cancellationToken);
        await AssertExclusionViolationAsync(options, NewReservation("EDGE-1715", tableId, Utc(2026, 11, 2, 10, 15), ReservationStatus.CONFIRMED), cancellationToken);
        await AssertExclusionViolationAsync(options, NewReservation("EDGE-2045", tableId, Utc(2026, 11, 2, 13, 45), ReservationStatus.CONFIRMED), cancellationToken);
        await SaveReservationAsync(options, NewReservation("EDGE-2100", tableId, Utc(2026, 11, 2, 14), ReservationStatus.CONFIRMED), cancellationToken);
        await SaveReservationAsync(options, NewReservation("CANCELLED-OVERLAP", tableId, Utc(2026, 11, 2, 12, 15), ReservationStatus.CANCELLED), cancellationToken);
        await SaveReservationAsync(options, NewReservation("EXPIRED-OVERLAP", tableId, Utc(2026, 11, 2, 12, 30), ReservationStatus.EXPIRED), cancellationToken);
    }

    private static async Task VerifyPartialUniqueIndexesAsync(DbContextOptions<RestaurantDbContext> options, CancellationToken cancellationToken)
    {
        int[] tables;
        int ingredientId;
        long[] reservationIds;
        await using (var db = new RestaurantDbContext(options))
        {
            tables = await db.Tables.OrderBy(table => table.Id).Select(table => table.Id).Take(4).ToArrayAsync(cancellationToken);
            ingredientId = await db.Ingredients.Select(item => item.Id).FirstAsync(cancellationToken);
            reservationIds = await db.Reservations.OrderBy(item => item.Id).Select(item => item.Id).Take(2).ToArrayAsync(cancellationToken);
        }

        await using (var db = new RestaurantDbContext(options))
        {
            db.Orders.Add(new OrderEntity { TableId = tables[1], Status = OrderStatus.OPEN, PartySize = 2, OpenedAt = FixedClock.Now });
            await db.SaveChangesAsync(cancellationToken);
        }
        await AssertUniqueViolationAsync(options, db => db.Orders.Add(new OrderEntity { TableId = tables[1], Status = OrderStatus.BILL_REQUESTED, PartySize = 2, OpenedAt = FixedClock.Now }), cancellationToken);
        await using (var db = new RestaurantDbContext(options))
        {
            db.Orders.Add(new OrderEntity { TableId = tables[1], Status = OrderStatus.PAID, PartySize = 2, OpenedAt = FixedClock.Now, ClosedAt = FixedClock.Now });
            await db.SaveChangesAsync(cancellationToken);
        }

        await using (var db = new RestaurantDbContext(options))
        {
            db.TableSwitchRequests.Add(NewSwitch(reservationIds[0], tables[0], tables[2], SwitchStatus.PENDING_CUSTOMER));
            await db.SaveChangesAsync(cancellationToken);
        }
        await AssertUniqueViolationAsync(options, db => db.TableSwitchRequests.Add(NewSwitch(reservationIds[1], tables[1], tables[2], SwitchStatus.ESCALATED)), cancellationToken);
        await using (var db = new RestaurantDbContext(options))
        {
            db.TableSwitchRequests.Add(NewSwitch(reservationIds[1], tables[1], tables[2], SwitchStatus.ACCEPTED));
            await db.SaveChangesAsync(cancellationToken);
        }

        long invoiceId;
        await using (var db = new RestaurantDbContext(options))
        {
            var order = new OrderEntity { TableId = tables[3], Status = OrderStatus.PAID, PartySize = 1, OpenedAt = FixedClock.Now, ClosedAt = FixedClock.Now };
            db.Orders.Add(order);
            await db.SaveChangesAsync(cancellationToken);
            var invoice = new InvoiceEntity { OrderId = order.Id, InvoiceNo = "TEST-INV-1", Status = "PAID", CreatedAt = FixedClock.Now, PaidAt = FixedClock.Now };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync(cancellationToken);
            invoiceId = invoice.Id;
            db.StockMovements.Add(new StockMovementEntity { IngredientId = ingredientId, RefInvoiceId = invoiceId, Type = "SALE", Delta = -1, CreatedAt = FixedClock.Now });
            await db.SaveChangesAsync(cancellationToken);
        }
        await AssertUniqueViolationAsync(options, db => db.StockMovements.Add(new StockMovementEntity { IngredientId = ingredientId, RefInvoiceId = invoiceId, Type = "SALE", Delta = -1, CreatedAt = FixedClock.Now }), cancellationToken);
        await using (var db = new RestaurantDbContext(options))
        {
            db.StockMovements.Add(new StockMovementEntity { IngredientId = ingredientId, RefInvoiceId = invoiceId, Type = "ADJUST", Delta = 1, CreatedAt = FixedClock.Now });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task VerifyAnonymousRoleIsBlockedAsync(PostgreSqlContainer postgres, CancellationToken cancellationToken)
    {
        var builder = new NpgsqlConnectionStringBuilder(postgres.GetConnectionString()) { Username = "anon", Password = "anon_test" };
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var select = new NpgsqlCommand("SELECT * FROM public.roles", connection);
        var selectAction = async () => await select.ExecuteReaderAsync(cancellationToken);
        (await selectAction.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        await using var insert = new NpgsqlCommand("INSERT INTO public.app_settings(key, value, updated_at) VALUES ('blocked', 'blocked', now())", connection);
        var insertAction = async () => await insert.ExecuteNonQueryAsync(cancellationToken);
        (await insertAction.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.InsufficientPrivilege);
    }

    private static ReservationEntity NewReservation(string code, int tableId, DateTime start, ReservationStatus status) => new()
    {
        Code = code,
        TableId = tableId,
        GuestName = "Integration Test",
        GuestPhone = "0900000000",
        StartTime = start,
        OccupancyMinutes = 105,
        OccupiedUntil = start.AddMinutes(105),
        PartySize = 2,
        ViewType = ViewType.STANDARD,
        Status = status,
        DepositStatus = DepositStatus.NONE,
        Source = "STAFF",
        CreatedAt = FixedClock.Now,
        UpdatedAt = FixedClock.Now
    };

    private static TableSwitchRequestEntity NewSwitch(long reservationId, int fromTableId, int toTableId, SwitchStatus status) => new()
    {
        ReservationId = reservationId,
        FromTableId = fromTableId,
        ToTableId = toTableId,
        Status = status,
        OfferedAt = FixedClock.Now,
        ExpiresAt = FixedClock.Now.AddMinutes(10),
        CreatedAt = FixedClock.Now
    };

    private static async Task SaveReservationAsync(DbContextOptions<RestaurantDbContext> options, ReservationEntity reservation, CancellationToken cancellationToken)
    {
        await using var db = new RestaurantDbContext(options);
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task AssertExclusionViolationAsync(DbContextOptions<RestaurantDbContext> options, ReservationEntity reservation, CancellationToken cancellationToken)
    {
        await using var db = new RestaurantDbContext(options);
        db.Reservations.Add(reservation);
        var action = async () => await db.SaveChangesAsync(cancellationToken);
        var exception = await action.Should().ThrowAsync<DbUpdateException>();
        exception.Which.GetBaseException().Should().BeOfType<PostgresException>().Which.SqlState.Should().Be(PostgresErrorCodes.ExclusionViolation);
    }

    private static async Task AssertUniqueViolationAsync(DbContextOptions<RestaurantDbContext> options, Action<RestaurantDbContext> arrange, CancellationToken cancellationToken)
    {
        await using var db = new RestaurantDbContext(options);
        arrange(db);
        var action = async () => await db.SaveChangesAsync(cancellationToken);
        var exception = await action.Should().ThrowAsync<DbUpdateException>();
        exception.Which.GetBaseException().Should().BeOfType<PostgresException>().Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    private static DateTime Utc(int year, int month, int day, int hour, int minute = 0) => new(year, month, day, hour, minute, 0, DateTimeKind.Utc);

    private static async Task ExecuteAsync(string connectionString, string sql, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<T> ScalarAsync<T>(string connectionString, string sql, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    private sealed class FixedClock : IClock
    {
        public static readonly DateTime Now = Utc(2026, 10, 31, 12);
        public DateTime UtcNow => Now;
    }
}
