import React, { forwardRef } from 'react';
import { clsx } from 'clsx';
import { Loader2 } from 'lucide-react';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(({
  className,
  variant = 'primary',
  size = 'md',
  isLoading = false,
  leftIcon,
  rightIcon,
  disabled,
  children,
  ...props
}, ref) => {
  const baseStyles = 'inline-flex items-center justify-center font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand focus-visible:ring-offset-2 disabled:opacity-50 disabled:pointer-events-none select-none rounded';

  const sizeStyles = {
    sm: 'min-h-[36px] px-3 py-1.5 text-xs gap-1.5',
    md: 'min-h-[44px] px-4 py-2 text-sm gap-2', // Chuẩn touch target 44px
    lg: 'min-h-[50px] px-6 py-3 text-base gap-2.5',
  };

  const variantStyles = {
    primary: 'bg-brand text-brand-ink hover:bg-brand-hover shadow-sm active:translate-y-px',
    secondary: 'bg-surface-subtle text-ink hover:bg-border active:translate-y-px',
    outline: 'border border-border text-ink hover:bg-surface-subtle active:translate-y-px',
    ghost: 'text-ink hover:bg-surface-subtle active:bg-border',
    danger: 'bg-status-occupied text-white hover:bg-red-700 shadow-sm active:translate-y-px',
  };

  return (
    <button
      ref={ref}
      disabled={disabled || isLoading}
      className={clsx(baseStyles, sizeStyles[size], variantStyles[variant], className)}
      {...props}
    >
      {isLoading ? (
        <Loader2 className="w-4 h-4 animate-spin" aria-hidden="true" />
      ) : (
        leftIcon
      )}
      <span>{children}</span>
      {!isLoading && rightIcon}
    </button>
  );
});

Button.displayName = 'Button';
