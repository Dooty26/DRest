# A4 — WPF: POS, in K80, phiếu bếp, KDS, đặt bàn, kho BOM, quản trị, báo cáo (Pha 2)

**Skills**: `wpf-xaml-ui-conventions`, `verify-and-handoff`. **Nguồn**: đặc tả 5.3–5.4, mục 6 (RBAC), `docs/05`, `docs/03 §8–§11`.
Dùng `--fake` cho tới khi Codex xong C6.

## Màn hình & yêu cầu
1. **`PosView`** (M,C,W): lưới món theo danh mục + tìm kiếm (món hết: mờ + "Tạm hết"); danh sách dòng order (±số lượng, ghi chú, **thẻ dị ứng** nổi bật); cột phải: tạm tính, giảm giá, VAT, **cọc đã khấu trừ**, **Thành tiền** (`NetAmount`). Nút Gửi bếp, In tạm tính, Thanh toán (ẩn với Waiter — theo `Can…`).
2. **`CheckoutDialog`**: phương thức (Tiền mặt/Chuyển khoản/Thẻ), mã giảm giá/Voucher, % giảm + lý do, tiền khách đưa → tiền thối. **Khi `IsRefundDue`**: banner đỏ lớn **"HOÀN LẠI KHÁCH 70.000đ"** (số âm hiển thị thành chữ + số), ô xác nhận "Đã hoàn tiền cho khách" bắt buộc tick mới bật nút Hoàn tất.
3. **In (K80)** — `WpfPrintService` (hiện thực `IPrintService`): (nếu có `LogoUrl` thì tải qua `IImageCache`, in logo đơn sắc nhỏ ở đầu hóa đơn; tải lỗi ⇒ bỏ qua logo, **không** làm hỏng hóa đơn) render `ReceiptModel` và `KitchenTicketModel` thành `FlowDocument` **rộng 80mm** (vùng in ≈72mm), font có dấu tiếng Việt, in qua driver Windows (không dùng ESC/POS thô để tránh lỗi font), xem trước (`ReceiptPreviewWindow`). **Phiếu bếp**: tên bàn/khu, giờ, danh sách món lớn, khung **"⚠ DỊ ỨNG: …"** đảo màu/đóng khung đậm; phân biệt nguồn QR/nhân viên. Hóa đơn: logo, thông tin, dòng món, tổng, giảm, cọc, **dòng hoàn lại**, cảm ơn.
4. **`PrintSettingsView`**: chọn máy in bếp/hóa đơn, bật `IsPrintHost`, in thử.
5. **`KitchenView` (KDS)**: 3 cột Mới / Đang nấu / Xong; thẻ ticket có tên bàn, thời gian chờ (đổi màu theo ngưỡng), danh sách món, cảnh báo dị ứng; nút lớn Bắt đầu / Xong / **Tạm hết món**; âm báo khi có ticket mới; chế độ toàn màn hình. Chỉ M & Kitchen.
6. **`ReservationsView`** (M,C): danh sách hôm nay/ngày chọn, lọc trạng thái, tìm theo tên/SĐT/mã; chi tiết: cọc, wishlist, dị ứng, ghi chú; nút **Check-in**, **Hủy** (hộp thoại xem trước hoàn cọc ≥2h/<2h), **No-show**, **Cập nhật số khách** (hiện gợi ý bàn lớn hơn hoặc "kê thêm ghế phụ"), **Ghi nhận hoàn cọc**.
7. **Kho & định lượng** (M): `InventoryView` (tồn theo đơn vị hiển thị kg/l/quả/lon, cảnh báo thấp, điều chỉnh có lý do, lịch sử xuất–nhập); `RecipeEditorView` (chọn món → bảng nguyên liệu + định lượng g/ml, thêm/xóa dòng, lưu); danh sách món chưa có định lượng.
8. **Quản trị** (M): `MenuAdminView` (CRUD món, giá, bật/tắt bán; **ảnh món**: xem trước `RemoteImage`, nút *Chọn ảnh…* → `PickAndUploadImageCommand` (hiện tiến trình `IsUploadingImage`, lỗi `ImageError` bằng tiếng Việt: sai kiểu/quá cỡ/lỗi mạng), nút *Xóa ảnh*; gợi ý kiểm sơ bộ jpg/png/webp ≤ 2 MB ở client nhưng **server mới là bên quyết định**), `UsersAdminView` (vai, khóa/mở, đặt lại mật khẩu), `SettingsView` (cấu hình `docs/03 §0` có kiểm hợp lệ + **upload logo nhà hàng** qua `PickAndUploadLogoCommand`).
9. **`ReportsView`** (M): doanh thu theo ngày/tháng (biểu đồ cột/đường), quỹ cọc, top món, tồn thấp, quỹ tiền; thư viện biểu đồ ưu tiên LiveCharts2 (nếu trục trặc dùng OxyPlot); xuất CSV (nếu VM hỗ trợ).
10. Mọi nút/menu **ẩn hoặc vô hiệu theo vai** (dựa `Can…`/`NavItems`; server vẫn kiểm quyền).

## Nghiệm thu (Windows, `--fake`)
Luồng: mở POS bàn đang có khách → thêm món có dị ứng → Gửi bếp (xem phiếu bếp preview đúng) → Thanh toán với cọc 200.000/bill 130.000 thấy banner hoàn 70.000đ → xem hóa đơn K80 preview. KDS nhận ticket giả từ `FakeRealtimeClient`. Đăng nhập từng vai kiểm tra phân quyền hiển thị. Ảnh → `docs/evidence/A4/`. HANDOFF.
