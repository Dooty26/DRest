---
name: vietqr-deposit-webhook
description: Tạo mã QR VietQR/NAPAS (EMVCo, CRC16) cho tiền cọc đặt bàn và nhận webhook giao dịch ngân hàng (SePay/Casso/payOS) để xác nhận cọc tự động, idempotent, kèm Mock cho demo. Dùng khi làm thanh toán cọc, sinh mã QR chuyển khoản, nội dung chuyển khoản RES..., webhook xác thực, đối soát thiếu/thừa/trễ tiền, hoàn cọc/Voucher, hoặc khi cần mô phỏng thanh toán trong môi trường dev.
---

# VietQR + webhook xác nhận cọc

## Kiến trúc
```
IPaymentQrService  → sinh payload QR (cục bộ, không gọi mạng)
IPaymentWebhookAdapter → SePay | Casso | payOS | Mock  (parse + xác thực → BankTransaction chuẩn hóa)
PaymentReconciler  → khớp giao dịch với reservation, đổi trạng thái, phát event
```
`BankTransaction { ProviderTxnId, Amount, Content, ReceivedAt, AccountNo, Raw }` — domain chỉ biết kiểu này.

## Sinh payload VietQR (EMVCo, chuẩn NAPAS 247)
```csharp
static string Tlv(string id, string v) => id + v.Length.ToString("00") + v;      // độ dài = số ký tự ASCII
static string Crc16(string s) {                                                    // CRC16-CCITT-FALSE: poly 0x1021, init 0xFFFF
    ushort crc = 0xFFFF;
    foreach (var b in Encoding.ASCII.GetBytes(s)) { crc ^= (ushort)(b << 8);
        for (int i = 0; i < 8; i++) crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1); }
    return crc.ToString("X4"); }

string Build(string bankBin, string accountNo, long amount, string content) {
    var acct = Tlv("00", bankBin) + Tlv("01", accountNo);
    var merchant = Tlv("00", "A000000727") + Tlv("01", acct) + Tlv("02", "QRIBFTTA");  // chuyển tới tài khoản
    var body = Tlv("00", "01") + Tlv("01", "12")                                        // 12 = QR động (có số tiền)
             + Tlv("38", merchant) + Tlv("53", "704") + Tlv("54", amount.ToString())
             + Tlv("58", "VN") + Tlv("62", Tlv("08", content)) + "6304";
    return body + Crc16(body);
}
```
- `content`: chỉ ASCII, không dấu cách, tối đa ~25 ký tự (vd `RES008899`). Mã đặt bàn nên là `RES` + số để ngân hàng không làm hỏng.
- **Bắt buộc**: unit test với vector kiểm thử độc lập, và **quét thử bằng app ngân hàng thật** trước khi demo (ghi vào HANDOFF). Nếu payload không được app chấp nhận, dùng `qrImageUrl` của dịch vụ ảnh VietQR công khai làm dự phòng (cấu hình bật/tắt). Đối chiếu đặc tả NAPAS/VietQR hiện hành khi nghi ngờ tag nào.
- Web hiển thị QR bằng thư viện QR từ chuỗi `qrPayload`.

## Webhook
- Endpoint `POST /webhooks/payment/{provider}`, **không** dùng JWT; xác thực theo cơ chế của nhà cung cấp (API key/chữ ký HMAC trong header) — **đọc tài liệu chính thức hiện hành** của SePay/Casso/payOS để biết tên header/trường, đừng đoán. Sai ⇒ `401 WEBHOOK_SIGNATURE_INVALID`, không xử lý.
- Luôn trả 200 nhanh sau khi **ghi nhận** (nhiều nhà cung cấp gọi lại nếu chậm/lỗi); xử lý nặng chạy sau.
- **Idempotent**: `payments.provider_txn_id` UNIQUE; gặp trùng → bỏ qua, vẫn 200.
- Chỉ xử lý giao dịch **tiền vào** đúng tài khoản cấu hình.
- Rate-limit + giới hạn kích thước body; lưu `raw_payload` (che dữ liệu nhạy cảm) để điều tra.

## Đối soát (PaymentReconciler)
Trích mã: `Regex(@"RES\d{6}", IgnoreCase)` trên nội dung đã chuẩn hóa (bỏ dấu cách/ký tự lạ; ngân hàng có thể chèn tiền tố/hậu tố).
| Tình huống | Hành động |
|---|---|
| `amount ≥ deposit` & reservation `PENDING_DEPOSIT` chưa hết hạn | `CONFIRMED`, `deposit_paid_at`, phát `DepositConfirmed`; thừa tiền → ghi chú để hoàn khi thanh toán |
| `amount < deposit` | giữ `PENDING`, `payments(REVIEW)`, phát `PaymentReviewNeeded(UNDERPAID)` |
| Đã `EXPIRED` mà tiền về | nếu slot còn trống (kiểm lại buffer có khóa) → khôi phục `CONFIRMED`; không thì `PaymentReviewNeeded(LATE_PAYMENT_REFUND)` |
| Reservation đã `CONFIRMED` (trả hai lần) | ghi `REVIEW` để hoàn |
| Không khớp mã | log, không lỗi |

## Hoàn cọc / Voucher
Hủy ≥ 2h: khách chọn `REFUND` (thu ngân chuyển khoản lại → ghi `payments(REFUND)` qua `refund-record`) hoặc `VOUCHER` (giá trị = cọc, hạn 90 ngày). Hủy < 2h: `FORFEITED`. Không tự động chuyển tiền ra ngoài.

## Mock & dev
`MockWebhookAdapter` nhận payload đơn giản `{ txnId, amount, content }`. `POST /dev/simulate-payment {code}` tạo giao dịch giả đi **qua đúng pipeline** (không gọi tắt vào reconciler). Cấu hình `Payment:Provider = Mock|SePay|…`.

## Test
Vector CRC/QR · trích mã nhiễu · đủ/thiếu/thừa/trễ/trùng · sai chữ ký · hai webhook song song cùng `txnId` (chỉ một hiệu lực).
