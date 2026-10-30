---
name: verify-and-handoff
description: Quy trình xác minh và bàn giao cuối mỗi task cho cả Codex và Antigravity — Definition of Done, lệnh build/test/lint, chạy lại acceptance trong prompt, cập nhật docs/HANDOFF.md, quy trình Contract Change Request, ảnh chụp bằng chứng, checklist PR. Dùng khi sắp kết thúc BẤT KỲ prompt/task nào (C0–C9, A0–A6), trước khi tạo PR/merge, hoặc khi đổi endpoint/DTO/event/ViewModel contract.
---

# Xác minh & bàn giao

## 1. Trước khi nói "xong"
Chạy **thật** và dán kết quả ngắn gọn:
- Backend: `docker compose up -d postgres` · `dotnet build Restaurant.sln -p:EnableWindowsTargeting=true` · `dotnet test Restaurant.sln` (Testcontainers PostgreSQL cần Docker) · (nếu đổi API) `scripts/export-openapi.sh` rồi `git diff docs/openapi`.
- Web: `cd web && npm run lint && npm run build && npm run test` (+ `test:e2e` nếu có). Giao diện (Web/WPF): `node scripts/check-ui-copy.mjs` phải 0 lỗi.
- WPF: `dotnet build src/Restaurant.Wpf -p:EnableWindowsTargeting=true`; chạy `-- --fake` nếu đang ở Windows.
- Chạy lại từng dòng trong mục **Nghiệm thu** của prompt; dòng nào không chạy được (thiếu Docker/Windows/khóa API) ⇒ nói rõ **chưa kiểm chứng** thay vì ngầm bỏ qua.

## 2. Definition of Done
Build/test xanh · có test cho logic mới và ca biên · không `TODO`/code chết/`Console.WriteLine` · không bí mật trong repo/log (chạy `git grep -nE "sb_(secret|publishable)_|Password=|apikey"` — phải sạch) · `.env.example` cập nhật nếu thêm cấu hình · docs/contract cập nhật · UI có ảnh chụp 4 trạng thái (loading/empty/error/success) trong `docs/evidence/<prompt-id>/` · chỉ sửa file thuộc quyền sở hữu (xem `AGENTS.md §2`).

## 3. Contract Change Request
Khi thêm/đổi endpoint, DTO, enum, event hay VM property:
1. Codex: sửa `docs/02`/`docs/05` **cùng PR**, chạy export OpenAPI, nhãn PR `contract`. Chỉ *thêm*; muốn đổi tên/xóa ⇒ hỏi Antigravity trong HANDOFF và giữ bản cũ một thời gian (đánh `[Obsolete]`/ghi chú).
2. Antigravity: **không tự sửa** hợp đồng; ghi yêu cầu vào HANDOFF ("Câu hỏi cho bên kia") với ví dụ JSON mong muốn, tạm mock bằng MSW/Fake.
3. Bên nhận chạy lại `npm run gen:api` / rebuild và báo kết quả trong HANDOFF.

## 4. Mục HANDOFF (append cuối `docs/HANDOFF.md`)
```
### [YYYY-MM-DD] <Prompt-ID> — <Codex|Antigravity> — <tiêu đề>
- Đã làm: (3–8 gạch đầu dòng, nêu file/thư mục chính)
- Cách chạy/kiểm tra: (lệnh chính xác, tài khoản demo, biến môi trường)
- Contract changes: (liệt kê hoặc "không")
- Còn dở / giới hạn đã biết / chưa kiểm chứng: …
- Câu hỏi/yêu cầu cho bên kia: …
- Giả định đã chọn (mơ hồ trong đặc tả): …
```

## 5. Checklist PR
Tiêu đề `be|fe/<id>: …` · mô tả liên kết prompt · diff không chạm file của bên kia · ảnh/chạy thử đính kèm · nêu rủi ro & cách hoàn tác. Không merge khi CI đỏ.
