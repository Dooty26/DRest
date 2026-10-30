/**
 * Ánh xạ mã lỗi chuẩn từ ProblemDetails / ErrorCodes sang thông báo tiếng Việt
 */
export const ERROR_MESSAGES: Record<string, string> = {
  VALIDATION_ERROR: 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại các trường thông tin.',
  NOT_FOUND: 'Không tìm thấy dữ liệu yêu cầu.',
  UNAUTHORIZED: 'Phiên làm việc đã hết hạn hoặc bạn chưa đăng nhập.',
  FORBIDDEN_ROLE: 'Bạn không có quyền thực hiện thao tác này.',
  INVALID_STATE: 'Trạng thái hiện tại không cho phép thực hiện thao tác này.',
  SLOT_UNAVAILABLE: 'Khung giờ này vừa có người đặt hoặc đã hết bàn, vui lòng chọn giờ khác.',
  TABLE_CONFLICT: 'Bàn được chọn đang có khách hoặc bị trùng lịch.',
  WALKIN_CONFLICT: 'Bàn này sắp có khách đặt trước trong ít phút nữa.',
  DEPOSIT_EXPIRED: 'Đã hết thời gian giữ chỗ thanh toán cọc. Bàn đã được mở lại cho khách khác.',
  OUT_OF_STOCK_ITEM: 'Một hoặc nhiều món trong giỏ hàng đã tạm hết. Vui lòng kiểm tra lại.',
  QR_SIGNATURE_INVALID: 'Mã QR không hợp lệ hoặc đã hết hạn. Vui lòng quét lại mã tại bàn.',
  TABLE_NOT_OPEN: 'Bàn hiện chưa được mở phiên phục vụ.',
  WEBHOOK_SIGNATURE_INVALID: 'Chữ ký webhook không hợp lệ.',
  RATE_LIMITED: 'Quá nhiều yêu cầu. Vui lòng chờ giây lát rồi thử lại.',
  NOT_IMPLEMENTED: 'Tính năng đang được phát triển.',
};

export interface ProblemDetailsDto {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  code?: string;
  errors?: Record<string, string[]>;
  warnings?: string[];
}

/**
 * Trích xuất câu thông báo lỗi thân thiện cho người dùng từ ProblemDetails hoặc Error object
 */
export function getErrorMessage(error: unknown, fallback = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.'): string {
  if (!error) return fallback;

  if (typeof error === 'string') return error;

  if (typeof error === 'object') {
    const p = error as ProblemDetailsDto;
    if (p.code && ERROR_MESSAGES[p.code]) {
      return ERROR_MESSAGES[p.code];
    }
    if (p.detail) {
      return p.detail;
    }
    if (p.title) {
      return p.title;
    }
    if ('message' in error && typeof (error as any).message === 'string') {
      return (error as any).message;
    }
  }

  return fallback;
}
