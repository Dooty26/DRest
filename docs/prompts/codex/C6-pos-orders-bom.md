# C6 — POS, QR order, KDS, hóa đơn (cọc âm), trừ kho BOM, báo cáo (Pha 2)

**Skills**: `pos-bom-inventory`, `postgres-concurrency-booking`, `restaurant-domain-rules`, `dotnet-api-conventions`, `verify-and-handoff`. **Nguồn**: `docs/03 §8–§11`.

## Việc cần làm
1. **Order**: mở khi check-in/walk-in (đã có ở C3) · `GET/POST /orders/{id}…` thêm/sửa/hủy món (nhân viên) · QR order `GET/POST /public/tables/{code}/…`: xác minh chữ ký, bàn phải có order `OPEN` (`TABLE_NOT_OPEN`), rate-limit theo IP+bàn, `Idempotency-Key`, chặn món `is_available=false` (`OUT_OF_STOCK_ITEM`), giới hạn số lượng/ghi chú.
2. **Bếp/KDS**: tạo `KitchenTicketModel` (cảnh báo dị ứng nổi bật), phát `OrderItemsAdded`; `GET /kitchen/tickets`, `POST /kitchen/items/{id}/status`; phát `KitchenItemStatusChanged` (cả group public của bàn).
3. **Hết món đồng bộ**: `POST /menu/items/{id}/availability` → phát `MenuAvailabilityChanged` (staff + public menu). (Tùy chọn cờ `Menu.AutoUnavailableWhenStockLow`.)
4. **Hóa đơn**: `invoice-preview` & `pay` theo công thức `docs/03 §10` (giảm giá/voucher/VAT/**cọc khấu trừ**/**net âm**). Net âm → bắt buộc `refundConfirmed=true`, ghi `payments(REFUND)`, `deposit_status=APPLIED`. Số hóa đơn `HDyyyyMMdd-####` sinh bằng `daily_counters` (`INSERT … ON CONFLICT … RETURNING`), không trùng khi đồng thời. `GET /invoices/{id}/receipt` → `ReceiptModel`.
5. **Trừ kho BOM** trong **cùng transaction** với thanh toán: cộng dồn theo nguyên liệu, `stock_movements(SALE)`, cờ `stock_deducted_at`, idempotent; kho âm theo `Stock.AllowNegative`; phát `LowStock`.
6. Sau thanh toán: order `PAID`, bàn `CLEANING`, phát `TableVacatedEarly` nếu đủ điều kiện (**C7 tiêu thụ sự kiện**; ở đây chỉ phát qua `IDomainEventBus`).
7. **Kho/công thức/thực đơn** (Manager): `inventory/*`, `menu/items/{id}/recipe`, điều chỉnh kho có lý do.
8. **Báo cáo** (Manager): doanh thu theo ngày/tháng, quỹ cọc, top món, tồn thấp, quỹ tiền, món chưa có định lượng.
9. Cập nhật `Client.Core`: `PosViewModel`, `CheckoutViewModel`, `KitchenViewModel`, `InventoryViewModel`, `RecipeEditorViewModel`, `ReportsViewModel`, `PrintCoordinator` (khi nhận `OrderItemsAdded` và `IsPrintHost` → gọi `IPrintService`).

## Test bắt buộc
- Kịch bản đặc tả: cọc 200.000, tiêu 130.000 → `net=-70.000`, `refundDue=70.000`; thiếu `refundConfirmed` → lỗi.
- Bò bít tết ×2 → thịt bò −400g, khoai tây −100g; thanh toán **hai lần song song** → kho chỉ trừ 1 lần.
- QR: chữ ký sai/bàn chưa mở/món hết/gửi trùng key.
- Làm tròn giảm giá %, VAT, voucher hết hạn/đã dùng.

## Nghiệm thu
Xanh · HANDOFF kèm `curl` mẫu cho QR order, pay (cọc âm), kitchen ticket để Antigravity đối chiếu UI.
