import React from 'react';
import { clsx } from 'clsx';
import { X } from 'lucide-react';

export interface ChipProps {
  label: string;
  selected?: boolean;
  onClick?: () => void;
  onRemove?: () => void;
  icon?: React.ReactNode;
  variant?: 'default' | 'allergy' | 'brand';
  size?: 'sm' | 'md';
}

export const Chip: React.FC<ChipProps> = ({
  label,
  selected = false,
  onClick,
  onRemove,
  icon,
  variant = 'default',
  size = 'md',
}) => {
  const isInteractive = !!onClick || !!onRemove;

  const baseStyles = 'inline-flex items-center gap-1.5 rounded-full font-medium transition-all select-none';
  const sizeStyles = {
    sm: 'text-xs px-2.5 py-1 min-h-[30px]',
    md: 'text-sm px-3.5 py-1.5 min-h-[38px]',
  };

  const variantStyles = {
    default: selected
      ? 'bg-ink text-surface border border-ink'
      : 'bg-surface-subtle text-ink-muted border border-border hover:border-ink-subtle',
    allergy: selected
      ? 'bg-status-occupied text-white border border-status-occupied'
      : 'bg-status-occupied-bg text-status-occupied-ink border border-red-200 hover:border-status-occupied',
    brand: selected
      ? 'bg-brand text-brand-ink border border-brand'
      : 'bg-brand-light text-brand border border-amber-300 hover:border-brand',
  };

  const Component = onClick ? 'button' : 'div';

  return (
    <Component
      type={onClick ? 'button' : undefined}
      onClick={onClick}
      className={clsx(
        baseStyles,
        sizeStyles[size],
        variantStyles[variant],
        isInteractive && 'cursor-pointer active:scale-95',
        !isInteractive && 'cursor-default'
      )}
    >
      {icon}
      <span>{label}</span>
      {onRemove && (
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onRemove();
          }}
          className="p-0.5 rounded-full hover:bg-black/10 transition-colors"
          aria-label={`Xóa ${label}`}
        >
          <X className="w-3.5 h-3.5" />
        </button>
      )}
    </Component>
  );
};

export interface BadgeProps {
  children: React.ReactNode;
  variant?: 'default' | 'brand' | 'success' | 'warning' | 'danger';
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({
  children,
  variant = 'default',
  className,
}) => {
  const styles = {
    default: 'bg-surface-subtle text-ink-muted border-border',
    brand: 'bg-brand-light text-brand border-amber-200',
    success: 'bg-status-available-bg text-status-available-ink border-green-200',
    warning: 'bg-status-reserved-bg text-status-reserved-ink border-yellow-200',
    danger: 'bg-status-occupied-bg text-status-occupied-ink border-red-200',
  };

  return (
    <span
      className={clsx(
        'inline-flex items-center px-2 py-0.5 text-xs font-semibold rounded-full border',
        styles[variant],
        className
      )}
    >
      {children}
    </span>
  );
};
