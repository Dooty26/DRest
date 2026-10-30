using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restaurant.Contracts;

namespace Restaurant.Client.Core;

public sealed class EscalationItemViewModel : ObservableObject
{
    public int RequestId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string TableName { get; init; } = string.Empty;
    public DateTime ReservationTime { get; init; }
    public string ElapsedText { get; init; } = "10:00";
    public string EscalationRemainingText { get; init; } = "10:00";
    public IAsyncRelayCommand CustomerAgreedCommand { get; } = new AsyncRelayCommand(() => Task.CompletedTask);
    public IAsyncRelayCommand CustomerKeepOldCommand { get; } = new AsyncRelayCommand(() => Task.CompletedTask);
    public IAsyncRelayCommand NoAnswerCommand { get; } = new AsyncRelayCommand(() => Task.CompletedTask);
    public IAsyncRelayCommand CopyPhoneCommand { get; } = new AsyncRelayCommand(() => Task.CompletedTask);
}

public sealed class EscalationQueueViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private EscalationItemViewModel? current;
    public ObservableCollection<EscalationItemViewModel> Items { get; } = [];
    public EscalationItemViewModel? Current { get => current; set => SetProperty(ref current, value); }
    public IAsyncRelayCommand RefreshCommand { get; }
    public EscalationQueueViewModel(IRestaurantApi api) { this.api = api; RefreshCommand = new AsyncRelayCommand(RefreshAsync); }
    public async Task RefreshAsync()
    {
        var values = await api.GetSwitchRequestsAsync(); Items.Clear();
        foreach (var value in values.Where(value => value.Status == SwitchStatus.ESCALATED)) Items.Add(new EscalationItemViewModel { RequestId = value.Id, CustomerName = value.Reservation.GuestName, Phone = value.Reservation.Phone, TableName = value.ToTable.Name, ReservationTime = value.Reservation.StartsAt });
        Current = Items.FirstOrDefault();
    }
}

public sealed class AiActivityViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private SwitchRequestDto? selected;
    public ObservableCollection<SwitchRequestDto> Requests { get; } = [];
    public SwitchRequestDto? Selected { get => selected; set { if (SetProperty(ref selected, value)) OnPropertyChanged(nameof(Timeline)); } }
    public IReadOnlyList<SwitchTimelineItemDto> Timeline => Selected?.Timeline ?? [];
    public IAsyncRelayCommand RefreshCommand { get; }
    public AiActivityViewModel(IRestaurantApi api) { this.api = api; RefreshCommand = new AsyncRelayCommand(RefreshAsync); }
    public async Task RefreshAsync() { var values = await api.GetSwitchRequestsAsync(); Requests.Clear(); foreach (var value in values) Requests.Add(value); Selected = Requests.FirstOrDefault(); }
}

public sealed class WaitlistViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private string newName = string.Empty;
    private string newPhone = string.Empty;
    private int newPartySize = 2;
    public ObservableCollection<WaitlistEntryDto> Entries { get; } = [];
    public string NewName { get => newName; set => SetProperty(ref newName, value); }
    public string NewPhone { get => newPhone; set => SetProperty(ref newPhone, value); }
    public int NewPartySize { get => newPartySize; set => SetProperty(ref newPartySize, value); }
    public ObservableCollection<TableDto> SuggestedTables { get; } = [];
    public IRelayCommand AddCommand { get; }
    public IAsyncRelayCommand NotifyCommand { get; }
    public IRelayCommand<TableDto> SeatCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public WaitlistViewModel(IRestaurantApi api)
    {
        this.api = api; AddCommand = new RelayCommand(() => Entries.Add(new WaitlistEntryDto(Entries.Count + 1, NewName, NewPhone, NewPartySize, WaitlistStatus.WAITING, DateTime.UtcNow)));
        NotifyCommand = new AsyncRelayCommand(NoopAsync); SeatCommand = new RelayCommand<TableDto>(_ => { }); CancelCommand = new AsyncRelayCommand(NoopAsync); RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }
    public async Task RefreshAsync() { var values = await api.GetWaitlistAsync(); Entries.Clear(); foreach (var value in values) Entries.Add(value); }
}

