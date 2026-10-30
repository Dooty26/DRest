using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restaurant.Contracts;

namespace Restaurant.Client.Core;

public sealed class ReservationsViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private DateOnly date = DateOnly.FromDateTime(DateTime.Today);
    private ReservationStatus? statusFilter;
    private string searchText = string.Empty;
    private ReservationSummaryDto? selected;
    public DateOnly Date { get => date; set => SetProperty(ref date, value); }
    public ReservationStatus? StatusFilter { get => statusFilter; set => SetProperty(ref statusFilter, value); }
    public string SearchText { get => searchText; set => SetProperty(ref searchText, value); }
    public ObservableCollection<ReservationSummaryDto> Items { get; } = [];
    public ReservationSummaryDto? Selected { get => selected; set => SetProperty(ref selected, value); }
    public IAsyncRelayCommand CheckInCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }
    public IAsyncRelayCommand MarkNoShowCommand { get; }
    public IAsyncRelayCommand UpdateGuestCountCommand { get; }
    public IAsyncRelayCommand ApplyApologyCommand { get; }
    public IAsyncRelayCommand RecordRefundCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    public ReservationsViewModel(IRestaurantApi api)
    {
        this.api = api;
        CheckInCommand = new AsyncRelayCommand(NoopAsync); CancelCommand = new AsyncRelayCommand(NoopAsync); MarkNoShowCommand = new AsyncRelayCommand(NoopAsync);
        UpdateGuestCountCommand = new AsyncRelayCommand(NoopAsync); ApplyApologyCommand = new AsyncRelayCommand(NoopAsync); RecordRefundCommand = new AsyncRelayCommand(NoopAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public async Task RefreshAsync()
    {
        var values = await api.GetReservationsAsync();
        Items.Clear(); foreach (var value in values) Items.Add(value);
        Selected = Items.FirstOrDefault();
    }
}

public sealed class OrderLineVm : ObservableObject
{
    public int ItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Qty { get; set; }
    public decimal UnitPrice { get; init; }
    public decimal Amount => Qty * UnitPrice;
    public string? Note { get; set; }
    public IReadOnlyList<string> AllergyTags { get; set; } = [];
    public OrderItemStatus Status { get; set; } = OrderItemStatus.NEW;
}

public sealed class PosViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private MenuCategoryDto? selectedCategory;
    private string searchText = string.Empty;
    public ObservableCollection<MenuCategoryDto> Categories { get; } = [];
    public MenuCategoryDto? SelectedCategory { get => selectedCategory; set { if (SetProperty(ref selectedCategory, value)) OnPropertyChanged(nameof(MenuItems)); } }
    public string SearchText { get => searchText; set => SetProperty(ref searchText, value); }
    public IReadOnlyList<MenuItemDto> MenuItems => SelectedCategory?.Items ?? [];
    public ObservableCollection<OrderLineVm> Lines { get; } = [];
    public decimal Subtotal => Lines.Sum(line => line.Amount);
    public string DiscountText => "0đ";
    public decimal VatAmount => 0;
    public decimal DepositApplied => 200000;
    public decimal NetAmount => Subtotal - DepositApplied;
    public bool IsRefundDue => NetAmount < 0;
    public decimal RefundDueAmount => Math.Max(0, -NetAmount);
    public bool HasAllergy => Lines.Any(line => line.AllergyTags.Count > 0);
    public IRelayCommand<MenuItemDto> AddItemCommand { get; }
    public IRelayCommand<OrderLineVm> IncQtyCommand { get; }
    public IRelayCommand<OrderLineVm> DecQtyCommand { get; }
    public IRelayCommand<OrderLineVm> RemoveLineCommand { get; }
    public IRelayCommand<OrderLineVm> SetNoteCommand { get; }
    public IAsyncRelayCommand SendToKitchenCommand { get; }
    public IAsyncRelayCommand PrintTempBillCommand { get; }
    public IAsyncRelayCommand OpenCheckoutCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    public PosViewModel(IRestaurantApi api)
    {
        this.api = api;
        AddItemCommand = new RelayCommand<MenuItemDto>(AddItem);
        IncQtyCommand = new RelayCommand<OrderLineVm>(line => ChangeQty(line, 1));
        DecQtyCommand = new RelayCommand<OrderLineVm>(line => ChangeQty(line, -1));
        RemoveLineCommand = new RelayCommand<OrderLineVm>(line => { if (line is not null) Lines.Remove(line); NotifyTotals(); });
        SetNoteCommand = new RelayCommand<OrderLineVm>(_ => { });
        SendToKitchenCommand = new AsyncRelayCommand(NoopAsync); PrintTempBillCommand = new AsyncRelayCommand(NoopAsync); OpenCheckoutCommand = new AsyncRelayCommand(NoopAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    private async Task RefreshAsync()
    {
        var menu = await api.GetMenuAsync(); Categories.Clear(); foreach (var category in menu.Categories) Categories.Add(category); SelectedCategory = Categories.FirstOrDefault();
    }

    private void AddItem(MenuItemDto? item)
    {
        if (item is null || !item.IsAvailable) return;
        var line = Lines.FirstOrDefault(value => value.ItemId == item.Id);
        if (line is null) Lines.Add(new OrderLineVm { ItemId = item.Id, Name = item.Name, Qty = 1, UnitPrice = item.Price }); else line.Qty++;
        NotifyTotals();
    }

    private void ChangeQty(OrderLineVm? line, int change) { if (line is null) return; line.Qty = Math.Max(1, line.Qty + change); NotifyTotals(); }
    private void NotifyTotals() { OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(NetAmount)); OnPropertyChanged(nameof(IsRefundDue)); OnPropertyChanged(nameof(RefundDueAmount)); OnPropertyChanged(nameof(HasAllergy)); }
}

public sealed class CheckoutViewModel : ViewModelBase
{
    private readonly IRestaurantApi api;
    private PaymentMethod method = PaymentMethod.CASH;
    private string? voucherCode;
    private decimal? discountPercent;
    private string? discountReason;
    private decimal? amountTendered;
    private InvoiceDto? preview;
    private bool refundConfirmed;
    public PaymentMethod Method { get => method; set => SetProperty(ref method, value); }
    public string? VoucherCode { get => voucherCode; set => SetProperty(ref voucherCode, value); }
    public decimal? DiscountPercent { get => discountPercent; set => SetProperty(ref discountPercent, value); }
    public string? DiscountReason { get => discountReason; set => SetProperty(ref discountReason, value); }
    public decimal? AmountTendered { get => amountTendered; set { if (SetProperty(ref amountTendered, value)) OnPropertyChanged(nameof(Change)); } }
    public decimal Change => Math.Max(0, (AmountTendered ?? 0) - Math.Max(0, Preview?.NetAmount ?? 0));
    public InvoiceDto? Preview { get => preview; private set { if (SetProperty(ref preview, value)) OnPropertyChanged(nameof(IsRefundDue)); } }
    public bool RefundConfirmed { get => refundConfirmed; set => SetProperty(ref refundConfirmed, value); }
    public bool IsRefundDue => Preview?.RefundDue > 0;
    public IAsyncRelayCommand ApplyVoucherCommand { get; }
    public IAsyncRelayCommand ConfirmPayCommand { get; }
    public IAsyncRelayCommand PrintReceiptCommand { get; }

    public CheckoutViewModel(IRestaurantApi api)
    {
        this.api = api;
        ApplyVoucherCommand = new AsyncRelayCommand(LoadPreviewAsync); ConfirmPayCommand = new AsyncRelayCommand(NoopAsync); PrintReceiptCommand = new AsyncRelayCommand(NoopAsync);
    }
    public async Task LoadPreviewAsync() => Preview = await api.GetInvoicePreviewAsync();
}

public sealed class KitchenTicketVm
{
    public string TableName { get; init; } = string.Empty;
    public string ElapsedText { get; init; } = string.Empty;
    public IReadOnlyList<KitchenTicketItemDto> Items { get; init; } = [];
    public bool HasAllergy { get; init; }
}

public sealed class KitchenViewModel : ViewModelBase
{
    public ObservableCollection<KitchenTicketVm> NewTickets { get; } = [];
    public ObservableCollection<KitchenTicketVm> CookingTickets { get; } = [];
    public ObservableCollection<KitchenTicketVm> ReadyTickets { get; } = [];
    public string Sound { get; set; } = "order-new";
    public IRelayCommand<KitchenTicketVm> StartCookingCommand { get; }
    public IRelayCommand<KitchenTicketVm> MarkReadyCommand { get; }
    public IRelayCommand<KitchenTicketVm> MarkOutOfStockCommand { get; }
    public KitchenViewModel()
    {
        StartCookingCommand = new RelayCommand<KitchenTicketVm>(ticket => Move(ticket, NewTickets, CookingTickets));
        MarkReadyCommand = new RelayCommand<KitchenTicketVm>(ticket => Move(ticket, CookingTickets, ReadyTickets));
        MarkOutOfStockCommand = new RelayCommand<KitchenTicketVm>(_ => { });
        NewTickets.Add(new KitchenTicketVm { TableName = "Bàn 201", ElapsedText = "02:10", HasAllergy = true, Items = [new(1, "Bò bít tết", 1, "Chín vừa", ["Đậu phộng"])] });
    }
    private static void Move(KitchenTicketVm? ticket, ObservableCollection<KitchenTicketVm> from, ObservableCollection<KitchenTicketVm> to) { if (ticket is null) return; from.Remove(ticket); to.Add(ticket); }
}
