---
name: wpf-xaml-ui-conventions
description: Quy ước viết giao diện WPF (XAML) cho phần mềm POS nhà hàng — MVVM thuần (không logic trong code-behind), ResourceDictionary token màu trạng thái bàn, Canvas/ItemsControl cho sơ đồ bàn, DataTemplate theo ViewModel, converter, attached behavior (kéo-thả, PasswordBox), design-time data, ảo hóa, cảm ứng, popup leo thang, in FlowDocument 80mm cho K80, MaterialDesignThemes. Dùng khi tạo hoặc sửa bất kỳ View/Style/Control/Converter/Print layout nào trong src/Restaurant.Wpf.
---

# Quy ước WPF XAML

## Ranh giới
Bạn chỉ sở hữu `src/Restaurant.Wpf/**`. ViewModel/service nằm ở `Restaurant.Client.Core` (của Codex) — **chỉ bind**, không sửa. Cần thuộc tính/lệnh mới ⇒ ghi HANDOFF.

## Cấu trúc
```
Restaurant.Wpf/
  App.xaml(.cs)            bootstrap (do Codex tạo ở C0; sau đó của bạn)
  Styles/ Tokens.xaml Theme.xaml Controls.xaml
  Controls/ TableTile, StatusBadge, MoneyText, NavRail, ToastHost, DialogHost…
  Converters/ …
  Behaviors/ PasswordBoxBehavior, DragDropMergeBehavior, FocusOnLoad…
  Views/ <Tên>View.xaml(.cs)    # code-behind chỉ InitializeComponent
  Windows/ ShellWindow, EscalationPopupWindow, ReceiptPreviewWindow
  Services/ WpfUiDispatcher, WpfDialogService, WpfPrintService, WpfNotificationSound, WpfClipboardService
  Print/ ReceiptDocument, KitchenTicketDocument
```

## Quy tắc
- **MVVM thuần**: không logic nghiệp vụ trong code-behind; sự kiện UI → `Command`/attached behavior. Chọn View theo VM bằng `DataTemplate DataType="{x:Type vm:PosViewModel}"` trong `ContentControl` của Shell.
- Màu/kích thước lấy từ `Tokens.xaml` bằng `DynamicResource`; không hard-code hex trong View. 5 màu trạng thái đúng `docs/04 §2` + nền nhạt; luôn kèm **chữ + biểu tượng**.
- **Design-time data** cho mọi View: `d:DataContext="{d:DesignInstance Type=vm:…, IsDesignTimeCreatable=False}"` hoặc `d:DesignData`; để dùng `FakeRestaurantApi` tạo dữ liệu mẫu.
- **Sơ đồ bàn**: `ItemsControl` + `Canvas` (`ItemsPanel`), `ItemContainerStyle` đặt `Canvas.Left/Top` bind `X/Y`; `TableTile` là `UserControl`/`ControlTemplate` bind `DisplayStatus`; đếm ngược bind `HoldRemainingText` (VM tự cập nhật — **không** tạo `DispatcherTimer` trong View, trừ hiệu ứng thuần hình ảnh bằng `Storyboard`).
- **Hiệu năng**: `VirtualizingStackPanel` cho danh sách dài; `Freeze()` Brush/Geometry tĩnh; tránh `Binding` kiểu `UpdateSourceTrigger=PropertyChanged` không cần thiết; `x:Shared`; không dựng lại toàn bộ collection khi một phần tử đổi; không chặn UI thread.
- **Cảm ứng/đọc xa**: mục tiêu chạm ≥ 44×44 DIP, khoảng cách đủ, chữ ≥ 14pt cho tiền/số bàn, phản hồi nhấn rõ; hỗ trợ bàn phím (`TabIndex`, `AccessKey`, Enter/Esc trong dialog).
- **Tiền/số**: dùng `VndConverter` (`350.000đ`, số âm đỏ kèm nhãn "Hoàn lại khách"); `StringFormat` theo `vi-VN` (đặt `FrameworkElement.LanguageProperty` ở App start).
- **Dialog**: qua `IDialogService` (MaterialDesign `DialogHost` hoặc `Window` modal); hành động hủy/đổi bàn có xác nhận 2 bước nhẹ.
- **Popup leo thang**: `Window` không modal, `Topmost`, không chiếm focus ngoài ý muốn nhưng nhấp nháy `FlashWindowEx`; xếp hàng nhiều yêu cầu; **không tự đóng** khi chưa xử lý; nút 3 hành động đúng nhãn đặc tả.
- **Phân quyền hiển thị**: bind `Visibility`/`IsEnabled` vào `Can…` của VM; server vẫn kiểm quyền.
- **Ảnh từ URL Storage**: `Image` bind `ImageUrl` qua converter tạo `BitmapImage` (`CacheOption=OnLoad`, `CreateOptions=IgnoreColorProfile`, `DecodePixelWidth` theo kích thước hiển thị, `Freeze()`), tải bất đồng bộ, lỗi/`null` → ảnh placeholder; không chặn UI thread. Logo in hóa đơn: tải về cache đĩa (`IImageCache`) rồi in, hỏng thì bỏ qua logo chứ không hỏng hóa đơn. Upload ảnh: chỉ chọn file qua `IFilePickerService`, gọi lệnh VM; không có khóa Supabase nào trong WPF.
- **A11y**: `AutomationProperties.Name` cho nút icon, tương phản ≥ 4.5:1, không chỉ dựa vào màu.

## In K80 (80mm) bằng FlowDocument
- `FlowDocument { PageWidth = 302, PagePadding = new Thickness(6), ColumnWidth = 9999, FontFamily = "Segoe UI" }` (80mm ≈ 302 DIP ở 96dpi; vùng in thực ~72mm — đặt lề phù hợp máy). Dùng font có đủ dấu tiếng Việt, **không** dùng ESC/POS thô.
- Dựng `FlowDocument` trên **UI thread (STA)**. Layout dòng món bằng `Table`/`Grid` hai cột (tên | thành tiền), số căn phải.
- Hóa đơn: tên nhà hàng, địa chỉ, số HĐ, bàn, giờ, thu ngân, dòng món, tạm tính, giảm, VAT, **cọc đã trừ**, **thành tiền**; nếu `refundDue > 0` in khối nổi bật **"HOÀN LẠI KHÁCH: 70.000đ"**; lời cảm ơn.
- Phiếu bếp: tên bàn + khu to, giờ, món cỡ lớn, `Note` thụt dòng, khối **"⚠ DỊ ỨNG: …"** nền đen chữ trắng viền đậm; ghi nguồn (QR/NV).
- In im lặng tới máy đã chọn: lấy `PrintQueue` theo tên (`LocalPrintServer`), gán vào `PrintDialog.PrintQueue`, gọi `PrintDocument(paginator, title)`; có cửa sổ xem trước và nút in lại. Lỗi máy in ⇒ toast + nút thử lại, không mất đơn.

## Bằng chứng
Chạy `dotnet run --project src/Restaurant.Wpf -- --fake`, chụp từng trạng thái chính (5 màu bàn, Tím đếm ngược, popup leo thang, POS cọc âm, preview K80, KDS) → `docs/evidence/<prompt-id>/`. Kiểm ở 1366×768 và 1920×1080.
