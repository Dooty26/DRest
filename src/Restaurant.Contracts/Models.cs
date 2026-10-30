namespace Restaurant.Contracts;

public sealed record ApiProblemDto(
    string Type,
    string Title,
    int Status,
    string? Detail,
    string Code,
    IReadOnlyDictionary<string, string[]>? Errors = null,
    IReadOnlyList<string>? Warnings = null);

public sealed record UserDto(int Id, string FullName, Role Role);
public sealed record LoginRequest(string Username, string Password);
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt, UserDto User);
public sealed record NavItemDto(string Key, string Title, string IconKey);
public sealed record ToastDto(Guid Id, string Message, string Kind = "info");

public sealed record ViewTypeOptionDto(ViewType ViewType, string Label, string Description);
public sealed record PublicConfigDto(
    string RestaurantName,
    string? LogoUrl,
    decimal DepositAmount,
    int StayMinutes,
    int CleaningMinutes,
    int SlotStepMinutes,
    int DepositHoldMinutes,
    int FullRefundHoursBefore,
    IReadOnlyList<string> AllergyOptions,
    IReadOnlyList<ViewTypeOptionDto> ViewTypes);

public sealed record AvailabilityViewDto(ViewType ViewType, bool IsAvailable, int AvailableTables, string? Reason);
public sealed record AvailabilitySlotDto(string Time, IReadOnlyList<AvailabilityViewDto> ViewTypes);
public sealed record AvailabilityDto(DateOnly Date, IReadOnlyList<AvailabilitySlotDto> Slots);
public sealed record WishlistRequest(bool Enabled, ViewType TargetViewType);
public sealed record CreateReservationRequest(
    string CustomerName,
    string Phone,
    string? Email,
    DateOnly Date,
    string Time,
    int PartySize,
    ViewType ViewType,
    string? Note,
    IReadOnlyList<string> Allergies,
    string? Occasion,
    bool BabyChair,
    WishlistRequest? Wishlist);
public sealed record ReservationTableDto(string Name, ViewType ViewType, string AreaName);
public sealed record BankInfoDto(string BankName, string AccountNo, string AccountName);
public sealed record DepositDto(decimal Amount, DateTime ExpiresAt, string TransferContent, string QrPayload, string? QrImageUrl, BankInfoDto Bank);
public sealed record CreateReservationResponse(string Code, ReservationStatus Status, ReservationTableDto Table, DateTime StartsAt, DepositDto Deposit);
public sealed record RefundPreviewDto(bool Eligible, IReadOnlyList<RefundMode> Mode, decimal Amount);
public sealed record ReservationLookupDto(string Code, ReservationStatus Status, DepositStatus DepositStatus, DateTime StartsAt, int PartySize, ReservationTableDto Table, bool CanCancel, RefundPreviewDto RefundPreview, WishlistRequest? Wishlist, SwitchRequestDto? SwitchOffer);
public sealed record CancelReservationRequest(string Phone, RefundMode RefundMode);
public sealed record CancelReservationResponse(ReservationStatus Status, DepositStatus DepositStatus, string? VoucherCode);
public sealed record RescheduleReservationRequest(string Phone, DateOnly Date, string Time);

public sealed record MenuItemDto(int Id, string Name, decimal Price, string? ImageUrl, string Description, bool IsAvailable);
public sealed record MenuCategoryDto(int Id, string Name, IReadOnlyList<MenuItemDto> Items);
public sealed record PublicMenuDto(IReadOnlyList<MenuCategoryDto> Categories);
public sealed record TableSessionDto(string TableName, string AreaName, bool IsOpen, int? OrderId);
public sealed record PublicOrderItemRequest(int ItemId, int Qty, string? Note, IReadOnlyList<string> AllergyTags);
public sealed record CreatePublicOrderRequest(IReadOnlyList<PublicOrderItemRequest> Items, string? Note);
public sealed record CreatedOrderItemDto(int DetailId, OrderItemStatus Status);
public sealed record CreatePublicOrderResponse(int OrderId, IReadOnlyList<CreatedOrderItemDto> Items);
public sealed record PublicOrderStatusItemDto(int DetailId, string Name, int Qty, OrderItemStatus Status);
public sealed record PublicOrderStatusDto(int OrderId, IReadOnlyList<PublicOrderStatusItemDto> Items);

