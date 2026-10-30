---
name: postgres-concurrency-booking
description: Chống trùng bàn và xử lý đồng thời với PostgreSQL (Supabase) + EF Core/Npgsql — SELECT ... FOR UPDATE, exclusion constraint btree_gist trên tstzrange, transaction, retry deadlock (40P01), lock timeout, idempotency bằng ON CONFLICT, thuật toán khoảng đệm 105 phút, chọn bàn best-fit, giảm số vòng truy vấn do độ trễ mạng tới Supabase, test đồng thời. Dùng khi viết hoặc sửa bất cứ thao tác nào ghi vào bàn/đặt bàn/hoán đổi bàn/order/hóa đơn có thể bị hai người thao tác cùng lúc, hoặc khi gặp lỗi trùng lịch, deadlock, dữ liệu không nhất quán.
---

# Đồng thời & chống trùng bàn (PostgreSQL)

Hai lớp bảo vệ, **cả hai đều bắt buộc**:
1. **Ứng dụng**: khóa hàng bàn `FOR UPDATE` → kiểm tra buffer (gồm order đang mở/khách vãng lai) → ghi. Cho phép trả gợi ý đẹp.
2. **CSDL**: exclusion constraint `ex_reservations_no_overlap` (xem `db/schema.sql`) — lưới an toàn cuối cùng, chặn cả lỗi logic và đường ghi không qua khuôn mẫu.

## Quy trình khuôn mẫu (tạo đặt bàn / dời lịch / hoán đổi bàn)
Mục tiêu: **~4 vòng truy vấn trong transaction** (Supabase ở Seoul, mỗi vòng tốn độ trễ mạng; khóa giữ càng lâu càng dễ tranh chấp).
1. Trước `BEGIN`: nạp cấu hình (cache), validate, sinh mã.
2. `BEGIN` (ReadCommitted — mặc định của PostgreSQL), `SET LOCAL lock_timeout = '3s'`.
3. Vòng 1 — khóa bàn ứng viên theo `table_id` tăng dần.
4. Vòng 2 — **một** truy vấn lấy mọi lượt chiếm dụng của các bàn đó: `WHERE table_id = ANY(@ids) AND status IN (…) AND occupied_until >= @from AND start_time <= @to` (và order đang mở); tính buffer trong bộ nhớ.
5. Vòng 3 — (nếu cần) chuyển `PENDING_DEPOSIT` quá hạn của bàn ứng viên sang `EXPIRED` (xem dưới).
6. Vòng 4 — insert/update + `COMMIT`. Phát SignalR **sau** commit.

```csharp
await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
await db.Database.ExecuteSqlRawAsync("SET LOCAL lock_timeout = '3s'", ct);

// Gọi ToListAsync() TRỰC TIẾP trên FromSql — KHÔNG ghép thêm LINQ (sẽ bọc subquery, mất ý nghĩa khóa)
var tables = await db.Tables
    .FromSqlInterpolated($@"SELECT * FROM tables
         WHERE is_active AND view_type = {viewType} AND seats >= {party}
         ORDER BY table_id FOR UPDATE")
    .AsTracking().ToListAsync(ct);

var ids = tables.Select(t => t.Id).ToArray();
var busy = await db.Reservations.AsNoTracking()
    .Where(r => ids.Contains(r.TableId) && Active.Contains(r.Status) && r.OccupiedUntil >= from && r.StartTime <= to)
    .Select(r => new { r.TableId, r.StartTime }).ToListAsync(ct);   // 1 truy vấn cho mọi bàn
```
(`ids.Contains` → `= ANY(@ids)` trong Npgsql.) Ghi `DateTime` với `Kind=Utc`; Npgsql ném lỗi nếu không.

## BufferRule (hàm thuần, ở `Core`)
```csharp
public readonly record struct BufferRule(int StayMinutes = 90, int CleaningMinutes = 15)
{
    public int Occupancy => StayMinutes + CleaningMinutes;                    // 105
    public bool Conflicts(DateTime existingStartUtc, DateTime newStartUtc)
        => Math.Abs((newStartUtc - existingStartUtc).TotalMinutes) <= Occupancy;   // biên ĐÓNG
    public DateTime OccupiedUntil(DateTime startUtc) => startUtc.AddMinutes(Occupancy);
}
```
Khoảng đóng `[S, S+105]` của hai lượt giao nhau ⇔ `|N−S| ≤ 105` ⇒ **khớp** `tstzrange(start_time, occupied_until, '[]') WITH &&` trong constraint. Test bắt buộc: S=19:00 → 17:00 ✔, 17:15 ✖, 20:45 ✖, 21:00 ✔ — chạy **ở cả hàm thuần lẫn qua INSERT thật** (hai kết quả phải trùng nhau). Cột `occupancy_minutes` chụp lại cấu hình lúc đặt; `occupied_until = start + occupancy`.

