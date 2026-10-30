---
name: supabase-postgres-storage
description: Làm việc với Supabase Cloud cho dự án nhà hàng — kết nối PostgreSQL qua Session pooler bằng Npgsql/EF Core, cấu hình SSL/pool, quy trình migration lên Supabase, khóa Data API bằng RLS + REVOKE (bắt buộc), cài btree_gist, Supabase Storage (bucket công khai, upload qua backend, IFileStorage + bản giả cho dev/test), quản lý bí mật (secret key, mật khẩu DB), độ trễ/Free tier. Dùng khi cấu hình DbContext/chuỗi kết nối, viết migration, upload/xóa ảnh món/logo/nền sơ đồ, kiểm tra bảo mật Supabase, seed lên cloud, hoặc khi gặp lỗi kết nối/SSL/pooler/storage.
---

# Supabase = PostgreSQL + Storage (và chỉ vậy)

Không dùng Supabase Auth, Realtime, PostgREST hay SDK phía client. Web và WPF **không bao giờ** giữ khóa Supabase — chỉ gọi backend; ảnh hiển thị bằng URL CDN công khai.

## 1. Bí mật & cấu hình
- Giá trị thật (mật khẩu DB, **Secret Key `sb_secret_…`**) chỉ ở `dotnet user-secrets`, biến môi trường, hoặc `appsettings.*.local.json` đã `.gitignore`. **Cấm** ghi vào `appsettings.json` hay bất kỳ file nào commit. `.env.example` chỉ có giá trị giả.
- Khóa:
  - `ConnectionStrings:Default` (Npgsql), `Supabase:Url`, `Supabase:SecretKey`, `Supabase:Buckets:Menu|Branding|FloorPlans`.
  - Bind bằng `IOptions<SupabaseOptions>` + validate khi khởi động (thiếu khóa ⇒ báo lỗi rõ, không crash mơ hồ); ở Development không có khóa ⇒ tự dùng `LocalFileStorage`.
- Không log chuỗi kết nối/khóa/URL có token; che khi in cấu hình.
- Phát hiện khóa lộ (trong repo/log/chat) ⇒ dừng, báo người dùng **xoay khóa**.

## 2. Kết nối (Session pooler, cổng 5432)
```
Host=aws-0-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<DB_PASSWORD>;
SSL Mode=Require;Trust Server Certificate=true;Maximum Pool Size=10;Timeout=15;Command Timeout=30;Keepalive=30
```
- Username có dạng `postgres.<project-ref>` (khác kết nối trực tiếp).
- **Session pooler** phù hợp backend chạy lâu (hỗ trợ prepared statement/transaction). Tránh Transaction pooler (cổng 6543) trừ khi biết rõ hạn chế.
- Pool nhỏ vì pooler giới hạn số kết nối; đừng mở kết nối song song vô tội vạ; không giữ transaction lâu; `AsNoTracking` + projection + gộp truy vấn (độ trễ Seoul–Hà Nội).
- Dev/production: `SSL Mode=VerifyFull` + chứng chỉ CA của Supabase khi có thể.
- `UseNpgsql(cs, o => o.EnableRetryOnFailure(...))` ⇒ transaction thủ công phải bọc `CreateExecutionStrategy()`.
- `UseSnakeCaseNamingConvention()`; enum lưu **chuỗi** (`HasConversion<string>()`); JSON ↔ `jsonb` (`HasColumnType("jsonb")`).
- Npgsql 6+: `DateTime` ghi vào `timestamptz` phải `Kind=Utc`. Dùng `TimeProvider.GetUtcNow().UtcDateTime`; không `DateTime.Now`, không `Unspecified`.

## 3. Migration
- Dùng **EF Core migrations**; các phần EF không biểu diễn được đưa vào `migrationBuilder.Sql(...)`:
  - `CREATE SCHEMA IF NOT EXISTS extensions; CREATE EXTENSION IF NOT EXISTS btree_gist WITH SCHEMA extensions;`
  - Exclusion constraint `ex_reservations_no_overlap`, các chỉ mục duy nhất từng phần, CHECK.
  - **Khối khóa RLS/REVOKE** ở cuối `db/schema.sql` (đóng gói thành SQL dùng chung, gọi ở mọi migration tạo bảng mới).
