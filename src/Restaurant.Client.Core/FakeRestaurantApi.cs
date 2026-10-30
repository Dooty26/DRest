using Restaurant.Contracts;

namespace Restaurant.Client.Core;

public sealed class FakeRestaurantApi(TimeProvider timeProvider) : IRestaurantApi
{
    private readonly DateTime now = timeProvider.GetUtcNow().UtcDateTime;

    public Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var role = username.ToLowerInvariant() switch
        {
            "manager" => Role.MANAGER,
            "waiter" => Role.WAITER,
            "kitchen" => Role.KITCHEN,
            _ => Role.CASHIER
        };
        return Task.FromResult(new LoginResponse("fake-jwt", now.AddHours(8), new UserDto(1, $"Nhân viên {username}", role)));
    }

    public Task<FloorPlanDto> GetFloorAsync(CancellationToken cancellationToken = default)
    {
        var areas = new[]
        {
            new AreaDto(1, "Tầng 1"), new AreaDto(2, "Tầng 2"), new AreaDto(3, "Tầng 3")
        };
        var statuses = new[] { TableDisplayStatus.AVAILABLE, TableDisplayStatus.RESERVED, TableDisplayStatus.OCCUPIED, TableDisplayStatus.CLEANING, TableDisplayStatus.HELD };
        var tables = Enumerable.Range(1, 20).Select(index =>
        {
            var status = statuses[(index - 1) % statuses.Length];
            var areaId = ((index - 1) % 3) + 1;
            var tableNumber = areaId * 100 + index;
            var next = status == TableDisplayStatus.RESERVED
                ? new NextReservationDto(55, "RES008899", "Anh Long", now.AddMinutes(45), 4)
                : null;
            var order = status == TableDisplayStatus.OCCUPIED
                ? new ActiveOrderDto(901, now.AddMinutes(-47), 3, 350000)
                : null;
            var hold = status == TableDisplayStatus.HELD
                ? new TableHoldDto(12, index == 5 ? "ESCALATED" : "AWAITING_CUSTOMER", now.AddMinutes(10), "Anh Long")
                : null;
            return new TableDto(index, $"T{tableNumber}", $"Bàn {tableNumber}", areaId, areas[areaId - 1].Name, index % 3 == 0 ? 6 : 4,
                areaId == 2 ? ViewType.WINDOW : areaId == 3 ? ViewType.BALCONY : ViewType.STANDARD,
                "RECT", 40 + ((index - 1) % 5) * 120, 50 + ((index - 1) / 5) * 110, 90, 90, status, now.AddMinutes(-index * 3), next, order, hold,
                new TableFlagsDto(false, false, Array.Empty<int>()));
        }).ToArray();
        var counts = Enum.GetValues<TableDisplayStatus>().ToDictionary(status => status.ToString(), status => tables.Count(table => table.DisplayStatus == status));
        return Task.FromResult(new FloorPlanDto(now, areas, tables, counts));
    }

    public Task<IReadOnlyList<ReservationSummaryDto>> GetReservationsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ReservationSummaryDto>>([
            new(55, "RES008899", "Anh Long", "0987654321", now.AddMinutes(45), 4, ReservationStatus.CONFIRMED, DepositStatus.PAID, "Bàn 104")
        ]);

    public Task<PublicMenuDto> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        var names = new[] { "Gỏi cuốn", "Súp bí đỏ", "Bò bít tết", "Cá hồi áp chảo", "Mì Ý", "Cơm chiên", "Gà nướng", "Sườn nướng", "Salad", "Khoai tây chiên", "Bánh mì bơ tỏi", "Pizza", "Trà đào", "Cà phê", "Nước cam", "Soda", "Tiramisu", "Kem vani", "Chè khúc bạch", "Trái cây" };
        var items = names.Select((name, index) => new MenuItemDto(index + 1, name, 45000 + index * 10000, null, $"Mô tả {name}", index != 7)).ToArray();
        return Task.FromResult(new PublicMenuDto([
            new MenuCategoryDto(1, "Món chính", items.Take(12).ToArray()),
            new MenuCategoryDto(2, "Đồ uống", items.Skip(12).Take(4).ToArray()),
            new MenuCategoryDto(3, "Tráng miệng", items.Skip(16).ToArray())
        ]));
    }

    public async Task<IReadOnlyList<SwitchRequestDto>> GetSwitchRequestsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return [CreateSwitchRequest(now)];
    }

    public Task<IReadOnlyList<WaitlistEntryDto>> GetWaitlistAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WaitlistEntryDto>>([new(1, "Chị Mai", "0912345678", 4, WaitlistStatus.WAITING, now.AddMinutes(-20))]);

    public Task<IReadOnlyList<IngredientDto>> GetIngredientsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<IngredientDto>>([
            new(1, "Thịt bò", 5200, "g", 2000, "5,2 kg", false),
            new(2, "Khoai tây", 800, "g", 1000, "0,8 kg", true),
            new(3, "Sữa tươi", 6000, "ml", 1500, "6 l", false)
        ]);

    public Task<ReportsDto> GetReportsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new ReportsDto(
        [new("Hôm nay", 12500000), new("Hôm qua", 9800000)],
        new DepositSummaryDto(4000000, 400000, 200000, 2600000),
        [new MenuItemDto(3, "Bò bít tết", 189000, null, string.Empty, true)],
        [new IngredientDto(2, "Khoai tây", 800, "g", 1000, "0,8 kg", true)],
        new CashFundDto(5000000, 6000000, 1500000, 200000, 12300000)));

    public Task<InvoiceDto> GetInvoicePreviewAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new InvoiceDto(77, "HD20261031-0007", 130000, 0, 0, 200000, -70000, 70000, "PREVIEW", null));

    internal static SwitchRequestDto CreateSwitchRequest(DateTime now) => new(
        12, SwitchStatus.ESCALATED,
        new SwitchReservationDto(55, "RES008899", "Anh Long", "0987654321", now.AddMinutes(45), 4),
        new SwitchTableDto(104, "Bàn 104"), new SwitchTableDto(201, "Bàn 201"),
        now.AddMinutes(-10), now, now, now.AddMinutes(10),
        "Bàn 201 vừa trống. Anh Long vui lòng trả lời Đồng ý hoặc Không trước 19:00.",
        [new(now.AddMinutes(-10), "HOLD_STARTED", "Đã giữ Bàn 201"), new(now, "ESCALATED", "Chuyển nhân viên gọi điện")] );
}

