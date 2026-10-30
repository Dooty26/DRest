import React, { useState } from 'react';
import { clsx } from 'clsx';
import { UtensilsCrossed } from 'lucide-react';
import { Skeleton } from './Skeleton';

export interface ResponsiveImageProps extends Omit<React.ImgHTMLAttributes<HTMLImageElement>, 'src'> {
  src?: string | null;
  alt: string;
  aspectRatio?: '4/3' | '1/1' | '16/9' | 'auto';
  fallbackIcon?: React.ReactNode;
  containerClassName?: string;
}

export const ResponsiveImage: React.FC<ResponsiveImageProps> = ({
  src,
  alt,
  aspectRatio = '4/3',
  fallbackIcon,
  className,
  containerClassName,
  ...props
}) => {
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [hasError, setHasError] = useState<boolean>(false);

  const isInvalidSrc = !src || src.trim() === '';

  const ratioClass = {
    '4/3': 'aspect-[4/3]',
    '1/1': 'aspect-square',
    '16/9': 'aspect-video',
    'auto': '',
  }[aspectRatio];

  return (
    <div
      className={clsx(
        'relative overflow-hidden bg-surface-subtle border border-border/50 rounded-lg flex items-center justify-center select-none',
        ratioClass,
        containerClassName
      )}
    >
      {/* 1. Placeholder khi src null/trống hoặc khi tải lỗi */}
      {(isInvalidSrc || hasError) ? (
        <div
          className="w-full h-full flex flex-col items-center justify-center p-4 text-ink-subtle bg-slate-100"
          role="img"
          aria-label={alt}
        >
          {fallbackIcon || <UtensilsCrossed className="w-8 h-8 opacity-30 mb-1" />}
          <span className="text-[10px] text-ink-subtle opacity-60 text-center line-clamp-1">{alt}</span>
        </div>
      ) : (
        <>
          {/* 2. Skeleton khi đang tải ảnh */}
          {isLoading && (
            <Skeleton className="absolute inset-0 w-full h-full z-10" />
          )}

          {/* 3. Thẻ ảnh thật */}
          <img
            src={src}
            alt={alt}
            loading="lazy"
            decoding="async"
            onLoad={() => setIsLoading(false)}
            onError={() => {
              setIsLoading(false);
              setHasError(true);
            }}
            className={clsx(
              'w-full h-full object-cover transition-opacity duration-300',
              isLoading ? 'opacity-0' : 'opacity-100',
              className
            )}
            {...props}
          />
        </>
      )}
    </div>
  );
};
