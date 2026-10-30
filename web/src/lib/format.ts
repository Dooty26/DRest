/**
 * Định dạng tiền tệ và thời gian chuẩn Việt Nam theo AGENTS.md
 */

/**
 * Định dạng tiền VND: 350.000đ.
 * Nếu số âm, trả về dạng âm (ví dụ: -70.000đ).
 */
export function formatVnd(amount: number): string {
  const formatted = new Intl.NumberFormat('vi-VN').format(Math.abs(amount));
  if (amount < 0) {
    return `-${formatted}đ`;
  }
  return `${formatted}đ`;
}

/**
 * Định dạng giờ phút VN: 19:00
 */
export function formatVnTime(utcDateStringOrDate: string | Date): string {
  const date = typeof utcDateStringOrDate === 'string' ? new Date(utcDateStringOrDate) : utcDateStringOrDate;
  if (isNaN(date.getTime())) return '--:--';
  return new Intl.DateTimeFormat('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
    timeZone: 'Asia/Ho_Chi_Minh',
  }).format(date);
}

/**
 * Định dạng ngày tháng VN: dd/MM/yyyy
 */
export function formatVnDate(utcDateStringOrDate: string | Date): string {
  const date = typeof utcDateStringOrDate === 'string' ? new Date(utcDateStringOrDate) : utcDateStringOrDate;
  if (isNaN(date.getTime())) return '--/--/----';
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    timeZone: 'Asia/Ho_Chi_Minh',
  }).format(date);
}

/**
 * Định dạng ngày và giờ VN: 19:00, 31/10/2026
 */
export function formatVnDateTime(utcDateStringOrDate: string | Date): string {
  return `${formatVnTime(utcDateStringOrDate)}, ${formatVnDate(utcDateStringOrDate)}`;
}

/**
 * Định dạng giây thành mm:ss (ví dụ: 09:45)
 */
export function formatMmSs(seconds: number): string {
  if (seconds <= 0) return '00:00';
  const m = Math.floor(seconds / 60);
  const s = Math.floor(seconds % 60);
  return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
}
