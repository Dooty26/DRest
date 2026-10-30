# C3 — Engine đặt bàn: khoảng đệm, chống trùng, cọc giữ chỗ, sơ đồ bàn (Pha 1) — QUAN TRỌNG NHẤT

**Skills**: `postgres-concurrency-booking`, `restaurant-domain-rules`, `dotnet-api-conventions`, `verify-and-handoff`.
**Nguồn nghiệp vụ**: `docs/03 §1–§7`.

## Việc cần làm
1. **Domain thuần (`Core`)**: `BufferRule` (biên đóng, `|N−S| ≤ 105`), `AvailabilityCalculator`, `TableSelector` (best-fit), `DisplayStatusResolver`, `RefundPolicy`, `ReservationCodeGenerator`. Hàm thuần, không phụ thuộc DB, test được bằng `FakeTimeProvider`.
2. **`GET /public/availability`**: lưới slot × viewType, `reason` đúng (`PAST`/`TOO_SOON`/`BOOKED`/`NO_CAPACITY`); đúng ví dụ đặc tả (bàn 201 có lịch 19:00 → slot 17:30…20:30 bị khóa, 17:00 & 21:00 mở).
3. **`POST /public/reservations`** (idempotent theo `Idempotency-Key`): transaction + `FOR UPDATE` theo thứ tự `table_id` (≈ 4 vòng truy vấn — xem skill, DB ở Supabase có độ trễ), chuyển `PENDING_DEPOSIT` quá hạn của bàn ứng viên sang `EXPIRED`, kiểm tra lại xung đột, gán bàn best-fit, ghi `occupancy_minutes` + `occupied_until`; **lỗi `23P01` từ exclusion constraint ⇒ `409 SLOT_UNAVAILABLE`**, tạo `PENDING_DEPOSIT` + `deposit_expires_at`, ghi wishlist khi `wishlist.enabled` và view đặt ≠ target; trả `deposit` (tạm thời `qrPayload` rỗng — C5 hoàn thiện). Xung đột → `409 SLOT_UNAVAILABLE` kèm gợi ý slot gần nhất.
4. **Tra cứu/hủy/dời online** theo `docs/03 §5` (kèm `refundPreview`; yêu cầu khớp `phone`).
5. **`BackgroundService` hết hạn cọc** (`PENDING_DEPOSIT` quá `deposit_expires_at` → `EXPIRED`, phát `ReservationExpired`).
6. **Staff**: `GET /floor` (trả `FloorPlanDto` với `displayStatus` do server tính, `nextReservation`, `activeOrder` tạm), `POST /tables/{id}/open` (walk-in + cảnh báo `409 WALKIN_CONFLICT`, `force`), `check-in`, `mark-cleaned`, `request-cleaning`, `move`, `no-show`, `cancel`, `PUT …/guest-count` (gợi ý bàn lớn hơn/kê ghế), `POST /reservations` đặt qua điện thoại, `GET /reservations`.
7. Phát SignalR `TableStateChanged`/`FloorChanged`/`ReservationChanged` **sau khi commit** (qua `IRealtimePublisher`; hub thật ở C8 — dùng publisher tạm ghi log).
8. Cập nhật `Client.Core` ở C4 (không làm ở đây). Cập nhật HANDOFF cho Antigravity: ví dụ request/response thật đã chạy.

## Test bắt buộc (không được bỏ)
- Bảng giá trị `BufferRule`: S=19:00 → 17:00 ✔, 17:15 ✖, 17:30 ✖, 20:45 ✖, 21:00 ✔; lưới 15' cũng đúng.
- **Đồng thời** (Testcontainers **PostgreSQL**): 20 request song song đặt cùng slot/bàn duy nhất → **đúng 1 thành công**, 19 nhận `SLOT_UNAVAILABLE`, không deadlock treo; lặp 10 lần.
- **Lưới an toàn CSDL**: vô hiệu hóa kiểm tra ở tầng ứng dụng trong một test (bypass) rồi chạy song song ⇒ constraint vẫn chặn (`23P01`) và dữ liệu không bao giờ có hai lượt chồng nhau.
- Dời lịch & hoán đổi bàn bị constraint chặn ⇒ giữ nguyên lịch cũ, không mất cọc.
- Số vòng truy vấn của `POST /public/reservations` và `GET /public/availability` được **đếm trong test** (interceptor/đếm `DbCommand`) để chặn hồi quy N+1.
- Idempotency: gửi lại cùng key → cùng kết quả, không tạo bản ghi thứ hai.
- Hủy ≥2h/<2h, dời lịch, no-show job, hết hạn cọc, walk-in cảnh báo, lệch số khách (3 nhánh).
- `displayStatus` ưu tiên đúng (HELD > OCCUPIED > CLEANING > RESERVED > AVAILABLE).

## Nghiệm thu
`dotnet test` xanh · OpenAPI cập nhật · HANDOFF kèm `curl` mẫu cho: availability, tạo đặt bàn, floor.
