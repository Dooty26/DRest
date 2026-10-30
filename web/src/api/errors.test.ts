import { describe, it, expect } from 'vitest';
import { getErrorMessage } from './errors';

describe('getErrorMessage', () => {
  it('ánh xạ đúng mã lỗi SLOT_UNAVAILABLE sang tiếng Việt', () => {
    const error = { code: 'SLOT_UNAVAILABLE' };
    expect(getErrorMessage(error)).toContain('Khung giờ này vừa có người đặt');
  });

  it('ánh xạ đúng mã lỗi DEPOSIT_EXPIRED', () => {
    const error = { code: 'DEPOSIT_EXPIRED' };
    expect(getErrorMessage(error)).toContain('Đã hết thời gian giữ chỗ thanh toán cọc');
  });

  it('trả về fallback khi không có thông tin lỗi', () => {
    expect(getErrorMessage(null)).toBe('Đã có lỗi xảy ra. Vui lòng thử lại sau.');
  });
});
