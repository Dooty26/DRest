import React from 'react';
import { clsx } from 'clsx';

export interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'default' | 'flat' | 'outline' | 'interactive';
}

export const Card: React.FC<CardProps> = ({
  className,
  variant = 'default',
  children,
  ...props
}) => {
  const styles = {
    default: 'bg-surface text-ink border border-border shadow-sm rounded-lg',
    flat: 'bg-surface-subtle text-ink rounded-lg',
    outline: 'bg-surface text-ink border border-border rounded-lg',
    interactive: 'bg-surface text-ink border border-border shadow-sm rounded-lg hover:border-brand hover:shadow-md transition-all cursor-pointer',
  };

  return (
    <div className={clsx(styles[variant], className)} {...props}>
      {children}
    </div>
  );
};
