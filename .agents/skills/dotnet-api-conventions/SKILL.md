---
name: dotnet-api-conventions
description: Quy ước viết backend ASP.NET Core 8 + EF Core (Npgsql/PostgreSQL trên Supabase) + FluentValidation + ProblemDetails + JWT + Swagger cho dự án nhà hàng. Dùng khi tạo/sửa controller, service, DbContext, migration, DTO, validation, cấu hình DI, logging, xử lý lỗi, JWT/RBAC, Swagger/OpenAPI, hoặc khi dựng solution .NET mới — kể cả khi chỉ nói "viết API", "thêm endpoint", "sửa lỗi backend".
---

# Quy ước .NET API

## Cấu trúc & phụ thuộc (một chiều)
`Contracts` ← `Core` ← `Infrastructure` ← `Api`. `Client.Core` chỉ phụ thuộc `Contracts`.
- `Core`: entity, domain service **thuần** (không EF/HTTP), use-case service, interface cổng (`IClock`, `IMessageChannel`, `IAiTextService`, `IRealtimePublisher`…).
- `Infrastructure`: `RestaurantDbContext` (Npgsql), adapter ngoài (VietQR/Zalo/Gemini/`IFileStorage`), seed.
- `Api`: controller mỏng (validate → gọi use case → map DTO), hub, hosted service.
- Không dùng repository bọc EF (thừa); inject `RestaurantDbContext` vào use-case service là đủ.

## Quy ước mã
- `net8.0`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `Directory.Packages.props` quản lý phiên bản.
- Controller: `[ApiController]`, route `api/v1/...`, trả `ActionResult<T>` có `[ProducesResponseType]` đầy đủ để OpenAPI chính xác; mỗi action có `[Authorize(Policy=…)]` hoặc `[AllowAnonymous]` **tường minh**.
- DTO nằm ở `Contracts` (record, immutable). Không trả entity EF ra ngoài.
- Validation: FluentValidation đăng ký tự động → lỗi thành `ValidationProblemDetails` + `code=VALIDATION_ERROR`.
- Lỗi nghiệp vụ: ném `DomainException(code, message, httpStatus)` → middleware chuyển thành `ProblemDetails` có `code`. Không dùng exception cho luồng thường; không nuốt exception.
- Async xuyên suốt, truyền `CancellationToken`. Không `.Result/.Wait()`.
- Thời gian: chỉ qua `TimeProvider`/`IClock`. Không `DateTime.Now`.
- Truy vấn đọc: `AsNoTracking()` + `Select` ra DTO; tránh N+1 (`Include` có chủ đích hoặc projection).
- Giao dịch: dùng `Database.BeginTransactionAsync` cho thao tác nhiều bước; nếu bật `EnableRetryOnFailure` thì bọc bằng `CreateExecutionStrategy().ExecuteAsync`. DB ở cloud (Supabase, Seoul) ⇒ mỗi truy vấn có độ trễ: gộp truy vấn, projection, tránh N+1, giữ transaction ngắn.
- Npgsql: `DateTime` ghi vào `timestamptz` phải `Kind=Utc`; `UseSnakeCaseNamingConvention()`; enum lưu chuỗi; JSON ↔ `jsonb`; bắt lỗi qua `PostgresException.SqlState` (xem skill `postgres-concurrency-booking`). Cấu hình/khóa Supabase: skill `supabase-postgres-storage`.
- Cấu hình: `IOptions<T>` + validate lúc khởi động; bí mật (khóa Supabase, mật khẩu DB…) qua user-secrets/biến môi trường — **không** ở `appsettings.json`; cập nhật `.env.example` (giá trị giả).
- Log: Serilog có cấu trúc (`{ReservationCode}`, `{RequestId}`), không log mật khẩu/token/SĐT đầy đủ (che 4 số giữa).
- OpenAPI: sau mỗi thay đổi endpoint chạy `scripts/export-openapi.sh` và commit file.

## Bảo mật mặc định
JWT: khóa ≥ 32 byte từ secret, `ValidateLifetime`, `ClockSkew ≤ 1 phút`. Mật khẩu BCrypt (work factor ≥ 11). Rate limiting (`AddRateLimiter`) cho login, public API, webhook. CORS theo danh sách origin cấu hình. `/dev/*` chỉ map khi `IsDevelopment()`. SQL thô luôn tham số hóa (`FromSqlInterpolated`).

## Mẫu test
- Domain thuần: xUnit + FluentAssertions, bảng giá trị (`[Theory]`).
- API: `WebApplicationFactory<Program>` + Testcontainers **PostgreSQL** (`PostgreSqlBuilder`, image `postgres:16`), seed tối thiểu mỗi test, `FakeTimeProvider`, `FakeFileStorage`. Test không bao giờ trỏ vào Supabase cloud.
- Ma trận quyền: `[Theory]` vai × endpoint.
