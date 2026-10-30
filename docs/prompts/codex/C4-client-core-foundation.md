# C4 — Client.Core nối API thật: HTTP, SignalR, đăng nhập, sơ đồ bàn, đặt bàn (Pha 2)

**Skills**: `wpf-client-core-mvvm`, `signalr-realtime-hosted-services`, `verify-and-handoff`.
**Điều kiện**: C2 + C3 đã merge; Antigravity đã có A3 (WPF login + sơ đồ bàn dùng `--fake`).

## Việc cần làm
1. **`HttpRestaurantApi`** (typed `HttpClient` + `DelegatingHandler` gắn JWT, ánh xạ `ProblemDetails` → `ApiException(code, message, errors, warnings)`; 401 → phát `SessionExpired`; retry có giới hạn cho GET; timeout; `Idempotency-Key` cho POST quan trọng).
2. **`SignalRRealtimeClient`** (`HubConnection` tới `/hubs/staff`, `WithAutomaticReconnect`, token provider; ánh xạ event → sự kiện .NET kiểu mạnh theo `HubEvents`; khi `Reconnected` gọi `ResyncAsync` để nạp lại `GET /floor` và danh sách đang mở; tính **độ lệch đồng hồ** từ `serverTime`).
3. **`SessionStore` + `LoginViewModel` + `ShellViewModel` thật**: lọc `NavItems` theo vai (`docs/03 §13`), `ConnectionState`, hàng đợi toast, đếm leo thang.
4. **`FloorPlanViewModel` thật**: nạp `/floor`, vá từng ô theo `TableStateChanged`, bộ đếm `HoldRemainingText` mỗi giây (bù độ lệch đồng hồ, không làm chặn UI), các lệnh mở bàn/dọn xong/đổi bàn/ghép.
5. **`ReservationsViewModel` thật** (check-in, hủy + xem trước hoàn cọc, no-show, lệch số khách).
6. `appsettings.json`: `Api:BaseUrl`, `Realtime:HubUrl`; `--fake` vẫn chạy được (chuyển đổi bằng DI).
7. Giữ nguyên tên/chữ ký trong `docs/05`; nếu cần thêm → thêm mới + ghi "Contract changes".

## Test
`ClientCore.Tests` với `HttpMessageHandler` giả + `FakeRealtimeClient`: đăng nhập, ánh xạ lỗi, vá ô bàn, bộ đếm hold (đồng hồ giả), resync sau reconnect, lọc menu theo vai.
Test tích hợp tùy chọn: chạy Api thật qua `WebApplicationFactory` và gọi bằng `HttpRestaurantApi`.

## Nghiệm thu
`dotnet test` xanh · chạy Api + `dotnet run --project src/Restaurant.Wpf` (Windows) đăng nhập `cashier` thấy sơ đồ bàn thật · HANDOFF: hướng dẫn Antigravity chuyển từ `--fake` sang API thật.
