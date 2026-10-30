# A3 — WPF: Đăng nhập, Shell, Sơ đồ bàn thời gian thực (Pha 1)

**Skills**: `wpf-xaml-ui-conventions`, `verify-and-handoff`. **Nguồn**: đặc tả 5.1, `docs/05` (`LoginViewModel`, `ShellViewModel`, `FloorPlanViewModel`, `TableTileViewModel`, `TableDetailViewModel`), `docs/04`.
Chạy bằng `--fake`; sang API thật sau khi Codex xong C4 (chỉ cần bỏ `--fake`).

## Việc cần làm
1. **`LoginView`**: logo/tên nhà hàng, tên đăng nhập, mật khẩu (`PasswordBox` + attached behavior đẩy vào `Password`), nút đăng nhập có trạng thái bận, hiển thị `ErrorMessage`, Enter để gửi.
2. **Hoàn thiện Shell** (từ A0): khi chưa đăng nhập chỉ hiện Login; sau đăng nhập hiện nav theo vai; chỉ báo `ConnectionState` (chấm xanh/vàng/đỏ + chữ), chuông thông báo, đồng hồ.
3. **`FloorPlanView`**:
   - Tab theo **khu vực** (Tầng 1/2/3/Ngoài trời) bằng `TabControl`/`ListBox`; mỗi tab là `ItemsControl` với `Canvas` làm `ItemsPanel`; nếu khu có `FloorPlanImageUrl` thì hiển thị **ảnh mặt bằng làm nền** canvas (`RemoteImage`, `Stretch=Uniform`, mờ nhẹ để ô bàn vẫn đọc rõ; lỗi/null → nền trơn), ô bàn đặt theo `X/Y/Width/Height` từ `TableTileViewModel` (`ItemContainerStyle` gắn `Canvas.Left/Top`).
   - **`TableTile`** theo 5 trạng thái đúng màu/nhãn/biểu tượng; hiển thị **Subtitle** (giờ khách đến / thời gian ngồi + tạm tính); **Tím**: viền nổi + đồng hồ đếm ngược `HoldRemainingText`, đổi màu cảnh báo < 60s, nhấp nháy nhẹ khi `ESCALATED`; huy hiệu quá giờ khi `HasOverstay`.
   - **Legend** có số lượng mỗi trạng thái (`LegendCounts`) và bộ lọc theo trạng thái.
   - Chọn ô → **panel chi tiết** bên phải (`TableDetailViewModel`): thông tin đặt bàn kế tiếp, order hiện tại, các nút theo quyền (Mở bàn, Check-in, Báo dọn xong, Đổi bàn, Ghép bàn, Cập nhật số khách, Mở POS). Nút bị ẩn/vô hiệu theo `Can…`.
   - **Kéo–thả ghép bàn**: attached behavior kéo ô này thả lên ô kia → gọi `MergeTablesCommand` sau hộp thoại xác nhận (`IDialogService`). Đổi bàn: menu ngữ cảnh/hộp thoại chọn bàn đích.
   - Mở bàn: hộp thoại nhập số khách; nếu API trả cảnh báo `WALKIN_CONFLICT` → hộp thoại cảnh báo màu vàng với nút "Vẫn mở" (gửi `force`).
   - Chế độ **chỉnh sơ đồ** (`IsEditMode`, chỉ Manager): kéo di chuyển, kéo góc đổi cỡ, "Lưu bố cục" (P2 — làm sau cùng).
4. **Hiệu năng & mượt**: ảo hóa/không dựng lại toàn bộ khi một ô đổi; animation nhẹ; không chặn UI thread.
5. **Design-time data** cho mọi View (`d:DataContext`), để thiết kế được trong Visual Studio.

## Nghiệm thu (Windows)
`dotnet run --project src/Restaurant.Wpf -- --fake`: đăng nhập từng vai thấy menu khác nhau; thấy đủ 5 màu trên sơ đồ; bàn Tím đếm ngược chạy mỗi giây; kéo ghép bàn có hộp thoại xác nhận; cửa sổ co giãn 1366×768 → 1920×1080 không vỡ bố cục. Ảnh từng trạng thái → `docs/evidence/A3/`. HANDOFF (nêu VM property cần thêm nếu có).
