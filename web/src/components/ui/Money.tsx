import React from 'react';
import { clsx } from 'clsx';
import { formatVnd } from '../../lib/format';

export interface MoneyProps {
  amount: number | null | undefined;
  className?: string;
  showRefundLabel?: boolean; // Tự động hiển thị nhãn "Hoàn lại khách" khi số âm
  align?: 'left' | 'right' | 'center';
  size?: 'sm' | 'md' | 'lg' | 'xl';
}

export const Money: React.FC<MoneyProps> = ({
  amount = 0,
  className,
  showRefundLabel = true,
  align = 'right',
  size = 'md',
}) => {
  const num = amount ?? 0;
  const isNegative = num < 0;

  const sizeStyles = {
    sm: 'text-xs',
    md: 'text-sm font-semibold',
    lg: 'text-base font-bold',
    xl: 'text-xl font-bold sm:text-2xl',
  };

  const alignStyles = {
    left: 'text-left',
    right: 'text-right',
    center: 'text-center',
  };

  return (
    <div
      className={clsx(
        'tabular-nums transition-colors',
        sizeStyles[size],
        alignStyles[align],
        isNegative ? 'text-status-occupied font-bold' : 'text-ink',
        className
      )}
    >
      {isNegative && showRefundLabel ? (
        <span className="inline-flex items-center gap-1">
          <span className="text-xs font-normal text-status-occupied uppercase tracking-wider">
            Hoàn lại khách:
          </span>{' '}
          <span>{formatVnd(Math.abs(num))}</span>
        </span>
      ) : (
        <span>{formatVnd(num)}</span>
      )}
    </div>
  );
};
