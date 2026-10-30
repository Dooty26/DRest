---
name: web-react-ui-conventions
description: Quy ước xây giao diện Web khách hàng bằng React 18 + Vite + TypeScript + Tailwind cho nhà hàng — cấu trúc thư mục, design tokens, form/validation, gọi API bằng type sinh từ OpenAPI, MSW mock, SignalR hook, định dạng tiền/giờ VN, mobile-first, a11y, đủ trạng thái loading/empty/error, kiểm thử bằng trình duyệt và Playwright. Dùng khi tạo hoặc sửa bất kỳ trang/thành phần nào trong web/ (đặt bàn, thực đơn, QR order, Zalo simulator, ui-kit).
---

# Quy ước Web React

## Cấu trúc
```
web/src/
  api/        client openapi-fetch, schema.d.ts (sinh), errors.ts (code→thông báo VN)
  mocks/      handlers MSW + dữ liệu mẫu (bật bằng VITE_USE_MOCK)
  components/ ui/ (Button, Input, Chip, Modal, Toast, Skeleton, Countdown, QrDisplay, Money…)
  features/   booking/, menu/, qr-order/, dev/ (mỗi feature: components, hooks, schemas)
  hooks/      useSignalR, useServerClock, useCountdown
  lib/        format.ts (tiền/giờ), idempotency.ts, storage.ts (try/catch)
  pages/      route components mỏng
  styles/     tokens.css (CSS variables) + tailwind config đọc token
```

## Quy tắc
- **Không viết logic nghiệp vụ**: không tự tính slot bị khóa, tiền cọc, hoàn tiền, trạng thái bàn — chỉ hiển thị dữ liệu server trả về.
- Type API sinh từ `docs/openapi/openapi.v1.json` (`npm run gen:api`); không tự gõ tay kiểu response. Thiếu field ⇒ ghi HANDOFF, tạm mock.
- Lỗi: ánh xạ `ProblemDetails.code` → câu tiếng Việt (`SLOT_UNAVAILABLE` → "Khung giờ này vừa có người đặt, vui lòng chọn giờ khác"). Không hiện mã thô/stack.
- Form: react-hook-form + zod; SĐT VN `^(0|\+84)\d{9}$`; hiện lỗi cạnh trường, `aria-invalid`, `aria-describedby`; không mất dữ liệu khi lỗi mạng.
- `Idempotency-Key` (`crypto.randomUUID()`) tạo khi bắt đầu thao tác, giữ nguyên khi thử lại.
- Định dạng: `Intl.NumberFormat('vi-VN')` + "đ" (`350.000đ`); giờ/ngày `Intl.DateTimeFormat('vi-VN', { timeZone: 'Asia/Ho_Chi_Minh' })`; thời điểm API là UTC.
- Đếm ngược: `useCountdown(expiresAt)` bù lệch đồng hồ (`useServerClock`), cập nhật bằng `requestAnimationFrame`/1s, đổi màu < 60s, `aria-live="polite"` (không đọc mỗi giây — chỉ thông báo mốc).
- SignalR: `useSignalR(hub, handlers)` tự reconnect; **luôn có polling dự phòng**; dọn kết nối khi unmount.
- Lưu trữ cục bộ qua `lib/storage.ts` (bọc try/catch, chấp nhận rỗng). Không lưu dữ liệu thanh toán/PII quá mức; không `dangerouslySetInnerHTML`.
- **Ảnh Supabase Storage**: dùng thành phần `ResponsiveImage` (khung tỉ lệ cố định, skeleton, `onError` → placeholder, `loading="lazy"`, `decoding="async"`, `width/height`). URL lấy nguyên từ API; **không** cài Supabase SDK, không có biến môi trường Supabase trong `web/`. CSP `img-src` cho phép `https://*.supabase.co`.
- Dev tools (`/dev/*`) chỉ xuất hiện khi `import.meta.env.VITE_ENABLE_DEV_TOOLS === 'true'` và bị loại khỏi bản production.

## Nội dung hiển thị
Tuân thủ `.agents/rules/ui-copy-discipline.md`: không chữ dành cho dev trên UI; không đoạn giới thiệu; nhãn ngắn; mã enum luôn ánh xạ sang tiếng Việt (`lib/labels.ts`); chạy `node scripts/check-ui-copy.mjs` trước khi báo xong.

## Thiết kế & a11y
- Mobile-first (390px là chuẩn), kiểm thêm 768 và 1280. Mục chạm ≥ 44px. Tương phản ≥ 4.5:1. Focus ring rõ. Điều hướng bàn phím đủ. Màu trạng thái luôn kèm chữ/biểu tượng.
- Mỗi màn/bước đủ 4 trạng thái: loading (skeleton, không nhảy bố cục), empty, error (có "Thử lại"), success.
- Văn phong lịch sự, ngắn, nhất quán xưng hô. Ảnh có `alt`, `loading="lazy"`, kích thước cố định để tránh CLS.
- Dùng token trong `tokens.css`; không hard-code hex ngoài file token.

## Kiểm thử & bằng chứng
- Vitest cho `lib/` và hook; Playwright smoke cho luồng chính trên MSW (chạy ở 390px và 1280px).
- Dùng **trình duyệt** của agent để chạy thật luồng, chụp ảnh từng trạng thái vào `docs/evidence/<prompt-id>/`.
- `npm run lint && npm run build` phải sạch; Lighthouse mobile cho `/dat-ban` và `/t/…` (mục tiêu ≥ 90 Performance/Accessibility ở A6).