## Exclusion constraint — cách xử lý trong code
- Vi phạm ⇒ `PostgresException { SqlState: "23P01" }` (bọc trong `DbUpdateException`) ⇒ dịch thành `409 SLOT_UNAVAILABLE` (kèm gợi ý slot gần nhất). Không coi là lỗi 500.
- `PENDING_DEPOSIT` đã quá `deposit_expires_at` vẫn **chiếm chỗ** trong constraint cho tới khi bị chuyển `EXPIRED`. Vì vậy trong transaction đặt/dời, trước khi insert: `UPDATE reservations SET status='EXPIRED' WHERE table_id = ANY(@ids) AND status='PENDING_DEPOSIT' AND deposit_expires_at < now()`. Job nền vẫn quét định kỳ cho phần còn lại.
- Hoán đổi bàn (`UPDATE reservations SET table_id = @to`) cũng bị constraint kiểm ⇒ vi phạm ⇒ hủy hoán đổi, giữ bàn cũ.
- Order đang mở/khách vãng lai **không** nằm trong constraint ⇒ phải kiểm bằng logic ứng dụng dưới khóa (đó là lý do vẫn cần `FOR UPDATE`).

## Idempotency & chống trùng bằng chỉ mục
- `reservations.idempotency_key` UNIQUE: tìm trước theo key → trả kết quả cũ; đụng unique (`23505`) lúc race ⇒ đọc lại và trả.
- Webhook: `payments.provider_txn_id` UNIQUE; ghi bằng `INSERT … ON CONFLICT (provider_txn_id) DO NOTHING RETURNING …`.
- Còn có sẵn chỉ mục duy nhất từng phần: 1 order mở/bàn, 1 yêu cầu đổi bàn mở/bàn đích & /reservation, 1 dòng trừ kho SALE/(hóa đơn, nguyên liệu). Vi phạm (`23505`) ⇒ coi là "đã có" chứ không phải lỗi hệ thống.
- Bộ đếm theo ngày (số hóa đơn): `INSERT INTO daily_counters(counter_date,name,value) VALUES (@d,'invoice',1) ON CONFLICT (counter_date,name) DO UPDATE SET value = daily_counters.value + 1 RETURNING value` — nguyên tử, không cần khóa riêng.

## Mã lỗi PostgreSQL cần bắt (qua `PostgresException.SqlState`)
| SQLSTATE | Ý nghĩa | Xử lý |
|---|---|---|
| `23P01` | exclusion_violation | `SLOT_UNAVAILABLE` |
| `23505` | unique_violation | idempotent / `INVALID_STATE` tùy bảng |
| `40P01` | deadlock_detected | retry ≤ 3 lần, jitter 20–80ms, chạy lại **toàn bộ** transaction |
| `40001` | serialization_failure | như trên (nếu dùng mức cô lập cao hơn) |
| `55P03` | lock_not_available (hết `lock_timeout`) | retry 1–2 lần rồi `409`/`503` thân thiện |
Nếu bật `EnableRetryOnFailure` của Npgsql thì bọc transaction thủ công bằng `CreateExecutionStrategy().ExecuteAsync(...)`. Không gọi HTTP ngoài (Gemini/Zalo/Storage) khi đang giữ transaction.

## Khóa khác khi cần
`pg_advisory_xact_lock(hashtext('vacancy:' || @tableId))` để tuần tự hóa việc AI xử lý một lần bàn trống; `FOR UPDATE SKIP LOCKED` để nhiều worker lấy việc không đạp nhau. Khóa tự nhả khi transaction kết thúc.

## Khóa lạc quan
Dùng cột hệ thống `xmin` làm concurrency token trong EF Core (`uint Version` + `IsRowVersion()` của Npgsql) cho cập nhật ít xung đột (ghi chú, số khách). Xung đột ⇒ `409` kèm dữ liệu mới.

## Thời gian
`timestamptz` lưu UTC; so sánh/`now()` của DB chỉ dùng ở truy vấn thuần-SQL, còn logic dùng `TimeProvider` truyền tham số (để test đồng hồ giả). "Hôm nay" tính theo giờ VN rồi đổi sang khoảng UTC.

## Test đồng thời (bắt buộc, Testcontainers **PostgreSQL**)
```csharp
var tasks = Enumerable.Range(0, 20).Select(_ => client.PostAsJsonAsync("/api/v1/public/reservations", req(key: Guid.NewGuid())));
var results = await Task.WhenAll(tasks);
results.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
results.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(19);
```
Thêm: (1) insert thẳng bằng SQL hai lượt chồng nhau ⇒ `23P01` (chứng minh lưới an toàn); (2) biên 17:15/20:45/21:00 khớp hàm thuần; (3) lặp 10 lần để bắt lỗi chập chờn; (4) deadlock giả lập hai transaction khóa ngược thứ tự ⇒ retry thành công. **Chỉ chạy trên PostgreSQL local/container, không chạy lên Supabase cloud.**

## Chỉ mục
`ix_res_table_start`, các chỉ mục từng phần trong `db/schema.sql`; kiểm bằng `EXPLAIN (ANALYZE)` cho truy vấn availability (lưới giờ × bàn).
