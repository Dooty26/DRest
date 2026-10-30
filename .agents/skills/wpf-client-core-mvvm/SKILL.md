---
name: wpf-client-core-mvvm
description: Viết phần logic của ứng dụng WPF trong Restaurant.Client.Core (net10.0, không phụ thuộc WPF) — ViewModel với CommunityToolkit.Mvvm, HttpRestaurantApi + JWT, SignalR client có reconnect/resync, FakeRestaurantApi cho chế độ --fake, DI, điều hướng theo vai, bộ đếm hold, điều phối in ấn qua IPrintService, test trên Linux. Dùng khi tạo/sửa ViewModel, service client, API client, realtime client, hay khi cần giữ đúng hợp đồng docs/05 để Antigravity bind XAML.
---

# Client.Core — logic WPF tách khỏi giao diện

Hợp đồng ViewModel: `docs/05`. **Không đổi tên/xóa** thuộc tính/lệnh đã công bố; chỉ thêm và ghi "Contract changes".

## Nguyên tắc
- Dự án `net10.0`, **cấm** tham chiếu `PresentationFramework`/`System.Windows`. Mọi thứ chạm UI đi qua interface: `IUiDispatcher`, `IDialogService`, `IPrintService`, `INotificationSound`, `IClipboardService`, `IFilePickerService` (Antigravity cài ở `Restaurant.Wpf`). Nhờ vậy test được trên Linux.
- VM dùng `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` (async: `AsyncRelayCommand`, có `IsRunning`, không cho chạy chồng).
- Mọi VM chạy được với `FakeRestaurantApi` **và** `HttpRestaurantApi` qua cùng `IRestaurantApi` (chọn bằng DI: `AddClientCore(services, fake: bool)`).
- Lỗi: bắt `ApiException` trong lệnh → gán `ErrorMessage` (tiếng Việt thân thiện theo `code`), `IsBusy=false`; không ném ra UI. Cảnh báo cần xác nhận (`warnings`, `WALKIN_CONFLICT`) → hỏi qua `IDialogService.ConfirmAsync` rồi gửi lại `force=true`.
- Cập nhật collection từ luồng nền: dùng `IUiDispatcher.InvokeAsync`.

## API client
- Typed `HttpClient` + `DelegatingHandler` gắn `Authorization` từ `ISessionStore`; 401 ⇒ phát `SessionExpired` (Shell quay về Login).
- Ánh xạ `ProblemDetails` → `ApiException(code, title, errors, warnings, status)`.
- Retry có giới hạn cho GET (backoff ngắn); POST chỉ retry khi có `Idempotency-Key`.
- Cấu hình `Api:BaseUrl` từ `appsettings.json`/biến môi trường; timeout rõ ràng.

## Realtime client
- `HubConnectionBuilder().WithUrl(hubUrl, o => o.AccessTokenProvider = …).WithAutomaticReconnect()`.
- Ánh xạ event → sự kiện .NET kiểu mạnh theo `HubEvents`; VM đăng ký/hủy đăng ký đúng vòng đời (tránh rò rỉ).
- `Reconnected` ⇒ `ResyncAsync()`: nạp lại `/floor` và danh sách đang mở. Hiển thị `ConnectionState` (Online/Reconnecting/Offline) lên Shell.
- **Độ lệch đồng hồ**: `offset = serverTime − clock.UtcNow` mỗi lần nhận `FloorPlanDto`; mọi bộ đếm dùng `clock.UtcNow + offset`.

## Bộ đếm
Một `ITicker` duy nhất (`PeriodicTimer` 1s, dùng `TimeProvider`) phát tick cho các tile có `HoldRemaining`/thời gian ngồi; không tạo timer riêng trong từng VM. Hết giờ ⇒ chỉ cập nhật hiển thị, **không** tự đổi trạng thái (server mới là nguồn chân lý, sẽ gửi event).

## Điều hướng & phân quyền
`ShellViewModel.NavItems` lọc theo `Role` đúng ma trận `docs/03 §13`; `Can…` trong từng VM dựa vai + trạng thái. Server vẫn kiểm quyền; UI chỉ là tiện lợi.

## In ấn
`PrintCoordinator` nghe `OrderItemsAdded`: nếu `Settings.IsPrintHost` ⇒ `IPrintService.PrintKitchenTicketAsync(model)`; tránh in trùng bằng tập `detailId` đã in (trong phiên) + gọi API đánh dấu đã in nếu có. `PosViewModel`/`CheckoutViewModel` gọi `PrintReceiptAsync(ReceiptModel)` sau thanh toán. Việc dựng giao diện in thuộc Antigravity.

## FakeRestaurantApi
Dữ liệu mẫu phong phú và **xác định** (không ngẫu nhiên): đủ 5 trạng thái bàn, hold còn 10 phút, một yêu cầu `ESCALATED`, đơn QR giả về sau N giây, vài món hết, kho có nguyên liệu thấp, báo cáo mẫu. Hành vi lệnh phải "có thật" (thêm món → tổng đổi; thanh toán cọc 200k/bill 130k → net −70k) để giao diện thử được trọn luồng.

## Test (`ClientCore.Tests`)
`HttpMessageHandler` giả cho API client (đăng nhập, ánh xạ lỗi, 401); `FakeRealtimeClient` + `FakeTimeProvider` cho bộ đếm và resync; mỗi VM ít nhất một test cho luồng chính và một cho lỗi; lọc `NavItems` theo từng vai.
