# 04 — Design system & danh sách màn hình (Antigravity sở hữu)

## 1. Định hướng
Nhà hàng tầm trung–cao: **ấm áp, gọn, sang vừa phải**. Web ưu tiên mobile-first (khách quét QR trên điện thoại). WPF ưu tiên **thao tác nhanh, chạm được, đọc xa** (thu ngân/bếp đứng, màn cảm ứng).
Antigravity đề xuất bảng màu thương hiệu ở A0 theo ràng buộc: không trùng 5 màu trạng thái, tương phản chữ/nền ≥ 4.5:1. Tên nhà hàng là placeholder `Nhà hàng [TÊN]` lấy từ `GET /public/config`.

## 2. Token bắt buộc (dùng chung Web & WPF)
| Token | Hex | Nền nhạt | Nhãn VN | Biểu tượng |
|---|---|---|---|---|
| `status.available` | `#16A34A` | `#DCFCE7` | Trống | ✓ |
| `status.reserved` | `#EAB308` | `#FEF9C3` | Đã đặt | 🕒 |
| `status.occupied` | `#DC2626` | `#FEE2E2` | Có khách | ● |
| `status.cleaning` | `#6B7280` | `#E5E7EB` | Đang dọn | ✦ |
| `status.held` | `#7C3AED` | `#EDE9FE` | Đang giữ | ⏳ |

Màu **không** là dấu hiệu duy nhất: luôn kèm nhãn chữ + biểu tượng. Thêm token: `bg`, `surface`, `ink`, `muted`, `brand`, `brand-ink`, `danger`, `warning`, `success`, bán kính (8/12/16), khoảng cách (4-8-12-16-24-32), bóng đổ nhẹ, font: Web `Be Vietnam Pro` (fallback system-ui), WPF `Segoe UI` (đủ dấu tiếng Việt).

## 3. Quy tắc giao diện
- **Nội dung hiển thị**: tuân thủ `.agents/rules/ui-copy-discipline.md` — giao diện chỉ chứa chữ người dùng cuối cần; không giới thiệu hệ thống, không tham chiếu tài liệu/pha/agent, không thuật ngữ kỹ thuật hay mã enum thô. Trang `/dev/*` chỉ có nhãn ngắn.
- Mọi màn có đủ **4 trạng thái**: loading (skeleton), empty (câu chữ + hành động), error (thử lại), success.
- Tiền `350.000đ` (tabular-nums, căn phải). Hoàn cọc/số âm: chữ đỏ + nhãn "Hoàn lại khách".
- Đếm ngược: định dạng `mm:ss`, đổi màu cảnh báo khi < 60s; nhớ dùng `serverTime` để bù lệch đồng hồ.
- Mục tiêu chạm WPF ≥ 44×44 px; Web ≥ 44px; focus ring rõ; điều hướng bàn phím đầy đủ; `aria-*` đúng.
- Văn phong: lịch sự, ngắn, xưng "quý khách/anh/chị" nhất quán.
- **Ảnh từ Supabase Storage** (URL CDN công khai): luôn có khung tỉ lệ cố định (món 4:3, logo vuông/ngang, nền sơ đồ theo kích thước canvas) để không giật bố cục; skeleton khi tải, **ảnh placeholder khi `null` hoặc lỗi tải**; `alt` có nghĩa; Web `loading="lazy"` + `width/height`; WPF tải bất đồng bộ có cache. Không bao giờ giữ/gọi khóa Supabase từ giao diện.

## 4. Danh sách màn hình & ưu tiên (P0 = bắt buộc cho Demo 5 bước)
### Web
| Màn | Route | Ưu tiên |
|---|---|---|
| Trang chủ + CTA đặt bàn | `/` | P1 |
| Đặt bàn 4 bước (giờ → view → thông tin → cọc VietQR) + thành công | `/dat-ban` | **P0** |
| Tra cứu / hủy / dời đặt bàn | `/dat-ban/:code` | P1 |
| Thực đơn công khai (món hết bị mờ) | `/thuc-don` | P1 |
| QR order tại bàn | `/t/:tableCode?sig=` | **P0** |
| Zalo simulator (dev) | `/dev/zalo-simulator` | **P0** (để demo ổn định) |
| UI kit (dev) | `/dev/ui-kit` | P1 |

### WPF
| Màn | Vai | Ưu tiên |
|---|---|---|
| Đăng nhập, Shell (nav theo vai, trạng thái kết nối, chuông) | tất cả | **P0** |
| Sơ đồ bàn thời gian thực + legend + panel chi tiết bàn | M,C,W | **P0** |
| Popup leo thang AI (3 nút) + đếm ngược Tím | M,C | **P0** |
| POS (gọi món, tạm tính, thanh toán, hoàn cọc âm) + in K80 | M,C,W | **P0** |
| Phiếu bếp có cảnh báo dị ứng + KDS | M,K | **P0** |
| Danh sách đặt bàn (check-in/hủy/no-show/lệch số khách) | M,C | **P0** |
| Kho & Công thức (BOM) | M | **P0** |
| Hàng chờ + mời bàn | M,C | P1 |
| Cảnh báo quá giờ + "Chờ bàn phát sinh" | M,C | P1 |
| Timeline hoạt động AI | M,C | P1 (rất có giá trị khi bảo vệ) |
| Quản lý thực đơn (kèm **upload ảnh món**) / tài khoản / cấu hình (kèm **upload logo**) | M | P1 |
| Báo cáo doanh thu, quỹ tiền | M | P1 |
| Trình sửa sơ đồ bàn (kéo thả) + **upload ảnh nền mặt bằng** | M | P2 |
| Chọn bàn cụ thể trên Web | khách | P2 |

## 5. Quyết định thiết kế
(Antigravity ghi **ngắn gọn** các quyết định đáng giữ: bảng màu thương hiệu + lý do, kích thước chạm, font… — thay cho việc viết lên giao diện.)
