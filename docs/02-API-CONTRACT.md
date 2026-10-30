# 02 — Hợp đồng API & SignalR (cầu nối Codex ↔ Antigravity)

Thỏa thuận khởi điểm. Khi backend chạy, **OpenAPI (`docs/openapi/openapi.v1.json`) là nguồn sự thật**; file này mô tả ý định và phải được cập nhật cùng PR (xem AGENTS.md mục 5). Chỉ Codex sửa file này.

## 0. Quy ước chung
- Base path `/api/v1`. JSON `camelCase`; enum là **chuỗi** `UPPER_SNAKE`.
- Tiền: số nguyên VND. Thời điểm: ISO-8601 **UTC** (`2026-10-20T12:00:00Z`). Giờ slot: chuỗi `"19:00"` theo giờ VN; ngày: `"2026-10-20"`.
- Lỗi: RFC 7807 `ProblemDetails` + trường `code` (mã máy), `errors{field:[msg]}`, `warnings[]` (cho cảnh báo cần xác nhận `force=true`).
- Ảnh (`imageUrl`, `logoUrl`, `floorPlanImageUrl`) là **URL HTTPS tuyệt đối** tới CDN công khai của Supabase Storage; client chỉ hiển thị, **không tự dựng URL**, không giữ khóa Supabase. Có thể `null` ⇒ hiện ảnh placeholder.
- Header `Idempotency-Key: <uuid>` cho `POST /public/reservations` và `POST /public/tables/{code}/orders`.
- Auth nhân viên: `Authorization: Bearer <JWT>`; claim `role`. Public/Webhook không cần JWT (webhook xác thực bằng bí mật riêng).
- Mã lỗi (`code`): `VALIDATION_ERROR`, `NOT_FOUND`, `UNAUTHORIZED`, `FORBIDDEN_ROLE`, `INVALID_STATE`, `SLOT_UNAVAILABLE`, `TABLE_CONFLICT`, `WALKIN_CONFLICT`, `DEPOSIT_EXPIRED`, `OUT_OF_STOCK_ITEM`, `QR_SIGNATURE_INVALID`, `TABLE_NOT_OPEN`, `WEBHOOK_SIGNATURE_INVALID`, `RATE_LIMITED`, `NOT_IMPLEMENTED`.

## 1. Enum
`Role`: MANAGER, CASHIER, WAITER, KITCHEN
`ViewType`: STANDARD, WINDOW, BALCONY, VIP, OUTDOOR
`TableDisplayStatus`: AVAILABLE, RESERVED, OCCUPIED, CLEANING, HELD
`ReservationStatus`: PENDING_DEPOSIT, CONFIRMED, CHECKED_IN, COMPLETED, CANCELLED, NO_SHOW, EXPIRED
`DepositStatus`: NONE, PENDING, PAID, REFUNDED, FORFEITED, CONVERTED_TO_VOUCHER, APPLIED
`OrderStatus`: OPEN, BILL_REQUESTED, PAID, CANCELLED
`OrderItemStatus`: NEW, COOKING, READY, SERVED, CANCELLED, OUT_OF_STOCK
`SwitchStatus`: PENDING_CUSTOMER, ACCEPTED, DECLINED, ESCALATED, STAFF_ACCEPTED, STAFF_KEPT_OLD, STAFF_NO_ANSWER, AUTO_RELEASED
`SwitchResolution` (nhân viên chọn): CUSTOMER_AGREED, CUSTOMER_KEEP_OLD, NO_ANSWER
`WaitlistStatus`: WAITING, NOTIFIED, SEATED, CANCELLED
`PaymentMethod`: VIETQR, CASH, CARD, TRANSFER
`RefundMode`: REFUND, VOUCHER

