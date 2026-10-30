# Thay đổi so với kit bản 1 (đặc tả v2: PostgreSQL trên Supabase)

## Kiến trúc & dữ liệu
- MySQL/Pomelo → **PostgreSQL (Supabase Cloud) + Npgsql**. `db/schema.sql` viết lại: `timestamptz`, `numeric`, `jsonb`, identity, CHECK thay enum, 13 bảng lõi + bảng bổ sung + `daily_counters`.
- **Chống trùng bàn 2 lớp**: `FOR UPDATE` + exclusion constraint `btree_gist` trên `tstzrange(start_time, occupied_until, '[]')` (khoảng đóng ⇔ `|N−S| ≤ 105'`). Thêm chỉ mục duy nhất từng phần: 1 order mở/bàn, 1 yêu cầu đổi bàn mở/bàn đích & /đặt bàn, trừ kho SALE không trùng.
- **Supabase chỉ là PostgreSQL + Storage**. Không Supabase Auth/Realtime/PostgREST/SDK client. Web & WPF không giữ khóa.
- **Khóa Data API**: bật RLS mọi bảng, không policy, REVOKE `anon`/`authenticated`; test + script kiểm tra.
- Kết nối Session pooler (5432, SSL, pool nhỏ); dev/test trên PostgreSQL local (Docker/Testcontainers); migration lên Supabase là lệnh chủ động có xác nhận.

## Storage (ảnh)
- `IFileStorage` (Supabase/Local/Fake); upload qua backend, kiểm magic bytes; bucket `menu-images`, `branding`, `floor-plans`.
- Hợp đồng mới (`docs/02`): `POST/DELETE /manage/menu-items/{id}/image`, `…/areas/{id}/floor-plan-image`, `POST /manage/branding/logo`; `logoUrl` trong `/public/config`; `floorPlanImageUrl` trong khu vực.
- Giao diện: `ResponsiveImage` (Web), `RemoteImage` + `UrlToImageSourceConverter` (WPF), upload ảnh món/logo ở `MenuAdminView`/`SettingsView`, ảnh nền sơ đồ, logo trên hóa đơn K80.

## Bảo mật
- Đặc tả v2 gốc chứa Secret Key chữ rõ → bản trong kit đã che; **hãy xoay Secret Key**. Khóa chỉ ở user-secrets/biến môi trường, `.gitignore` + bước CI `git grep` chặn lọt khóa.

## File đổi
Skill: `mysql-concurrency-booking` → `postgres-concurrency-booking` (viết lại); thêm `supabase-postgres-storage`; sửa 8 skill còn lại. Prompt: C0, C1 (viết lại), C2, C3, C6, C9, C-MASTER, A-MASTER, A0, A2, A3, A4. Docs: 00, 01 (thêm mục D), 02, 03, 04, 05, README, AGENTS.md.
