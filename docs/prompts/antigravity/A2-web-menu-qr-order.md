# A2 — Web: thực đơn, QR order tại bàn, Zalo simulator (Pha 2)

**Skills**: `web-react-ui-conventions`, `verify-and-handoff`. **Nguồn**: đặc tả 3.5, 5.3 (đồng bộ hết món), `docs/02 §2`, `docs/03 §7–§8`.

## 1. `/thuc-don` — thực đơn công khai
Tab danh mục, tìm kiếm, ảnh (qua `ResponsiveImage`, URL CDN Supabase Storage do API trả; món chưa có ảnh dùng placeholder đẹp) + giá + mô tả. Món `isAvailable=false`: **làm mờ + nhãn "Tạm hết"**, không thêm được. Cập nhật tức thì qua SignalR `MenuAvailabilityChanged` (fallback polling 15s); có hiệu ứng chuyển mềm.

## 2. `/t/:tableCode?sig=` — QR order (mobile-first, tối ưu một tay)
- Gọi `GET …/session`: hiển thị "Bàn 201 · Tầng 2". `QR_SIGNATURE_INVALID` → trang lỗi thân thiện; `TABLE_NOT_OPEN` → "Bàn chưa được mở, vui lòng gọi nhân viên".
- Thực đơn theo danh mục, nút thêm nhanh, **giỏ hàng dạng ngăn kéo** cố định đáy màn hình (số món + tạm tính hiển thị chỉ để tham khảo).
- Mỗi món: số lượng, ghi chú, **thẻ dị ứng** (đậu phộng, hải sản…); một ô lưu ý chung cho cả đơn.
- **Gửi món**: gửi kèm `Idempotency-Key`, chống bấm đúp, trạng thái đang gửi; thành công → màn "Đã gửi cho bếp" + danh sách theo dõi (NEW/COOKING/READY/SERVED, cập nhật qua SignalR `KitchenItemStatusChanged` hoặc polling). Có thể gọi thêm đợt nữa.
- Món hết khi đang ở trong giỏ: tô nổi bật, chặn gửi, gợi ý bỏ món; xử lý `409 OUT_OF_STOCK_ITEM` đúng `itemId`.
- Lưu giỏ trong `localStorage` theo mã bàn (try/catch), xóa khi gửi xong.
- Hiệu năng: ảnh lazy-load, danh sách ảo hóa nếu dài, tải < 2s trên 4G giả lập.

## 3. `/dev/zalo-simulator` (chỉ khi `VITE_ENABLE_DEV_TOOLS=true`)
Giao diện **điện thoại giả** hiển thị hội thoại Zalo của một SĐT: polling `GET /dev/zalo-outbox?phone=` hiện tin AI gửi (kèm hạn giữ bàn), ô nhập + nút nhanh **"Đồng ý"** / **"Không"** gọi `POST /dev/simulate-zalo-reply`; nút "Mô phỏng thanh toán cọc" (`POST /dev/simulate-payment`) và "Mô phỏng bàn trống sớm" (`POST /dev/trigger-early-vacate`). Dùng để diễn tập/demo khi Zalo thật gặp sự cố. Ẩn hoàn toàn ở bản production build.

## Nghiệm thu
Chạy trên MSW (mock cả outbox và SignalR giả) ở 390px: duyệt menu → gửi đơn có dị ứng → theo dõi trạng thái; món hết bị mờ tức thì; simulator hiển thị hội thoại. Ảnh → `docs/evidence/A2/`; test Playwright smoke cho QR order. HANDOFF.
