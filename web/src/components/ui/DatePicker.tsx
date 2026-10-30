import React, { useId } from 'react';
import { clsx } from 'clsx';
import { Calendar as CalendarIcon, Clock } from 'lucide-react';

export interface DatePickerProps {
  label?: string;
  value: string; // YYYY-MM-DD
  onChange: (val: string) => void;
  min?: string;
  max?: string;
  error?: string;
  disabled?: boolean;
}

export const DatePicker: React.FC<DatePickerProps> = ({
  label,
  value,
  onChange,
  min,
  max,
  error,
  disabled,
}) => {
  const id = useId();

  return (
    <div className="w-full flex flex-col gap-1.5 text-left">
      {label && (
        <label htmlFor={id} className="text-xs font-semibold text-ink-muted uppercase tracking-wider select-none">
          {label}
        </label>
      )}
      <div className="relative">
        <input
          id={id}
          type="date"
          value={value}
          min={min}
          max={max}
          disabled={disabled}
          onChange={(e) => onChange(e.target.value)}
          className={clsx(
            'w-full min-h-[44px] px-3.5 pl-10 py-2 text-sm bg-surface text-ink border rounded transition-colors',
            'focus:outline-none focus:ring-2 focus:ring-brand focus:border-transparent',
            error ? 'border-status-occupied text-status-occupied' : 'border-border'
          )}
        />
        <CalendarIcon className="w-4 h-4 text-ink-subtle absolute left-3.5 top-1/2 -translate-y-1/2 pointer-events-none" />
      </div>
      {error && <span className="text-xs font-medium text-status-occupied">{error}</span>}
    </div>
  );
};

export interface SlotOption {
  time: string; // e.g. "19:00"
  isAvailable: boolean;
  reason?: string;
}

export interface TimePickerProps {
  label?: string;
  slots: SlotOption[];
  selectedTime?: string;
  onSelect: (time: string) => void;
  error?: string;
}

export const TimePicker: React.FC<TimePickerProps> = ({
  label,
  slots,
  selectedTime,
  onSelect,
  error,
}) => {
  return (
    <div className="w-full flex flex-col gap-2 text-left">
      {label && (
        <div className="flex items-center gap-1.5 text-xs font-semibold text-ink-muted uppercase tracking-wider select-none">
          <Clock className="w-3.5 h-3.5" />
          <span>{label}</span>
        </div>
      )}
      <div className="grid grid-cols-4 sm:grid-cols-6 gap-2">
        {slots.map((slot) => {
          const isSelected = selectedTime === slot.time;
          return (
            <button
              key={slot.time}
              type="button"
              disabled={!slot.isAvailable}
              onClick={() => onSelect(slot.time)}
              title={slot.reason || undefined}
              className={clsx(
                'min-h-[44px] py-2 px-1 text-sm font-medium rounded transition-all select-none border',
                'focus-visible:ring-2 focus-visible:ring-brand focus-visible:outline-none',
                isSelected
                  ? 'bg-brand text-brand-ink border-brand shadow-sm font-semibold'
                  : slot.isAvailable
                  ? 'bg-surface text-ink border-border hover:border-brand hover:text-brand'
                  : 'bg-surface-subtle text-ink-subtle border-transparent opacity-40 cursor-not-allowed line-through'
              )}
            >
              {slot.time}
            </button>
          );
        })}
      </div>
      {error && <span className="text-xs font-medium text-status-occupied">{error}</span>}
    </div>
  );
};
