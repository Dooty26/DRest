import React from 'react';
import { clsx } from 'clsx';
import { Check, Clock, CircleDot, Sparkles, Hourglass } from 'lucide-react';

export type TableDisplayStatusType = 'AVAILABLE' | 'RESERVED' | 'OCCUPIED' | 'CLEANING' | 'HELD';

export interface StatusPillProps {
  status: TableDisplayStatusType | string;
  className?: string;
  showIcon?: boolean;
  showLabel?: boolean;
}

export const STATUS_CONFIG: Record<TableDisplayStatusType, {
  label: string;
  icon: React.ReactNode;
  bgClass: string;
  textClass: string;
  borderClass: string;
  dotColor: string;
}> = {
  AVAILABLE: {
    label: 'Trống',
    icon: <Check className="w-3.5 h-3.5" aria-hidden="true" />,
    bgClass: 'bg-status-available-bg',
    textClass: 'text-status-available-ink',
    borderClass: 'border-green-300',
    dotColor: 'bg-status-available',
  },
  RESERVED: {
    label: 'Đã đặt',
    icon: <Clock className="w-3.5 h-3.5" aria-hidden="true" />,
    bgClass: 'bg-status-reserved-bg',
    textClass: 'text-status-reserved-ink',
    borderClass: 'border-yellow-300',
    dotColor: 'bg-status-reserved',
  },
  OCCUPIED: {
    label: 'Có khách',
    icon: <CircleDot className="w-3.5 h-3.5" aria-hidden="true" />,
    bgClass: 'bg-status-occupied-bg',
    textClass: 'text-status-occupied-ink',
    borderClass: 'border-red-300',
    dotColor: 'bg-status-occupied',
  },
  CLEANING: {
    label: 'Đang dọn',
    icon: <Sparkles className="w-3.5 h-3.5" aria-hidden="true" />,
    bgClass: 'bg-status-cleaning-bg',
    textClass: 'text-status-cleaning-ink',
    borderClass: 'border-gray-300',
    dotColor: 'bg-status-cleaning',
  },
  HELD: {
    label: 'Đang giữ',
    icon: <Hourglass className="w-3.5 h-3.5" aria-hidden="true" />,
    bgClass: 'bg-status-held-bg',
    textClass: 'text-status-held-ink',
    borderClass: 'border-purple-300',
    dotColor: 'bg-status-held',
  },
};

export const StatusPill: React.FC<StatusPillProps> = ({
  status,
  className,
  showIcon = true,
  showLabel = true,
}) => {
  const normStatus = (status?.toUpperCase() as TableDisplayStatusType) || 'AVAILABLE';
  const config = STATUS_CONFIG[normStatus] || STATUS_CONFIG.AVAILABLE;

  return (
    <span
      className={clsx(
        'inline-flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold rounded-full border shadow-2xs select-none',
        config.bgClass,
        config.textClass,
        config.borderClass,
        className
      )}
      title={`Trạng thái: ${config.label}`}
    >
      {showIcon ? (
        config.icon
      ) : (
        <span className={clsx('w-2 h-2 rounded-full', config.dotColor)} aria-hidden="true" />
      )}
      {showLabel && <span>{config.label}</span>}
    </span>
  );
};
