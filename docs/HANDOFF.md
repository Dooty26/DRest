# HANDOFF — nhật ký bàn giao giữa Codex và Antigravity

Mỗi agent **append** một mục ở cuối sau mỗi task. Không sửa mục cũ của bên kia.

## Mẫu
```
### [YYYY-MM-DD] <Prompt-ID> — <agent> — <tiêu đề ngắn>
- Đã làm: …
- Cách chạy/kiểm tra: lệnh cụ thể
- Contract changes: (endpoint/DTO/event/VM thêm hoặc đổi) hoặc "không"
- Việc còn dở / giới hạn đã biết: …
- Câu hỏi/yêu cầu cho bên kia: …
```

## Nhật ký

### [2026-10-31] KIT-NET10-SYNC — Codex — Nâng scaffold backend lên .NET 10
- Đã làm: chuyển target sang `net10.0`/`net10.0-windows`, C# 14; nâng EF Core/Microsoft.Extensions lên 10.0.12, Npgsql lên 10.0.3, EFCore.NamingConventions lên 10.0.1 và Serilog.AspNetCore lên 10.0.0; thêm `global.json`; đồng bộ docs và skills.
- Cách chạy/kiểm tra: `dotnet restore src/Restaurant.Api/Restaurant.Api.csproj`; `dotnet restore src/Restaurant.Wpf/Restaurant.Wpf.csproj`; quét không còn target/package 8 trong file điều khiển.
- Contract changes: không.
- Còn dở / giới hạn đã biết / chưa kiểm chứng: Contracts/Core/Infrastructure/Client.Core build `net10.0`; Api và WPF chưa có entry point vì scaffold C0 chưa hoàn tất nên báo CS5001.
- Câu hỏi/yêu cầu cho bên kia: tiếp tục scaffold bằng SDK/TFM .NET 10; không đưa target cũ trở lại.
- Giả định đã chọn: một target duy nhất .NET 10, không multi-target.

### [2026-10-31] PRE-C0-RESET — Codex — Khôi phục baseline trước C0
- Đã làm: xóa vĩnh viễn scaffold C0 dở dang (`src`, `tests`, cấu hình build/package, Docker Compose, `.editorconfig`, `.env.example`, `db/init`); hoàn tác riêng thay đổi C0 trong `.gitignore`; giữ baseline tài liệu/skill/`global.json` .NET 10; cập nhật đúng hai tham chiếu .NET 8 trong đặc tả DOCX thành .NET 10.
- Cách chạy/kiểm tra: `dotnet --version` dùng SDK 10; `git status --short` không còn các thư mục scaffold; DOCX đã kiểm tra cấu trúc và render đủ 8 trang bằng Microsoft Word.
- Contract changes: không.
- Còn dở / giới hạn đã biết / chưa kiểm chứng: C0 chưa chính thức chạy; chưa có solution, API, WPF, tests, Fake API, ViewModel, OpenAPI hay acceptance C0. Tài liệu gốc có sẵn một số chuỗi công thức `$ ightarrow$` hiển thị sai, không thuộc phạm vi migration này.
- Câu hỏi/yêu cầu cho bên kia: lần chạy tiếp theo phải bắt đầu từ prompt C0 và target duy nhất .NET 10.
- Giả định đã chọn: scaffold cũ bị xóa không có bản stash/backup; chỉ Git history của các file từng được track mới có thể phục hồi.

