import { forwardRef, useId, SelectHTMLAttributes } from 'react';
import { clsx } from 'clsx';
import { ChevronDown } from 'lucide-react';

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  helperText?: string;
  options: SelectOption[];
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(({
  className,
  label,
  error,
  helperText,
  options,
  id,
  disabled,
  ...props
}, ref) => {
  const generatedId = useId();
  const selectId = id || generatedId;
  const errorId = `${selectId}-error`;
  const helperId = `${selectId}-helper`;

  return (
    <div className="w-full flex flex-col gap-1.5 text-left">
      {label && (
        <label
          htmlFor={selectId}
          className="text-xs font-semibold text-ink-muted uppercase tracking-wider select-none"
        >
          {label}
        </label>
      )}
      <div className="relative">
        <select
          ref={ref}
          id={selectId}
          disabled={disabled}
          aria-invalid={!!error}
          aria-describedby={error ? errorId : helperText ? helperId : undefined}
          className={clsx(
            'w-full min-h-[44px] appearance-none px-3.5 pr-10 py-2 text-sm bg-surface text-ink border rounded transition-colors',
            'focus:outline-none focus:ring-2 focus:ring-brand focus:border-transparent',
            'disabled:bg-surface-subtle disabled:text-ink-subtle disabled:cursor-not-allowed',
            error ? 'border-status-occupied text-status-occupied' : 'border-border',
            className
          )}
          {...props}
        >
          {options.map((opt) => (
            <option key={opt.value} value={opt.value} disabled={opt.disabled}>
              {opt.label}
            </option>
          ))}
        </select>
        <ChevronDown
          className="w-4 h-4 text-ink-subtle absolute right-3.5 top-1/2 -translate-y-1/2 pointer-events-none"
          aria-hidden="true"
        />
      </div>
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

Select.displayName = 'Select';
