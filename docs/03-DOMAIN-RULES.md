# 03 — Quy tắc nghiệp vụ (nguồn sự thật cho logic)

Rút ra từ đặc tả gốc (`docs/spec/`). Mâu thuẫn/mơ hồ: **tài liệu này thắng**; ghi lại trong `HANDOFF.md` để người dùng xác nhận.

## 0. Cấu hình (bảng `app_settings`, giá trị mặc định)
| Key | Mặc định | Ý nghĩa |
|---|---|---|
| `Booking.StayMinutes` | 90 | Thời gian dùng bữa |
| `Booking.CleaningMinutes` | 15 | Dọn bàn |
| `Booking.SlotStepMinutes` | 30 | Lưới giờ đặt |
| `Booking.OpenTime` / `LastSeatingTime` | 10:00 / 21:30 | Giờ VN |
| `Booking.MinLeadMinutes` | 60 | Đặt online phải trước giờ đến tối thiểu |
| `Booking.MaxAdvanceDays` | 30 | |
| `Deposit.Amount` | 200000 | VND |
| `Deposit.HoldMinutes` | 15 | Giữ bàn chờ thanh toán cọc |
| `Cancel.FullRefundHoursBefore` | 2 | |
| `NoShow.AutoMarkMinutes` | 30 | Quá giờ hẹn bao lâu tự đánh dấu NO_SHOW |
| `WalkIn.WarnIfReservationWithinMinutes` | 45 | |
| `Overstay.WarnBeforeMinutes` | 30 | |
| `Overstay.ApologyDiscountPercent` | 10 | |
| `Switch.HoldMinutes` | 10 | Giữ bàn chờ khách trả lời Zalo |
| `Switch.EscalationWindowMinutes` | 10 | Chờ nhân viên xử lý sau leo thang |
| `Switch.LookaheadHours` | 3 | Chỉ xét đặt bàn bắt đầu trong N giờ tới |
| `Switch.MaxAttemptsPerVacancy` | 3 | Số ứng viên thử tối đa cho 1 lần bàn trống |
| `Ai.Model` / `Ai.TimeoutSeconds` | (cấu hình) / 8 | Không hard-code tên model |
| `Stock.AllowNegative` | true | Cho bán khi kho âm, nhưng cảnh báo |
| `Billing.VatPercent` | 0 | |
| `Tables.QrBaseUrl` | (cấu hình) | Gốc URL web cho QR |
| `Storage.MaxImageBytes` | 2097152 | Kích thước ảnh upload tối đa |
| `Brand.LogoUrl` | (rỗng) | URL logo trong Storage (do upload cập nhật) |

## 1. Trạng thái bàn
Lưu trong `tables.status`: `AVAILABLE | OCCUPIED | CLEANING | HELD`.
`displayStatus` (server tính, ưu tiên từ trên xuống) — **một nguồn duy nhất cho Web & WPF**:
| Ưu tiên | Điều kiện | displayStatus | Màu |
|---|---|---|---|
| 1 | `status = HELD` | `HELD` | 🟣 Tím |
| 2 | `status = OCCUPIED` | `OCCUPIED` | 🔴 Đỏ (kèm thời gian ngồi + tạm tính) |
| 3 | `status = CLEANING` | `CLEANING` | ⚪ Xám |
| 4 | AVAILABLE và có reservation `CONFIRMED`/`PENDING_DEPOSIT` của bàn này bắt đầu trong hôm nay & chưa tới giờ + 30' | `RESERVED` | 🟡 Vàng (kèm giờ khách đến) |
| 5 | còn lại | `AVAILABLE` | 🟢 Xanh |

Chuyển trạng thái: `AVAILABLE→OCCUPIED` (mở bàn/check-in) · `OCCUPIED→CLEANING` (thanh toán xong) · `CLEANING→AVAILABLE` (báo dọn xong) · `AVAILABLE→HELD` (AI giữ bàn) · `HELD→AVAILABLE` (nhả) · `HELD→AVAILABLE` rồi reservation gán bàn mới (khi khách đồng ý).

