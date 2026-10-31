# Security checklist

## Supabase / PostgreSQL

- [x] Chuỗi kết nối và mật khẩu thật chỉ nằm trong user-secrets hoặc biến môi trường.
- [x] Backend dùng Session pooler cổng 5432, SSL, pool tối đa khoảng 10 và keepalive.
- [x] `Database:AutoMigrate` mặc định `false`, chỉ cho Development và từ chối host `supabase.com`.
- [x] Migration bật RLS trên mọi bảng `public`, kể cả `__EFMigrationsHistory`, không tạo policy Data API.
- [x] Migration thu hồi mọi quyền bảng, sequence và function của `anon`/`authenticated` khi các vai tồn tại.
- [x] Test PostgreSQL tạo vai `anon` giả, xác minh không có quyền SELECT/INSERT và mọi bảng đều có `rowsecurity=true`.
- [ ] Trước khi áp cloud: kiểm tra host được in bởi `scripts/migrate-supabase` rồi nhập đúng câu xác nhận.
- [ ] Sau khi áp cloud: chạy `scripts/verify-supabase-lockdown` bằng publishable key, sau đó kiểm tra trực tiếp extension, exclusion constraint và RLS trong SQL Editor.
- [ ] Đổi mật khẩu tài khoản demo hoặc tắt seed sau lần khởi tạo đầu tiên.