public sealed record AreaDto(int Id, string Name, string? FloorPlanImageUrl = null);
public sealed record NextReservationDto(int ReservationId, string Code, string GuestName, DateTime StartsAt, int PartySize);
public sealed record ActiveOrderDto(int OrderId, DateTime OpenedAt, int PartySize, decimal RunningTotal);
public sealed record TableHoldDto(int RequestId, string Phase, DateTime ExpiresAt, string CustomerName);
public sealed record TableFlagsDto(bool Overstay, bool NeedsExtraChairs, IReadOnlyList<int> MergedWith);
public sealed record TableDto(
    int Id,
    string Code,
    string Name,
    int AreaId,
    string AreaName,
    int Seats,
    ViewType ViewType,
    string Shape,
    double X,
    double Y,
    double Width,
    double Height,
    TableDisplayStatus DisplayStatus,
    DateTime StatusSince,
    NextReservationDto? NextReservation,
    ActiveOrderDto? ActiveOrder,
    TableHoldDto? Hold,
    TableFlagsDto Flags);
public sealed record FloorPlanDto(DateTime ServerTime, IReadOnlyList<AreaDto> Areas, IReadOnlyList<TableDto> Tables, IReadOnlyDictionary<string, int> Counts);

public sealed record ReservationSummaryDto(int Id, string Code, string GuestName, string Phone, DateTime StartsAt, int PartySize, ReservationStatus Status, DepositStatus DepositStatus, string TableName);
public sealed record ReservationDetailDto(ReservationSummaryDto Summary, IReadOnlyList<string> Allergies, string? Note, WishlistRequest? Wishlist);
public sealed record OpenTableRequest(int PartySize, int? ReservationId, bool Force = false);
public sealed record MoveTableRequest(int ToTableId);
public sealed record MergeTablesRequest(int PrimaryTableId, IReadOnlyList<int> MergedTableIds);
public sealed record GuestCountRequest(int PartySize);
public sealed record GuestCountResponse(bool Ok, bool NeedsExtraChairs, IReadOnlyList<TableDto> Suggestions);
public sealed record RefundRecordRequest(decimal Amount, string Note);

public sealed record OrderLineDto(int DetailId, int ItemId, string Name, int Qty, decimal UnitPrice, decimal Amount, string? Note, IReadOnlyList<string> AllergyTags, OrderItemStatus Status);
public sealed record OrderDto(int Id, int TableId, string TableName, OrderStatus Status, IReadOnlyList<OrderLineDto> Lines, decimal RunningTotal);
public sealed record StaffOrderItemsRequest(IReadOnlyList<PublicOrderItemRequest> Items);
public sealed record UpdateOrderItemRequest(int? Qty, OrderItemStatus? Status);
public sealed record UpdateKitchenStatusRequest(OrderItemStatus Status);
public sealed record AvailabilityRequest(bool IsAvailable);

public sealed record InvoiceDto(int Id, string InvoiceNo, decimal Subtotal, decimal Discount, decimal Vat, decimal DepositApplied, decimal NetAmount, decimal RefundDue, string Status, DateTime? PaidAt);
public sealed record PayOrderRequest(PaymentMethod Method, string? VoucherCode, decimal? DiscountPercent, string? DiscountReason, decimal? AmountTendered, bool? RefundConfirmed);
public sealed record ReceiptLineDto(string Name, int Qty, decimal UnitPrice, decimal Amount);
public sealed record ReceiptModel(string RestaurantName, string Address, string InvoiceNo, string TableName, DateTime OpenedAt, DateTime PaidAt, string CashierName, IReadOnlyList<ReceiptLineDto> Lines, decimal Subtotal, string DiscountLabel, decimal Discount, decimal Vat, decimal DepositApplied, decimal NetAmount, decimal RefundDue, PaymentMethod PaymentMethod, decimal AmountTendered, decimal Change, string Footer);
public sealed record KitchenTicketItemDto(int DetailId, string Name, int Qty, string? Note, IReadOnlyList<string> AllergyTags);
public sealed record KitchenTicketModel(string TicketNo, string TableName, string AreaName, DateTime CreatedAt, string Source, bool HasAllergy, IReadOnlyList<KitchenTicketItemDto> Items);

