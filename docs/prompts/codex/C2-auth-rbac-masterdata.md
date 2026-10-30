# C2 — Xác thực, phân quyền RBAC, CRUD dữ liệu gốc (Pha 1)

**Skills**: `dotnet-api-conventions`, `supabase-postgres-storage`, `restaurant-domain-rules`, `verify-and-handoff`.

## Việc cần làm
1. **Đăng nhập**: `POST /auth/login` (BCrypt verify, JWT 8h chứa `sub`, `name`, `role`), `GET /auth/me`. Khóa tạm tài khoản sau 5 lần sai (`failed_login_count`, `locked_until`), rate-limit endpoint login, không tiết lộ "sai user hay sai mật khẩu". Khóa ký JWT từ secret, kiểm `ValidateLifetime`, clock skew nhỏ.
2. **Chính sách quyền** đúng ma trận `docs/03 §13`: policy theo vai (`RequireManager`, `CashierOrAbove`, `OrderTakers`, `KitchenOrManager`…). Áp `[Authorize(Policy=…)]` cho **mọi** endpoint staff trong `docs/02`; hub staff yêu cầu JWT (token qua query `access_token` cho WebSocket).
3. **CRUD Manager**: `areas`, `tables` (+ `PUT …/layout`), `categories`, `menu_items` (+ `availability`), `users` (đổi vai, khóa/mở, đặt lại mật khẩu — băm BCrypt), `settings` (validate kiểu/khoảng giá trị, cache + vô hiệu hóa cache khi sửa). `GET /public/config` và `GET /public/menu` hoàn chỉnh.
4. **QR bàn**: chữ ký `HMAC-SHA256(table.qr_secret, tableCode)` (base64url, rút gọn 16 byte); `GET /manage/tables/{id}/qr` trả `{ url, pngBase64 }` với `url = {Tables.QrBaseUrl}/t/{code}?sig=…`. Hàm `QrSigner.Verify` so sánh hằng thời gian.
5. **Audit log** cho thao tác quản trị (đổi giá, đổi vai, đổi cấu hình, xóa).
6. Validation (FluentValidation hoặc tương đương) + ProblemDetails `VALIDATION_ERROR`.
7. **Upload ảnh lên Supabase Storage** (Manager): `IFileStorage` + `SupabaseFileStorage` (Secret Key từ cấu hình/secret; xác nhận cách gọi REST trong **tài liệu Supabase hiện hành**), `LocalFileStorage` (Development) và `FakeFileStorage` (test). Endpoint: `POST/DELETE /manage/menu-items/{id}/image`, `POST/DELETE /manage/areas/{id}/floor-plan-image`, `POST /manage/branding/logo` (đúng `docs/02 §3`). Bắt buộc: chỉ `jpeg|png|webp`, kiểm **magic bytes**, ≤ `Storage.MaxImageBytes`, tên object ngẫu nhiên, thay ảnh ⇒ xóa object cũ, lưu `image_url` + `image_path`, lỗi Storage ⇒ `502` không ghi dữ liệu nửa vời. `GET /public/menu` và `/public/config` trả `imageUrl`/`logoUrl` đúng. Health check kiểm Storage (bỏ qua ở Development local).

## Test bắt buộc
- **Ma trận vai × endpoint** (data-driven): mỗi endpoint staff cho từng vai → 200/204 hoặc 403; không token → 401.
- Login: đúng/sai, khóa tài khoản, token hết hạn. `QrSigner`: đúng/sai/bị sửa mã bàn.
- Upload: đúng kiểu; sai đuôi nhưng nội dung giả (magic bytes) ✖; quá cỡ ✖; vai không phải Manager ✖; thay ảnh xóa ảnh cũ (kiểm trên `FakeFileStorage`); lỗi Storage ⇒ 502 và DB không đổi; tên file người dùng chứa `../` không ảnh hưởng đường dẫn.

## Nghiệm thu
`dotnet test` xanh; export lại OpenAPI; HANDOFF nêu tài khoản demo & cách lấy token để Antigravity thử.