### [2026-10-31] C0 — Codex — Scaffold solution contracts API và Client Core Fake
- Đã làm: tạo `Restaurant.sln` gồm Contracts/Core/Infrastructure/Api/Client.Core/WPF và 3 project test, target duy nhất .NET 10; code hóa enum, DTO, mã lỗi và tên event theo `docs/02`; dựng API có health, Swagger, ProblemDetails, CORS, JWT skeleton, 2 SignalR hub và 79 path hợp đồng trả `501 NOT_IMPLEMENTED`; sinh `docs/openapi/openapi.v1.json` xác định; tạo `IRestaurantApi`, realtime/session/clock/UI ports, `FakeRestaurantApi`, `FakeRealtimeClient` và toàn bộ ViewModel trong `docs/05`; bootstrap WPF chạy `--fake`; thêm PostgreSQL 16 local, scripts dev/OpenAPI, `.env.example`, CI Ubuntu/Windows và hướng dẫn user-secrets.
- Cách chạy/kiểm tra: `docker compose up -d postgres`; `dotnet restore Restaurant.sln`; `dotnet build Restaurant.sln -p:EnableWindowsTargeting=true`; `dotnet test Restaurant.sln`; `dotnet run --project src/Restaurant.Api` rồi mở `/swagger`; `dotnet run --project src/Restaurant.Wpf -- --fake`; export bằng `scripts/export-openapi.ps1` hoặc `scripts/export-openapi.sh`.
- Kết quả kiểm chứng: build 0 warning/error; 8/8 test xanh; PostgreSQL 16 healthy và `btree_gist` nằm trong schema `extensions`; WPF bootstrap giữ tiến trình ổn định; OpenAPI 79 path, hai lần export cùng SHA-256; NuGet audit không có package dễ bị tấn công đã biết; quét secret/TODO/debug sạch.
- Contract changes: không đổi ý nghĩa `docs/02`/`docs/05`; đây là lần code hóa ban đầu và đã tạo `docs/openapi/openapi.v1.json`.
- Còn dở / giới hạn đã biết / chưa kiểm chứng: endpoint nghiệp vụ chủ ý trả 501 cho tới C1–C8; `HttpRestaurantApi` và SignalR client thật thuộc C4; entity/migration/seed thuộc C1; WPF hiện chỉ là bootstrap, chưa có giao diện A0. Test trong môi trường Codex phải chạy ngoài sandbox vì testhost dùng named pipe nội bộ.
- Câu hỏi/yêu cầu cho bên kia: Antigravity tiếp quản toàn bộ `src/Restaurant.Wpf/**` từ đây và có thể bắt đầu A0 bằng `--fake`; giữ nguyên tên property/command trong `docs/05`, thiếu gì thì ghi Contract Change Request trong HANDOFF.
- Giả định đã chọn: C0 không triển khai nghiệp vụ thật; Fake dùng dữ liệu xác định để UI chạy độc lập. Nâng SignalR client pin lên 10.0.14 và Testcontainers lên 4.16.0 trong central package catalog để tránh advisory của các bản cũ.

