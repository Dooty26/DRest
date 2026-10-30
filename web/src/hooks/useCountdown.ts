import { useState, useEffect, useCallback } from 'react';
import { getCorrectedNow } from './useServerClock';
import { formatMmSs } from '../lib/format';

export interface CountdownState {
  remainingSeconds: number;
  formatted: string;
  isExpired: boolean;
  isUrgent: boolean; // < 60 giây
}

export function useCountdown(expiresAt: string | Date | number | null | undefined): CountdownState {
  const getSeconds = useCallback((): number => {
    if (!expiresAt) return 0;
    const target = typeof expiresAt === 'number'
      ? expiresAt
      : new Date(expiresAt).getTime();

    if (isNaN(target)) return 0;
    const diff = Math.max(0, Math.floor((target - getCorrectedNow()) / 1000));
    return diff;
  }, [expiresAt]);

  const [remaining, setRemaining] = useState<number>(getSeconds());

  useEffect(() => {
    setRemaining(getSeconds());

    const interval = setInterval(() => {
      const sec = getSeconds();
      setRemaining(sec);
      if (sec <= 0) {
        clearInterval(interval);
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [getSeconds]);

  return {
    remainingSeconds: remaining,
    formatted: formatMmSs(remaining),
    isExpired: remaining <= 0,
    isUrgent: remaining > 0 && remaining < 60,
  };
}