## 2. Khoảng đệm (Buffer Time Rule)
`occupancy = StayMinutes + CleaningMinutes = 105`.
Với mỗi bàn T và mỗi *lượt chiếm dụng* hiện có bắt đầu lúc `S` (reservation `CONFIRMED`, `CHECKED_IN`, `PENDING_DEPOSIT` chưa hết hạn; hoặc order đang mở với `S = opened_at`), giờ bắt đầu mới `N` bị **khóa** khi:

`S − occupancy ≤ N ≤ S + occupancy`  (biên đóng, đối xứng)

Ví dụ đặc tả: bàn 201 có lịch 19:00 → khóa 17:15…20:45; cho phép ≤ 17:00 hoặc ≥ 21:00.
Hệ quả: hai lượt cùng bàn phải cách nhau **> 105 phút** (lưới 30' → ≥ 120').
**Cài đặt ở CSDL**: mỗi reservation lưu `occupied_until = start_time + occupancy_minutes` và có ràng buộc `EXCLUDE USING gist (table_id WITH =, tstzrange(start_time, occupied_until, '[]') WITH &&)` cho trạng thái hiệu lực (`PENDING_DEPOSIT`, `CONFIRMED`, `CHECKED_IN`). Khoảng đóng `[S, S+105]` của hai lượt giao nhau ⇔ `|N−S| ≤ 105` — khớp đúng quy tắc trên. Ràng buộc là **lưới an toàn**; logic ứng dụng vẫn phải kiểm trước (để trả gợi ý đẹp và để tính cả order đang mở/khách vãng lai).
Bàn đang `OCCUPIED` mà order mở lúc `O`: coi như một lượt chiếm dụng bắt đầu ở `O` (dùng `max(O + occupancy, now)` làm mốc kết thúc khi khách ngồi quá giờ).

## 3. Tính khả dụng (availability)
Với ngày D, số khách P: duyệt các slot từ `OpenTime` tới `LastSeatingTime` theo `SlotStepMinutes`.
Bàn *đủ điều kiện* cho slot N: `is_active`, `seats ≥ P` (cho phép tới `seats + extra_chairs_max` nếu cấu hình), không bị khóa theo §2.
Slot khả dụng theo `viewType` nếu có ≥ 1 bàn đủ điều kiện. Lý do không khả dụng: `PAST`, `TOO_SOON` (< MinLead), `BOOKED`, `NO_CAPACITY`.
**Chọn bàn gán** (best-fit): sắp theo `seats` tăng dần (ít dư ghế nhất), rồi `table_id`; ưu tiên không gán bàn lớn hơn `P + 2` nếu còn bàn nhỏ hơn.

## 4. Tạo đặt bàn Web
1. Validate (SĐT VN, P ≥ 1, ngày/giờ hợp lệ, `Idempotency-Key`).
2. Trong **một transaction** (xem skill `postgres-concurrency-booking`): khóa các hàng bàn ứng viên `FOR UPDATE` (thứ tự `table_id` tăng) → kiểm tra lại §2 → chọn bàn → insert `reservations(status=PENDING_DEPOSIT, deposit_expires_at = now + 15')` + `payments(DEPOSIT, PENDING)` → nếu `wishlist.enabled` và loại view đặt ≠ `targetViewType` thì insert `reservation_wishlists`.
3. Trả mã `code` (`RES` + 6 chữ số, duy nhất), thông tin QR cọc. Hết `deposit_expires_at` mà chưa nhận cọc → job chuyển `EXPIRED`, bàn tự trả. Vì ràng buộc CSDL vẫn tính `PENDING_DEPOSIT` đã quá hạn là chiếm chỗ, bước 2 phải **chuyển các `PENDING_DEPOSIT` quá hạn của bàn ứng viên sang `EXPIRED` ngay trong transaction** trước khi kiểm tra/insert.
4. Nhận cọc (webhook) → `CONFIRMED`, `deposit_status=PAID`, `deposit_paid_at=now`, đẩy `DepositConfirmed`.

