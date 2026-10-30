# C8 — Realtime hoàn chỉnh & ca ngoại lệ vận hành (Pha 3)

**Skills**: `signalr-realtime-hosted-services`, `pos-bom-inventory`, `postgres-concurrency-booking`, `restaurant-domain-rules`, `verify-and-handoff`. **Nguồn**: `docs/03 §6–§7`.

## Việc cần làm
1. **Hub thật**: `StaffHub` (JWT, nhóm theo vai), `PublicHub` (`JoinReservation(code)`, `JoinMenu()`, `JoinTable(code, sig)`); `IRealtimePublisher` thật thay publisher tạm; phát **sau commit** (outbox-lite: lưu sự kiện trong transaction rồi phát bởi worker, hoặc `SaveChanges` hook) để không phát event cho dữ liệu chưa commit; đủ mọi event trong `docs/02 §5`.
2. **Hàng chờ**: `waitlist` API + `WaitlistMatcherService` (khi bàn → AVAILABLE và không vướng §2 → `WaitlistTableReady` cho mục đầu hàng vừa chỗ); mời/seat/cancel.
3. **Quá giờ**: `OverstayMonitorService` (mỗi 30s): `PRE` tại `S−30'`, `OVERDUE` tại `S`, `APOLOGY` khi quán kín → tạo voucher nước miễn phí + `apply-apology` (đặt `apology_discount_pct=10`, idempotent) + phát `OverstayWarning`; hóa đơn của khách đặt tự áp 10%.
4. **No-show sweeper** (nếu chưa gộp ở C3), nhả bàn + kích hoạt wishlist/hàng chờ.
5. **Ghép/tách bàn** (`table_merges`), **đổi bàn** khi đang có order (cập nhật order, trạng thái hai bàn, audit).
6. **Đồng bộ đồng hồ**: `FloorPlanDto.serverTime`; mọi bộ đếm dựa `expiresAt` UTC.
7. **Độ bền**: `BackgroundService` bắt exception, không chết; health check cho DB + từng service nền; log có tương quan (`reservationCode`, `requestId`).
8. Cập nhật `Client.Core`: `WaitlistViewModel`, cảnh báo quá giờ, cờ `HasOverstay`, toast đa dạng.

## Test
Hub: client nhận đúng event theo vai/nhóm; không nhận event của nhóm khác; sau reconnect `Resync` đủ dữ liệu. Waitlist: thứ tự FIFO + bỏ qua mục quá lớn. Overstay: 3 mức với đồng hồ giả, idempotent. Ghép/tách/đổi bàn giữ nguyên tổng tiền & trạng thái.

## Nghiệm thu
Xanh · HANDOFF.
