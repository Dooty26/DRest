import React, { useState } from 'react';
import {
  Button,
  Input,
  Select,
  DatePicker,
  TimePicker,
  Chip,
  Badge,
  StatusPill,
  Card,
  Modal,
  Skeleton,
  EmptyState,
  ErrorState,
  Stepper,
  Countdown,
  QrDisplay,
  Money,
  ResponsiveImage,
  useToast,
} from '../components/ui';
import { TableDisplayStatusType } from '../components/ui/StatusPill';

export const DevUiKitPage: React.FC = () => {
  const { success, warning, error, info } = useToast();

  // State controls for interactive testing
  const [inputText, setInputText] = useState('0987654321');
  const [inputError, setInputError] = useState('');
  const [selectedDate, setSelectedDate] = useState('2026-10-31');
  const [selectedTime, setSelectedTime] = useState('19:00');
  const [selectedChips, setSelectedChips] = useState<string[]>(['Hải sản', 'Đậu phộng']);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [activeStep, setActiveStep] = useState(1);

  // Fake slot data test
  const demoSlots = [
    { time: '17:00', isAvailable: true },
    { time: '17:30', isAvailable: false, reason: 'BOOKED (Khóa 105 phút theo quy tắc Buffer)' },
    { time: '18:00', isAvailable: false, reason: 'BOOKED' },
    { time: '18:30', isAvailable: false, reason: 'BOOKED' },
    { time: '19:00', isAvailable: false, reason: 'BOOKED (Lịch đặt hiện tại)' },
    { time: '19:30', isAvailable: false, reason: 'BOOKED' },
    { time: '20:00', isAvailable: false, reason: 'BOOKED' },
    { time: '20:30', isAvailable: false, reason: 'BOOKED' },
    { time: '21:00', isAvailable: true },
  ];

  const nowMs = Date.now();
  const urgentExpire = nowMs + 45 * 1000; // 45 giây để test màu đỏ nhấp nháy
  const normalExpire = nowMs + 14 * 60 * 1000; // 14 phút
  const pastExpire = nowMs - 1000; // Đã hết hạn

  const toggleChip = (tag: string) => {
    setSelectedChips((prev) =>
      prev.includes(tag) ? prev.filter((t) => t !== tag) : [...prev, tag]
    );
  };

  return (
    <div className="max-w-4xl mx-auto px-4 py-8 space-y-12">
      {/* Tiêu đề & Thông tin Token */}
      <div className="border-b border-border pb-6 text-left">
        <span className="text-xs font-mono font-bold text-brand uppercase tracking-wider bg-brand-light px-2.5 py-1 rounded">
          UI Kit Catalog (Pha 0 - Antigravity)
        </span>
        <h1 className="text-2xl sm:text-3xl font-extrabold text-ink mt-2 tracking-tight">
          Hệ thống Design System &amp; Thành phần Giao diện
        </h1>
        <p className="text-sm text-ink-muted mt-1">
          Đầy đủ 5 trạng thái bàn, bảng màu thương hiệu Warm Amber, a11y touch target ≥ 44px, định dạng VND và xử lý ảnh Supabase Storage.
        </p>
      </div>

      {/* 1. 5 Token màu trạng thái bàn bắt buộc */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          1. 5 Token màu trạng thái bàn (docs/04 §2)
        </h2>
        <p className="text-xs text-ink-muted">
          Bắt buộc dùng chung Web &amp; WPF; màu luôn đi kèm nhãn tiếng Việt và biểu tượng.
        </p>
        <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
          {(['AVAILABLE', 'RESERVED', 'OCCUPIED', 'CLEANING', 'HELD'] as TableDisplayStatusType[]).map((st) => (
            <Card key={st} className="p-3.5 flex flex-col items-center gap-2">
              <span className="text-[10px] font-mono text-ink-subtle">{st}</span>
              <StatusPill status={st} />
            </Card>
          ))}
        </div>
      </section>

      {/* 2. Tiền VND (350.000đ & Hoàn cọc âm) */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          2. Định dạng Tiền VND &amp; Xử lý Âm (docs/04 §3)
        </h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="p-4 flex flex-col justify-between">
            <span className="text-xs text-ink-subtle mb-1">Món ăn thông thường:</span>
            <Money amount={350000} size="lg" align="left" />
          </Card>
          <Card className="p-4 flex flex-col justify-between">
            <span className="text-xs text-ink-subtle mb-1">Tổng bill lớn:</span>
            <Money amount={1890000} size="lg" align="left" />
          </Card>
          <Card className="p-4 flex flex-col justify-between bg-red-50/50 border-red-200">
            <span className="text-xs text-status-occupied font-medium mb-1">Hóa đơn net âm (Cọc &gt; Bill):</span>
            <Money amount={-70000} size="lg" align="left" />
          </Card>
        </div>
      </section>

      {/* 3. Nút bấm (Button) */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          3. Nút bấm (Button) — Chuẩn touch ≥ 44px
        </h2>
        <div className="flex flex-wrap items-center gap-3">
          <Button variant="primary">Primary (Brand)</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="danger">Danger</Button>
          <Button variant="primary" isLoading>Đang xử lý</Button>
          <Button variant="primary" disabled>Bị vô hiệu</Button>
        </div>
      </section>

      {/* 4. Form Controls (Input, Select, DatePicker, TimePicker) */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          4. Form Controls &amp; Khả năng tiếp cận (a11y)
        </h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Input
            label="Số điện thoại khách hàng"
            value={inputText}
            onChange={(e) => {
              setInputText(e.target.value);
              if (e.target.value.length < 10) {
                setInputError('Số điện thoại phải có ít nhất 10 số');
              } else {
                setInputError('');
              }
            }}
            error={inputError}
            helperText="Nhập số điện thoại để nhận thông báo cọc và mã đặt chỗ"
          />

          <Select
            label="Khu vực / Loại View mong muốn"
            options={[
              { value: 'STANDARD', label: 'Tiêu chuẩn (Tầng 1)' },
              { value: 'WINDOW', label: 'Cửa sổ ngắm phố (Bàn 201)' },
              { value: 'BALCONY', label: 'Ban công thoáng mát' },
              { value: 'VIP', label: 'Phòng VIP sang trọng' },
            ]}
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2">
          <DatePicker
            label="Ngày dùng bữa"
            value={selectedDate}
            onChange={setSelectedDate}
          />
          <div>
            <TimePicker
              label="Khung giờ (Lưới 30 phút - Khóa theo quy tắc 105 phút)"
              slots={demoSlots}
              selectedTime={selectedTime}
              onSelect={setSelectedTime}
            />
          </div>
        </div>
      </section>

      {/* 5. Dị ứng & Tag (Chip & Badge) */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          5. Thẻ dị ứng &amp; Huy hiệu (Chip &amp; Badge)
        </h2>
        <div className="flex flex-wrap items-center gap-2">
          {['Hải sản', 'Đậu phộng', 'Sữa/Lactose', 'Trứng', 'Gluten'].map((tag) => (
            <Chip
              key={tag}
              label={tag}
              variant="allergy"
              selected={selectedChips.includes(tag)}
              onClick={() => toggleChip(tag)}
            />
          ))}
          <Badge variant="brand">Bàn VIP</Badge>
          <Badge variant="success">Đã cọc</Badge>
          <Badge variant="danger">Quá giờ</Badge>
        </div>
      </section>

      {/* 6. Tiến trình (Stepper 4 bước đặt bàn) */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          6. Tiến trình đặt bàn (Stepper)
        </h2>
        <Card className="p-4">
          <Stepper
            steps={[
              { id: 1, title: 'Giờ đến', description: 'Chọn ngày & giờ' },
              { id: 2, title: 'Chọn view', description: 'Khu vực bàn' },
              { id: 3, title: 'Thông tin', description: 'Tên & SĐT' },
              { id: 4, title: 'Cọc VietQR', description: 'Quét mã 200k' },
            ]}
            currentStep={activeStep}
            onStepClick={setActiveStep}
          />
        </Card>
      </section>

      {/* 7. Đồng hồ đếm ngược (Countdown) */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          7. Đếm ngược (Countdown) bù lệch đồng hồ
        </h2>
        <div className="flex flex-wrap items-center gap-3">
          <Countdown expiresAt={normalExpire} prefix="Giữ chỗ cọc (15'):" />
          <Countdown expiresAt={urgentExpire} prefix="Cảnh báo gấp (&lt;60s):" />
          <Countdown expiresAt={pastExpire} prefix="Trạng thái:" />
        </div>
      </section>

      {/* 8. Hiển thị mã QR VietQR (QrDisplay) */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          8. Render Mã VietQR (QrDisplay)
        </h2>
        <QrDisplay
          qrPayload="00020101021238540010A000000727012600069704220112RES00889952040000530370454062000005802VN62250821RES008899 09876543216304E1F2"
          transferContent="RES008899 0987654321"
          amount={200000}
        />
      </section>

      {/* 9. Ảnh Supabase Storage (ResponsiveImage - 3 trạng thái: OK, Null placeholder, Lỗi) */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          9. Ảnh Supabase Storage (ResponsiveImage) — 3 trạng thái
        </h2>
        <p className="text-xs text-ink-muted">
          Tỷ lệ cố định 4:3 cho món ăn, không giật layout, lazy load, tự hiển thị placeholder khi null hoặc tải lỗi.
        </p>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="p-3">
            <span className="text-xs font-semibold text-ink block mb-2">1. Tải thành công (URL hợp lệ):</span>
            <ResponsiveImage
              src="https://images.unsplash.com/photo-1544025162-d76694265947?w=500"
              alt="Bò bít tết Wagyu sốt tiêu đen"
              aspectRatio="4/3"
            />
            <div className="text-xs font-medium text-ink mt-2">Bò bít tết Wagyu</div>
            <Money amount={350000} size="sm" align="left" />
          </Card>

          <Card className="p-3">
            <span className="text-xs font-semibold text-ink block mb-2">2. URL rỗng / null (Placeholder):</span>
            <ResponsiveImage
              src={null}
              alt="Súp bí đỏ kem tươi Pháp"
              aspectRatio="4/3"
            />
            <div className="text-xs font-medium text-ink mt-2">Súp bí đỏ kem tươi</div>
            <Money amount={85000} size="sm" align="left" />
          </Card>

          <Card className="p-3">
            <span className="text-xs font-semibold text-ink block mb-2">3. Ảnh lỗi kết nối (Fallback):</span>
            <ResponsiveImage
              src="https://invalid-broken-url.example/broken.jpg"
              alt="Món ăn tải lỗi"
              aspectRatio="4/3"
            />
            <div className="text-xs font-medium text-ink mt-2">Cá hồi áp chảo</div>
            <Money amount={285000} size="sm" align="left" />
          </Card>
        </div>
      </section>

      {/* 10. Trạng thái Loading / Empty / Error */}
      <section className="space-y-4 text-left">
        <h2 className="text-lg font-bold text-ink border-l-4 border-brand pl-3">
          10. Đủ 4 trạng thái (Loading / Empty / Error / Modal &amp; Toast)
        </h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="p-4 space-y-2">
            <span className="text-xs font-semibold text-ink-muted">Skeleton Loading</span>
            <Skeleton className="h-6 w-3/4" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-5/6" />
          </Card>

          <EmptyState
            title="Chưa có món nào"
            description="Hãy quét mã QR trên bàn để bắt đầu gọi món"
            actionText="Xem menu"
            onAction={() => info('Mở danh sách thực đơn')}
          />

          <ErrorState
            title="Lỗi tải bàn"
            message="Không thể kết nối tới máy chủ SignalR"
            onRetry={() => success('Đã kết nối lại thành công')}
          />
        </div>

        <div className="flex flex-wrap items-center gap-3 pt-2">
          <Button variant="outline" onClick={() => setIsModalOpen(true)}>
            Mở Modal Dialog
          </Button>
          <Button variant="secondary" onClick={() => success('Đặt bàn thành công!', 'Thông báo')}>
            Toast Success
          </Button>
          <Button variant="secondary" onClick={() => warning('Bàn 201 sắp có khách tới!', 'Cảnh báo')}>
            Toast Warning
          </Button>
          <Button variant="secondary" onClick={() => error('Khung giờ này vừa có người đặt!', 'Lỗi đặt bàn')}>
            Toast Error
          </Button>
        </div>
      </section>

      {/* Modal Dialog */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Xác nhận chuyển bàn"
        description="Bàn 201 (View cửa sổ) đang trống sớm, bạn có muốn đổi sang bàn này không?"
        footer={
          <>
            <Button variant="outline" size="sm" onClick={() => setIsModalOpen(false)}>
              Giữ bàn cũ
            </Button>
            <Button
              variant="primary"
              size="sm"
              onClick={() => {
                setIsModalOpen(false);
                success('Đã xác nhận đổi sang Bàn 201');
              }}
            >
              Đồng ý đổi bàn
            </Button>
          </>
        }
      >
        <p className="text-xs text-ink-muted leading-relaxed">
          Bàn mới được giữ tối đa trong 10 phút. Nếu bạn không phản hồi trong thời gian này, bàn sẽ được chuyển cho ứng viên tiếp theo trong danh sách nguyện vọng.
        </p>
      </Modal>
    </div>
  );
};
