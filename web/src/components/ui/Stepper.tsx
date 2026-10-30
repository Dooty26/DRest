import React from 'react';
import { clsx } from 'clsx';
import { Check } from 'lucide-react';

export interface StepItem {
  id: number | string;
  title: string;
  description?: string;
}

export interface StepperProps {
  steps: StepItem[];
  currentStep: number; // 0-indexed hoặc 1-indexed (hỗ trợ cả 2 dựa trên index)
  onStepClick?: (stepIndex: number) => void;
  className?: string;
}

export const Stepper: React.FC<StepperProps> = ({
  steps,
  currentStep,
  onStepClick,
  className,
}) => {
  return (
    <div className={clsx('w-full py-4', className)}>
      <nav aria-label="Tiến trình đặt bàn">
        <ol className="flex items-center justify-between w-full">
          {steps.map((step, index) => {
            const isCompleted = index < currentStep;
            const isCurrent = index === currentStep;
            const isClickable = !!onStepClick && isCompleted;

            return (
              <li
                key={step.id}
                className={clsx(
                  'flex-1 flex flex-col items-center relative',
                  index !== steps.length - 1 &&
                    'after:content-[""] after:w-full after:h-0.5 after:bg-border after:top-4 after:left-1/2 after:absolute after:-z-10',
                  index !== steps.length - 1 && isCompleted && 'after:bg-brand'
                )}
              >
                <button
                  type="button"
                  disabled={!isClickable}
                  onClick={() => isClickable && onStepClick(index)}
                  className={clsx(
                    'w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold transition-all z-10 border-2 select-none',
                    isCompleted && 'bg-brand text-brand-ink border-brand',
                    isCurrent && 'bg-surface text-brand border-brand ring-4 ring-brand-light',
                    !isCompleted && !isCurrent && 'bg-surface text-ink-subtle border-border',
                    isClickable && 'cursor-pointer hover:scale-105'
                  )}
                  aria-current={isCurrent ? 'step' : undefined}
                >
                  {isCompleted ? <Check className="w-4 h-4 stroke-[3]" /> : index + 1}
                </button>
                <div className="mt-1.5 text-center px-1">
                  <span
                    className={clsx(
                      'block text-xs font-semibold whitespace-nowrap',
                      isCurrent && 'text-brand',
                      isCompleted && 'text-ink',
                      !isCompleted && !isCurrent && 'text-ink-subtle'
                    )}
                  >
                    {step.title}
                  </span>
                  {step.description && (
                    <span className="hidden sm:block text-[10px] text-ink-subtle">
                      {step.description}
                    </span>
                  )}
                </div>
              </li>
            );
          })}
        </ol>
      </nav>
    </div>
  );
};
