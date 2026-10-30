# Rule: Kỷ luật nội dung hiển thị (UI copy)

**Chế độ kích hoạt: Always On.** Áp dụng cho MỌI chuỗi người dùng nhìn thấy ở Web và WPF.

## Nguyên tắc gốc
Prompt, `docs/`, đặc tả và skill là **chỉ dẫn dành cho bạn**, không phải nội dung để đưa lên giao diện. Giao diện chỉ nói với người dùng cuối (khách đặt bàn, thu ngân, phục vụ, bếp, quản lý nhà hàng) — họ không biết và không cần biết hệ thống được làm thế nào.

Trước khi viết bất kỳ chữ nào lên giao diện, tự hỏi: **"Người dùng cần câu này để làm gì ở bước này?"** Không có câu trả lời ⇒ không viết.

## Cấm xuất hiện trong chuỗi hiển thị (nhãn, tiêu đề, mô tả, tooltip, placeholder, thông báo, `aria-label`, `ToolTip`, `Header`, `Content`, `Text`)
1. Tên công cụ/agent/giai đoạn: Antigravity, Codex, Claude, "Pha 0", "A0", "Phase", "Sprint".
2. Tham chiếu tài liệu: `docs/04 §2`, "theo đặc tả", "theo hợp đồng", "contract".
3. Thuật ngữ kỹ thuật: token, design system, a11y, WCAG, touch target, `≥ 44px`, mock, stub, API, JSON, enum, Supabase, SignalR, Postgres, endpoint, payload, backend, frontend, UI kit, Storybook.
4. Mã nội bộ chưa dịch: `AVAILABLE`, `RESERVED`, `OCCUPIED`, `CLEANING`, `HELD`, `PENDING_DEPOSIT`, `CONFIRMED`, `NO_SHOW`, mã lỗi (`SLOT_UNAVAILABLE`…). Luôn ánh xạ sang nhãn tiếng Việt.
5. Quy tắc vận hành nội bộ giải thích cho dev: "lưới 30 phút", "quy tắc 105 phút", "buffer", "net âm", "Cọc > Bill", "bắt buộc dùng chung Web & WPF".
6. Lời giới thiệu hệ thống, khẩu hiệu mô tả thành phần ("Đầy đủ 5 trạng thái…", "Hệ thống Design System…"), lorem ipsum, TODO, dữ liệu giả lộ liễu ("Test", "abc").

## Quy tắc viết
- Tiếng Việt, ngắn, dùng từ khách quen. Tiêu đề ≤ 6 từ. Nhãn trường ≤ 3 từ. Nút = động từ ngắn ("Đặt bàn", "Thanh toán", "Hủy").
- **Không đoạn văn mô tả** mở đầu trang hay mục. Trang đặt bàn bắt đầu bằng việc cần làm, không bằng giới thiệu.
- Gợi ý (helper text) **chỉ** khi dễ nhập sai/hiểu nhầm, tối đa **một câu ngắn**. Chính sách quan trọng (cọc, hủy) viết một-hai câu gần nút liên quan.
- Quy tắc nghiệp vụ phức tạp biến thành **thông điệp theo tình huống**, đúng lúc: slot kín → "Đã kín"; hết hạn cọc → "Đã hết thời gian giữ bàn". Không đặt quy tắc thành chú thích thường trực.
- Số liệu/tiền: `350.000đ`; số âm = "Hoàn lại khách: 70.000đ" (không in "net âm").
- Giải thích cho lập trình viên đặt ở **comment code**, `README`, hoặc `docs/04` mục "Quyết định thiết kế" — không vào UI.

## Trang phát triển (`/dev/*`, màn hình thử trong WPF)
- Chỉ hiển thị khi bật cờ dev; **không** có liên kết trong thanh điều hướng công khai; bị loại khỏi bản production.
- Vẫn tuân thủ rule này: nhãn **ngắn** (tên thành phần/trạng thái), không đoạn mô tả, không tham chiếu docs/pha/agent, không giải thích quy tắc. Mục đích của trang là xem component, không phải tài liệu.

## Tự kiểm bắt buộc trước khi báo xong
1. Chạy `node scripts/check-ui-copy.mjs` — phải **0 lỗi** (cảnh báo thì xem xét từng cái). Bỏ qua một dòng chỉ khi có lý do chính đáng bằng comment `copy-ok: <lý do>`; không dùng để "lách".
2. Mở giao diện thật, đọc từng màn như người dùng: có câu nào khiến họ thắc mắc "cái này nói gì với mình?" → xóa/viết lại.
3. Nếu người dùng yêu cầu thêm văn bản giải thích vào UI, vẫn giữ ngắn gọn; nếu không chắc, hỏi lại thay vì thêm.

## Ví dụ
| Đừng | Nên |
|---|---|
| `UI KIT CATALOG (PHA 0 - ANTIGRAVITY)` (huy hiệu đầu trang) | (bỏ) |
| `Đầy đủ 5 trạng thái bàn, bảng màu thương hiệu Warm Amber, a11y touch target ≥ 44px…` | (bỏ) |
| `1. 5 Token màu trạng thái bàn (docs/04 §2)` | `Trạng thái bàn` |
| `Bắt buộc dùng chung Web & WPF; màu luôn đi kèm nhãn…` | (bỏ) |
| thẻ hiển thị `AVAILABLE` + `Trống` | chỉ `Trống` |
| `Hóa đơn net âm (Cọc > Bill): HOÀN LẠI KHÁCH` | `Hoàn lại khách: 70.000đ` |
| `KHUNG GIỜ (LƯỚI 30 PHÚT - KHÓA THEO QUY TẮC 105 PHÚT)` | `Chọn giờ` (slot kín: gạch + tooltip "Đã kín") |
| `Nút bấm (Button) — Chuẩn touch ≥ 44px` | `Nút` |
| `Form Controls & Khả năng tiếp cận (a11y)` | `Biểu mẫu` |
| Liên kết `UI Kit` ở thanh đầu trang công khai | (chỉ hiện khi bật cờ dev) |
| Trang WPF tạm: "Placeholder cho PosViewModel" | tiêu đề trang + trạng thái trống ngắn gọn ("Chưa có dữ liệu") |
