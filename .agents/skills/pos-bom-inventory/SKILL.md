---
name: pos-bom-inventory
description: Logic POS nhà hàng — order tại bàn và QR order (chữ ký HMAC, bàn phải mở), phiếu bếp/KDS, đồng bộ hết món, hóa đơn với giảm giá/voucher/VAT/khấu trừ cọc (kể cả net âm phải hoàn tiền), trừ kho theo định lượng BOM idempotent trong transaction, đơn vị nguyên liệu, cảnh báo tồn thấp, báo cáo. Dùng khi viết hoặc sửa orders, order_details, invoices, payments loại BILL/REFUND, recipes, ingredients, stock_movements, vouchers, báo cáo doanh thu/kho.
---

# POS, hóa đơn, kho BOM

Nguồn: `docs/03 §8–§11`.

## QR order tại bàn — kiểm tra theo thứ tự
1. `QrSigner.Verify(tableCode, sig)` (HMAC-SHA256 với `tables.qr_secret`, so sánh hằng thời gian) → sai: `QR_SIGNATURE_INVALID`.
2. Bàn phải có order `OPEN` → không: `TABLE_NOT_OPEN` (ngăn người ngoài quán đặt món).
3. Rate-limit theo IP + bàn; giới hạn: ≤ 20 dòng/đơn, `qty` 1–20, ghi chú ≤ 200 ký tự (làm sạch).
4. `Idempotency-Key` (lưu ở `order_details.idempotency_key` hoặc bảng riêng) để chống gửi trùng.
5. Từng món phải `is_active && is_available` → không: `409 OUT_OF_STOCK_ITEM` kèm `itemId`; **giá lấy từ DB** (không tin giá từ client). `imageUrl` món là URL Storage công khai; backend không bao giờ nhận URL ảnh từ client.
6. Tạo `order_details(NEW, source=QR)`, dị ứng → `allergy_note`; phát `OrderItemsAdded(KitchenTicketModel)`.

## Phiếu bếp & KDS
`KitchenTicketModel.hasAllergy = items.Any(allergy)`; mỗi ticket in/hiện **cảnh báo dị ứng nổi bật**. Chuyển trạng thái món hợp lệ: `NEW→COOKING→READY→SERVED`; `CANCELLED` từ NEW/COOKING (ghi lý do). Việc in thực tế do WPF máy `IsPrintHost` thực hiện sau khi nhận event; API đánh dấu `kitchen_printed_at` khi client báo (tùy chọn).

## Hết món
`POST /menu/items/{id}/availability` đổi `is_available` rồi phát `MenuAvailabilityChanged` tới cả hub staff và nhóm public-menu. (Tùy chọn cờ cấu hình: tự đặt hết món khi nguyên liệu không đủ định lượng.)

## Hóa đơn (hàm thuần `InvoiceCalculator`, test bảng-giá-trị)
```
subtotal = Σ qty×unit_price (bỏ CANCELLED/OUT_OF_STOCK)
discount = min(subtotal, voucher | percent | apology)   // làm tròn AwayFromZero tới 1 VND
vat      = round((subtotal − discount) × VatPercent/100)
deposit  = reservation.deposit_status == PAID ? deposit_amount : 0
net      = subtotal − discount + vat − deposit           // CÓ THỂ ÂM
refundDue= max(0, −net)
```
- Thanh toán là **một transaction**: tạo/cập nhật `invoices` → nếu `net < 0` bắt buộc `refundConfirmed` rồi ghi `payments(REFUND)` → `deposit_status=APPLIED` → trừ kho → `orders.status=PAID` → `tables.status=CLEANING` → ghi voucher đã dùng → commit → phát event.
- Gọi `pay` lặp lại cho cùng order ⇒ trả hóa đơn đã có (idempotent), không trừ kho/ghi tiền lần hai.
- Số hóa đơn `HDyyyyMMdd-####`: dùng bảng `daily_counters` với `INSERT … ON CONFLICT (counter_date, name) DO UPDATE SET value = daily_counters.value + 1 RETURNING value` (nguyên tử, chạy trong transaction thanh toán); không dùng `MAX+1`. Ngày tính theo giờ VN.
- Voucher: kiểm hạn, chưa dùng; đánh dấu `redeemed_*` trong cùng transaction. Giảm giá tay yêu cầu quyền Cashier+ và `discount_reason`.
- Giảm giá xin lỗi (`apology_discount_pct`) áp tự động cho order của reservation tương ứng.

## Trừ kho BOM
```csharp
// trong cùng transaction thanh toán, chỉ chạy nếu invoice.StockDeductedAt == null (chỉ mục duy nhất từng phần ux_sm_sale là lưới an toàn)
var usage = details.Where(d => d.Status != Cancelled)
    .SelectMany(d => recipes[d.ItemId].Select(r => (r.IngredientId, Qty: d.Qty * r.Qty)))
    .GroupBy(x => x.IngredientId, (id, g) => (id, total: g.Sum(x => x.Qty)));
foreach (var (id, total) in usage) {
    ingredient.StockQty -= total;                                  // đơn vị gốc g/ml/pcs; cho phép âm nếu AllowNegative
    db.StockMovements.Add(new(id, -total, Sale, invoiceId));       // chỉ mục duy nhất (ref_invoice_id, ingredient_id) WHERE type='SALE' chặn trừ trùng
}
invoice.StockDeductedAt = now;
```
- Đơn vị: lưu/tính ở đơn vị gốc; chuyển sang hiển thị (kg, l…) bằng `display_factor` **chỉ ở DTO**. Không so sánh số khác đơn vị.
- Kho âm: nếu `Stock.AllowNegative=false` ⇒ vẫn **không chặn thanh toán** khách đã ăn; ghi cảnh báo + `LowStock`. Món không có công thức: bỏ qua, liệt kê ở `reports/items-without-recipe`.
- `LowStock` phát khi `stock_qty ≤ min_stock` (chỉ khi vừa vượt ngưỡng, tránh spam).
- Điều chỉnh tay: `inventory/ingredients/{id}/adjust` luôn ghi `stock_movements(ADJUST|WASTE|PURCHASE)` + lý do + người làm.

## Báo cáo (Manager)
Doanh thu theo ngày/tháng (theo **giờ VN**), quỹ cọc (đã thu / đã hoàn / giữ do hủy muộn / đã khấu trừ), top món, tồn thấp, quỹ tiền (tiền mặt/chuyển khoản/thẻ − hoàn), món chưa có định lượng. Truy vấn tổng hợp dùng projection phía DB, có chỉ mục theo `paid_at`.

## Test tối thiểu
Cọc 200k + bill 130k ⇒ net −70k, thiếu `refundConfirmed` ⇒ lỗi · Bò bít tết ×2 ⇒ −400g bò, −100g khoai · `pay` song song ⇒ trừ kho một lần · làm tròn % và VAT · voucher hết hạn/đã dùng · QR: sai chữ ký, bàn chưa mở, món hết, trùng key.