public sealed class InventoryViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private IngredientDto? selected;
    public ObservableCollection<IngredientDto> Ingredients { get; } = [];
    public ObservableCollection<StockMovementDto> Movements { get; } = [];
    public IngredientDto? Selected { get => selected; set => SetProperty(ref selected, value); }
    public IAsyncRelayCommand AdjustCommand { get; }
    public IAsyncRelayCommand SaveIngredientCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public InventoryViewModel(IRestaurantApi api) { this.api = api; AdjustCommand = new AsyncRelayCommand(NoopAsync); SaveIngredientCommand = new AsyncRelayCommand(NoopAsync); RefreshCommand = new AsyncRelayCommand(RefreshAsync); }
    public async Task RefreshAsync() { var values = await api.GetIngredientsAsync(); Ingredients.Clear(); foreach (var value in values) Ingredients.Add(value); Selected = Ingredients.FirstOrDefault(); }
}

public sealed class RecipeEditorViewModel : ViewModelBase
{
    public MenuItemDto? MenuItem { get; set; }
    public ObservableCollection<RecipeRowDto> Rows { get; } = [];
    public ObservableCollection<IngredientDto> AvailableIngredients { get; } = [];
    public IRelayCommand AddRowCommand { get; }
    public IRelayCommand<RecipeRowDto> RemoveRowCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public RecipeEditorViewModel() { AddRowCommand = new RelayCommand(() => { }); RemoveRowCommand = new RelayCommand<RecipeRowDto>(row => { if (row is not null) Rows.Remove(row); }); SaveCommand = new AsyncRelayCommand(NoopAsync); }
}

public sealed class MenuAdminViewModel : ViewModelBase
{
    private string? imageUrl;
    private bool isUploadingImage;
    private string? imageError;
    public ObservableCollection<MenuItemDto> Items { get; } = [];
    public string? ImageUrl { get => imageUrl; set => SetProperty(ref imageUrl, value); }
    public bool IsUploadingImage { get => isUploadingImage; set => SetProperty(ref isUploadingImage, value); }
    public string? ImageError { get => imageError; set => SetProperty(ref imageError, value); }
    public IAsyncRelayCommand SaveCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand DeleteCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand PickAndUploadImageCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand RemoveImageCommand { get; } = new AsyncRelayCommand(NoopAsync);
}

public sealed class UsersAdminViewModel : ViewModelBase
{
    public ObservableCollection<UserDto> Items { get; } = [];
    public IAsyncRelayCommand SaveCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand DeleteCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand ResetPasswordCommand { get; } = new AsyncRelayCommand(NoopAsync);
}

public sealed class SettingsViewModel : ViewModelBase
{
    private string? logoUrl;
    private bool isUploadingLogo;
    public int StayMinutes { get; set; } = 90;
    public int CleaningMinutes { get; set; } = 15;
    public int DepositHoldMinutes { get; set; } = 15;
    public string? LogoUrl { get => logoUrl; set => SetProperty(ref logoUrl, value); }
    public bool IsUploadingLogo { get => isUploadingLogo; set => SetProperty(ref isUploadingLogo, value); }
    public IAsyncRelayCommand SaveCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand PickAndUploadLogoCommand { get; } = new AsyncRelayCommand(NoopAsync);
}

public sealed class ReportsViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    public DateOnly From { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
    public DateOnly To { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string GroupBy { get; set; } = "day";
    public IReadOnlyList<RevenuePointDto> RevenueSeries { get; private set; } = [];
    public DepositSummaryDto? DepositSummary { get; private set; }
    public IReadOnlyList<MenuItemDto> TopItems { get; private set; } = [];
    public IReadOnlyList<IngredientDto> LowStock { get; private set; } = [];
    public CashFundDto? CashFund { get; private set; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }
    public ReportsViewModel(IRestaurantApi api) { this.api = api; LoadCommand = new AsyncRelayCommand(LoadAsync); ExportCommand = new AsyncRelayCommand(NoopAsync); }
    public async Task LoadAsync() { var value = await api.GetReportsAsync(); RevenueSeries = value.RevenueSeries; DepositSummary = value.DepositSummary; TopItems = value.TopItems; LowStock = value.LowStock; CashFund = value.CashFund; OnPropertyChanged(string.Empty); }
}

public sealed class PrintSettingsViewModel : ViewModelBase
{
    public bool IsPrintHost { get; set; }
    public string? KitchenPrinterName { get; set; }
    public string? ReceiptPrinterName { get; set; }
    public ObservableCollection<string> Printers { get; } = [];
    public IAsyncRelayCommand SaveCommand { get; } = new AsyncRelayCommand(NoopAsync);
    public IAsyncRelayCommand TestPrintCommand { get; } = new AsyncRelayCommand(NoopAsync);
}