## 5. Hủy / dời / no-show / check-in
| Tình huống | Quy tắc |
|---|---|
| Hủy trước giờ hẹn **≥ 2h** | Hoàn 100% (`REFUND` do thu ngân chuyển khoản lại, ghi `payments`) **hoặc** Voucher giá trị = cọc (hạn 90 ngày). Khách chọn `refundMode`. `deposit_status = REFUNDED` / `CONVERTED_TO_VOUCHER` |
| Hủy **< 2h** | Giữ cọc: `deposit_status = FORFEITED` |
| Dời lịch **≥ 2h** | Cho phép, giữ cọc, chạy lại §2–§3 cho slot mới trong cùng transaction (thất bại → giữ lịch cũ). Tối đa 1 lần/đơn |
| Dời **< 2h** online | Từ chối `INVALID_STATE`, hướng dẫn gọi nhà hàng; nhân viên có thể ghi đè |
| No-show | Quá `start + NoShow.AutoMarkMinutes` mà chưa check-in → job đặt `NO_SHOW`, `FORFEITED`, nhả bàn. Nhân viên có thể đánh dấu sớm |
| Check-in | Thu ngân bấm "Khách đến": tạo `orders(OPEN)`, bàn → `OCCUPIED`, reservation → `CHECKED_IN` (có thể đổi bàn nếu bàn đã gán chưa trống) |

## 6. Khách vãng lai, hàng chờ, ghép/đổi bàn
- **Walk-in**: thu ngân chọn bàn trống đủ chỗ → mở bàn. Nếu bàn đó có lịch bắt đầu trong ≤ `WalkIn.WarnIfReservationWithinMinutes` → API trả `409 WALKIN_CONFLICT` kèm `warnings`; gửi lại `force=true` để vẫn mở.
- **Hàng chờ** (`waitlist_entries`): khi mọi bàn đủ chỗ đều bận → ghi tên + SĐT + số khách. Khi một bàn thành `AVAILABLE` (sau dọn) và không bị lịch đặt chiếm (§2) → phát `WaitlistTableReady` cho mục **đầu hàng vừa chỗ** (bỏ qua mục quá lớn nhưng giữ thứ tự). Trạng thái: `WAITING → NOTIFIED → SEATED | CANCELLED`.
- **Đổi bàn**: chuyển `orders.table_id`, bàn cũ → `CLEANING`, bàn mới → `OCCUPIED`; ghi `audit_logs`.
- **Ghép bàn**: bảng `table_merges`; các bàn phụ hiển thị "ghép với T…", dùng chung 1 order; tách bàn được.

