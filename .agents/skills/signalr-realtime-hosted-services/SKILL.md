---
name: signalr-realtime-hosted-services
description: Thiết kế SignalR hub (staff có JWT, public cho khách), nhóm kết nối, phát sự kiện sau commit, resync khi reconnect, và các BackgroundService định kỳ (hết hạn cọc, hold timeout 10 phút, leo thang, quá giờ, no-show, hàng chờ) chạy bền vững, test được bằng TimeProvider. Dùng khi thêm/sửa hub, event realtime, job nền, bộ đếm hết hạn, đồng bộ trạng thái sơ đồ bàn, hoặc xử lý mất kết nối.
---

# SignalR & dịch vụ nền

Danh sách event + payload: `docs/02 §5`. Tên event là hằng trong `Contracts/HubEvents` — không dùng chuỗi rời.

## Hub
- `StaffHub` tại `/hubs/staff`: `[Authorize]`; khi kết nối `OnConnectedAsync` thêm vào nhóm `role:{ROLE}` và `all-staff`. Token cho WebSocket đi qua query `access_token` → xử lý ở `JwtBearerEvents.OnMessageReceived` **chỉ cho đường dẫn `/hubs`**.
- `PublicHub` tại `/hubs/public`: không JWT; client tự `JoinReservation(code)`, `JoinMenu()`, `JoinTable(code, sig)` — **xác minh** (mã tồn tại / chữ ký đúng) trước khi `AddToGroupAsync`; payload public **không chứa dữ liệu cá nhân** của người khác.
- Gửi qua `IRealtimePublisher` (bọc `IHubContext<…>`), không gọi `Clients` rải rác. Định tuyến: sự kiện vận hành → `all-staff` (hoặc theo vai nếu chỉ vài vai cần: `OrderItemsAdded` → bếp + thu ngân; `SwitchOffer*` → Manager + Cashier).

## Phát sau commit (outbox-lite)
Không phát event trước khi dữ liệu commit (client sẽ gọi lại API và thấy dữ liệu cũ).
- Cách đơn giản: thu thập event vào `IEventCollector` (scoped) trong use case; filter/hàm `SaveChanges` hậu xử lý phát **sau** `CommitAsync`.
- Cách bền hơn: ghi bảng `outbox_events` trong transaction, worker đọc & phát, đánh dấu đã phát (cho phép phát lại khi crash). Chọn một cách và dùng thống nhất.
- Event là **gợi ý đồng bộ**: payload nhỏ (id + trạng thái); client luôn có thể `GET` lại để lấy chân lý. Vì vậy mất event không gây sai dữ liệu.

## Resync
Khách hàng (WPF/Web) `onreconnected` ⇒ gọi lại `GET /floor` và các danh sách đang mở. `FloorPlanDto.serverTime` giúp client tính `offset = serverTime − localNow` để đếm ngược đúng.

## BackgroundService bền vững
```csharp
protected override async Task ExecuteAsync(CancellationToken ct)
{
    using var timer = new PeriodicTimer(_period, _timeProvider);   // test được bằng FakeTimeProvider
    while (await timer.WaitForNextTickAsync(ct))
    {
        try { await using var scope = _scopes.CreateAsyncScope(); await scope.ServiceProvider.GetRequiredService<IJob>().RunAsync(ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
        catch (Exception ex) { _log.LogError(ex, "Job {Job} lỗi", GetType().Name); }   // KHÔNG để service chết
    }
}
```
- Mỗi service một việc: `DepositExpiryService`, `HoldTimeoutService` (+ leo thang + auto-release), `OverstayMonitorService`, `NoShowSweeper`, `WaitlistMatcherService`.
- **Trạng thái nằm trong DB** (`expires_at`…), service chỉ quét "những gì đã đến hạn" ⇒ restart không mất việc; xử lý từng bản ghi trong transaction riêng và **kiểm lại điều kiện dưới khóa** (có thể nhân viên vừa xử lý xong).
- Chạy đa instance: dùng `SELECT … FOR UPDATE SKIP LOCKED` hoặc `pg_advisory_xact_lock` để không xử lý trùng (đồ án thường 1 instance nhưng viết sao cho an toàn).
- Health check riêng cho mỗi service (thời điểm chạy gần nhất).

## Test
Hub: dùng `HubConnection` thật qua `WebApplicationFactory` (TestServer WebSocket): đúng vai/nhóm nhận đúng event; client public không nhận event nhóm khác. Job: `FakeTimeProvider.Advance(...)` rồi khẳng định chuyển trạng thái; kịch bản restart (dựng lại service giữa chừng); job ném exception không làm vòng lặp dừng.
