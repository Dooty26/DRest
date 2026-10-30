# Hướng dẫn điều phối Codex + Antigravity

## 1. Chuẩn bị máy
| Cần | Ghi chú |
|---|---|
| Windows 10/11 | Bắt buộc để **chạy** WPF. Codex cloud/Linux chỉ *build* được WPF với `-p:EnableWindowsTargeting=true`, không chạy được. |
| .NET 8 SDK, Node 20+, Git, Docker Desktop | Docker chạy PostgreSQL 16 local cho dev/test |
| Dự án Supabase (đã có: `DootyRes`) | Dùng cho staging/demo. Tạo 3 bucket Storage công khai: `menu-images`, `branding`, `floor-plans`. Lấy mật khẩu DB + Secret Key **chỉ để đặt vào user-secrets** (xem `docs/01 §D`) |
| Visual Studio 2022 hoặc Rider | Để mở XAML designer, debug WPF |
| Codex (CLI/IDE/app) và Antigravity | Đăng nhập sẵn |
| Gemini API key | Tạo ở Google AI Studio; đặt vào user-secrets (`Ai:GeminiApiKey`) |
| (Tùy chọn, cho demo thật) | Zalo OA + template ZNS đã duyệt; tài khoản dịch vụ webhook ngân hàng (SePay/Casso/payOS) |

> Không có Zalo/ngân hàng thật vẫn làm được hết: mọi tích hợp có **Mock** + công cụ `/dev/*` mô phỏng.

## 2. Cài kit vào repo
1. Copy toàn bộ kit vào gốc repo → `git init` → commit `chore: add AI kit` → đẩy lên GitHub.
2. Tạo 2 thư mục làm việc song song bằng **git worktree** để 2 agent không đạp file nhau:
   ```
   git worktree add ../repo-be -b be/work
   git worktree add ../repo-fe -b fe/work
   ```
   Codex mở `../repo-be`, Antigravity mở `../repo-fe`. Mỗi prompt xong → PR/merge vào `main` → bên kia `git merge main`.

## 3. Lộ trình & thứ tự prompt
| Pha | Codex (`docs/prompts/codex/`) | Antigravity (`docs/prompts/antigravity/`) | Cổng kiểm thu (bạn tự chạy) |
|---|---|---|---|
| 0 | **C0** Scaffold + hợp đồng + Fake | **A0** Design system + khung Web/WPF | Solution build xanh · Swagger lên · `docker compose up -d postgres` chạy được · `web` chạy trang ui-kit · WPF `--fake` mở được Shell |
| 1 | **C1** CSDL → **C2** Auth/RBAC/CRUD → **C3** Engine đặt bàn | **A1** Web đặt bàn (MSW) · **A3** WPF Login + Sơ đồ bàn (`--fake`) | Test buffer: 19:00 khóa 17:15–20:45 (cả ở exclusion constraint) · migration áp lên Supabase thành công & **RLS khóa Data API** · đặt bàn Web → bàn hiện Vàng trên sơ đồ · upload ảnh món lên Storage hiển thị trên Web |
| 2 | **C4** Client.Core nối API thật → **C5** VietQR cọc → **C6** POS/QR order/BOM | **A2** Web menu + QR order + Zalo simulator · **A4** WPF POS/KDS/Kho/Báo cáo/Admin | Cọc (Mock hoặc thật) xác nhận tức thì · QR order → phiếu bếp có cảnh báo dị ứng · thanh toán: cọc 200k − bill 130k = **−70k**, kho trừ đúng |
| 3 | **C7** AI điều phối + Zalo → **C8** Realtime & ca ngoại lệ | **A5** WPF Tím đếm ngược, popup leo thang, hàng chờ, quá giờ, timeline AI | Chạy được **Demo bước 2→4** bằng Mock Zalo |
| 4 | **C9** QA, bảo mật, seed demo | **A6** Tích hợp, đánh bóng, kịch bản demo | Diễn tập đủ 5 bước, 3 lần liên tiếp không lỗi |

Mỗi pha, hai cột chạy **song song**. Antigravity không cần chờ Codex nhờ MSW (Web) và `FakeRestaurantApi` (WPF).

## 4. Cách giao việc cho từng công cụ
**Lần đầu (mỗi agent một lần):** dán nội dung `C-MASTER.md` cho Codex / `A-MASTER.md` cho Antigravity. Nó bắt agent đọc `AGENTS.md`, `docs/`, danh sách skill.