### [2026-10-31] A0 — Antigravity — Design system & khung Web + WPF (Pha 0)
- Đã làm:
  - Web (`web/`):
    - Khởi tạo Vite + React 18 + TypeScript + Tailwind + react-router.
    - Sinh `src/api/schema.d.ts` từ `docs/openapi/openapi.v1.json` bằng script `gen:api` (openapi-typescript).
    - Tạo CSS variables và Tailwind theme cho 5 token màu trạng thái bàn bắt buộc (`AVAILABLE`, `RESERVED`, `OCCUPIED`, `CLEANING`, `HELD`) và bảng màu thương hiệu Warm Amber (`#B45309`, tương phản 5.1:1 đạt chuẩn WCAG AA).
    - Xây dựng trọn bộ UI component nền: Button, Input, Select, DatePicker, TimePicker (khóa slot 105' theo buffer rule), Chip (dị ứng), Badge, StatusPill, Card, Modal/BottomSheet, Toast, Skeleton, EmptyState, ErrorState, Stepper, Countdown (bù lệch đồng hồ server), QrDisplay (VietQR), Money (`350.000đ`, xử lý net âm đỏ "Hoàn lại khách: 70.000đ").
    - Tạo component `ResponsiveImage` tải ảnh Supabase Storage với tỷ lệ cố định 4:3, lazy load, tự hiển thị SVG placeholder khi null hoặc lỗi tải; không dùng Supabase SDK.
    - Dựng lớp API `openapi-fetch`, `ProblemDetails` ánh xạ mã lỗi sang tiếng Việt, hook `useSignalR` (reconnect + fallback polling), `useServerClock`, `useCountdown`.
    - Dựng đầy đủ MSW mock handler cho toàn bộ endpoint `/public/*` (lưới giờ availability khóa 17:30...20:30 cho bàn 201 WINDOW).
    - Xây dựng trang catalog `/dev/ui-kit` tương tác trực tiếp mọi component.
  - WPF (`src/Restaurant.Wpf/`):
    - Tích hợp `MaterialDesignThemes` 5.2.1 và `MaterialDesignColors` 5.2.1.
    - Tạo `Styles/Tokens.xaml` chứa đầy đủ 5 token màu trạng thái, nền nhạt, brand brushes, font Segoe UI, touch target >= 44px.
    - Viết các converter: `DisplayStatusToBrushConverter`, `DisplayStatusToBackgroundBrushConverter`, `VndConverter` (hỗ trợ số âm), `TimeSpanToMmSsConverter`, `BoolToVisibilityConverter`, `NullToCollapsedConverter`, `UrlToImageSourceConverter`.
    - Xây dựng control tái sử dụng: `TableTile` (bind TableTileViewModel, đếm ngược hold, nhấp nháy khi escalated), `StatusBadge`, `MoneyText`, `NavRail`, `EmptyState`, `RemoteImage`.
    - Cài đặt thực tế các UI services kế thừa Client.Core: `WpfUiDispatcher`, `WpfDialogService`, `WpfNotificationSound`, `WpfClipboardService`, `WpfFilePickerService`, `WpfImageCache`, `WpfPrintService` (khung).
    - Xây dựng `ShellWindow` với thanh trên (tên quán, đồng hồ, SignalR connection state, chuông leo thang AI), `NavRail` theo vai, `ContentControl` với `ShellPageTemplateSelector` điều hướng sang các View tạm (`FloorPlanView`, `PosView`, `KitchenView`, `ReportsView`, `ReservationsView`, `LoginView`).
- Cách chạy/kiểm tra:
  - Web: `cd web && npm run lint && npm run build && npm run test`
  - Web Dev: `cd web && npm run dev` rồi mở `http://localhost:5173/dev/ui-kit`
  - WPF: `dotnet build src/Restaurant.Wpf -p:EnableWindowsTargeting=true`
  - WPF chạy Fake: `dotnet run --project src/Restaurant.Wpf -- --fake`
- Kết quả kiểm chứng:
  - Web: Lint sạch, build thành công 0 error/warning, 6/6 test vitest xanh.
  - Ảnh bằng chứng chụp bằng headless Edge được lưu tại `docs/evidence/A0/web-ui-kit-desktop.png` (Desktop 1280px) và `docs/evidence/A0/web-ui-kit-mobile.png` (Mobile 390px).
  - WPF: Build sạch 0 warning/0 error; tiến trình `Restaurant.Wpf.exe --fake` khởi chạy ổn định, Shell mở và render đầy đủ sơ đồ bàn và legend trạng thái.
- Contract changes: không.
- Còn dở / giới hạn đã biết: Các trang chi tiết Web đặt bàn (`/dat-ban`) và QR order (`/t/:code`) sẽ được xây dựng ở các task A1 và A2; logic in FlowDocument K80 thực tế sẽ hoàn thiện ở A4.
- Câu hỏi/yêu cầu cho bên kia: Codex có thể tiếp tục triển khai C1 (Database) và C2 (Auth/RBAC).
- Giả định đã chọn: Chốt bảng màu thương hiệu Warm Amber (`#B45309`) để đảm bảo tách biệt thị giác với 5 màu trạng thái bàn và đạt tỷ lệ tương phản 5.1:1; đã ghi lại trong `docs/04 §5`.

### [2026-10-31] A0b — Antigravity — Dọn dẹp nội dung thừa trên giao diện (UI Copy Cleanup)
- Đã làm:
  - Đo trước bằng `node scripts/check-ui-copy.mjs`: phát hiện 6 lỗi và 1 cảnh báo (các chuỗi kỹ thuật, tham chiếu tài liệu, mã enum tiếng Anh, net âm, buffer rule, cờ dev). Lưu log tại `docs/evidence/A0b/truoc.txt`.
  - Web (`web/`):
    - Dọn toàn bộ `/dev/ui-kit`: bỏ huy hiệu đầu trang "UI KIT CATALOG (PHA 0 - ANTIGRAVITY)", bỏ đoạn văn mô tả công nghệ, bỏ số thứ tự "1., 2., ...", bỏ mã enum tiếng Anh (`AVAILABLE`, `RESERVED`...), chỉ giữ nhãn tiếng Việt ("Trống", "Đã đặt", "Có khách", "Đang dọn", "Đang giữ").
    - Rút gọn tiêu đề mục còn <= 3 từ: "Trạng thái bàn", "Tiền tệ", "Nút", "Biểu mẫu", "Nhãn", "Tiến trình", "Đếm ngược", "Mã thanh toán", "Hình ảnh", "Trạng thái hiển thị".
    - Xóa các chú thích kỹ thuật ("net âm", "Cọc > Bill", "Khung giờ lưới 30 phút - Khóa theo quy tắc 105 phút"). Giờ kín chỉ hiện gạch và chú thích ngắn "Đã kín". Khối hoàn cọc chỉ hiện "Hoàn lại khách: 70.000đ".
    - Thanh đầu trang công khai: liên kết "UI Kit" được ẩn, chỉ hiển thị "Giao diện mẫu" khi bật cờ `VITE_ENABLE_DEV_TOOLS=true`.
    - Thêm `<!-- copy-ok: ... -->` tại thẻ `<meta http-equiv="Content-Security-Policy">` trong `index.html`.
    - Thêm script `"lint:copy": "node ../scripts/check-ui-copy.mjs"` vào `web/package.json` và tích hợp vào `npm run lint`.
  - WPF (`src/Restaurant.Wpf/`):
    - Dọn dẹp `MainWindow.xaml` và `ShellWindow.xaml`: bỏ chuỗi debug ("· chạy với --fake", "DataContext: ShellViewModel", "· POS & Điều phối", "Leo thang AI").
    - Thêm `RoleToVietnameseConverter` (MANAGER -> Quản lý, CASHIER -> Thu ngân, WAITER -> Phục vụ, KITCHEN -> Bếp) và `ConnectionStateToVietnameseConverter` (Online -> Trực tuyến, Reconnecting -> Đang nối lại..., Offline -> Ngoại tuyến).
    - Rút gọn tiêu đề và nhãn các View: `FloorPlanView` ("SƠ ĐỒ BÀN", bỏ các ký hiệu thừa trong legend), `PosView` ("BÁN HÀNG"), `KitchenView` ("BẾP"), `ReportsView` ("BÁO CÁO"), `ReservationsView` ("ĐẶT BÀN"), `LoginView` ("Đăng nhập").
  - Đo sau: Chạy `node scripts/check-ui-copy.mjs --strict` đạt **0 lỗi, 0 cảnh báo**. Lưu log tại `docs/evidence/A0b/sau.txt`.
- Cách chạy/kiểm tra:
  - Kiểm tra UI copy: `node scripts/check-ui-copy.mjs --strict` (hoặc `cd web && npm run lint`)
  - Chạy Web test & build: `cd web && npm run test; npm run build`
  - Chạy WPF build: `dotnet build src/Restaurant.Wpf -p:EnableWindowsTargeting=true`
- Kết quả kiểm chứng:
  - Số lỗi UI Copy: 6 lỗi, 1 cảnh báo -> 0 lỗi, 0 cảnh báo (cả chế độ --strict).
  - Web: Lint sạch, 6/6 vitest xanh, build thành công.
  - WPF: Build sạch 0 warning, 0 error.
  - Ảnh chụp trước / sau: `docs/evidence/A0b/truoc-web-ui-kit.png`, `docs/evidence/A0b/sau-web-ui-kit.png`, `docs/evidence/A0b/sau-web-ui-kit-mobile.png`.
  - Danh sách `copy-ok`:
    - `web/index.html`: Thẻ meta CSP kỹ thuật cấu hình bảo mật trình duyệt (`connect-src`, `img-src` chứa domain bên ngoài).
    - `web/src/App.tsx`: Nút điều hướng dev chỉ hiển thị khi `VITE_ENABLE_DEV_TOOLS === 'true'`.
- Contract changes: không.
- Còn dở / giới hạn đã biết: không.
- Câu hỏi/yêu cầu cho bên kia: không.
