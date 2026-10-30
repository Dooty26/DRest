# C5 — Cọc VietQR + webhook xác nhận (Pha 2)

**Skills**: `vietqr-deposit-webhook`, `dotnet-api-conventions`, `verify-and-handoff`. **Nguồn**: `docs/03 §4`, đặc tả 3.3.

## Việc cần làm
1. **`IPaymentQrService`**: sinh payload EMVCo/VietQR **cục bộ** (TLV + CRC16-CCITT) với số tiền, nội dung = mã đặt bàn (`RES008899`, không dấu/không khoảng trắng), thông tin tài khoản từ cấu hình (`Payment:BankBin`, `AccountNo`, `AccountName`). Trả thêm `qrImageUrl?` (dịch vụ ảnh công khai) làm phương án dự phòng. Gắn vào `POST /public/reservations`.
2. **`IPaymentWebhookAdapter`**: `SePayWebhookAdapter` (mặc định; đối chiếu **tài liệu chính thức hiện hành** về cách xác thực & tên trường), `MockWebhookAdapter`. `POST /webhooks/payment/{provider}`: xác thực bí mật → `WEBHOOK_SIGNATURE_INVALID` nếu sai; tìm mã `RES\d{6}` trong nội dung (chịu được ngân hàng làm hỏng định dạng); **idempotent theo `provider_txn_id`**.
3. **Quy tắc đối soát**: đủ/thừa tiền → `CONFIRMED`, `deposit_paid_at`, phát `DepositConfirmed` (staff + public group theo mã); thiếu tiền → giữ `PENDING`, ghi `payments(REVIEW)` + `PaymentReviewNeeded`; thanh toán **trễ** sau khi `EXPIRED` → nếu slot còn trống thì khôi phục, ngược lại `PaymentReviewNeeded(reason=LATE_PAYMENT_REFUND)`; mã không khớp → bỏ qua có log.
4. **Hoàn cọc/Voucher**: `POST /public/reservations/{code}/cancel` & staff cancel tạo `payments(REFUND)` ghi nhận (thu ngân chuyển khoản thủ công) hoặc phát `vouchers` (hạn 90 ngày); `POST /reservations/{id}/refund-record`.
5. **Dev**: `POST /dev/simulate-payment {code}` (chỉ Development) đi qua đúng pipeline webhook Mock.
6. Cập nhật `Client.Core`: `ReservationsViewModel` hiển thị trạng thái cọc, nhận `DepositConfirmed`, `PaymentReviewNeeded` → toast.

## Test
Đơn vị: payload QR so với **vector kiểm thử** (tự tính CRC bằng cách độc lập hoặc từ ví dụ công khai) · trích mã từ nội dung nhiễu · thiếu/đủ/thừa/trễ · webhook trùng · sai chữ ký. Tích hợp: tạo đặt bàn → simulate-payment → `CONFIRMED` + event phát.

## Nghiệm thu
Xanh · HANDOFF nêu cách cấu hình tài khoản thật + lưu ý "kiểm chứng QR bằng app ngân hàng thật" và cách đăng ký webhook nhà cung cấp.
