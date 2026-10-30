# C1 — CSDL PostgreSQL (Supabase), EF Core/Npgsql, khóa RLS, seed (Pha 1)

**Skills**: `supabase-postgres-storage`, `postgres-concurrency-booking`, `dotnet-api-conventions`, `restaurant-domain-rules`, `verify-and-handoff`.
**Mục tiêu**: hiện thực `db/schema.sql` bằng EF Core code-first (Npgsql), migration chạy sạch trên PostgreSQL **local** rồi áp (chủ động) lên Supabase; dữ liệu seed đủ cho demo; Data API của Supabase bị khóa.
**Lưu ý bí mật**: bạn sẽ không được cấp khóa thật. Chỉ dùng giá trị giả trong `.env.example`; người dùng tự đặt khóa thật vào user-secrets. Nếu thấy khóa thật ở bất kỳ file nào → dừng và báo.

## Việc cần làm
1. **Entity + `IEntityTypeConfiguration<>`** cho *toàn bộ* bảng trong `db/schema.sql` (13 bảng lõi + bảng bổ sung): `UseSnakeCaseNamingConvention()`; enum lưu **chuỗi**; `jsonb` cho `allergies`/`raw_payload`/`detail`; `timestamptz` (mọi `DateTime` ghi vào phải `Kind=Utc` — thêm `ValueConverter`/kiểm tra để lỗi `Unspecified` bị bắt sớm); `numeric(12,0)`; identity `BY DEFAULT`; `xmin` làm concurrency token cho `reservations` (`uint Version` + `IsRowVersion()`); mọi CHECK, chỉ mục thường và **chỉ mục duy nhất từng phần** như schema.
2. **Migration `Initial`** gồm cả phần EF không tự sinh, bằng `migrationBuilder.Sql(...)`:
   - `CREATE SCHEMA IF NOT EXISTS extensions; CREATE EXTENSION IF NOT EXISTS btree_gist WITH SCHEMA extensions;`
   - Exclusion constraint `ex_reservations_no_overlap` (`gist`, `tstzrange(start_time, occupied_until, '[]')`, `WHERE status IN (...)`).
   - **Khối khóa Data API**: bật RLS mọi bảng `public` (kể cả `__EFMigrationsHistory`), không policy, `REVOKE ALL` cho `anon`/`authenticated` (bọc `IF EXISTS` vai trò). Đóng gói thành helper SQL dùng lại ở **mọi** migration tạo bảng mới.
   Ghi sai lệch so với schema (nếu có) vào `docs/DB-DEVIATIONS.md`.
3. **Cấu hình kết nối**: `AddRestaurantDb(configuration)` đọc `ConnectionStrings:Default`; `EnableRetryOnFailure` (nhớ bọc `CreateExecutionStrategy()` cho transaction thủ công); `Database:AutoMigrate` (mặc định `false`; `true` chỉ cho local). Chuỗi kết nối mẫu Session pooler ở `docs/01 §D` (SSL Require, pool ≈ 10, keepalive) — **chỉ đặt ở `.env.example` với mật khẩu giả**. `docker-compose.yml` (C0) cung cấp PostgreSQL 16 local.
4. **Script migrate chủ động**: `scripts/migrate-supabase.sh|.ps1` — đọc chuỗi kết nối từ biến môi trường `SUPABASE_CS`, in **host đích (che mật khẩu)** và hỏi xác nhận trước khi chạy `dotnet ef database update`.
5. **`DbSeeder` idempotent** (chạy lại không nhân bản):
   - `roles` 4 vai; `app_settings` đủ key + mặc định ở `docs/03 §0`.
   - Khu: Tầng 1, Tầng 2, Tầng 3, Ngoài trời. Bàn ~20: Tầng 1 bàn thường (T101–T108, vài bàn 2/4/6 ghế); **Tầng 2 `WINDOW`** (T201–T206; T201 = 4 ghế view cửa sổ); Tầng 3 `BALCONY` (T301–T304) và `VIP` phòng kín (V01–V02); Ngoài trời `OUTDOOR`. Có tọa độ/cỡ hợp lý để vẽ sơ đồ; `qr_secret` ngẫu nhiên (CSPRNG).
   - `categories` + ~20 món tiếng Việt giá thực tế (có **Bò bít tết**); `image_url` để `NULL` (ảnh thật do Manager upload hoặc script `scripts/seed-images` tùy chọn); không hard-code URL Storage thật.
   - `ingredients` (thịt bò, khoai tây, phô mai, sữa, cà phê hạt…; đơn vị gốc g/ml/pcs + hiển thị kg/l/quả/lon) và `recipes` (Bò bít tết = 200g thịt bò + 50g khoai tây; ≥ 8 món).
   - `users`: `manager`, `cashier`, `waiter`, `kitchen` — mật khẩu demo từ cấu hình (không hard-code), **BCrypt**, cảnh báo đổi mật khẩu khi chạy.
   - `customers` mẫu: Anh Long `0987654321` (có `zalo_user_id` giả).
6. Tiện ích: `IClock` dựa `TimeProvider`; helper UTC ↔ giờ VN (`Asia/Ho_Chi_Minh` / `SE Asia Standard Time`).
7. **Test tích hợp (Testcontainers PostgreSQL, image `postgres:16`)**:
   - Migration áp được; seeder idempotent; round-trip `jsonb` và thời gian UTC; ghi `DateTime Unspecified` bị từ chối.
   - **Exclusion constraint**: insert thẳng hai reservation hiệu lực chồng nhau trên cùng bàn ⇒ `23P01`; biên: S=19:00 → 17:00 ✔, 17:15 ✖, 20:45 ✖, 21:00 ✔; trạng thái `CANCELLED/EXPIRED` không chiếm chỗ.
   - Chỉ mục duy nhất từng phần: 2 order mở/bàn ✖; 2 yêu cầu đổi bàn mở cùng bàn đích ✖; trừ kho SALE trùng ✖.
   - **RLS**: tạo vai `anon` giả trong container → không SELECT/INSERT được bảng nào; test "mọi bảng `public` có `rowsecurity = true`".
8. `scripts/verify-supabase-lockdown.sh|.ps1` (xem skill) và ghi mục "Supabase" vào `docs/SECURITY-CHECKLIST.md`.

## Nghiệm thu
`docker compose up -d postgres && dotnet test` xanh · `Database:AutoMigrate=true dotnet run --project src/Restaurant.Api` tạo DB + seed trên local · SQL `SELECT count(*)` khớp kỳ vọng · `git grep -nE "sb_(secret|publishable)_|Password=[^<]"` sạch (chỉ giá trị giả). **Chưa** áp lên Supabase nếu người dùng chưa xác nhận: ghi trong HANDOFF đúng lệnh `scripts/migrate-supabase` và checklist kiểm tra sau khi áp (extension, constraint, RLS). HANDOFF nêu tài khoản demo, cách reset DB local.
