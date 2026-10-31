using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Restaurant.Core;
using Restaurant.Infrastructure.Persistence;

namespace Restaurant.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRestaurantInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileStorage, FakeFileStorage>();
        return services.AddRestaurantDb(configuration);
    }

    public static IServiceCollection AddRestaurantDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<RestaurantDbContext>(options => options
                .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null))
                .UseSnakeCaseNamingConvention());
            services.AddScoped<DbSeeder>();
        }
        return services;
    }

    public static async Task InitializeRestaurantDatabaseAsync(this IServiceProvider services, IConfiguration configuration, bool isDevelopment, CancellationToken cancellationToken = default)
    {
        if (!bool.TryParse(configuration["Database:AutoMigrate"], out var autoMigrate) || !autoMigrate) return;
        if (!isDevelopment) throw new InvalidOperationException("Database:AutoMigrate chỉ được phép trong Development.");
        var connectionString = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Thiếu ConnectionStrings:Default.");
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (!IsLocalHost(builder.Host)) throw new InvalidOperationException("Database:AutoMigrate chỉ được phép kết nối PostgreSQL local.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync(cancellationToken);
    }

    private static bool IsLocalHost(string? host) =>
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));
}
