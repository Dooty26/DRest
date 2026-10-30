# A6 — Tích hợp, đánh bóng, kịch bản demo (Pha 4)

**Skills**: `web-react-ui-conventions`, `wpf-xaml-ui-conventions`, `verify-and-handoff`.
**Điều kiện**: Codex đã xong C9 (`/dev/seed-demo`, `DEMO-SCRIPT` phần backend).

## Việc cần làm
1. **Chạy với API thật** cả Web (`VITE_USE_MOCK=false`) và WPF (bỏ `--fake`). Sửa mọi lệch hợp đồng: cái nào thiếu ở backend → ghi HANDOFF cho Codex; cái nào sai ở UI → sửa.
2. **Diễn tập Demo 5 bước** (`docs/00 §7`) đầu–cuối trên 2 thiết bị thật (điện thoại cho Web/Zalo simulator hoặc Zalo thật, PC chạy WPF). Ghi lại mọi chỗ vấp (chậm, khó thấy, sai chữ) và sửa.
3. **Đánh bóng**: nhất quán khoảng cách/chữ/biểu tượng; trạng thái rỗng/lỗi/offline (banner "Mất kết nối — đang thử lại" ở WPF, có nút thử lại); hoạt ảnh vừa phải; thông điệp tiếng Việt thống nhất; kiểm tra tương phản; điều hướng bàn phím; chạy được ở 1366×768 và màn cảm ứng.
4. **Hiệu năng**: Web Lighthouse mobile (Performance/Accessibility ≥ 90 cho `/dat-ban` và `/t/…`); WPF mở sơ đồ 60+ bàn không giật, thao tác không treo UI.
5. **Chế độ trình diễn** (tùy chọn nhưng nên có): WPF phóng to chữ/tiles khi chiếu màn hình (phím tắt), Web có `?demo=1` hiển thị gợi ý bước.
6. **Tài liệu**: bổ sung phần giao diện vào `docs/DEMO-SCRIPT.md` (cần mở màn nào, bấm gì, nói gì ở mỗi bước, kế hoạch dự phòng nếu Zalo/ngân hàng lỗi → dùng `/dev/zalo-simulator`); bộ ảnh chụp cuối trong `docs/evidence/final/`; danh sách vấn đề còn tồn trong `docs/KNOWN-ISSUES.md` (phần UI).
7. Dọn dẹp: xóa code chết, mock không dùng, cảnh báo build; `npm run build` và `dotnet build` sạch cảnh báo.

## Nghiệm thu
Diễn tập đủ 5 bước **3 lần liên tiếp** không lỗi giao diện; Lighthouse đạt; HANDOFF tổng kết (đã làm / còn dở / rủi ro khi demo).
