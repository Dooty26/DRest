using Microsoft.Extensions.DependencyInjection;

namespace Restaurant.Client.Core;

public static class ClientCoreModule
{
    public static IServiceCollection AddClientCore(this IServiceCollection services, bool fake)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, ClientClock>();
        services.AddSingleton<ISessionStore, SessionStore>();
        services.AddSingleton<IRestaurantApi, FakeRestaurantApi>();
        services.AddSingleton<IRealtimeClient, FakeRealtimeClient>();
        services.AddSingleton<IUiDispatcher, ImmediateUiDispatcher>();
        services.AddSingleton<IDialogService, AcceptDialogService>();
        services.AddSingleton<IPrintService, NullPrintService>();
        services.AddSingleton<INotificationSound, NullNotificationSound>();
        services.AddSingleton<IClipboardService, MemoryClipboardService>();
        services.AddSingleton<IFilePickerService, NullFilePickerService>();
        services.AddSingleton<IImageCache, NullImageCache>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<FloorPlanViewModel>();
        services.AddTransient<ReservationsViewModel>();
        services.AddTransient<PosViewModel>();
        services.AddTransient<CheckoutViewModel>();
        services.AddTransient<KitchenViewModel>();
        services.AddTransient<EscalationQueueViewModel>();
        services.AddTransient<AiActivityViewModel>();
        services.AddTransient<WaitlistViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<RecipeEditorViewModel>();
        services.AddTransient<MenuAdminViewModel>();
        services.AddTransient<UsersAdminViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<PrintSettingsViewModel>();
        return services;
    }
}
