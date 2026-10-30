using Microsoft.Extensions.DependencyInjection;
using Restaurant.Core;

namespace Restaurant.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRestaurantInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileStorage, FakeFileStorage>();
        return services;
    }
}
