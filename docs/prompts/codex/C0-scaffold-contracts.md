# C0 — Khung solution, hợp đồng, Fake (Pha 0)

**Vai**: Codex. **Skills**: `dotnet-api-conventions`, `supabase-postgres-storage`, `restaurant-domain-rules`, `wpf-client-core-mvvm`, `verify-and-handoff`.
**Mục tiêu**: có solution build xanh trên Linux và Windows, hợp đồng (DTO/enum/event) đã code hóa, OpenAPI export được, và `Client.Core` chạy hoàn chỉnh bằng dữ liệu giả để Antigravity bắt đầu làm giao diện **ngay**.

## Việc cần làm
1. **Solution** `Restaurant.sln` theo `docs/01 §A`: `Contracts`, `Core`, `Infrastructure`, `Api`, `Client.Core` (`net10.0`); `Wpf` (`net10.0-windows`, `UseWPF`, `EnableWindowsTargeting=true` trong `Directory.Build.props`); tests `Core.Tests`, `Api.Tests`, `ClientCore.Tests`. Bật nullable, `TreatWarningsAsErrors`, `.editorconfig`, `Directory.Packages.props` (quản lý phiên bản tập trung; EF Core/Npgsql/ASP.NET-related cùng major 10, không dùng wildcard). Dùng `global.json` ở gốc repo để khóa dòng SDK 10 với `rollForward: latestFeature`; CI chỉ build/test .NET 10 và Windows runner phải build được WPF.
2. **`Restaurant.Contracts`**: code hóa *toàn bộ* enum, DTO (record, `System.Text.Json` camelCase, enum dạng chuỗi), `HubEvents` (hằng tên event), `ErrorCodes`, `ProblemDetails` mở rộng (`code`, `errors`, `warnings`) đúng `docs/02`.
3. **`Api` khung**: Serilog, health `/health`, Swagger/Swashbuckle, ProblemDetails toàn cục, CORS cho `http://localhost:5173`, cấu hình JWT (chưa cần logic), 2 hub rỗng `/hubs/staff` & `/hubs/public`, **mọi endpoint trong `docs/02` là controller có chữ ký đúng** trả `501 NOT_IMPLEMENTED` (để OpenAPI đầy đủ ngay).
4. **Export OpenAPI**: `scripts/export-openapi.sh` (+ `.ps1`) ghi `docs/openapi/openapi.v1.json`; thêm kiểm tra CI "file OpenAPI đã cập nhật".
5. **`Client.Core`**:
   - `IRestaurantApi` bao phủ mọi endpoint staff cần cho WPF; `IRealtimeClient`; `ISessionStore`; `IClock`.
   - `FakeRestaurantApi` + `FakeRealtimeClient`: dữ liệu mẫu **phong phú**: 3 khu (Tầng 1/2/3) ~20 bàn đủ 5 trạng thái (có bàn HELD với `expiresAt = now+10'`, bàn ESCALATED, bàn OCCUPIED có tạm tính, bàn RESERVED có giờ), thực đơn tiếng Việt ~20 món (1 món hết), 1 đơn đặt bàn "Anh Long 0987654321", 1 yêu cầu leo thang, kho vài nguyên liệu (1 thấp), báo cáo mẫu. `FakeRealtimeClient` phát event theo kịch bản hẹn giờ (đơn QR mới, hết hạn hold → leo thang).
   - **Toàn bộ ViewModel theo `docs/05`** có đủ property/command, hoạt động trọn vẹn với Fake (bấm lệnh → trạng thái đổi hợp lý).
   - Interface dịch vụ UI (`IUiDispatcher`, `IDialogService`, `IPrintService`, `INotificationSound`, `IClipboardService`) + bản test double.
   - `ClientCoreModule.AddClientCore(IServiceCollection, bool fake)`.
6. **`Restaurant.Wpf` (ngoại lệ được phép — chỉ bootstrap)**: csproj, `App.xaml(.cs)` dựng DI/Host, đọc `--fake`, `appsettings.json` (`Api:BaseUrl`), 1 `MainWindow` tạm hiện tên VM. Ghi rõ trong HANDOFF: "Từ đây Antigravity sở hữu thư mục này".
7. **Hạ tầng dev**: `docker-compose.yml` (**PostgreSQL 16**, volume, healthcheck, script init tạo schema `extensions`), `.env.example` (chỉ giá trị GIẢ: `ConnectionStrings__Default`, `Supabase__Url`, `Supabase__SecretKey`, `Supabase__Buckets__*`, `Database__AutoMigrate`), `scripts/dev.sh|.ps1`, `.gitignore` (**bắt buộc** chặn `.env`, `*.local.json`, `appsettings.*.local.json`, `secrets.json`), cấu hình `appsettings.json` **không chứa bí mật**, hướng dẫn `dotnet user-secrets init/set` trong README, GitHub Actions (ubuntu: dịch vụ PostgreSQL/Testcontainers + build/test với `EnableWindowsTargeting`; windows: build `Restaurant.Wpf`; thêm bước `git grep` chặn khóa Supabase lọt vào repo).
8. `docs/openapi/` + cập nhật `docs/HANDOFF.md` (cách chạy `--fake`, danh sách VM, quy ước DI).

## Ràng buộc
Khai báo sẵn các interface cổng `IFileStorage` (Storage) cùng `FakeFileStorage`; endpoint upload ảnh trong `docs/02` có chữ ký đúng và trả `501`. Không cài logic nghiệp vụ thật (đó là C1…C8). Không thêm thư viện ngoài danh sách AGENTS.md mục 3 mà không ghi lý do. Tên trong Contracts khớp `docs/02` từng chữ.

## Nghiệm thu (tự chạy và dán kết quả)
```
dotnet build Restaurant.sln -p:EnableWindowsTargeting=true     # 0 warning/error
dotnet test Restaurant.sln                                      # xanh; ClientCore.Tests kiểm tra VM với Fake
docker compose up -d postgres                                   # healthy
dotnet run --project src/Restaurant.Api                         # /swagger hiện đủ endpoint, 501 NOT_IMPLEMENTED
scripts/export-openapi.sh && git diff --exit-code docs/openapi  # không đổi sau lần chạy thứ 2
```
Cuối: append HANDOFF (đã làm · cách chạy · contract · việc dở · câu hỏi cho Antigravity).
