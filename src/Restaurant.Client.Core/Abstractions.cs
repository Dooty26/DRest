using Restaurant.Contracts;

namespace Restaurant.Client.Core;

public interface IRestaurantApi
{
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<FloorPlanDto> GetFloorAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReservationSummaryDto>> GetReservationsAsync(CancellationToken cancellationToken = default);
    Task<PublicMenuDto> GetMenuAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SwitchRequestDto>> GetSwitchRequestsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WaitlistEntryDto>> GetWaitlistAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IngredientDto>> GetIngredientsAsync(CancellationToken cancellationToken = default);
    Task<ReportsDto> GetReportsAsync(CancellationToken cancellationToken = default);
    Task<InvoiceDto> GetInvoicePreviewAsync(CancellationToken cancellationToken = default);
}

public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class ClientClock(TimeProvider timeProvider) : IClock
{
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}

public interface IRealtimeClient
{
    event EventHandler<TableDto>? TableStateChanged;
    event EventHandler<KitchenTicketModel>? OrderItemsAdded;
    event EventHandler<SwitchRequestDto>? SwitchOfferEscalated;
    event EventHandler<ConnectionState>? ConnectionStateChanged;
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public interface ISessionStore
{
    UserDto? CurrentUser { get; set; }
    string? AccessToken { get; set; }
}

public sealed class SessionStore : ISessionStore
{
    public UserDto? CurrentUser { get; set; }
    public string? AccessToken { get; set; }
}

public interface IUiDispatcher { Task InvokeAsync(Action action); }
public interface IDialogService { Task<bool> ConfirmAsync(string title, string message, CancellationToken cancellationToken = default); }
public interface IPrintService
{
    Task PrintReceiptAsync(ReceiptModel model, CancellationToken cancellationToken = default);
    Task PrintKitchenTicketAsync(KitchenTicketModel model, CancellationToken cancellationToken = default);
}
public interface INotificationSound { Task PlayAsync(string sound, CancellationToken cancellationToken = default); }
public interface IClipboardService { Task SetTextAsync(string text, CancellationToken cancellationToken = default); }
public sealed record FileResult(string Name, Stream Content, long Length);
public interface IFilePickerService { Task<FileResult?> PickImageAsync(CancellationToken cancellationToken = default); }
public interface IImageCache { Task<string?> GetLocalPathAsync(string url, CancellationToken cancellationToken = default); }

public sealed class ImmediateUiDispatcher : IUiDispatcher
{
    public Task InvokeAsync(Action action) { action(); return Task.CompletedTask; }
}

public sealed class AcceptDialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, CancellationToken cancellationToken = default) => Task.FromResult(true);
}

public sealed class NullPrintService : IPrintService
{
    public Task PrintReceiptAsync(ReceiptModel model, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task PrintKitchenTicketAsync(KitchenTicketModel model, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class NullNotificationSound : INotificationSound
{
    public Task PlayAsync(string sound, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class MemoryClipboardService : IClipboardService
{
    public string? Text { get; private set; }
    public Task SetTextAsync(string text, CancellationToken cancellationToken = default) { Text = text; return Task.CompletedTask; }
}

public sealed class NullFilePickerService : IFilePickerService
{
    public Task<FileResult?> PickImageAsync(CancellationToken cancellationToken = default) => Task.FromResult<FileResult?>(null);
}

public sealed class NullImageCache : IImageCache
{
    public Task<string?> GetLocalPathAsync(string url, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
}
