import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export interface UseSignalROptions {
  hubPath: string; // e.g. '/hubs/public'
  handlers?: Record<string, (...args: any[]) => void>;
  pollingFallback?: () => void | Promise<void>;
  pollingIntervalMs?: number; // fallback polling mỗi N ms khi mất kết nối
  enabled?: boolean;
}

export function useSignalR({
  hubPath,
  handlers = {},
  pollingFallback,
  pollingIntervalMs = 10000,
  enabled = true,
}: UseSignalROptions) {
  const [connectionState, setConnectionState] = useState<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected
  );
  const [isConnected, setIsConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const handlersRef = useRef(handlers);
  handlersRef.current = handlers;

  useEffect(() => {
    if (!enabled) return;

    const base = import.meta.env.VITE_API_BASE || '';
    const cleanBase = base.endsWith('/api/v1') ? base.replace('/api/v1', '') : base;
    const url = `${cleanBase}${hubPath}`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;
    const currentHandlers = handlersRef.current;

    // Đăng ký các event handlers
    Object.entries(currentHandlers).forEach(([eventName, handler]) => {
      connection.on(eventName, handler);
    });

    connection.onreconnecting(() => {
      setConnectionState(signalR.HubConnectionState.Reconnecting);
      setIsConnected(false);
    });

    connection.onreconnected(() => {
      setConnectionState(signalR.HubConnectionState.Connected);
      setIsConnected(true);
    });

    connection.onclose(() => {
      setConnectionState(signalR.HubConnectionState.Disconnected);
      setIsConnected(false);
    });

    // Bắt đầu kết nối
    connection
      .start()
      .then(() => {
        setConnectionState(signalR.HubConnectionState.Connected);
        setIsConnected(true);
      })
      .catch(() => {
        setConnectionState(signalR.HubConnectionState.Disconnected);
        setIsConnected(false);
      });

    return () => {
      Object.keys(currentHandlers).forEach((eventName) => {
        connection.off(eventName);
      });
      connection.stop();
    };
  }, [hubPath, enabled]);

  // Polling fallback khi không có kết nối SignalR hoặc khi SignalR disconnected
  useEffect(() => {
    if (!pollingFallback || !enabled) return;

    // Chạy polling ngay nếu chưa connected
    if (!isConnected) {
      pollingFallback();
    }

    const timer = setInterval(() => {
      if (!isConnected) {
        pollingFallback();
      }
    }, pollingIntervalMs);

    return () => clearInterval(timer);
  }, [isConnected, pollingFallback, pollingIntervalMs, enabled]);

  return {
    connection: connectionRef.current,
    connectionState,
    isConnected,
  };
}
