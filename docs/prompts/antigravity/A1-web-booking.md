# A1 — Web: đặt bàn 4 bước + cọc VietQR + tra cứu (Pha 1)

**Skills**: `web-react-ui-conventions`, `verify-and-handoff`. **Nguồn**: đặc tả 3.1–3.4, `docs/02 §2`, `docs/03 §2–§5`.
Làm trên MSW; khi Codex xong C3/C5, đổi `VITE_USE_MOCK=false` và sửa lệch.

## Màn hình `/dat-ban` (mobile-first, có thanh tiến trình)
1. **Chọn ngày – giờ – số khách**: lịch ngày (giới hạn `MaxAdvanceDays`), số khách (stepper), lưới giờ từ `GET /public/availability`. Slot bị khóa **vô hiệu hóa** + gợi ý ngắn ("Bàn cần 105 phút gồm dọn dẹp — khung giờ này đã được giữ"). Hiển thị tình trạng theo từng loại view ngay trên slot.
2. **Chọn view/vị trí**: thẻ lớn cho "Bàn tiêu chuẩn tầng 1", "View cửa sổ tầng 2", "Ban công tầng 3", "Phòng VIP khép kín" (+ ngoài trời) kèm mô tả/ảnh. Loại hết chỗ: hiển thị rõ **"Hết bàn view này lúc 19:00"** và CTA *"Đặt bàn thường + nhận thông báo khi bàn view cửa sổ trống sớm"* (đặt `wishlist.enabled=true`, `targetViewType`). Khách vẫn có thể chọn loại khác.
3. **Thông tin & yêu cầu đặc biệt**: họ tên, SĐT (kiểm tra số VN), email tùy chọn; dịp (sinh nhật…), ghế em bé; **tick-box dị ứng** (đậu phộng, hải sản, gluten, sữa, trứng, ăn chay, khác→ô nhập); ghi chú. Hiển thị chính sách cọc + hủy ≥ 2 giờ hoàn 100%/Voucher rõ ràng trước khi tiếp tục.
4. **Thanh toán cọc**: gọi `POST /public/reservations` (Idempotency-Key) → hiện **mã QR** (từ `qrPayload`), số tiền, nội dung chuyển khoản `RES…` (nút sao chép), thông tin ngân hàng, **đếm ngược** `expiresAt`. Theo dõi trạng thái bằng SignalR `DepositConfirmed` (fallback polling 3 giây) → chuyển sang **màn thành công**: tóm tắt, mã đặt bàn, nút "Thêm vào lịch (.ics)", link tra cứu, ghi chú về nguyện vọng đổi bàn nếu có. Trạng thái hết hạn: thông báo + nút "Đặt lại".
5. Xử lý lỗi theo `code`: `SLOT_UNAVAILABLE` (quay về bước 1 với gợi ý), `VALIDATION_ERROR` (đánh dấu trường), mạng lỗi (thử lại, không mất dữ liệu đã nhập).

## Màn `/dat-ban/:code`
Tra cứu bằng mã + SĐT → trạng thái, bàn, giờ; hiển thị `refundPreview`; nút **Hủy** (chọn *hoàn tiền* hoặc *Voucher*, xác nhận 2 bước) và **Dời lịch** (chọn lại slot). Trạng thái hết hạn hoàn/hủy hiển thị đúng chính sách.

## Yêu cầu chung
Đủ 4 trạng thái (loading/empty/error/success) mỗi bước; focus/ARIA; tiếng Việt tự nhiên; không tính toán nghiệp vụ ở client (không tự tính slot khóa); lưu nháp form trong `sessionStorage` (không lưu dữ liệu thanh toán).

## Nghiệm thu
Dùng trình duyệt chạy đủ luồng trên MSW ở **390px và 1280px**: đặt thường, đặt kèm wishlist, hết hạn cọc, tra cứu/hủy. Ảnh chụp từng bước → `docs/evidence/A1/`. Thêm test Playwright smoke (`npm run test:e2e`) cho luồng chính. HANDOFF (nêu field/endpoint còn thiếu).
