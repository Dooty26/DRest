import { forwardRef, useId, InputHTMLAttributes } from 'react';
import { clsx } from 'clsx';

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(({
  className,
  label,
  error,
  helperText,
  id,
  disabled,
  ...props
}, ref) => {
  const generatedId = useId();
  const inputId = id || generatedId;
  const errorId = `${inputId}-error`;
  const helperId = `${inputId}-helper`;

  return (
    <div className="w-full flex flex-col gap-1.5 text-left">
      {label && (
        <label
          htmlFor={inputId}
          className="text-xs font-semibold text-ink-muted uppercase tracking-wider select-none"
        >
          {label}
        </label>
      )}
      <input
        ref={ref}
        id={inputId}
        disabled={disabled}
        aria-invalid={!!error}
        aria-describedby={error ? errorId : helperText ? helperId : undefined}
        className={clsx(
          'w-full min-h-[44px] px-3.5 py-2 text-sm bg-surface text-ink border rounded transition-colors',
          'placeholder:text-ink-subtle focus:outline-none focus:ring-2 focus:ring-brand focus:border-transparent',
          'disabled:bg-surface-subtle disabled:text-ink-subtle disabled:cursor-not-allowed',
          error ? 'border-status-occupied text-status-occupied focus:ring-status-occupied' : 'border-border',
          className
        )}
        {...props}
      />
      {error && (
        <span id={errorId} className="text-xs font-medium text-status-occupied" role="alert">
          {error}
        </span>
      )}
      {!error && helperText && (
        <span id={helperId} className="text-xs text-ink-subtle">
          {helperText}
        </span>
      )}
    </div>
  );
});

Input.displayName = 'Input';