public sealed record SwitchReservationDto(int Id, string Code, string GuestName, string Phone, DateTime StartsAt, int PartySize);
public sealed record SwitchTableDto(int Id, string Name);
public sealed record SwitchTimelineItemDto(DateTime At, string Step, string Text);
public sealed record SwitchRequestDto(int Id, SwitchStatus Status, SwitchReservationDto Reservation, SwitchTableDto FromTable, SwitchTableDto ToTable, DateTime OfferedAt, DateTime ExpiresAt, DateTime? EscalatedAt, DateTime? EscalationExpiresAt, string MessageText, IReadOnlyList<SwitchTimelineItemDto> Timeline);
public sealed record StaffResolveSwitchRequest(SwitchResolution Resolution);

public sealed record WaitlistEntryDto(int Id, string Name, string Phone, int PartySize, WaitlistStatus Status, DateTime CreatedAt);
public sealed record AddWaitlistRequest(string Name, string Phone, int PartySize);
public sealed record SeatWaitlistRequest(int TableId);
public sealed record IngredientDto(int Id, string Name, decimal StockQty, string Unit, decimal MinStock, string StockDisplay, bool IsLow);
public sealed record StockMovementDto(int Id, int IngredientId, decimal Delta, string Type, string Reason, DateTime CreatedAt);
public sealed record AdjustStockRequest(decimal Delta, string Reason);
public sealed record RecipeRowDto(int IngredientId, string IngredientName, decimal Qty, string Unit);
public sealed record RecipeDto(int MenuItemId, IReadOnlyList<RecipeRowDto> Rows);
public sealed record RevenuePointDto(string Label, decimal Amount);
public sealed record DepositSummaryDto(decimal Collected, decimal Refunded, decimal Forfeited, decimal Applied);
public sealed record CashFundDto(decimal Cash, decimal Transfer, decimal Card, decimal Refunds, decimal Net);
public sealed record ReportsDto(IReadOnlyList<RevenuePointDto> RevenueSeries, DepositSummaryDto DepositSummary, IReadOnlyList<MenuItemDto> TopItems, IReadOnlyList<IngredientDto> LowStock, CashFundDto CashFund);

public sealed record UploadImageResponse(string ImageUrl);
public sealed record DevSimulatePaymentRequest(string Code);
public sealed record DevSimulateZaloReplyRequest(string Phone, string Text);
public sealed record DevTriggerEarlyVacateRequest(int TableId);
public sealed record DevAdvanceClockRequest(int Minutes);
public sealed record ZaloOutboxMessageDto(string Phone, string Text, DateTime SentAt);

public sealed record FloorChangedEvent(IReadOnlyList<int> TableIds);
public sealed record ReservationChangedEvent(int ReservationId, ReservationStatus Status);
public sealed record DepositConfirmedEvent(int ReservationId, string Code);
public sealed record ReservationExpiredEvent(string Code);
public sealed record KitchenItemStatusChangedEvent(int DetailId, int OrderId, OrderItemStatus Status);
public sealed record MenuAvailabilityChangedEvent(int ItemId, bool IsAvailable);
public sealed record WaitlistTableReadyEvent(int EntryId, int TableId);
public sealed record OverstayWarningEvent(int TableId, int ReservationId, string Level, int Minutes);
public sealed record LowStockEvent(int IngredientId, string Name, decimal StockQty, decimal MinStock);
public sealed record PaymentReviewNeededEvent(int ReservationId, string Reason);