**Mỗi task:** dán nguyên file prompt (hoặc ngắn gọn: *"Thực hiện `docs/prompts/codex/C3-reservation-engine.md`, tuân thủ AGENTS.md"*).
- **Codex**: skill nằm ở `.agents/skills/` nên được nhận diện tự động theo mô tả; có thể gọi tường minh bằng `$tên-skill` hoặc `/skills` (xem tài liệu Codex hiện hành). Mỗi prompt đều ghi rõ skill cần dùng.
- **Antigravity**: dùng **Manager view**, mỗi prompt một agent/conversation; bật **Planning mode** và duyệt kế hoạch trước khi cho code. Với Web, yêu cầu agent dùng **browser** để tự kiểm tra và đính ảnh chụp. WPF không xem trước được trong IDE → agent build rồi bạn chạy trên Windows, chụp ảnh gửi lại nếu cần sửa.

**Khi agent dừng giữa chừng / trả lời thiếu:** nói *"Đọc docs/HANDOFF.md và checklist trong prompt, làm tiếp các mục chưa tick, rồi chạy lại phần Acceptance."*

## 5. Giao thức hợp đồng
- `docs/02` (API/SignalR) và `docs/05` (ViewModel WPF) là **cầu nối**. Chỉ Codex sửa; Antigravity muốn thêm/đổi → ghi vào `HANDOFF.md` mục câu hỏi.
- Sau mỗi PR backend đổi API: Codex chạy `scripts/export-openapi.sh` → `docs/openapi/openapi.v1.json`; Antigravity chạy `npm run gen:api` để sinh lại type TypeScript.
- Không bao giờ để 2 agent cùng sửa một file (xem bảng sở hữu trong `AGENTS.md`).

## 6. Quản lý rủi ro
| Rủi ro | Cách xử lý |
|---|---|
| Zalo ZNS cần template duyệt + khách đã quan tâm OA | Dùng `MockMessageChannel` + trang `/dev/zalo-simulator` để demo ổn định; nộp duyệt template sớm (xem `docs/01`) |
| Webhook ngân hàng cần dịch vụ trung gian | Mock + `POST /dev/simulate-payment`; khi demo thật dùng SePay/Casso qua adapter |
| Lộ khóa Supabase (đặc tả v2 chứa khóa chữ rõ) | Dùng bản đã che trong `docs/spec/`; **xoay Secret Key**; khóa chỉ ở user-secrets/biến môi trường; thêm `.gitignore` cho `.env`, `*.local.json` |
| Supabase Free tạm dừng / độ trễ Seoul–Hà Nội | Đánh thức dự án trước demo; dev/test dùng PostgreSQL local; giữ transaction ít vòng truy vấn |
| Quên bật RLS ⇒ ai cũng gọi được bảng qua REST | Migration tự bật RLS + test "mọi bảng public có `rowsecurity = true`" + script `verify-supabase-lockdown` |
| WPF không chạy trên Linux | Logic để ở `Client.Core` (test được trên Linux); XAML chỉ build kiểm tra |
| Agent "sáng tạo" ngoài phạm vi | Nhắc lại bảng sở hữu trong `AGENTS.md`; từ chối PR chạm file ngoài quyền |
| Thiếu thời gian | Cắt theo nhãn P0/P1/P2 trong `docs/04` mục 4; **P0 = đủ chạy Demo 5 bước** |

## 7. Checklist demo cuối (trích từ đặc tả mục 8.2)
1. Web: đặt 19:00, view cửa sổ kín → đặt Bàn 104 + tick nguyện vọng → quét VietQR cọc 200k → Web nhận tiền tức thì.
2. WPF: Bàn 201 thanh toán sớm lúc 18:15 → trống sớm.
3. Zalo (hoặc simulator) nhận tin mời đổi · WPF: Bàn 201 màu Tím đếm ngược 10 phút.
4. Khách không phản hồi → popup leo thang trên WPF → bấm **[Khách đồng ý đổi]** → Bàn 201 gán cho anh Long.
5. Khách đến, quét QR gọi bò bít tết → phiếu bếp có cảnh báo dị ứng → thanh toán trừ cọc 200k → thịt bò trong kho bị trừ đúng.