## 2. Public API (Web khách hàng)
| Method & path | Mô tả | Request → Response chính |
|---|---|---|
| `GET /public/config` | Cấu hình hiển thị | `{ restaurantName, logoUrl?, depositAmount, stayMinutes, cleaningMinutes, slotStepMinutes, depositHoldMinutes, fullRefundHoursBefore, allergyOptions[], viewTypes[{viewType,label,description}] }` |
| `GET /public/availability?date&partySize` | Lưới giờ | `{ date, slots:[{ time, viewTypes:[{ viewType, isAvailable, availableTables, reason? }] }] }` (`reason`: PAST, TOO_SOON, BOOKED, NO_CAPACITY) |
| `POST /public/reservations` | Tạo đặt bàn + giữ chỗ cọc | `{ customerName, phone, email?, date, time, partySize, viewType, note?, allergies[], occasion?, babyChair?, wishlist?:{ enabled, targetViewType } }` → `201 { code, status, table:{name,viewType,areaName}, startsAt, deposit:{ amount, expiresAt, transferContent, qrPayload, qrImageUrl?, bank:{ bankName, accountNo, accountName } } }` |
| `GET /public/reservations/{code}?phone=` | Tra cứu/trạng thái | `{ code, status, depositStatus, startsAt, partySize, table, canCancel, refundPreview:{ eligible, mode[], amount } , wishlist?, switchOffer? }` |
| `POST /public/reservations/{code}/cancel` | Hủy | `{ phone, refundMode }` → `{ status, depositStatus, voucherCode? }` |
| `POST /public/reservations/{code}/reschedule` | Dời | `{ phone, date, time }` → reservation mới/cập nhật |
| `GET /public/menu` | Thực đơn | `{ categories:[{ id,name,items:[{ id,name,price,imageUrl,description,isAvailable }] }] }` |
| `GET /public/tables/{tableCode}/session?sig=` | Phiên QR | `{ tableName, areaName, isOpen, orderId? }` — lỗi `QR_SIGNATURE_INVALID`/`TABLE_NOT_OPEN` |
| `POST /public/tables/{tableCode}/orders?sig=` | Gửi món | `{ items:[{ itemId, qty, note?, allergyTags[] }], note? }` → `{ orderId, items:[{ detailId, status }] }` (`409 OUT_OF_STOCK_ITEM` kèm `itemId`) |
| `GET /public/tables/{tableCode}/orders?sig=` | Theo dõi món | `{ orderId, items:[{ detailId,name,qty,status }] }` |

Webhook (không JWT): `POST /webhooks/payment/{provider}`, `POST /webhooks/zalo`.

## 3. Staff API (JWT)
| Nhóm | Endpoint | Quyền |
|---|---|---|
| Auth | `POST /auth/login {username,password}` → `{ accessToken, expiresAt, user:{id,fullName,role} }` · `GET /auth/me` | Public / mọi vai |
| Sơ đồ | `GET /floor?areaId?` → `FloorPlanDto` (§5) | M,C,W |
| | `PUT /manage/tables/{id}/layout {x,y,width,height,shape}` · CRUD `/manage/areas`, `/manage/tables` · `GET /manage/tables/{id}/qr` | M |
| Bàn | `POST /tables/{id}/open {partySize, reservationId?, force?}` · `POST /tables/{id}/mark-cleaned` · `POST /tables/{id}/request-cleaning` | M,C (mở) / M,C,W (dọn) |
| | `POST /tables/{id}/move {toTableId}` · `POST /tables/merge {primaryTableId, mergedTableIds[]}` · `POST /tables/{id}/unmerge` | M,C |
| Đặt bàn | `GET /reservations?date&status&q` · `POST /reservations` (đặt qua điện thoại) · `GET /reservations/{id}` | M,C |
| | `POST /reservations/{id}/check-in {tableId?}` · `/cancel {refundMode}` · `/no-show` · `PUT /reservations/{id}/guest-count {partySize}` → `{ ok, needsExtraChairs, suggestions:[TableDto] }` · `POST /reservations/{id}/apply-apology` · `POST /reservations/{id}/refund-record {amount, note}` | M,C |
| Hàng chờ | `GET /waitlist` · `POST /waitlist {name,phone,partySize}` · `POST /waitlist/{id}/notify` · `/seat {tableId}` · `/cancel` | M,C (W: GET) |
| Đơn | `GET /orders/{id}` · `POST /orders/{id}/items {items[]}` · `PATCH /orders/{id}/items/{detailId} {qty?,status?:CANCELLED}` · `POST /orders/{id}/request-bill` | M,C,W |
| Bếp | `GET /kitchen/tickets?status` · `POST /kitchen/items/{detailId}/status {status}` | M,K |
| Món | `POST /menu/items/{id}/availability {isAvailable}` | M,C,K |
| Hóa đơn | `GET /orders/{id}/invoice-preview?voucherCode&discountPercent` · `POST /orders/{id}/pay {method, voucherCode?, discountPercent?, discountReason?, amountTendered?, refundConfirmed?}` → `InvoiceDto` · `GET /invoices/{id}/receipt` → `ReceiptModel` · `GET /orders/{id}/kitchen-ticket?detailIds` → `KitchenTicketModel` | M,C |
| AI/Leo thang | `GET /switch-requests?status` · `GET /switch-requests/{id}` (có `timeline[]`) · `POST /switch-requests/{id}/staff-resolve {resolution}` | M,C |
| Kho | `GET/POST/PUT /inventory/ingredients` · `POST /inventory/ingredients/{id}/adjust {delta, reason}` · `GET /inventory/movements` · `GET/PUT /menu/items/{id}/recipe` | M |
| Thực đơn | CRUD `/manage/categories`, `/manage/menu-items` | M |
| Người dùng | CRUD `/manage/users` | M |
| Ảnh (Storage) | `POST /manage/menu-items/{id}/image` (multipart `file`) → `{ imageUrl }` · `DELETE /manage/menu-items/{id}/image` · `POST /manage/areas/{id}/floor-plan-image` · `DELETE …/floor-plan-image` · `POST /manage/branding/logo` | M |
| Cấu hình | `GET/PUT /manage/settings` | M |
| Báo cáo | `GET /reports/revenue?from&to&groupBy=day|month` · `/reports/deposits` · `/reports/top-items` · `/reports/low-stock` · `/reports/cash-fund` · `/reports/items-without-recipe` | M |

