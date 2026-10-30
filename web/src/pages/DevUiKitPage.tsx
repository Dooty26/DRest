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

  const [inputText, setInputText] = useState('0987654321');
  const [inputError, setInputError] = useState('');
  const [selectedDate, setSelectedDate] = useState('2026-10-31');
  const [selectedTime, setSelectedTime] = useState('19:00');
  const [selectedChips, setSelectedChips] = useState<string[]>(['Hải sản', 'Đậu phộng']);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [activeStep, setActiveStep] = useState(1);

  const demoSlots = [
    { time: '17:00', isAvailable: true },
    { time: '17:30', isAvailable: false, reason: 'Đã kín' },
    { time: '18:00', isAvailable: false, reason: 'Đã kín' },
    { time: '18:30', isAvailable: false, reason: 'Đã kín' },
    { time: '19:00', isAvailable: false, reason: 'Đã kín' },
    { time: '19:30', isAvailable: false, reason: 'Đã kín' },
    { time: '20:00', isAvailable: false, reason: 'Đã kín' },
    { time: '20:30', isAvailable: false, reason: 'Đã kín' },
    { time: '21:00', isAvailable: true },
  ];

  const nowMs = Date.now();
  const urgentExpire = nowMs + 45 * 1000;
  const normalExpire = nowMs + 14 * 60 * 1000;
  const pastExpire = nowMs - 1000;

  const toggleChip = (tag: string) => {
    setSelectedChips((prev) =>
      prev.includes(tag) ? prev.filter((t) => t !== tag) : [...prev, tag]
    );
  };

  const statusList: TableDisplayStatusType[] = [
    'AVAILABLE',
    'RESERVED',
    'OCCUPIED',
    'CLEANING',
    'HELD',
  ];

  return (
    <div className="max-w-4xl mx-auto px-4 py-8 space-y-10">
      {/* Trạng thái bàn */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Trạng thái bàn</h2>
        <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
          {statusList.map((st) => (
            <Card key={st} className="p-3.5 flex items-center justify-center">
              <StatusPill status={st} />
            </Card>
          ))}
        </div>
      </section>

      {/* Tiền tệ */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Tiền tệ</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="p-4 flex flex-col justify-between">
            <span className="text-xs text-ink-subtle mb-1">Tạm tính:</span>
            <Money amount={350000} size="lg" align="left" />
          </Card>
          <Card className="p-4 flex flex-col justify-between">
            <span className="text-xs text-ink-subtle mb-1">Tổng cộng:</span>
            <Money amount={1890000} size="lg" align="left" />
          </Card>
          <Card className="p-4 flex flex-col justify-between bg-red-50/50 border-red-200">
            <span className="text-xs text-status-occupied font-medium mb-1">Hoàn lại khách:</span>
            <Money amount={-70000} size="lg" align="left" />
          </Card>
        </div>
      </section>

      {/* Nút */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Nút</h2>
        <div className="flex flex-wrap items-center gap-3">
          <Button variant="primary">Đặt bàn</Button>
          <Button variant="secondary">Đổi bàn</Button>
          <Button variant="outline">Đóng</Button>
          <Button variant="ghost">Bỏ qua</Button>
          <Button variant="danger">Hủy</Button>
          <Button variant="primary" isLoading>Đang lưu</Button>
          <Button variant="primary" disabled>Khóa</Button>
        </div>
      </section>

      {/* Ô nhập & Chọn giờ */}
      <section className="space-y-4 text-left">
        <h2 className="text-base font-bold text-ink">Biểu mẫu</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Input
            label="Số điện thoại"
            value={inputText}
            onChange={(e) => {
              setInputText(e.target.value);
              if (e.target.value.length < 10) {
                setInputError('Cần đủ 10 chữ số');
              } else {
                setInputError('');
              }
            }}
            error={inputError}
            helperText="Nhập số điện thoại để nhận thông tin bàn"
          />

          <Select
            label="Khu vực"
            options={[
              { value: 'STANDARD', label: 'Tầng 1' },
              { value: 'WINDOW', label: 'Cửa sổ' },
              { value: 'BALCONY', label: 'Ban công' },
              { value: 'VIP', label: 'Phòng riêng' },
            ]}
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-1">
          <DatePicker
            label="Ngày đến"
            value={selectedDate}
            onChange={setSelectedDate}
          />
          <div>
            <TimePicker
              label="Chọn giờ"
              slots={demoSlots}
              selectedTime={selectedTime}
              onSelect={setSelectedTime}
            />
          </div>
        </div>
      </section>

      {/* Nhãn */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Nhãn</h2>
        <div className="flex flex-wrap items-center gap-2">
          {['Hải sản', 'Đậu phộng', 'Sữa', 'Trứng', 'Gluten'].map((tag) => (
            <Chip
              key={tag}
              label={tag}
              variant="allergy"
              selected={selectedChips.includes(tag)}
              onClick={() => toggleChip(tag)}
            />
          ))}
          <Badge variant="brand">Ưu tiên</Badge>
          <Badge variant="success">Đã cọc</Badge>
          <Badge variant="danger">Quá giờ</Badge>
        </div>
      </section>

      {/* Tiến trình */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Tiến trình</h2>
        <Card className="p-4">
          <Stepper
            steps={[
              { id: 1, title: 'Giờ đến' },
              { id: 2, title: 'Chọn bàn' },
              { id: 3, title: 'Thông tin' },
              { id: 4, title: 'Đặt cọc' },
            ]}
            currentStep={activeStep}
            onStepClick={setActiveStep}
          />
        </Card>
      </section>

      {/* Đếm ngược */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Đếm ngược</h2>
        <div className="flex flex-wrap items-center gap-3">
          <Countdown expiresAt={normalExpire} prefix="Thời gian giữ bàn:" />
          <Countdown expiresAt={urgentExpire} prefix="Sắp hết hạn:" />
          <Countdown expiresAt={pastExpire} prefix="Hết giờ:" />
        </div>
      </section>

      {/* Mã thanh toán */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Mã thanh toán</h2>
        <QrDisplay
          qrPayload="00020101021238540010A000000727012600069704220112RES00889952040000530370454062000005802VN62250821RES008899 09876543216304E1F2"
          transferContent="RES008899 0987654321"
          amount={200000}
          title="Quét mã để đặt cọc"
        />
      </section>

      {/* Hình ảnh */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Hình ảnh</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="p-3">
            <span className="text-xs font-semibold text-ink block mb-2">Ảnh món ăn</span>
            <ResponsiveImage
              src="https://images.unsplash.com/photo-1544025162-d76694265947?w=500"
              alt="Bò bít tết"
              aspectRatio="4/3"
            />
            <div className="text-xs font-medium text-ink mt-2">Bò bít tết</div>
            <Money amount={350000} size="sm" align="left" />
          </Card>

          <Card className="p-3">
            <span className="text-xs font-semibold text-ink block mb-2">Chưa có ảnh</span>
            <ResponsiveImage
              src={null}
              alt="Súp bí đỏ"
              aspectRatio="4/3"
            />
            <div className="text-xs font-medium text-ink mt-2">Súp bí đỏ</div>
            <Money amount={85000} size="sm" align="left" />
          </Card>

          <Card className="p-3">
            <span className="text-xs font-semibold text-ink block mb-2">Tải lỗi</span>
            <ResponsiveImage
              src="https://invalid-broken-url.example/broken.jpg"
              alt="Cá hồi áp chảo"
              aspectRatio="4/3"
            />
            <div className="text-xs font-medium text-ink mt-2">Cá hồi áp chảo</div>
            <Money amount={285000} size="sm" align="left" />
          </Card>
        </div>
      </section>

      {/* Trạng thái hiển thị */}
      <section className="space-y-3 text-left">
        <h2 className="text-base font-bold text-ink">Trạng thái hiển thị</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="p-4 space-y-2">
            <Skeleton className="h-6 w-3/4" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-5/6" />
          </Card>

          <EmptyState
            title="Chưa có món"
            description="Quét mã tại bàn để xem danh sách món ăn."
            actionText="Xem thực đơn"
            onAction={() => info('Mở danh sách thực đơn')}
          />

          <ErrorState
            title="Lỗi tải dữ liệu"
            message="Không thể tải danh sách lúc này. Vui lòng thử lại."
            onRetry={() => success('Đã tải lại thành công')}
          />
        </div>

        <div className="flex flex-wrap items-center gap-3 pt-1">
          <Button variant="outline" onClick={() => setIsModalOpen(true)}>
            Mở hộp thoại
          </Button>
          <Button variant="secondary" onClick={() => success('Đặt bàn thành công')}>
            Báo thành công
          </Button>
          <Button variant="secondary" onClick={() => warning('Bàn sắp có khách')}>
            Báo cảnh báo
          </Button>
          <Button variant="secondary" onClick={() => error('Khung giờ đã kín')}>
            Báo lỗi
          </Button>
        </div>
      </section>

      {/* Hộp thoại xác nhận */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Đổi sang Bàn 201?"
        description="Bàn 201 có view cửa sổ đang trống, bạn có muốn đổi không?"
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
                success('Đã đổi sang Bàn 201');
              }}
            >
              Đồng ý
            </Button>
          </>
        }
      >
        <p className="text-xs text-ink-muted leading-relaxed">
          Bàn mới được giữ trong 10 phút. Nếu không chọn, hệ thống sẽ giữ nguyên bàn ban đầu.
        </p>
      </Modal>
    </div>
  );
};
