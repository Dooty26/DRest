# A0 — Design system & khung Web + WPF (Pha 0)

**Skills**: `web-react-ui-conventions`, `wpf-xaml-ui-conventions`, `verify-and-handoff`. **Nguồn**: `docs/04`.
**Điều kiện**: Codex đã xong C0 (có `docs/openapi/openapi.v1.json`, `Client.Core` + Fake). Nếu chưa, bắt đầu phần Web bằng type tạm và đồng bộ sau.

## A. Web (`web/`)
1. Khởi tạo Vite + React 18 + TypeScript + Tailwind + react-router. Script: `dev`, `build`, `lint`, `test`, `gen:api` (openapi-typescript từ `../docs/openapi/openapi.v1.json` → `src/api/schema.d.ts`).
2. **Design tokens** (CSS variables + Tailwind theme): 5 màu trạng thái đúng `docs/04 §2`, bảng màu thương hiệu do bạn đề xuất (ghi lý do + kiểm tra tương phản), font Be Vietnam Pro, bo góc, khoảng cách, bóng; light mode (dark mode là tùy chọn).
3. **Component nền** có a11y: Button, Input, Select, DatePicker/TimePicker, Chip (dị ứng), Badge, `StatusPill`, Card, Modal/BottomSheet, Toast, Skeleton, EmptyState, ErrorState, Stepper, `Countdown` (nhận `expiresAt`), `QrDisplay` (render từ chuỗi `qrPayload` bằng thư viện QR + fallback ảnh), `Money` (`350.000đ`).
4. **Lớp API**: `openapi-fetch` client, xử lý `ProblemDetails` → thông báo tiếng Việt theo `code`, header `Idempotency-Key`, hook `useSignalR` (reconnect, fallback polling).
5. **MSW**: handler cho *toàn bộ* endpoint `/public/*` với dữ liệu thực tế — đặc biệt `availability` trả slot bị khóa đúng quy tắc (bàn 201 có lịch 19:00 → 17:30…20:30 `BOOKED` cho WINDOW). Bật bằng `VITE_USE_MOCK=true`. Có biến `VITE_API_BASE`, `VITE_ENABLE_DEV_TOOLS`.
6. **`ResponsiveImage`** cho ảnh Supabase Storage (URL do API trả): khung tỉ lệ cố định (món 4:3), skeleton khi tải, `onError`/`null` → ảnh placeholder (SVG nội tuyến), `loading="lazy"`, `decoding="async"`, `width/height`, `alt`. CSP/`<meta>` `img-src` cho phép `https://*.supabase.co`. **Không cài `@supabase/*`**; `web/.env.example` chỉ có `VITE_API_BASE`, `VITE_USE_MOCK`, `VITE_ENABLE_DEV_TOOLS`. MSW trả `imageUrl` (một vài URL ảnh mẫu công khai hoặc data-URI) và có món `imageUrl=null` để thử placeholder.
7. Trang `/dev/ui-kit` trình bày mọi component + trạng thái (gồm `ResponsiveImage` ở 3 trạng thái: tải/ok/lỗi).

## B. WPF (`src/Restaurant.Wpf/`)
1. Thêm MaterialDesignThemes; `Styles/Tokens.xaml` (5 màu trạng thái + nền nhạt, brush, cỡ chữ, spacing), `Styles/Theme.xaml`, style cho Button/TextBox/DataGrid/ComboBox/TabControl/ListView, chạm ≥ 44px.
2. **Control tái sử dụng** (`Controls/`): `TableTile` (màu theo `DisplayStatus`, nhãn + biểu tượng, phụ đề, hiển thị đếm ngược khi `HasHold`, nhấp nháy nhẹ khi `HoldPhase=ESCALATED`), `StatusBadge`, `MoneyText`, `NavRail`, `ToastHost`, `DialogHost`, `EmptyState`.
3. **Converter** (`Converters/`): `DisplayStatusToBrush`, `VndConverter`, `TimeSpanToMmSs`, `BoolToVisibility`, `NullToCollapsed`.
4. `ShellWindow` + `ShellView` gắn `ShellViewModel`: thanh trên (tên nhà hàng, đồng hồ, trạng thái kết nối, chuông), thanh điều hướng theo `NavItems`, vùng nội dung bằng `DataTemplate` theo kiểu VM (`ContentControl`).
5. Cài đặt các interface UI trong Client.Core: `WpfUiDispatcher`, `WpfDialogService` (khung), `WpfNotificationSound`, `WpfClipboardService`, `WpfFilePickerService` (`OpenFileDialog` lọc jpg/png/webp), `WpfImageCache` (tải URL về cache đĩa, dùng cho logo in hóa đơn); `WpfPrintService` để dạng khung (hoàn thiện ở A4). Thêm converter **`UrlToImageSourceConverter`** (`BitmapImage` `CacheOption=OnLoad`, `DecodePixelWidth`, `Freeze()`, tải bất đồng bộ, lỗi/`null` → placeholder) và control `RemoteImage`.
6. Trang tạm cho mỗi VM trong `docs/05` (placeholder có tiêu đề) để Shell điều hướng được toàn bộ.

## Nghiệm thu
```
cd web && npm i && npm run lint && npm run build        # xanh
npm run dev   (VITE_USE_MOCK=true)                       # /dev/ui-kit hiển thị đủ component, ảnh chụp light + mobile 390px
dotnet build src/Restaurant.Wpf -p:EnableWindowsTargeting=true   # xanh
dotnet run --project src/Restaurant.Wpf -- --fake        # (Windows) Shell mở, điều hướng được, đổi vai thấy menu đổi
```
Lưu ảnh vào `docs/evidence/A0/`. Cập nhật `docs/04` nếu bạn chốt bảng màu/typography (ghi vào mục mới "Quyết định thiết kế"). HANDOFF.