**Dev-only** (chỉ môi trường Development; trả 404 ở Production):
`POST /dev/seed-demo` · `POST /dev/simulate-payment {code}` · `POST /dev/simulate-zalo-reply {phone,text}` · `GET /dev/zalo-outbox?phone=` · `POST /dev/trigger-early-vacate {tableId}` · `POST /dev/advance-clock {minutes}` (chỉ khi dùng đồng hồ giả).

**Upload ảnh** (`multipart/form-data`, trường `file`): chỉ `image/jpeg|png|webp`, ≤ `Storage.MaxImageBytes` (mặc định 2 MB), server kiểm tra **magic bytes** (không tin `Content-Type`/đuôi file), đặt tên ngẫu nhiên, ghi vào bucket tương ứng (`menu-images`, `floor-plans`, `branding`). Lỗi: `VALIDATION_ERROR` (sai kiểu/quá cỡ), `FORBIDDEN_ROLE`. Thay ảnh ⇒ server xóa object cũ.

## 4. DTO chính
```jsonc
// TableDto
{ "id":201, "code":"T201", "name":"Bàn 201", "areaId":2, "areaName":"Tầng 2", "seats":4, "viewType":"WINDOW",
  "shape":"RECT", "x":120, "y":80, "width":90, "height":90,
  "displayStatus":"HELD", "statusSince":"2026-10-20T11:15:00Z",
  "nextReservation": { "reservationId":55, "code":"RES008899", "guestName":"Anh Long", "startsAt":"2026-10-20T12:00:00Z", "partySize":4 },
  "activeOrder": { "orderId":901, "openedAt":"…", "partySize":3, "runningTotal":350000 },
  "hold": { "requestId":12, "phase":"AWAITING_CUSTOMER|ESCALATED", "expiresAt":"…", "customerName":"Anh Long" },
  "flags": { "overstay":false, "needsExtraChairs":false, "mergedWith":[] } }

// FloorPlanDto
{ "serverTime":"…", "areas":[{ "id":2, "name":"Tầng 2", "floorPlanImageUrl":"https://…/floor-plans/t2.webp" }], "tables":[ TableDto ],
  "counts": { "AVAILABLE":8, "RESERVED":3, "OCCUPIED":5, "CLEANING":1, "HELD":1 } }

// SwitchRequestDto
{ "id":12, "status":"ESCALATED", "reservation":{ "id":55, "code":"RES008899", "guestName":"Anh Long", "phone":"0987654321", "startsAt":"…", "partySize":4 },
  "fromTable":{ "id":104, "name":"Bàn 104" }, "toTable":{ "id":201, "name":"Bàn 201" },
  "offeredAt":"…", "expiresAt":"…", "escalatedAt":"…", "escalationExpiresAt":"…", "messageText":"…",
  "timeline":[ { "at":"…", "step":"CANDIDATES_FOUND|RANKED|HOLD_STARTED|MESSAGE_SENT|MESSAGE_FAILED|CUSTOMER_REPLIED|HOLD_EXPIRED|ESCALATED|STAFF_RESOLVED|SWAPPED|RELEASED", "text":"…" } ] }

// InvoiceDto
{ "id":77, "invoiceNo":"HD20261020-0007", "subtotal":130000, "discount":0, "vat":0, "depositApplied":200000,
  "netAmount":-70000, "refundDue":70000, "status":"PAID", "paidAt":"…" }

// ReceiptModel (in K80)
{ "restaurantName":"…", "address":"…", "invoiceNo":"…", "tableName":"Bàn 201", "openedAt":"…", "paidAt":"…", "cashierName":"…",
  "lines":[{ "name":"Bò bít tết", "qty":1, "unitPrice":189000, "amount":189000 }],
  "subtotal":0, "discountLabel":"", "discount":0, "vat":0, "depositApplied":200000, "netAmount":-70000, "refundDue":70000,
  "paymentMethod":"CASH", "amountTendered":0, "change":0, "footer":"Cảm ơn quý khách!" }

// KitchenTicketModel (phiếu bếp)
{ "ticketNo":"K-0123", "tableName":"Bàn 201", "areaName":"Tầng 2", "createdAt":"…", "source":"QR|STAFF", "hasAllergy":true,
  "items":[{ "detailId":1, "name":"Bò bít tết", "qty":1, "note":"Chín vừa", "allergyTags":["Đậu phộng"] }] }
```

