using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restaurant.Contracts;

namespace Restaurant.Client.Core;

public abstract class ViewModelBase : ObservableObject
{
    private bool isBusy;
    private string? errorMessage;
    public bool IsBusy { get => isBusy; protected set => SetProperty(ref isBusy, value); }
    public string? ErrorMessage { get => errorMessage; protected set => SetProperty(ref errorMessage, value); }
    protected static Task NoopAsync() => Task.CompletedTask;
}

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private readonly ISessionStore session;
    private string username = "cashier";
    private string password = "demo";
    public string Username { get => username; set => SetProperty(ref username, value); }
    public string Password { get => password; set => SetProperty(ref password, value); }
    public IAsyncRelayCommand LoginCommand { get; }

    public LoginViewModel(IRestaurantApi api, ISessionStore session)
    {
        this.api = api;
        this.session = session;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var result = await api.LoginAsync(Username, Password);
            session.CurrentUser = result.User;
            session.AccessToken = result.AccessToken;
        }
        catch (Exception)
        {
            ErrorMessage = "Không thể đăng nhập. Vui lòng thử lại.";
        }
        finally { IsBusy = false; }
    }
}

public sealed class ShellViewModel : ViewModelBase
{
    private readonly ISessionStore session;
    private object? currentPage;
    private ConnectionState connectionState = ConnectionState.Offline;
    private string clockText = string.Empty;
    public UserDto? CurrentUser => session.CurrentUser;
    public ObservableCollection<NavItemDto> NavItems { get; } = [];
    public object? CurrentPage { get => currentPage; set => SetProperty(ref currentPage, value); }
    public ConnectionState ConnectionState { get => connectionState; set => SetProperty(ref connectionState, value); }
    public string ClockText { get => clockText; set => SetProperty(ref clockText, value); }
    public ObservableCollection<ToastDto> Toasts { get; } = [];
    public int PendingEscalationCount => 1;
    public IRelayCommand<string> NavigateCommand { get; }
    public IRelayCommand LogoutCommand { get; }
    public IRelayCommand<ToastDto> DismissToastCommand { get; }

    public ShellViewModel(ISessionStore session)
    {
        this.session = session;
        NavigateCommand = new RelayCommand<string>(key => CurrentPage = key);
        LogoutCommand = new RelayCommand(() => { session.CurrentUser = null; session.AccessToken = null; OnPropertyChanged(nameof(CurrentUser)); });
        DismissToastCommand = new RelayCommand<ToastDto>(toast => { if (toast is not null) Toasts.Remove(toast); });
        RefreshNavigation();
        ClockText = DateTime.Now.ToString("HH:mm");
    }

    public void RefreshNavigation()
    {
        NavItems.Clear();
        var role = CurrentUser?.Role ?? Role.CASHIER;
        if (role != Role.KITCHEN) NavItems.Add(new("floor", "Sơ đồ bàn", "Table"));
        if (role is Role.MANAGER or Role.CASHIER or Role.WAITER) NavItems.Add(new("pos", "POS", "Cart"));
        if (role is Role.MANAGER or Role.KITCHEN) NavItems.Add(new("kitchen", "Bếp", "ChefHat"));
        if (role == Role.MANAGER) NavItems.Add(new("reports", "Báo cáo", "Chart"));
        OnPropertyChanged(nameof(CurrentUser));
    }
}

