# C9 — QA, bảo mật, dữ liệu demo, diễn tập (Pha 4)

**Skills**: `verify-and-handoff`, `supabase-postgres-storage`, `dotnet-api-conventions`, `postgres-concurrency-booking`, `restaurant-domain-rules`.

## Việc cần làm
1. **Test E2E mức API** cho **Demo 5 bước** (`docs/00 §7`) bằng Testcontainers + `MockMessageChannel` + `FakeTimeProvider`: đặt bàn Web (view cửa sổ kín → Bàn 104 + wishlist) → cọc simulate → bàn 201 trống 18:15 → tin mời + HELD → hết 10' → leo thang → staff đồng ý → QR order bò bít tết → phiếu bếp có dị ứng → thanh toán trừ cọc 200k → kho trừ đúng.
2. **Tải & đồng thời** (trên PostgreSQL **local/container**, KHÔNG trên Supabase cloud): 100 người cùng xem availability + 30 đặt song song vào 5 bàn → không trùng, p95 hợp lý; kiểm tra deadlock retry (`40P01`). Thêm một lần **đo độ trễ thực** các endpoint chính khi trỏ vào Supabase (chỉ đọc, ít request) và ghi vào `docs/KNOWN-ISSUES.md` kèm khuyến nghị cache/gộp truy vấn.
3. **Rà soát bảo mật** (ghi kết quả vào `docs/SECURITY-CHECKLIST.md`): **Supabase — RLS bật mọi bảng, chạy `verify-supabase-lockdown`, không khóa trong repo/lịch sử git (`git log -p | grep`), Storage chỉ ghi qua backend, upload an toàn**; authz từng endpoint (đã có ma trận), rate-limit endpoint public + login + webhook, CORS chặt ở Production, không lộ stack trace, header bảo mật, SQL thô chỉ dùng tham số, `/dev/*` **404 ở Production**, secrets không nằm trong repo/log, webhook & QR chữ ký, kiểm tra IDOR (`phone` + `code` khi tra cứu), giới hạn kích thước body, mật khẩu demo bắt đổi.
4. **`POST /dev/seed-demo`**: dựng trạng thái sẵn sàng diễn tập (đặt bàn Anh Long 19:00 ở Bàn 104 có wishlist, bàn 201 đang có khách sắp thanh toán, kho đủ bò, vài đơn lịch sử cho báo cáo). Kèm `scripts/reset-demo.sh|.ps1`. **An toàn cloud**: `seed-demo` và các script reset từ chối chạy khi chuỗi kết nối trỏ `supabase.com` trừ khi có cờ xác nhận rõ ràng (`Dev:AllowCloudSeed=true`), và luôn in host đích (che mật khẩu) trước khi chạy.
5. **Vận hành**: `README.md` gốc (cài đặt, biến môi trường, `dotnet user-secrets`, cách tạo bucket Storage, cách áp migration lên Supabase, chạy từng thành phần), `scripts/backup-db.sh|.ps1` (`pg_dump` qua session pooler, chuỗi kết nối từ biến môi trường, hướng dẫn khôi phục), Dockerfile cho Api (tùy chọn), cấu hình `Production` mẫu.
6. **Tài liệu**: `docs/DEMO-SCRIPT.md` (phần backend: lệnh, dữ liệu, mốc thời gian, cách dùng `/dev/*` nếu Zalo/ngân hàng thật gặp sự cố) và `docs/KNOWN-ISSUES.md`.
7. Sửa mọi lỗi tìm được; không "ẩn" lỗi bằng cách bỏ test.

## Nghiệm thu
`dotnet test` xanh toàn bộ · chạy `seed-demo` rồi diễn tập 5 bước bằng API thủ công thành công 3 lần liên tiếp · HANDOFF tổng kết (đã làm / còn dở / rủi ro).
