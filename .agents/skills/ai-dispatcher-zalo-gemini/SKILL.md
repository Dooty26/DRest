---
name: ai-dispatcher-zalo-gemini
description: Trợ lý AI điều phối đổi bàn — pipeline sự kiện bàn trống sớm (lọc ứng viên theo wishlist, xếp hạng VIP/cọc sớm, giữ bàn 10 phút, gửi Zalo OA/ZNS, nhận trả lời, leo thang nhân viên gọi điện), tích hợp Gemini để sinh tin nhắn và phân loại ý định, state machine table_switch_requests, hoán đổi bàn nguyên tử, hosted service hết hạn. Dùng khi làm bất cứ phần nào của AI dispatcher, Zalo, Gemini, wishlist, hold timeout, escalation, hoặc mô phỏng Zalo trong dev.
---

# AI điều phối đổi bàn (Gemini + Zalo)

Nghiệp vụ chuẩn: `docs/03 §12` (state machine + thứ tự xếp hạng). Quy tắc cốt lõi: **code quyết định, AI chỉ viết và hiểu chữ.**

## Luồng
```
TableVacatedEarly → FindCandidates → Rank → CreateRequest(PENDING_CUSTOMER) + T=HELD
  → GenerateMessage (Gemini | template) → Send (IMessageChannel) → [chờ]
  ← Reply (webhook/dev) → ClassifyIntent → ACCEPT: Swap | DECLINE: Release→NextCandidate | UNCLEAR: nhắc 1 lần
  ⏱ HoldTimeoutService: PENDING_CUSTOMER quá hạn → ESCALATED (popup WPF) → quá cửa sổ → AUTO_RELEASED
  👤 staff-resolve: CUSTOMER_AGREED→Swap | CUSTOMER_KEEP_OLD / NO_ANSWER → Release→NextCandidate
```
Mỗi bước ghi `switch_request_events` (timeline) **trong cùng transaction** với thay đổi trạng thái.

## Lọc & xếp hạng
Lọc bằng một truy vấn có tham số (điều kiện đầy đủ ở `docs/03 §12 Bước 1`), sau đó **kiểm lại `BufferRule`** bằng hàm thuần cho bàn đích. Xếp hạng bằng `CandidateRanker` thuần: `is_vip DESC, deposit_paid_at ASC, created_at ASC`; trả kèm chuỗi lý do ("VIP; cọc lúc 10:02") để hiển thị. Test: VIP thắng; cùng hạng → cọc sớm thắng; ứng viên không đủ ghế bị loại.

## Cổng (interface) — mỗi cái có Mock/Fake
- `IAiTextService.ComposeOfferAsync(OfferContext) → string`; `GeminiTextService` + `TemplateTextService` (dự phòng).
- `IIntentClassifier.ClassifyAsync(text) → (Intent, Confidence)`.
- `IMessageChannel.SendOfferAsync(...) → SendResult`; `ZaloMessageChannel`, `MockMessageChannel` (ghi `notification_logs` + bộ đệm cho `GET /dev/zalo-outbox`).

### Gemini
- REST `generateContent`; key từ secret (`Ai:GeminiApiKey`), **tên model lấy từ cấu hình** (`Ai:Model`; xem tài liệu Google AI hiện hành để chọn model còn hỗ trợ). Timeout cứng `Ai:TimeoutSeconds` (8s); lỗi/timeout/safety-block ⇒ template. Không gọi Gemini trong transaction DB.
- Sinh tin: prompt hệ thống nêu *giọng điệu lịch thiệp, ngắn (≤ 60 từ), tiếng Việt, xưng hô theo `Anh/Chị` + tên*; đưa dữ kiện dưới dạng JSON (tên khách, tên bàn, mô tả view, giờ đặt, **hạn chót HH:mm giờ VN**); yêu cầu kết thúc bằng hướng dẫn trả lời "Đồng ý"/"Không".
- **Kiểm tra đầu ra bằng code**: phải chứa tên bàn, hạn chót `HH:mm`, và cả hai từ khóa trả lời; chứa URL/số điện thoại lạ ⇒ bỏ, dùng template.
- Phân loại: `responseMimeType=application/json` + schema `{intent: ACCEPT|DECLINE|UNCLEAR, confidence: 0..1}`. Nội dung khách là **dữ liệu không tin cậy**: đặt trong khối dữ liệu, dặn model chỉ phân loại, và **chỉ nhận enum** làm đầu ra — mọi chuỗi khác ⇒ `UNCLEAR`.
- Đường nhanh không cần Gemini: chuẩn hóa (chữ thường, bỏ dấu, bỏ dấu câu) rồi khớp `{dong y, ok, oke, okay, duoc, co, yes, y}` → ACCEPT; `{khong, ko, k, no, thoi, giu ban cu}` → DECLINE.