## 5. SignalR
Hub `/hubs/staff` (JWT, server → client). Hub `/hubs/public` (không JWT; client `JoinReservation(code)`, `JoinMenu()`).

| Event | Payload | Hub | Mục đích |
|---|---|---|---|
| `FloorChanged` | `{ tableIds:[int] }` | staff | Client gọi lại `GET /floor` hoặc nhận `TableStateChanged` |
| `TableStateChanged` | `TableDto` | staff | Cập nhật 1 ô bàn |
| `ReservationChanged` | `{ reservationId, status }` | staff | |
| `DepositConfirmed` | `{ reservationId, code }` | staff + public(reservation) | Web chuyển sang màn "Đặt bàn thành công" |
| `ReservationExpired` | `{ code }` | public(reservation) | |
| `OrderItemsAdded` | `KitchenTicketModel` | staff | Máy `IsPrintHost` tự in phiếu bếp; KDS hiện món |
| `KitchenItemStatusChanged` | `{ detailId, orderId, status }` | staff + public(table) | |
| `MenuAvailabilityChanged` | `{ itemId, isAvailable }` | staff + public(menu) | Web làm mờ món |
| `SwitchOfferStarted` | `SwitchRequestDto` | staff | Bàn chuyển Tím |
| `SwitchOfferEscalated` | `SwitchRequestDto` | staff | **Popup gọi điện** |
| `SwitchOfferResolved` | `SwitchRequestDto` | staff | Đóng popup, bỏ Tím |
| `WaitlistTableReady` | `{ entryId, tableId }` | staff | Mời khách hàng chờ |
| `OverstayWarning` | `{ tableId, reservationId, level:PRE|OVERDUE|APOLOGY, minutes }` | staff | |
| `LowStock` | `{ ingredientId, name, stockQty, minStock }` | staff | |
| `PaymentReviewNeeded` | `{ reservationId, reason }` | staff | Cọc thiếu/trễ/dư cần xử lý |

Client sau khi (re)connect **phải** gọi lại `GET /floor` và các danh sách đang mở để đồng bộ (không giả định không mất event). `FloorPlanDto.serverTime` dùng tính độ lệch đồng hồ cho bộ đếm ngược.