## 7. Ngoại lệ vận hành
- **Quá giờ (ngâm bàn)**: với reservation `CONFIRMED` bắt đầu lúc `S` trên bàn T đang `OCCUPIED` bởi khách khác: tại `S − 30'` phát `OverstayWarning(level=PRE)`; tại `S` mà T vẫn bận → `level=OVERDUE`. Nếu **không còn bàn nào khác** đủ chỗ cho khách đặt (quán kín) → chế độ `WAITING_FOR_TABLE`: tạo phiếu tặng nước miễn phí + đặt `reservations.apology_discount_pct = 10` (áp lên bill của khách đặt), phát `OverstayWarning(level=APOLOGY)`. Endpoint áp dụng phải idempotent.
- **Lệch số người** (`PUT …/guest-count`): `n ≤ seats` → ok; `n ≤ seats + extra_chairs_max` → ghi `needs_extra_chairs`; ngược lại trả `suggestions[]` = bàn trống phù hợp (không vướng §2 trong 105' tới) sắp theo `seats` tăng.
- **Hoàn cọc thừa**: xem §10.
- **Hết món**: `POST /menu/items/{id}/availability` → cập nhật `menu_items.is_available` → phát `MenuAvailabilityChanged` tới `/hubs/public` & `/hubs/staff`; Web làm mờ ngay. Đặt món đã hết → `409 OUT_OF_STOCK_ITEM`.

## 8. Đơn hàng, bếp
`orders.status`: `OPEN → BILL_REQUESTED → PAID | CANCELLED`. `order_details.status`: `NEW → COOKING → READY → SERVED | CANCELLED | OUT_OF_STOCK`.
Món gửi từ QR hoặc nhân viên vào thẳng `NEW`; phát `OrderItemsAdded` → WPF máy `IsPrintHost` tự in **phiếu bếp**, nổi bật `allergy_note`/`allergyTags`. QR order chỉ hợp lệ khi chữ ký đúng **và** bàn có order `OPEN` (xem skill `pos-bom-inventory`).

## 9. Voucher & giảm giá
`vouchers.type`: `FIXED | PERCENT`; nguồn `DEPOSIT_REFUND | APOLOGY | PROMO`; dùng 1 lần; kiểm hạn. Giảm giá nhân viên nhập tay cần quyền Cashier+ và ghi `discount_reason`.

## 10. Hóa đơn & cọc
```
subtotal      = Σ qty × unit_price   (bỏ CANCELLED/OUT_OF_STOCK)
discount      = voucher/percent/apology, làm tròn VND (AwayFromZero), ≤ subtotal
vat           = round((subtotal − discount) × VatPercent / 100)
depositApplied= reservation.deposit_status = PAID ? deposit_amount : 0
net           = subtotal − discount + vat − depositApplied      // CÓ THỂ ÂM
refundDue     = net < 0 ? −net : 0
```
Ví dụ: cọc 200.000, tiêu 130.000 → `net = −70.000` → UI hiển thị **"Hoàn lại khách 70.000đ"**; thu ngân phải xác nhận đã hoàn (ghi `payments(type=REFUND)`) mới đóng hóa đơn; `deposit_status → APPLIED`.
Thanh toán xong (một transaction): hóa đơn `PAID` → trừ kho BOM (§11) → order `PAID` → bàn `CLEANING` → nếu bàn trống sớm so với lượt kế tiếp → phát sự kiện `TableVacatedEarly` cho AI (§12).

## 11. Kho định lượng (BOM)
Nguyên liệu lưu theo **đơn vị gốc** (`g`, `ml`, `pcs`) + `display_unit`/`display_factor` (kg=1000g). `recipes(item_id, ingredient_id, qty)` theo đơn vị gốc.
Khi thanh toán: với mỗi `order_detail` không bị hủy: `trừ = qty × recipe.qty` cộng dồn theo nguyên liệu → ghi `stock_movements(SALE, delta âm, ref_invoice_id)` + giảm `ingredients.stock_qty`, **đúng một lần** (cờ `invoices.stock_deducted_at`). Kho âm: cho phép nếu `Stock.AllowNegative`, vẫn phát `LowStock`. Món không có công thức: bỏ qua, liệt kê trong báo cáo "món chưa có định lượng". Ví dụ: 1 bò bít tết = 200g thịt bò + 50g khoai tây.

## 12. Trợ lý AI điều phối bàn (đặc tả mục 4)
**Sự kiện kích hoạt `TableVacatedEarly(tableId, vacatedAt)`**: bàn T thanh toán/rời sớm (hoặc nhân viên bấm "đã rời" / `POST /dev/trigger-early-vacate`).

**Bước 1 — Lọc ứng viên**: reservation R thỏa tất cả: `CONFIRMED` + cọc `PAID`; `R.start` trong `[now, now + Lookahead]`; R **chưa** ngồi; có `wishlist` đang bật với `target_view_type = T.view_type` (hoặc `target_table_id = T`); `R.party ≤ T.seats(+extra)`; R.table ≠ T; T **không bị khóa** §2 bởi lượt khác ngoài R, và `T dọn xong ≤ R.start` (`vacatedAt + 15' ≤ R.start`); R không có yêu cầu đổi bàn đang mở.
**Bước 2 — Xếp hạng**: `ORDER BY customers.is_vip DESC, reservations.deposit_paid_at ASC, reservations.created_at ASC`. Lưu lý do xếp hạng vào timeline.
**Bước 3 — Giữ & gửi**: tạo `table_switch_requests(PENDING_CUSTOMER, expires_at = now + 10')`, đặt `T.status = HELD` (Tím + đếm ngược), sinh tin nhắn (Gemini; lỗi → template), gửi qua `IMessageChannel`. Tin phải nêu: tên bàn, loại view, **hạn chót `HH:mm`**, cách trả lời ("Đồng ý"/"Không"). Gửi lỗi → leo thang ngay (`reason=SEND_FAILED`).

**State machine `table_switch_requests.status`**
```
PENDING_CUSTOMER ──khách "Đồng ý"──────────────► ACCEPTED         (hoán đổi bàn nguyên tử)
       │──khách "Không"────────────────────────► DECLINED         (nhả T → thử ứng viên kế)
       │──quá expires_at────────────────────────► ESCALATED        (popup WPF; T vẫn HELD; escalation_expires_at = +10')
ESCALATED ──khách vẫn trả lời "Đồng ý"──────────► ACCEPTED
       │──nhân viên [Khách đồng ý đổi]──────────► STAFF_ACCEPTED   (hoán đổi)
       │──nhân viên [Khách giữ bàn cũ]──────────► STAFF_KEPT_OLD   (nhả T → ứng viên kế)
       │──nhân viên [Không nghe máy]────────────► STAFF_NO_ANSWER  (giữ bàn cũ, nhả T → ứng viên kế)
       │──quá escalation_expires_at──────────────► AUTO_RELEASED   (giữ bàn cũ, ghi log, nhả T)
```
**Hoán đổi nguyên tử** (một transaction, khóa request + T + bàn cũ): kiểm request còn hợp lệ, T còn HELD đúng request, R còn `CONFIRMED`, không phát sinh xung đột §2 → `R.table_id = T`; T: `HELD → AVAILABLE` (hiển thị Vàng nhờ R); giải phóng bàn cũ; request `ACCEPTED`; gửi tin xác nhận; phát event. Race "khách đồng ý" vs "hết hạn": ai khóa request trước thắng; bên thua nhận phản hồi lịch sự ("đã hết hạn").
**Ý định trả lời**: nhanh bằng từ khóa (đồng ý/ok/được/có/yes → ACCEPT; không/ko/thôi/giữ bàn cũ → DECLINE) trước, Gemini phân loại JSON `{intent, confidence}` khi không rõ; `confidence < 0.7` → `UNCLEAR` (nhắc lại 1 lần, không tự quyết).
Mọi bước ghi `switch_request_events` (timeline hiển thị ở WPF).

## 13. Ma trận phân quyền (RBAC)
| Chức năng | Manager | Cashier | Waiter | Kitchen |
|---|:-:|:-:|:-:|:-:|
| Xem sơ đồ bàn | ✔ | ✔ | ✔ | ✖ |
| Mở bàn / check-in / walk-in / hàng chờ | ✔ | ✔ | xem | ✖ |
| Gọi món tại bàn | ✔ | ✔ | ✔ | ✖ |
| Báo dọn / báo dọn xong | ✔ | ✔ | ✔ | ✖ |
| Đổi / ghép bàn | ✔ | ✔ | ✖ | ✖ |
| Nhận cọc, hủy, hoàn cọc, xử lý leo thang AI | ✔ | ✔ | ✖ | ✖ |
| Hóa đơn tạm tính / thanh toán / mã giảm giá | ✔ | ✔ | ✖ | ✖ |
| Màn hình Bếp (KDS), đổi trạng thái món | ✔ | ✖ | ✖ | ✔ |
| Đánh dấu "Tạm hết món" | ✔ | ✔ | ✖ | ✔ |
| Kho, công thức, thực đơn, giá | ✔ | ✖ | ✖ | ✖ |
| Báo cáo doanh thu, quỹ tiền | ✔ | ✖ | ✖ | ✖ |
| Quản lý tài khoản, sơ đồ bàn, cấu hình | ✔ | ✖ | ✖ | ✖ |

## 14. Định dạng mã
`RES` + 6 số (đặt bàn; nội dung chuyển khoản không dấu cách) · `HD` + `yyyyMMdd` + `-` + 4 số (hóa đơn) · `VC` + 8 ký tự A-Z0-9 (voucher).