public sealed class FloorPlanViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private AreaDto? selectedArea;
    private TableTileViewModel? selectedTile;
    private string? statusFilter;
    private bool isEditMode;
    public ObservableCollection<AreaDto> Areas { get; } = [];
    public AreaDto? SelectedArea { get => selectedArea; set => SetProperty(ref selectedArea, value); }
    public ObservableCollection<TableTileViewModel> Tiles { get; } = [];
    public TableTileViewModel? SelectedTile { get => selectedTile; set => SetProperty(ref selectedTile, value); }
    public IReadOnlyDictionary<string, int> LegendCounts { get; private set; } = new Dictionary<string, int>();
    public string? StatusFilter { get => statusFilter; set => SetProperty(ref statusFilter, value); }
    public bool IsEditMode { get => isEditMode; set => SetProperty(ref isEditMode, value); }
    public IRelayCommand<AreaDto> SelectAreaCommand { get; }
    public IAsyncRelayCommand OpenTableCommand { get; }
    public IAsyncRelayCommand MarkCleanedCommand { get; }
    public IAsyncRelayCommand RequestCleaningCommand { get; }
    public IRelayCommand<object?> MoveTableCommand { get; }
    public IAsyncRelayCommand MergeTablesCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand SaveLayoutCommand { get; }

    public FloorPlanViewModel(IRestaurantApi api)
    {
        this.api = api;
        SelectAreaCommand = new RelayCommand<AreaDto>(area => SelectedArea = area);
        OpenTableCommand = new AsyncRelayCommand(NoopAsync);
        MarkCleanedCommand = new AsyncRelayCommand(NoopAsync);
        RequestCleaningCommand = new AsyncRelayCommand(NoopAsync);
        MoveTableCommand = new RelayCommand<object?>(_ => { });
        MergeTablesCommand = new AsyncRelayCommand(NoopAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        SaveLayoutCommand = new AsyncRelayCommand(NoopAsync);
    }

    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var floor = await api.GetFloorAsync();
            Areas.Clear(); foreach (var area in floor.Areas) Areas.Add(area);
            Tiles.Clear(); foreach (var table in floor.Tables) Tiles.Add(new TableTileViewModel(table));
            SelectedArea ??= Areas.FirstOrDefault();
            LegendCounts = floor.Counts;
            OnPropertyChanged(nameof(LegendCounts));
        }
        finally { IsBusy = false; }
    }
}

public sealed class TableTileViewModel : ObservableObject
{
    private TimeSpan? holdRemaining;
    public int Id { get; }
    public string Name { get; }
    public int Seats { get; }
    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }
    public string Shape { get; }
    public TableDisplayStatus DisplayStatus { get; }
    public string StatusLabel => DisplayStatus switch { TableDisplayStatus.AVAILABLE => "Trống", TableDisplayStatus.RESERVED => "Đã đặt", TableDisplayStatus.OCCUPIED => "Có khách", TableDisplayStatus.CLEANING => "Đang dọn", _ => "Đang giữ" };
    public string StatusIconKey => DisplayStatus.ToString();
    public string Subtitle { get; }
    public bool HasHold { get; }
    public string? HoldPhase { get; }
    public TimeSpan? HoldRemaining { get => holdRemaining; set { if (SetProperty(ref holdRemaining, value)) OnPropertyChanged(nameof(HoldRemainingText)); } }
    public string HoldRemainingText => HoldRemaining is { } value ? $"{(int)value.TotalMinutes:00}:{value.Seconds:00}" : string.Empty;
    public bool HasOverstay { get; }

    public TableTileViewModel(TableDto table)
    {
        Id = table.Id; Name = table.Name; Seats = table.Seats; X = table.X; Y = table.Y; Width = table.Width; Height = table.Height; Shape = table.Shape;
        DisplayStatus = table.DisplayStatus; HasHold = table.Hold is not null; HoldPhase = table.Hold?.Phase; HoldRemaining = table.Hold is null ? null : table.Hold.ExpiresAt - DateTime.UtcNow; HasOverstay = table.Flags.Overstay;
        Subtitle = table.NextReservation is not null ? $"{table.NextReservation.StartsAt.ToLocalTime():HH:mm} · {table.NextReservation.GuestName}" : table.ActiveOrder is not null ? $"47' · {table.ActiveOrder.RunningTotal:N0}đ" : $"{table.Seats} ghế";
    }
}

public sealed class TableDetailViewModel : ViewModelBase
{
    public TableDto? Table { get; set; }
    public ReservationSummaryDto? Reservation { get; set; }
    public ActiveOrderDto? ActiveOrder { get; set; }
    public bool CanOpen { get; set; } = true;
    public bool CanMove { get; set; } = true;
    public bool CanMerge { get; set; } = true;
    public bool CanCheckout { get; set; } = true;
    public IAsyncRelayCommand OpenCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand CheckInCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand UpdateGuestCountCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand GoToPosCommand { get; } = new AsyncRelayCommand(NoopAsync);
}
