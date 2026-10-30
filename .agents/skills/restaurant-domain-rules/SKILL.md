---
name: restaurant-domain-rules
description: Quy tắc nghiệp vụ bất biến của hệ thống đặt bàn & vận hành nhà hàng (khoảng đệm 90+15 phút, trạng thái bàn, cọc VietQR, hủy/hoàn cọc, hold 10 phút, leo thang nhân viên, hóa đơn trừ cọc, BOM trừ kho, RBAC). Dùng skill này BẤT KỲ KHI NÀO viết hoặc sửa logic liên quan đến bàn, đặt bàn, cọc, hóa đơn, kho, AI đổi bàn, phân quyền, hay khi cần giải thích/đối chiếu hành vi với đặc tả — kể cả khi người dùng không nhắc "nghiệp vụ".
---

# Quy tắc nghiệp vụ nhà hàng

Nguồn đầy đủ: `docs/03-DOMAIN-RULES.md` (công thức, state machine, ma trận RBAC, giá trị cấu hình). Skill này là **công tắc an toàn**: đọc đúng mục trong docs/03 rồi kiểm tra checklist bên dưới trước khi commit.

## Cách dùng
1. Xác định task thuộc mục nào của `docs/03` (§2 buffer · §3 availability · §4 tạo đặt bàn · §5 hủy/dời/no-show · §6 walk-in/hàng chờ · §7 ngoại lệ · §8 order/bếp · §10 hóa đơn · §11 kho · §12 AI · §13 RBAC) và **đọc mục đó**.
2. Hiện thực hàm thuần ở `Restaurant.Core` trước, test bằng bảng giá trị, rồi mới nối DB/API.
3. Gặp điểm mơ hồ: dùng mặc định đã ghi trong `docs/03`/`docs/01 §C`, ghi vào `docs/HANDOFF.md`; không tự bịa quy tắc mới.

## Bất biến (vi phạm = bug nghiêm trọng)
- **Buffer**: lượt chiếm dụng = 90 + 15 = 105'. Bàn bị khóa cho giờ `N` khi `|N − S| ≤ 105` (biên đóng). Ví dụ phải luôn đúng: S=19:00 → 17:00 mở, 17:15–20:45 khóa, 21:00 mở.
- **Không bao giờ** tin kiểm tra xung đột ở tầng ứng dụng đơn thuần — phải có khóa hàng `FOR UPDATE` **và** exclusion constraint ở CSDL (skill `postgres-concurrency-booking`). `PENDING_DEPOSIT` quá hạn vẫn chiếm chỗ trong constraint cho tới khi bị đánh `EXPIRED`.
- `displayStatus` do **server** tính, ưu tiên HELD > OCCUPIED > CLEANING > RESERVED > AVAILABLE.
- Tiền là `decimal` VND; `net = subtotal − discount + vat − depositApplied` **có thể âm** ⇒ phải hoàn tiền, cần xác nhận mới đóng hóa đơn.
- Trừ kho BOM **đúng một lần** cho mỗi hóa đơn, cùng transaction với thanh toán.
- AI **không tự hủy cọc/đơn**; quá hạn 10' ⇒ `ESCALATED` (nhân viên gọi điện), không phải tự quyết. Quyết định gán bàn là code xác định, Gemini chỉ viết/hiểu chữ.
- Hoán đổi bàn là **nguyên tử** (khóa request + bàn đích + bàn cũ); race "đồng ý" vs "hết hạn" chỉ một bên thắng.
- RBAC kiểm ở **server** theo ma trận `docs/03 §13`.
- Thời gian lưu UTC (`timestamptz`); logic dùng `TimeProvider`; hiển thị giờ VN.
- Supabase chỉ là PostgreSQL + Storage; mọi bảng bật RLS không policy; Web/WPF không giữ khóa Supabase.

## Checklist trước khi xong
- [ ] Có test bảng-giá-trị cho quy tắc vừa chạm (đưa cả ca biên).
- [ ] Ca đồng thời đã được nghĩ tới (hai request cùng lúc làm gì?).
- [ ] Mọi trạng thái chuyển đổi hợp lệ theo state machine; chuyển sai trả `INVALID_STATE`.
- [ ] Sự kiện realtime phát **sau commit**.
- [ ] Đã ghi `audit_logs`/`switch_request_events` ở nơi quy định.
