# Quyết định kiến trúc & điểm cần bạn xác nhận

## A. Kiến trúc
```
Web (React) ──HTTPS/SignalR──┐
WPF (Views) → Client.Core ───┼──► API ASP.NET Core ──► Supabase PostgreSQL (Session pooler :5432, SSL)
Zalo OA ◄──► Webhook/Adapter ┤        │  ├─ BackgroundServices (hết hạn cọc, hold 10', leo thang, quá giờ, no-show)
Gemini ◄──── Adapter ────────┤        │  ├─ SignalR hubs (/hubs/staff, /hubs/public)
VietQR/Bank webhook ─────────┘        │  ├─ Adapters: IMessageChannel, IAiTextService, IPaymentWebhookAdapter, IPaymentQrService
                                      └─► Supabase Storage (ảnh món, logo, nền sơ đồ) qua IFileStorage
Ảnh hiển thị: Web/WPF tải trực tiếp URL công khai CDN của Storage (không cần khóa).
```
Đặc tả nói WPF "kết nối" CSDL; ta cho **WPF gọi API** (không nối thẳng CSDL — khóa Supabase chỉ nằm ở backend) vì đặc tả mục 8.1 đã yêu cầu "API kết nối chung", đồng thời giữ RBAC, khóa bàn và logic AI ở một nơi.

Cấu trúc solution:
```
Restaurant.sln
src/Restaurant.Contracts       DTO, enum, tên event SignalR, mã lỗi (dùng chung Api + Client.Core)
src/Restaurant.Core            Entity, domain service thuần (BufferRule, CandidateRanker, RefundPolicy), use case, interface (port)
src/Restaurant.Infrastructure  EF Core/Npgsql, adapter VietQR/Zalo/Gemini/Supabase Storage, seed
src/Restaurant.Api             Controller, hub, hosted service, DI, Swagger
src/Restaurant.Client.Core     ViewModel, API client, SignalR client (net8.0, test được trên Linux)
src/Restaurant.Wpf             XAML, style, control, converter, render in (net8.0-windows)
web/                           React + Vite + TS
tests/                         Core.Tests, Api.Tests (Testcontainers), ClientCore.Tests
db/ docs/ scripts/
```

## B. Nhật ký quyết định
| # | Quyết định | Lý do |
|---|---|---|
| D1 | .NET 8 + EF Core 8 + Npgsql | LTS, Npgsql EF 8 ổn định; có thể nâng sau |
| D2 | API làm trung tâm; WPF không nối thẳng DB | RBAC, khóa bàn, realtime tập trung |
| D3 | Tách `Client.Core` khỏi `Wpf` | Codex test được ViewModel trên Linux; Antigravity chỉ làm XAML — ranh giới sạch |
| D4 | Trạng thái bàn hiển thị (`displayStatus`) do **server** tính | Web & WPF luôn cùng một màu; xem `docs/03 §1` |
| D5 | 4 trạng thái lưu (AVAILABLE/OCCUPIED/CLEANING/HELD) + RESERVED suy ra | Vàng = có lịch đặt sắp tới, không phải trạng thái vật lý |
| D6 | Mọi tích hợp ngoài qua adapter + Mock | Demo không phụ thuộc mạng/đối tác; test xác định |
| D7 | `timestamptz` lưu UTC, `TimeProvider` | Test đồng hồ giả cho hold 10 phút; Npgsql ép `Kind=Utc` nên ít lỗi múi giờ |
| D8 | Gemini chỉ sinh câu chữ + phân loại ý định | Quyết định bàn phải xác định, kiểm thử được |
| D9 | Khách chọn *loại view/khu vực*, hệ thống tự gán bàn vừa vặn nhất | Khớp kịch bản "đặt Bàn 104" mà khách không chọn số bàn; chọn bàn cụ thể là P2 |
| D10 | **Supabase chỉ dùng làm PostgreSQL + Storage** (không Supabase Auth/Realtime/PostgREST) | Đặc tả đã chọn JWT của backend và SignalR; giảm bề mặt tấn công và phụ thuộc |
| D11 | **Khóa Data API**: bật RLS trên mọi bảng, không policy; backend nối bằng role `postgres` | Publishable key vốn công khai — nếu không bật RLS, ai cũng đọc/ghi được bảng qua REST |
| D12 | **Chống trùng bàn 2 lớp**: `FOR UPDATE` + `EXCLUDE USING gist` (khoảng đóng `[start, start+105']`) | PostgreSQL có ràng buộc loại trừ — lưới an toàn thật sự ở tầng CSDL |
| D13 | Kết nối **Session pooler (cổng 5432)**, SSL bắt buộc, pool nhỏ (~10) | Backend chạy lâu dài, cần prepared statement/transaction; pooler giới hạn số kết nối |
| D14 | **Dev/test trên PostgreSQL local (Docker, Testcontainers)**; Supabase cho staging/demo; migration lên Supabase là lệnh *chủ động*, không tự chạy | Tránh phá dữ liệu cloud; test không phụ thuộc mạng |
| D15 | Ảnh upload **qua backend** (validate) → Storage; bucket công khai chỉ đọc | Web/WPF không giữ khóa; URL ảnh là HTTPS tuyệt đối |

