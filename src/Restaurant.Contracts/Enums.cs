namespace Restaurant.Contracts;

public enum Role { MANAGER, CASHIER, WAITER, KITCHEN }
public enum ViewType { STANDARD, WINDOW, BALCONY, VIP, OUTDOOR }
public enum TableDisplayStatus { AVAILABLE, RESERVED, OCCUPIED, CLEANING, HELD }
public enum TablePhysicalStatus { AVAILABLE, OCCUPIED, CLEANING, HELD }
public enum ReservationStatus { PENDING_DEPOSIT, CONFIRMED, CHECKED_IN, COMPLETED, CANCELLED, NO_SHOW, EXPIRED }
public enum DepositStatus { NONE, PENDING, PAID, REFUNDED, FORFEITED, CONVERTED_TO_VOUCHER, APPLIED }
public enum OrderStatus { OPEN, BILL_REQUESTED, PAID, CANCELLED }
public enum OrderItemStatus { NEW, COOKING, READY, SERVED, CANCELLED, OUT_OF_STOCK }
public enum SwitchStatus { PENDING_CUSTOMER, ACCEPTED, DECLINED, ESCALATED, STAFF_ACCEPTED, STAFF_KEPT_OLD, STAFF_NO_ANSWER, AUTO_RELEASED }
public enum SwitchResolution { CUSTOMER_AGREED, CUSTOMER_KEEP_OLD, NO_ANSWER }
public enum WaitlistStatus { WAITING, NOTIFIED, SEATED, CANCELLED }
public enum PaymentMethod { VIETQR, CASH, CARD, TRANSFER }
public enum RefundMode { REFUND, VOUCHER }
public enum ConnectionState { Online, Reconnecting, Offline }

public static class HubEvents
{
    public const string FloorChanged = nameof(FloorChanged);
    public const string TableStateChanged = nameof(TableStateChanged);
    public const string ReservationChanged = nameof(ReservationChanged);
    public const string DepositConfirmed = nameof(DepositConfirmed);
    public const string ReservationExpired = nameof(ReservationExpired);
    public const string OrderItemsAdded = nameof(OrderItemsAdded);
    public const string KitchenItemStatusChanged = nameof(KitchenItemStatusChanged);
    public const string MenuAvailabilityChanged = nameof(MenuAvailabilityChanged);
    public const string SwitchOfferStarted = nameof(SwitchOfferStarted);
    public const string SwitchOfferEscalated = nameof(SwitchOfferEscalated);
    public const string SwitchOfferResolved = nameof(SwitchOfferResolved);
    public const string WaitlistTableReady = nameof(WaitlistTableReady);
    public const string OverstayWarning = nameof(OverstayWarning);
    public const string LowStock = nameof(LowStock);
    public const string PaymentReviewNeeded = nameof(PaymentReviewNeeded);
}

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string ForbiddenRole = "FORBIDDEN_ROLE";
    public const string InvalidState = "INVALID_STATE";
    public const string SlotUnavailable = "SLOT_UNAVAILABLE";
    public const string TableConflict = "TABLE_CONFLICT";
    public const string WalkinConflict = "WALKIN_CONFLICT";
    public const string DepositExpired = "DEPOSIT_EXPIRED";
    public const string OutOfStockItem = "OUT_OF_STOCK_ITEM";
    public const string QrSignatureInvalid = "QR_SIGNATURE_INVALID";
    public const string TableNotOpen = "TABLE_NOT_OPEN";
    public const string WebhookSignatureInvalid = "WEBHOOK_SIGNATURE_INVALID";
    public const string RateLimited = "RATE_LIMITED";
    public const string NotImplemented = "NOT_IMPLEMENTED";
}