public sealed class FakeRealtimeClient(TimeProvider timeProvider) : IRealtimeClient
{
    private CancellationTokenSource? lifetime;
    public event EventHandler<TableDto>? TableStateChanged;
    public event EventHandler<KitchenTicketModel>? OrderItemsAdded;
    public event EventHandler<SwitchRequestDto>? SwitchOfferEscalated;
    public event EventHandler<ConnectionState>? ConnectionStateChanged;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ConnectionStateChanged?.Invoke(this, ConnectionState.Online);
        _ = RunScenarioAsync(lifetime.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        lifetime?.Cancel();
        ConnectionStateChanged?.Invoke(this, ConnectionState.Offline);
        return Task.CompletedTask;
    }

    private async Task RunScenarioAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2), timeProvider, cancellationToken);
            var now = timeProvider.GetUtcNow().UtcDateTime;
            OrderItemsAdded?.Invoke(this, new KitchenTicketModel("K-0123", "Bàn 201", "Tầng 2", now, "QR", true,
                [new KitchenTicketItemDto(1, "Bò bít tết", 1, "Chín vừa", ["Đậu phộng"])]));
            await Task.Delay(TimeSpan.FromSeconds(2), timeProvider, cancellationToken);
            SwitchOfferEscalated?.Invoke(this, FakeRestaurantApi.CreateSwitchRequest(now));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    public void EmitTable(TableDto table) => TableStateChanged?.Invoke(this, table);
}
