# Bộ kit AI cho đồ án: Hệ thống đặt bàn & vận hành nhà hàng (Web + WPF + Trợ lý AI)

Bộ kit này chia việc cho 2 AI agent:

- **Codex** → toàn bộ logic: API, PostgreSQL/Supabase, đặt bàn/buffer, VietQR, AI điều phối (Gemini + Zalo), POS/kho BOM, ViewModel & API client của WPF, test.
- **Antigravity** → toàn bộ giao diện: Web React (đặt bàn, menu, QR order) và các màn hình WPF (XAML).

## Cài đặt (1 phút)
1. Tạo repo mới, copy **toàn bộ** nội dung thư mục này vào gốc repo (giữ nguyên `.agents/`, `docs/`, `db/`, `AGENTS.md`).
2. `git init && git add . && git commit -m "chore: add AI kit"`.
3. Đọc `docs/00-HUONG-DAN-SU-DUNG.md` — có lộ trình, thứ tự chạy prompt và cách giao việc.

## Bản đồ tệp
| Đường dẫn | Dùng để |
|---|---|
| `AGENTS.md` | Luật chung, cả 2 agent tự đọc |
| `docs/00` | Hướng dẫn điều phối 2 agent |
| `docs/01` | Quyết định kiến trúc + điểm cần bạn xác nhận |
| `docs/02` | **Hợp đồng API/SignalR** (cầu nối giữa 2 agent) |
| `docs/03` | **Quy tắc nghiệp vụ** chi tiết từ đặc tả (công thức, state machine) |
| `docs/04` | Design system + danh sách màn hình |
| `docs/05` | Hợp đồng ViewModel WPF |
| `docs/prompts/codex/` | `C-MASTER` + C0…C9 |
| `docs/prompts/antigravity/` | `A-MASTER` + A0…A6 |
| `.agents/skills/` | 12 skill (10 cho Codex, 2 riêng cho Antigravity; `verify-and-handoff` dùng chung) |
| `db/schema.sql` | Lược đồ **PostgreSQL (Supabase)** tham chiếu: 13 bảng lõi + bảng bổ sung, exclusion constraint chống trùng bàn, khóa RLS |
| `docs/spec/` | Đặc tả v2 (.docx) — **đã che khóa Supabase** |

## ⚠ Bảo mật Supabase (đọc trước khi commit)
Đặc tả v2 gốc chứa **Secret Key** và Publishable Key ở dạng chữ rõ. Bản trong `docs/spec/` đã che. Đừng commit bản gốc; hãy **xoay Secret Key** trong Supabase Dashboard và chỉ để khóa/mật khẩu DB trong `dotnet user-secrets` hoặc biến môi trường (xem `docs/01 §D`).
