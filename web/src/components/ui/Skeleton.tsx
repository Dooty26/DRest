import React from 'react';
import { clsx } from 'clsx';
import { AlertCircle, RefreshCw, Inbox } from 'lucide-react';
import { Button } from './Button';

export interface SkeletonProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'rectangular' | 'circular' | 'text';
}

export const Skeleton: React.FC<SkeletonProps> = ({
  className,
  variant = 'rectangular',
  ...props
}) => {
  const styles = {
    rectangular: 'rounded',
    circular: 'rounded-full',
    text: 'rounded h-4 w-full my-1',
  };

  return (
    <div
      className={clsx('animate-pulse bg-border/60', styles[variant], className)}
      {...props}
    />
  );
};

export interface EmptyStateProps {
  title?: string;
  description?: string;
  icon?: React.ReactNode;
  actionText?: string;
  onAction?: () => void;
  className?: string;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  title = 'Không có dữ liệu',
  description = 'Hiện tại danh sách này đang trống hoặc chưa có thông tin phù hợp.',
  icon,
  actionText,
  onAction,
  className,
}) => {
  return (
    <div className={clsx('flex flex-col items-center justify-center p-8 text-center bg-surface-subtle border border-dashed border-border rounded-xl', className)}>
      <div className="w-12 h-12 rounded-full bg-border/40 flex items-center justify-center text-ink-subtle mb-3">
        {icon || <Inbox className="w-6 h-6" />}
      </div>
      <h3 className="text-sm font-bold text-ink mb-1">{title}</h3>
      <p className="text-xs text-ink-muted max-w-sm mb-4 leading-relaxed">{description}</p>
      {actionText && onAction && (
        <Button variant="outline" size="sm" onClick={onAction}>
          {actionText}
        </Button>
      )}
    </div>
  );
};

export interface ErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
  className?: string;
}

export const ErrorState: React.FC<ErrorStateProps> = ({
  title = 'Đã có lỗi xảy ra',
  message = 'Không thể tải được dữ liệu lúc này. Vui lòng kiểm tra lại kết nối mạng.',
  onRetry,
  className,
}) => {
  return (
    <div className={clsx('flex flex-col items-center justify-center p-8 text-center bg-status-occupied-bg/40 border border-status-occupied/20 rounded-xl', className)}>
      <div className="w-12 h-12 rounded-full bg-status-occupied-bg flex items-center justify-center text-status-occupied mb-3">
        <AlertCircle className="w-6 h-6" />
      </div>
      <h3 className="text-sm font-bold text-ink mb-1">{title}</h3>
      <p className="text-xs text-status-occupied-ink max-w-sm mb-4 leading-relaxed">{message}</p>
      {onRetry && (
        <Button
          variant="outline"
          size="sm"
          onClick={onRetry}
          leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
        >
          Thử lại
        </Button>
      )}
    </div>
  );
};