## C. Điểm cần bạn xác nhận (mặc định đã chọn sẵn — sửa trong `docs/03` nếu muốn khác)
1. **Khoảng đệm — biên đóng.** Đặc tả: bàn 201 có lịch 19:00 thì khóa **17:15 → 20:45**, cho phép ≤17:00 và ≥21:00. Ta cài điều kiện `|N − S| ≤ 105 phút` (biên *đóng*) để khớp đúng ví dụ này; với lưới 30 phút nghĩa là hai lượt cùng bàn cách nhau tối thiểu 120 phút. `Booking.SlotStepMinutes = 30`.
2. **Thứ tự xếp hạng ứng viên AI**: VIP trước → cọc sớm hơn → tạo đơn sớm hơn. Đặc tả liệt kê hai tiêu chí mà không nói trọng số.
3. **Sau 10 phút hold, nếu nhân viên cũng không xử lý**: đặc tả im lặng. Mặc định thêm cửa sổ leo thang 10 phút rồi **tự nhả bàn**, giữ bàn cũ cho khách, ghi log — tránh treo bàn vô hạn.
4. **Zalo thực tế**: ZNS thường gửi theo *template đã được Zalo duyệt* (có tham số), còn tin tự do qua OA thường chỉ gửi được khi khách đã quan tâm/tương tác OA gần đây; nhận câu trả lời "Đồng ý" cần webhook của OA. Thiết kế: Gemini sinh nội dung/tham số, `IMessageChannel` quyết định gửi bằng ZNS hay OA; nếu gửi lỗi → **leo thang nhân viên ngay**. Hãy đối chiếu tài liệu hiện hành tại developers.zalo.me trước khi nộp template.
5. **Xác nhận tiền cọc tự động** cần nguồn webhook giao dịch ngân hàng. Mặc định adapter cho SePay (có thể đổi Casso/payOS). QR sinh cục bộ theo chuẩn EMVCo/VietQR, kiểm chứng bằng app ngân hàng thật.
6. **Tiền cọc** 200.000đ, giữ chỗ chờ thanh toán **15 phút**; hủy ≥ 2 giờ trước giờ hẹn được hoàn 100% hoặc đổi Voucher.
7. **VAT**: đặc tả không nhắc → `Billing.VatPercent = 0` (có thể bật).
8. **Giờ mở cửa** mặc định 10:00–22:00, lượt đặt cuối 21:30 — đổi trong `app_settings`.

## D. Supabase — thông số & lưu ý vận hành
Thông số **không bí mật** (từ đặc tả v2): dự án `DootyRes`, project ref `bsedogqcvwlzridkegex`, URL `https://bsedogqcvwlzridkegex.supabase.co`, vùng `ap-northeast-2` (Seoul), host pooler `aws-0-ap-northeast-2.pooler.supabase.com`, cổng **5432** (Session pooler, IPv4), database `postgres`, user `postgres.bsedogqcvwlzridkegex`.

Mẫu chuỗi kết nối Npgsql (đặt trong user-secrets/biến môi trường `ConnectionStrings__Default`, **không** commit):
```
Host=aws-0-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.bsedogqcvwlzridkegex;Password=<DB_PASSWORD>;SSL Mode=Require;Trust Server Certificate=true;Maximum Pool Size=10;Timeout=15;Command Timeout=30;Keepalive=30
```
(`Trust Server Certificate=true` chỉ để dev/đồ án; production nên `SSL Mode=VerifyFull` + chứng chỉ CA của Supabase.)

**Bí mật — hành động cho bạn ngay:**
1. Bản đặc tả v2 chứa **Secret Key (`sb_secret_…`)** và Publishable Key ở dạng chữ rõ. Bản trong kit đã được **che**; bản gốc **đừng commit/chia sẻ**. Vì khóa đã xuất hiện trong tài liệu (và đã được tải lên đây), hãy **xoay (rotate) Secret Key** trong Supabase Dashboard → Settings → API Keys, và đổi mật khẩu DB nếu từng ghi ở đâu đó.
2. Đặc tả gợi ý để khóa trong `appsettings.json` của Backend. Kit **thay bằng** user-secrets/biến môi trường (xem AGENTS.md quy tắc 8) vì `appsettings.json` thường bị commit.
3. Publishable key **không cần dùng** trong kiến trúc này (Web chỉ gọi backend) — đừng nhúng vào Web.

**Rủi ro vận hành cần biết:**
- Máy chủ ở Seoul, người dùng ở Hà Nội ⇒ mỗi truy vấn tốn thêm độ trễ mạng (hàng chục ms). Vì vậy transaction giữ khóa phải **ít vòng truy vấn** (xem skill `postgres-concurrency-booking`) và tránh N+1.
- Gói Free của Supabase thường **tự tạm dừng khi lâu không hoạt động** và có giới hạn kết nối/dung lượng — kiểm tra trạng thái dự án trước buổi demo (đánh thức trước, cân nhắc nâng gói).
- Bucket Storage: `menu-images`, `branding`, `floor-plans` (công khai chỉ đọc; chỉ backend được ghi).
