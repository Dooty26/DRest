import React from 'react';
import { clsx } from 'clsx';
import { Timer, AlertTriangle } from 'lucide-react';
import { useCountdown } from '../../hooks/useCountdown';

export interface CountdownProps {
  expiresAt: string | Date | number | null | undefined;
  onExpire?: () => void;
  prefix?: string;
  className?: string;
  size?: 'sm' | 'md' | 'lg';
}

export const Countdown: React.FC<CountdownProps> = ({
  expiresAt,
  onExpire,
  prefix = 'Thời gian còn lại:',
  className,
  size = 'md',
}) => {
  const { formatted, isExpired, isUrgent } = useCountdown(expiresAt);

  React.useEffect(() => {
    if (isExpired && onExpire) {
      onExpire();
    }
  }, [isExpired, onExpire]);

  const sizeStyles = {
    sm: 'text-xs gap-1.5 px-2.5 py-1',
    md: 'text-sm gap-2 px-3.5 py-1.5',
    lg: 'text-base font-bold gap-2.5 px-4 py-2',
  };

  return (
    <div
      role="timer"
      aria-live="polite"
      className={clsx(
        'inline-flex items-center rounded-full font-medium transition-colors tabular-nums border shadow-2xs select-none',
        sizeStyles[size],
        isExpired
          ? 'bg-status-cleaning-bg text-status-cleaning-ink border-gray-300'
          : isUrgent
          ? 'bg-status-occupied-bg text-status-occupied border-red-300 animate-pulse font-bold'
          : 'bg-brand-light text-brand border-amber-300',
        className
      )}
    >
      {isUrgent ? (
        <AlertTriangle className="w-4 h-4 flex-shrink-0" />
      ) : (
        <Timer className="w-4 h-4 flex-shrink-0" />
      )}
      {prefix && <span className="font-normal opacity-90">{prefix}</span>}
      <span className="font-bold tracking-wider">
        {isExpired ? 'Đã hết hạn' : formatted}
      </span>
    </div>
  );
};
