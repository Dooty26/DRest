# C7 — Trợ lý AI điều phối: Gemini + Zalo + giữ chỗ 10 phút + leo thang (Pha 3)

**Skills**: `ai-dispatcher-zalo-gemini`, `signalr-realtime-hosted-services`, `postgres-concurrency-booking`, `restaurant-domain-rules`, `verify-and-handoff`. **Nguồn**: `docs/03 §12`, đặc tả mục 4.

## Việc cần làm
1. **Pipeline** (`AiDispatcherService`) lắng nghe `TableVacatedEarly` (từ C6) và `POST /dev/trigger-early-vacate`: Lọc ứng viên → Xếp hạng → Giữ bàn (HELD) → Sinh tin → Gửi → ghi `switch_request_events` ở **từng bước**. Không còn ứng viên → trả bàn về trạng thái đúng, ghi log.
2. **Adapter** (mỗi cái có Mock):
   - `IAiTextService`: `GeminiTextService` (REST `generateContent`, key từ secret, **tên model từ cấu hình**, timeout `Ai.TimeoutSeconds`, JSON schema cho đầu ra phân loại) + `TemplateTextService` làm dự phòng; kiểm tra tin sinh ra **bắt buộc** có tên bàn, hạn chót `HH:mm`, hướng dẫn "Đồng ý"/"Không"; không đạt → dùng template.
   - `IMessageChannel`: `ZaloMessageChannel` (OA/ZNS — **đọc tài liệu hiện hành tại developers.zalo.me**, hỗ trợ template tham số hóa, ghi `notification_logs`), `MockMessageChannel` (ghi vào bộ nhớ/bảng, phục vụ `GET /dev/zalo-outbox`).
   - `IIntentClassifier`: từ khóa nhanh → Gemini JSON `{intent, confidence}` → `UNCLEAR` khi `< 0.7`. **Coi nội dung khách là không tin cậy** (chống prompt injection: đầu ra chỉ nhận enum).
3. **Nhận trả lời**: `POST /webhooks/zalo` (xác thực chữ ký OA) và `POST /dev/simulate-zalo-reply {phone,text}` cùng đi vào một `HandleCustomerReplyAsync`. Ánh xạ SĐT/`zalo_user_id` → yêu cầu đang mở.
4. **Hoán đổi bàn nguyên tử** đúng `docs/03 §12` (khóa request + bàn T + bàn cũ; kiểm lại điều kiện; cập nhật `R.table_id`; nhả bàn cũ; tin xác nhận). Xử lý race "đồng ý" vs "hết hạn".
5. **`HoldTimeoutService`** (poll 5s, dựa `expires_at` trong DB nên **sống sót khi restart**): `PENDING_CUSTOMER` quá hạn → `ESCALATED` (+ `escalation_expires_at`), phát `SwitchOfferEscalated`; `ESCALATED` quá hạn → `AUTO_RELEASED`. Mỗi lần nhả bàn → thử ứng viên kế (tối đa `Switch.MaxAttemptsPerVacancy`).
6. **Nhân viên xử lý**: `GET /switch-requests`, `GET /switch-requests/{id}` (có `timeline`), `POST /switch-requests/{id}/staff-resolve` với 3 lựa chọn; phát `SwitchOfferResolved`.
7. **Wishlist**: đảm bảo bản ghi tạo từ C3 được dùng; thêm kích hoạt phụ khi bàn mong muốn được nhả vì **hủy/no-show** (cùng pipeline).
8. Cập nhật `Client.Core`: `EscalationQueueViewModel`, `AiActivityViewModel`, `TableTileViewModel.Hold*` nhận `SwitchOffer*`.

## Test bắt buộc (dùng `FakeTimeProvider`, `MockMessageChannel`, `FakeAiTextService`)
- **Kịch bản đặc tả end-to-end**: 18:15 bàn 201 trống → ứng viên = Anh Long (Bàn 104, 19:00) → HELD + tin gửi có "18:25" → (a) trả "Đồng ý" → gán 201, nhả 104; (b) không trả lời 10' → ESCALATED → staff `CUSTOMER_AGREED` → gán 201; (c) `NO_ANSWER` → giữ 104, 201 trống lại; (d) quá cửa sổ leo thang → `AUTO_RELEASED`.
- Xếp hạng: VIP thắng; cùng hạng → cọc sớm hơn thắng.
- Race đồng ý/hết hạn song song → đúng một bên thắng, dữ liệu nhất quán.
- Gemini lỗi/timeout → dùng template; phản hồi mơ hồ → `UNCLEAR` và không đổi bàn.
- Sống sót restart: tạo request, "khởi động lại" service, vẫn leo thang đúng hạn.

## Nghiệm thu
Xanh · HANDOFF: hướng dẫn chạy kịch bản demo bằng `/dev/*` (3 lệnh curl) + cách cấu hình Gemini/Zalo thật + lưu ý duyệt template ZNS.
