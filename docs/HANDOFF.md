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