- Quy trình: (1) chạy migration trên **PostgreSQL local** (`docker compose up -d postgres`) + test; (2) áp lên Supabase bằng lệnh **chủ động**: `dotnet ef database update --connection "$SUPABASE_CS"` (hoặc `scripts/migrate-supabase.sh|.ps1`) sau khi xác nhận; (3) **không** `Migrate()` tự động ở Production/Supabase. Auto-migrate chỉ bật cho local khi cờ cấu hình `Database:AutoMigrate=true`.
- Seed lên Supabase: seeder idempotent, chỉ dữ liệu gốc; `/dev/seed-demo` bị chặn khi `ASPNETCORE_ENVIRONMENT != Development` **hoặc** khi chuỗi kết nối trỏ tới host `supabase.com` mà chưa bật cờ `Dev:AllowCloudSeed=true`.

## 4. Khóa Data API (bắt buộc, kiểm tra được)
Mọi bảng `public` mặc định truy cập được qua REST bằng publishable key công khai. Vì backend nối bằng role `postgres` (bỏ qua RLS) nên:
- `ALTER TABLE … ENABLE ROW LEVEL SECURITY` cho **mọi** bảng (kể cả `__EFMigrationsHistory`), **không tạo policy**; `REVOKE ALL` cho `anon`, `authenticated`.
- Test tự động: `SELECT count(*) FROM pg_tables WHERE schemaname='public' AND NOT rowsecurity` = 0.
- `scripts/verify-supabase-lockdown.sh|.ps1`: đọc `SUPABASE_URL` + `SUPABASE_PUBLISHABLE_KEY` từ **biến môi trường**, gọi `GET {url}/rest/v1/users?select=*` và `…/reservations` → kỳ vọng bị từ chối/trả rỗng; ghi kết quả vào `docs/SECURITY-CHECKLIST.md`.

## 5. Storage
- Bucket: `menu-images`, `branding`, `floor-plans` — **công khai chỉ đọc**; chỉ backend ghi (bằng Secret Key). Người dùng cuối không có đường ghi trực tiếp.
- Giao diện cổng trong `Core`: `IFileStorage { UploadAsync(bucket, path, stream, contentType) → StoredFile{Url, Path}; DeleteAsync(bucket, path); }`.
  - `SupabaseFileStorage`: gọi REST Storage (`POST {Supabase:Url}/storage/v1/object/{bucket}/{path}`) bằng `HttpClient` có timeout. Khóa `sb_secret_…` **không phải JWT** ⇒ gửi trong header `apikey`; **đọc tài liệu Supabase hiện hành** (Storage API + "API keys") để xác nhận header/cú pháp trước khi viết, hoặc dùng thư viện C# chính thức nếu ổn định. URL công khai: `{Supabase:Url}/storage/v1/object/public/{bucket}/{path}`.
  - `LocalFileStorage` (Development/test): ghi vào thư mục cục bộ và phục vụ qua `/dev-files/...`; test dùng `FakeFileStorage` (bộ nhớ).
- Upload an toàn: kiểm **magic bytes** (JPEG `FF D8 FF`, PNG `89 50 4E 47`, WebP `RIFF….WEBP`), cho phép `jpeg|png|webp`, ≤ `Storage.MaxImageBytes`; tên object ngẫu nhiên `items/{itemId}/{guid}.{ext}` (không dùng tên file người dùng, chặn `..`); tùy chọn giảm cỡ/chuyển WebP (thư viện xử lý ảnh) trước khi lưu; luôn xóa object cũ khi thay ảnh; lưu `image_url` + `image_path`.
- Lỗi Storage không được làm hỏng nghiệp vụ chính: upload lỗi ⇒ `502` rõ ràng, không ghi `image_url` nửa vời.
- Cache: URL công khai ổn định; khi thay ảnh dùng **đường dẫn mới** (không ghi đè) để CDN không giữ ảnh cũ.

## 6. Vận hành
- Gói Free thường **tự tạm dừng khi lâu không hoạt động** và giới hạn kết nối/dung lượng: kiểm tra trạng thái dự án trước demo; health check `/health` kiểm cả DB lẫn Storage.
- Sao lưu: `pg_dump` qua session pooler (script `scripts/backup-db.sh`) + biết cách khôi phục; không chạy trên DB cloud khi chưa cần.
- Dev/test luôn trên PostgreSQL local; không chạy test tải hoặc `TRUNCATE` lên Supabase.

## 7. Checklist
- [ ] Không có khóa/mật khẩu nào trong git (`git grep -nE "sb_(secret|publishable)_|Password="` sạch).
- [ ] RLS bật mọi bảng + test; script lockdown chạy được.
- [ ] Migration áp được trên local và (chủ động) trên Supabase; `btree_gist` hoạt động; constraint chặn trùng.
- [ ] Upload: sai kiểu/quá cỡ/giả mạo magic bytes bị từ chối; thay ảnh xóa ảnh cũ.
- [ ] `IFileStorage` có Local/Fake; test không gọi mạng.
