# 05 — Hợp đồng ViewModel WPF (Codex viết VM, Antigravity viết XAML bind vào)

Vị trí: `src/Restaurant.Client.Core/ViewModels/`. Dùng CommunityToolkit.Mvvm (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). Tên dưới đây là **bản hợp đồng**: Codex không đổi tên/xóa nếu chưa cập nhật file này; có thể **thêm** thành viên.
Mọi VM chạy được với `FakeRestaurantApi` (`--fake`) và API thật qua cùng interface `IRestaurantApi`.
Lệnh async trả kết quả lỗi qua `ErrorMessage` (string?) + `IsBusy` (bool); không ném exception ra UI.

| ViewModel | Thuộc tính (bind) | Lệnh |
|---|---|---|
| `LoginViewModel` | `Username`, `Password`, `IsBusy`, `ErrorMessage` | `LoginCommand` |
| `ShellViewModel` | `CurrentUser`, `NavItems` (đã lọc theo vai: `Key,Title,IconKey`), `CurrentPage`, `ConnectionState` (Online/Reconnecting/Offline), `ClockText`, `Toasts`, `PendingEscalationCount` | `NavigateCommand(key)`, `LogoutCommand`, `DismissToastCommand` |
| `FloorPlanViewModel` | `Areas` (mỗi khu có `FloorPlanImageUrl?`), `SelectedArea`, `Tiles` (`TableTileViewModel[]`), `SelectedTile`, `LegendCounts`, `StatusFilter`, `IsEditMode` | `SelectAreaCommand`, `OpenTableCommand`, `MarkCleanedCommand`, `RequestCleaningCommand`, `MoveTableCommand(target)`, `MergeTablesCommand`, `RefreshCommand`, `SaveLayoutCommand` |
| `TableTileViewModel` | `Id,Name,Seats,X,Y,Width,Height,Shape,DisplayStatus,StatusLabel,StatusIconKey,Subtitle` (vd "19:00 · Anh Long" / "47' · 350.000đ"), `HasHold`, `HoldPhase`, `HoldRemaining` (TimeSpan?), `HoldRemainingText` (mm:ss, cập nhật mỗi giây), `HasOverstay` | — |
| `TableDetailViewModel` | `Table`, `Reservation`, `ActiveOrder`, `CanOpen/CanMove/CanMerge/CanCheckout` (theo vai & trạng thái) | `OpenCommand`, `CheckInCommand`, `UpdateGuestCountCommand`, `GoToPosCommand` |
| `ReservationsViewModel` | `Date`, `StatusFilter`, `SearchText`, `Items`, `Selected` | `CheckInCommand`, `CancelCommand`, `MarkNoShowCommand`, `UpdateGuestCountCommand`, `ApplyApologyCommand`, `RecordRefundCommand` |
| `PosViewModel` | `Categories`, `SelectedCategory`, `SearchText`, `MenuItems`, `Lines` (`OrderLineVm`: `Name,Qty,UnitPrice,Amount,Note,AllergyTags,Status`), `Subtotal`, `DiscountText`, `VatAmount`, `DepositApplied`, `NetAmount`, `IsRefundDue`, `RefundDueAmount`, `HasAllergy` | `AddItemCommand`, `IncQtyCommand`, `DecQtyCommand`, `RemoveLineCommand`, `SetNoteCommand`, `SendToKitchenCommand`, `PrintTempBillCommand`, `OpenCheckoutCommand` |
| `CheckoutViewModel` | `Method`, `VoucherCode`, `DiscountPercent`, `DiscountReason`, `AmountTendered`, `Change`, `Preview` (`InvoiceDto`), `RefundConfirmed`, `IsRefundDue` | `ApplyVoucherCommand`, `ConfirmPayCommand`, `PrintReceiptCommand` |
| `KitchenViewModel` | `NewTickets`, `CookingTickets`, `ReadyTickets` (mỗi ticket: `TableName, ElapsedText, Items, HasAllergy`), `Sound` | `StartCookingCommand`, `MarkReadyCommand`, `MarkOutOfStockCommand` |
| `EscalationQueueViewModel` | `Items` (`EscalationItemViewModel`: `RequestId, CustomerName, Phone, TableName, ReservationTime, ElapsedText, EscalationRemainingText`), `Current` | trong item: `CustomerAgreedCommand`, `CustomerKeepOldCommand`, `NoAnswerCommand`, `CopyPhoneCommand` |
| `AiActivityViewModel` | `Requests`, `Selected`, `Timeline` (bước, giờ, nội dung) | `RefreshCommand` |
| `WaitlistViewModel` | `Entries`, `NewName/NewPhone/NewPartySize`, `SuggestedTables` | `AddCommand`, `NotifyCommand`, `SeatCommand(table)`, `CancelCommand` |
| `InventoryViewModel` | `Ingredients` (`Name, StockDisplay, MinStock, IsLow`), `Movements`, `Selected` | `AdjustCommand`, `SaveIngredientCommand`, `RefreshCommand` |
| `RecipeEditorViewModel` | `MenuItem`, `Rows` (`Ingredient, Qty, Unit`), `AvailableIngredients` | `AddRowCommand`, `RemoveRowCommand`, `SaveCommand` |
| `MenuAdminViewModel` | danh sách + form sửa; `ImageUrl`, `IsUploadingImage`, `ImageError` | `SaveCommand`, `DeleteCommand`, `PickAndUploadImageCommand`, `RemoveImageCommand` |
| `UsersAdminViewModel` | danh sách + form sửa | `Save/Delete/ResetPassword…` |
| `SettingsViewModel` | các cấu hình `docs/03 §0`, `LogoUrl`, `IsUploadingLogo` | `SaveCommand`, `PickAndUploadLogoCommand` |
| `ReportsViewModel` | `From`, `To`, `GroupBy`, `RevenueSeries`, `DepositSummary`, `TopItems`, `LowStock`, `CashFund` | `LoadCommand`, `ExportCommand` |
| `PrintSettingsViewModel` | `IsPrintHost`, `KitchenPrinterName`, `ReceiptPrinterName`, `Printers` | `SaveCommand`, `TestPrintCommand` |

## Dịch vụ giao diện (Codex định nghĩa interface trong Client.Core, Antigravity cài trong Wpf)
| Interface | Antigravity cài đặt |
|---|---|
| `IUiDispatcher` | gọi `Application.Dispatcher` |
| `IDialogService` (`ConfirmAsync`, `ShowTableDialogAsync`, `ShowCheckoutAsync` …) | mở cửa sổ/dialog tương ứng |
| `IPrintService` (`PrintReceiptAsync(ReceiptModel)`, `PrintKitchenTicketAsync(KitchenTicketModel)`) | render `FlowDocument` 80mm → máy in |
| `INotificationSound` | phát âm thanh đơn mới / leo thang |
| `IClipboardService` | sao chép SĐT |
| `IFilePickerService` (`PickImageAsync() → FileResult?` gồm tên + luồng/đường dẫn) | `OpenFileDialog` lọc jpg/png/webp, kiểm tra sơ bộ kích thước (server vẫn kiểm lại) |
| `IImageCache` (tùy chọn) | tải & cache ảnh URL về đĩa cục bộ (dùng cho logo in hóa đơn) |

Bắt buộc có **design-time data**: mỗi View đặt `d:DataContext="{d:DesignInstance ...}"` hoặc dùng `FakeRestaurantApi`.
