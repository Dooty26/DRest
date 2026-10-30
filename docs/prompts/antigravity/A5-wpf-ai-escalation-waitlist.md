# A5 — WPF: Giữ bàn Tím, popup leo thang, timeline AI, hàng chờ, quá giờ (Pha 3)

**Skills**: `wpf-xaml-ui-conventions`, `verify-and-handoff`. **Nguồn**: đặc tả mục 4 & 5.2–5.3, `docs/03 §6–§7, §12`, `docs/05` (`EscalationQueueViewModel`, `AiActivityViewModel`, `WaitlistViewModel`).
Đây là phần **gây ấn tượng nhất khi bảo vệ** — làm kỹ và mượt.

## Việc cần làm
1. **Trạng thái Tím** (hoàn thiện từ A3): đếm ngược chính xác theo `expiresAt` (bù lệch đồng hồ do VM xử lý), màu cảnh báo < 60s, nhãn "Đang giữ cho Anh Long"; pha `ESCALATED` hiển thị biểu tượng điện thoại + nhấp nháy.
2. **`EscalationPopup`** (cửa sổ không modal, luôn nổi trên app, nhiều yêu cầu xếp hàng): hiển thị *"Khách Anh Long chưa xem tin nhắn Zalo"*, **SĐT lớn + nút sao chép**, bàn đề xuất, giờ đặt, thời gian đã chờ + thời gian còn lại của cửa sổ leo thang, tin đã gửi cho khách. **3 nút 1-click** đúng đặc tả: **[Khách đồng ý đổi]** · **[Khách giữ bàn cũ]** · **[Không nghe máy → giữ nguyên bàn cũ, nhả bàn đẹp]** (hộp xác nhận nhẹ cho hành động đổi bàn). Âm báo + nhấp nháy thanh tác vụ; không tự đóng khi chưa xử lý; chấm đỏ số lượng ở chuông Shell.
3. **`AiActivityView`** (M,C): danh sách yêu cầu đổi bàn + **timeline dọc** (Lọc ứng viên → Xếp hạng (lý do) → Giữ bàn → Gửi Zalo → Chờ phản hồi → Leo thang → Kết quả), mỗi bước có giờ và nội dung; xem **tin nhắn AI đã gửi**; trạng thái cuối bằng huy hiệu màu. Dùng được làm màn trình diễn demo.
4. **Hàng chờ `WaitlistView`**: thêm (tên, SĐT, số khách), danh sách theo thứ tự + thời gian chờ; khi nhận `WaitlistTableReady` → toast/hộp thoại *"Bàn 105 đã dọn xong — mời khách thứ 1 (Chị Mai, 4 người)"* với nút Mời / Xếp bàn (chọn bàn) / Bỏ qua; hiển thị gợi ý bàn phù hợp.
5. **Quá giờ**: huy hiệu cảnh báo trên ô bàn theo `OverstayWarning` (PRE → vàng "sắp tới giờ khách đặt", OVERDUE → cam, APOLOGY → đỏ); khi **"Chờ bàn phát sinh"** hiển thị panel hướng dẫn + nút **In phiếu tặng nước & áp giảm 10%** (`ApplyApologyCommand`), trạng thái đã áp.
6. **Hộp thoại lệch số khách** (từ A4 tích hợp vào sơ đồ): gợi ý danh sách bàn lớn hơn hoặc nút "Kê thêm ghế phụ".
7. **Trung tâm thông báo**: lịch sử toast (đơn QR mới, cọc xác nhận, `PaymentReviewNeeded`, `LowStock`…) với bộ lọc.

## Nghiệm thu (Windows)
Với `--fake`: `FakeRealtimeClient` kích bản → bàn Tím đếm ngược → hết giờ → popup leo thang → bấm 3 nút cho ra 3 kết quả khác nhau ở timeline. Với API thật (sau C7/C8) và `/dev/*`: chạy được Demo bước 2→4. Ảnh/clip ngắn → `docs/evidence/A5/`. HANDOFF.
