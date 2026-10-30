import { describe, it, expect } from 'vitest';
import { formatVnd, formatMmSs } from './format';

describe('formatVnd', () => {
  it('định dạng số dương thành VND với dấu chấm', () => {
    expect(formatVnd(350000)).toBe('350.000đ');
    expect(formatVnd(200000)).toBe('200.000đ');
    expect(formatVnd(0)).toBe('0đ');
  });

  it('định dạng số âm đúng quy chuẩn hoàn cọc', () => {
    expect(formatVnd(-70000)).toBe('-70.000đ');
  });
});

describe('formatMmSs', () => {
  it('định dạng số giây thành mm:ss', () => {
    expect(formatMmSs(600)).toBe('10:00');
    expect(formatMmSs(59)).toBe('00:59');
    expect(formatMmSs(0)).toBe('00:00');
    expect(formatMmSs(-10)).toBe('00:00');
  });
});