Template dự phòng (tham số hóa):
> Dạ nhà hàng {Restaurant} xin chào {Honorific} {Name}. Bàn {Table} ({ViewLabel}) vừa trống đúng khung giờ {Time} của {Honorific}. Nhà hàng tạm giữ riêng bàn này trong 10 phút (đến {Deadline}). {Honorific} vui lòng trả lời "Đồng ý" để đổi bàn, hoặc "Không" để giữ bàn cũ nhé!

### Zalo
- **Đọc tài liệu hiện hành tại developers.zalo.me** trước khi viết. Điểm cần nhớ: ZNS thường gửi theo *template đã được duyệt* (tham số hóa) và tính phí; tin tự do qua OA thường chỉ gửi được khi khách đã quan tâm/tương tác OA gần đây; nhận trả lời cần webhook sự kiện của OA (xác thực chữ ký) ⇒ ánh xạ `zalo_user_id` ↔ khách.
- `ZaloMessageChannel` chọn ZNS hay OA theo tình trạng khách, trả `SendResult{Success, ProviderMsgId, Error}`. **Gửi lỗi ⇒ leo thang ngay** (`MESSAGE_FAILED` + `ESCALATED`, `reason=SEND_FAILED`), không để khách "im lặng" 10 phút vô ích.
- Cấu hình `Messaging:Channel = Mock|Zalo`. Token Zalo lưu secret, có cơ chế làm mới.

## Hoán đổi nguyên tử, hold, timeout
- Dùng khuôn khóa trong skill `postgres-concurrency-booking` (request → bàn → reservation); hai chỉ mục duy nhất từng phần (`ux_sw_one_open_per_table`, `ux_sw_one_open_per_res`) đảm bảo mỗi bàn/đặt bàn chỉ có 1 yêu cầu đang mở — vi phạm (`23505`) nghĩa là đang có yêu cầu khác, không phải lỗi hệ thống. Hoàn tất: `R.table_id = T`, T `HELD→AVAILABLE`, bàn cũ trả lại, request `ACCEPTED`/`STAFF_ACCEPTED`, tin xác nhận, event.
- `HoldTimeoutService`: poll theo `PeriodicTimer(period, TimeProvider)`; chọn request quá hạn bằng truy vấn `status + expires_at` trong DB (**không dựa timer trong RAM** ⇒ sống sót restart); mỗi request xử lý trong transaction riêng, kiểm lại trạng thái dưới khóa.
- Nhả bàn ⇒ `NextCandidate` (≤ `Switch.MaxAttemptsPerVacancy`, đánh `attempt_no`), đánh giá lại điều kiện theo thời điểm hiện tại.

## Dev & test
`POST /dev/trigger-early-vacate`, `POST /dev/simulate-zalo-reply`, `GET /dev/zalo-outbox`. Test với `FakeTimeProvider` + Mock: các nhánh (a) đồng ý (b) im lặng→leo thang→staff đồng ý (c) NO_ANSWER (d) AUTO_RELEASED (e) Gemini lỗi → template (f) trả lời mơ hồ → UNCLEAR (g) race đồng ý/hết hạn (h) restart giữa chừng.
