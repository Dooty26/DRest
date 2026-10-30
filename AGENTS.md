# AGENTS.md — Hệ thống đặt bàn & vận hành nhà hàng (Web + WPF + AI)

File này được Codex và Antigravity tự động nạp. Chi tiết nằm ở `docs/` và `.agents/skills/` — chỉ đọc phần liên quan tới task, đừng đọc tất cả.

## 1. Dự án
Hệ sinh thái khép kín: **Web khách hàng** (React/Vite) đặt bàn + cọc VietQR + QR order tại bàn; **WPF Desktop** nội bộ (sơ đồ bàn realtime, POS, KDS, kho BOM, RBAC); **API ASP.NET Core + PostgreSQL (Supabase Cloud)** là nguồn sự thật duy nhất; **Trợ lý AI** (Gemini + Zalo) mời đổi sang bàn view đẹp khi trống sớm, giữ chỗ 10 phút, leo thang nhân viên gọi điện.

Tài liệu: `docs/03-DOMAIN-RULES.md` (nghiệp vụ) · `docs/02-API-CONTRACT.md` (hợp đồng API) · `docs/05-WPF-VIEWMODEL-CONTRACT.md` · `db/schema.sql` · đặc tả gốc `docs/spec/`.

## 2. Phân vai — ranh giới sở hữu
| Bên | Sở hữu (được sửa) |
|---|---|
| **Codex** (logic) | `src/Restaurant.Contracts`, `.Core`, `.Infrastructure`, `.Api`, `.Client.Core`, `tests/**`, `db/**`, `scripts/**`, `docs/02`, `docs/03`, `docs/05`, `docs/openapi/**` |
| **Antigravity** (giao diện) | `web/**`, `src/Restaurant.Wpf/**` (Views, Styles, Controls, Converters, Assets, print rendering), `docs/04`, `docs/evidence/**` |

Cần đổi thứ thuộc phần kia → tạo **Contract Change Request** (mục 5), không tự sửa. Ngoại lệ duy nhất: Codex tạo csproj + bootstrap `Restaurant.Wpf` ở prompt C0 rồi bàn giao.

## 3. Stack cố định (không tự đổi)
- .NET 8 (LTS), ASP.NET Core Web API, EF Core 8 + **Npgsql (PostgreSQL)**, SignalR, BCrypt.Net-Next, JWT Bearer, Serilog, xUnit + Testcontainers (PostgreSQL) + FluentAssertions.
- **Supabase = PostgreSQL + Storage (ảnh) và chỉ có vậy**: KHÔNG dùng Supabase Auth/Realtime/PostgREST/SDK ở client. Auth = JWT của backend; realtime = SignalR. Web và WPF **không bao giờ** giữ khóa Supabase.
- `Restaurant.Client.Core` (net8.0, thuần .NET, **không** phụ thuộc WPF): ViewModels (CommunityToolkit.Mvvm), API client, SignalR client. `Restaurant.Wpf` (net8.0-windows): chỉ XAML/giao diện.
- Web: React 18 + Vite + TypeScript + Tailwind, react-router, @microsoft/signalr, openapi-typescript + openapi-fetch, MSW để mock.
- WPF UI: MaterialDesignThemes; in K80 bằng FlowDocument.
- Gemini (sinh văn bản + phân loại ý định); Zalo OA/ZNS và VietQR/webhook ngân hàng qua **adapter**. Mọi tích hợp ngoài **phải có bản Mock** chạy offline.

