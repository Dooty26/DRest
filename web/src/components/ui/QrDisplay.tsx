import React, { useEffect, useState } from 'react';
import QRCode from 'qrcode';
import { clsx } from 'clsx';
import { Copy, Check, QrCode as QrIcon } from 'lucide-react';
import { Button } from './Button';

export interface QrDisplayProps {
  qrPayload?: string;
  qrImageUrl?: string;
  size?: number;
  title?: string;
  amount?: number;
  transferContent?: string;
  className?: string;
}

export const QrDisplay: React.FC<QrDisplayProps> = ({
  qrPayload,
  qrImageUrl,
  size = 220,
  title = 'Quét mã VietQR để thanh toán',
  transferContent,
  className,
}) => {
  const [dataUrl, setDataUrl] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!qrPayload) {
      setDataUrl(null);
      return;
    }

    QRCode.toDataURL(qrPayload, {
      width: size,
      margin: 2,
      color: {
        dark: '#000000',
        light: '#ffffff',
      },
      errorCorrectionLevel: 'M',
    })
      .then((url) => {
        setDataUrl(url);
        setError(false);
      })
      .catch(() => {
        setError(true);
      });
  }, [qrPayload, size]);

  const handleCopy = () => {
    const textToCopy = transferContent || qrPayload || '';
    if (!textToCopy) return;
    navigator.clipboard.writeText(textToCopy).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  };

  const imageSrc = dataUrl || qrImageUrl;

  return (
    <div
      className={clsx(
        'flex flex-col items-center p-5 bg-surface rounded-xl border border-border shadow-sm text-center max-w-sm w-full mx-auto',
        className
      )}
    >
      {title && <h3 className="text-sm font-bold text-ink mb-3">{title}</h3>}

      <div
        className="relative bg-white p-2.5 rounded-lg border border-border shadow-2xs flex items-center justify-center overflow-hidden"
        style={{ width: size + 20, height: size + 20 }}
      >
        {imageSrc && !error ? (
          <img
            src={imageSrc}
            alt="Mã QR thanh toán"
            width={size}
            height={size}
            className="rounded"
          />
        ) : (
          <div className="flex flex-col items-center justify-center text-ink-subtle">
            <QrIcon className="w-12 h-12 mb-1.5 opacity-40" />
            <span className="text-xs">Đang tải mã QR...</span>
          </div>
        )}
      </div>

      {transferContent && (
        <div className="mt-3 w-full bg-surface-subtle p-2.5 rounded border border-border flex items-center justify-between gap-2 text-left">
          <div className="truncate">
            <span className="text-[10px] text-ink-subtle block uppercase font-medium">Nội dung chuyển khoản</span>
            <span className="text-xs font-mono font-bold text-ink select-all">{transferContent}</span>
          </div>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={handleCopy}
            className="min-h-[32px] px-2 py-1 text-xs"
            leftIcon={copied ? <Check className="w-3.5 h-3.5 text-status-available" /> : <Copy className="w-3.5 h-3.5" />}
          >
            {copied ? 'Đã sao chép' : 'Sao chép'}
          </Button>
        </div>
      )}
    </div>
  );
};
