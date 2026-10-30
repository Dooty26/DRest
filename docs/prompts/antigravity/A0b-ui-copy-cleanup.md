# A0b — Dọn nội dung thừa trên giao diện (Web + WPF)

**Rule bắt buộc**: `.agents/rules/ui-copy-discipline.md` (đọc trước khi làm). **Skills**: `web-react-ui-conventions`, `wpf-xaml-ui-conventions`, `verify-and-handoff`.
**Bối cảnh**: sau A0, giao diện đang chứa chữ dành cho lập trình viên thay vì cho người dùng: huy hiệu "UI KIT CATALOG (PHA 0 - ANTIGRAVITY)", đoạn giới thiệu "Đầy đủ 5 trạng thái bàn… a11y touch target ≥ 44px…", tiêu đề kèm tham chiếu tài liệu "(docs/04 §2)", ghi chú "Bắt buộc dùng chung Web & WPF…", mã `AVAILABLE/RESERVED…` in dưới nhãn, nhãn "KHUNG GIỜ (LƯỚI 30 PHÚT - KHÓA THEO QUY TẮC 105 PHÚT)", "Hóa đơn net âm (Cọc > Bill)", liên kết "UI Kit" ở thanh đầu trang… Mục tiêu: giao diện **chỉ còn nội dung người dùng cuối cần**.

## Việc cần làm
1. **Đo trước**: chạy `node scripts/check-ui-copy.mjs`, lưu kết quả vào `docs/evidence/A0b/truoc.txt`; chụp ảnh `/dev/ui-kit` (1280px) và Shell WPF vào `docs/evidence/A0b/`.
2. **Web — rà toàn bộ `web/src`** (trang, component, toast, thông báo lỗi, trạng thái trống, `title/alt/aria-label/placeholder`), không chỉ ui-kit:
   - **Thanh đầu trang công khai**: bỏ liên kết "UI Kit"; chỉ hiện khi `VITE_ENABLE_DEV_TOOLS=true`.
   - **`/dev/ui-kit`**: bỏ huy hiệu đầu trang, bỏ tiêu đề lớn kiểu tài liệu + đoạn mô tả, bỏ đường kẻ giới thiệu. Mỗi mục chỉ gồm **tiêu đề ngắn + mẫu thành phần**: "Trạng thái bàn", "Tiền tệ", "Nút", "Ô nhập", "Chọn giờ", "Nhãn", "Tiến trình". Bỏ số thứ tự "1., 2., …", bỏ ghi chú giải thích (token/a11y/touch/enum). Thẻ trạng thái chỉ hiện nhãn tiếng Việt ("Trống", "Đã đặt", "Có khách", "Đang dọn", "Đang giữ"), **không** in mã tiếng Anh.
   - **Chọn giờ**: nhãn mục = "Chọn giờ"; slot không còn chỗ chỉ gạch + tooltip/`aria-label` "Đã kín"; không có chú thích về lưới/quy tắc/105 phút.
   - **Tiền**: khối số âm chỉ hiện "Hoàn lại khách: 70.000đ"; bỏ "net âm", "Cọc > Bill"; mẫu còn lại chỉ có nhãn ngắn ("Tạm tính", "Tổng cộng").
   - **Mọi trang khác** (đặt bàn, thực đơn, QR order, simulator): xóa đoạn giới thiệu không cần, rút gọn helper text còn **một câu ngắn và chỉ khi dễ nhập sai**; chính sách cọc/hủy viết ngắn cạnh nút liên quan.
3. **WPF — rà toàn bộ `src/Restaurant.Wpf`** (`Views`, `Windows`, `Controls`, `Styles` có chữ, `ToolTip`, `Header`, `Content`, `Text`, `AutomationProperties`):
   - Trang tạm theo ViewModel: bỏ chữ kiểu "Placeholder cho XViewModel"; chỉ hiện **tiêu đề trang** + một dòng trạng thái trống ngắn ("Chưa có dữ liệu").
   - Shell/tiêu đề cửa sổ: tên nhà hàng + tên chức năng; bỏ mọi thông tin gỡ lỗi (tên VM, chế độ `--fake`, số phiên bản nội bộ) khỏi giao diện — nếu cần cho dev, chỉ hiện trong cửa sổ "Thông tin kỹ thuật" mở bằng phím tắt và đặt sau cờ dev.
   - Nhãn trạng thái bàn/đơn/món luôn là tiếng Việt (ánh xạ qua converter), không lộ mã enum.
4. **Chuyển thông tin hữu ích cho dev về đúng chỗ**: những gì đã viết trên UI nhưng đáng giữ (bảng màu, quy ước kích thước chạm, lý do chọn màu thương hiệu) → ghi **ngắn gọn** vào `docs/04` mục "Quyết định thiết kế" hoặc `web/README.md`; không để lại trên giao diện.
5. **Chặn tái phát**: thêm vào `web/package.json` script `"lint:copy": "node ../scripts/check-ui-copy.mjs"` và gọi trong `npm run lint`; thêm bước kiểm tra tương tự vào checklist "Definition of Done" của bạn (ở A1–A6 phải chạy trước khi báo xong).
6. **Đo sau**: chạy lại script đến khi **0 lỗi**, xử lý cả cảnh báo "đoạn văn dài". Chỉ dùng `copy-ok: <lý do>` cho trường hợp thật sự chính đáng (nêu lý do cụ thể), ghi danh sách vào HANDOFF. Chụp lại ảnh trước/sau.

## Ràng buộc
- Không đổi hành vi, không đổi hợp đồng, không đụng `src/Restaurant.Client.Core` hay backend. Nếu một chuỗi hiển thị đến từ **backend/VM** (thông báo lỗi, nhãn trạng thái bằng tiếng Anh/mã thô) → ghi vào HANDOFF "Câu hỏi cho bên kia" thay vì tự sửa; tạm ánh xạ ở tầng hiển thị.
- Giữ nguyên bố cục, màu sắc, component đã có; chỉ bỏ/viết lại chữ và liên kết dev. Tiêu đề ≤ 6 từ, nhãn ≤ 3 từ.

## Nghiệm thu
```
node scripts/check-ui-copy.mjs        # 0 lỗi, 0 cảnh báo (hoặc mỗi cảnh báo còn lại có lý do trong HANDOFF)
cd web && npm run lint && npm run build
dotnet build src/Restaurant.Wpf -p:EnableWindowsTargeting=true
```
Trang `/dev/ui-kit` không còn đoạn mô tả, không còn tham chiếu tài liệu/pha/agent, không còn mã tiếng Anh; thanh đầu trang công khai không có "UI Kit". Ảnh trước/sau trong `docs/evidence/A0b/`. Cập nhật HANDOFF (số lỗi trước → sau, danh sách `copy-ok` nếu có, yêu cầu cho Codex nếu có).