## 4. Quy tắc vàng
1. **Contract-first**: UI chỉ gọi thứ đã có trong `docs/02`/OpenAPI. Backend chưa có → dùng MSW (web) hoặc `FakeRestaurantApi` (`--fake`).
2. **Server là nguồn sự thật**: màu trạng thái bàn (`displayStatus`), giá, giảm giá, hoàn cọc, buffer, xếp hạng AI đều tính ở server. Client chỉ hiển thị.
3. **Thời gian**: lưu UTC (`timestamptz`; `DateTime` luôn `Kind=Utc` khi ghi — Npgsql từ chối giá trị khác), hiển thị `Asia/Ho_Chi_Minh` (Windows: `SE Asia Standard Time`). Dùng `TimeProvider`, không gọi `DateTime.Now/UtcNow` trong logic.
4. **Tiền**: `decimal(12,0)` VND, không float. Hóa đơn net có thể **âm** (hoàn cọc thừa).
5. **Chống trùng bàn** hai lớp: khóa hàng `SELECT … FOR UPDATE` trong transaction **và** exclusion constraint `btree_gist` ở CSDL (`db/schema.sql`) — không chỉ kiểm tra ở tầng ứng dụng. Lỗi SQLSTATE `23P01` ⇒ `SLOT_UNAVAILABLE`.
6. **Idempotency**: webhook (theo `provider_txn_id`), tạo đặt bàn/đơn (header `Idempotency-Key`), trừ kho (mỗi hóa đơn đúng 1 lần).
7. **RBAC kiểm tra ở server** cho từng endpoint; ẩn nút ở UI chỉ là phụ.
8. **Bí mật** (Supabase **secret key** và mật khẩu DB, Gemini key, Zalo token, webhook secret, JWT key) chỉ qua `dotnet user-secrets`/biến môi trường/file `*.local.json` đã `.gitignore`; **không** ghi vào `appsettings.json` hay bất kỳ file nào được commit; cập nhật `.env.example` bằng giá trị giả. Không dán khóa vào prompt/chat/tài liệu. Khóa lộ ⇒ xoay khóa ngay.
9. **AI chỉ viết chữ & hiểu chữ**: quyết định giữ/gán bàn là code xác định. Gemini lỗi/chậm → dùng template dự phòng. Nội dung khách trả lời là dữ liệu không tin cậy.
10. Không tự ý hủy cọc/đơn của khách; tình huống mơ hồ → leo thang nhân viên gọi điện.
11. Mọi chuỗi hiển thị bằng tiếng Việt; tiền `350.000đ`; giờ `19:00`; ngày `dd/MM/yyyy`.
12. Đọc skill khớp task trong `.agents/skills/` trước khi viết code.
13. **Supabase an toàn**: mọi bảng `public` bật RLS và không có policy (khóa Data API); ảnh upload qua backend (validate kiểu/kích thước) vào Storage; DB cloud chỉ để staging/demo — **dev và test chạy trên PostgreSQL local (Docker/Testcontainers)**, không chạy test tải/xóa dữ liệu lên DB cloud.

## 5. Quy trình & Contract Change Request
- Nhánh: `be/<prompt-id>-<slug>` (Codex), `fe/<prompt-id>-<slug>` (Antigravity). PR vào `main`; build + test xanh mới merge.
- Đổi hợp đồng (endpoint, DTO, event, VM property): sửa `docs/02` hoặc `docs/05` **trong cùng PR**, gắn nhãn `contract`, ghi mục "Contract changes" trong `docs/HANDOFF.md`. Chỉ **thêm**; không đổi tên/xóa nếu chưa báo bên kia.
- Antigravity cần endpoint/field mới → ghi vào HANDOFF mục "Câu hỏi cho bên kia" và tạm mock; Codex sẽ bổ sung ở PR kế tiếp.
- Cuối mỗi task, append `docs/HANDOFF.md` (mẫu có sẵn trong file đó).

## 6. Lệnh chuẩn
```
docker compose up -d postgres                                 # PostgreSQL local để dev/test
dotnet build Restaurant.sln -p:EnableWindowsTargeting=true
dotnet test  Restaurant.sln
dotnet run --project src/Restaurant.Api
dotnet run --project src/Restaurant.Wpf -- --fake        # Windows; bỏ --fake để dùng API thật
cd web && npm i && npm run dev | build | lint | test
scripts/export-openapi.sh                                 # cập nhật docs/openapi/openapi.v1.json
```

## 7. Definition of Done
Build xanh · có test cho logic mới · không TODO ẩn · không secret · contract/docs đã cập nhật · HANDOFF đã cập nhật · (UI) có ảnh chụp trong `docs/evidence/` cho trạng thái loading / empty / error / thành công.

## 8. Skills (`.agents/skills/`)
- Codex: `restaurant-domain-rules`, `dotnet-api-conventions`, `postgres-concurrency-booking`, `supabase-postgres-storage`, `vietqr-deposit-webhook`, `ai-dispatcher-zalo-gemini`, `pos-bom-inventory`, `signalr-realtime-hosted-services`, `wpf-client-core-mvvm`, `verify-and-handoff`.
- Antigravity: `web-react-ui-conventions`, `wpf-xaml-ui-conventions`, `verify-and-handoff`.
