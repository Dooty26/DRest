import { useState, useEffect } from 'react';

let globalServerOffsetMs = 0;

/**
 * Cập nhật độ lệch giữa đồng hồ máy khách và máy chủ
 */
export function setServerOffset(serverIsoTime: string) {
  const serverTime = new Date(serverIsoTime).getTime();
  if (!isNaN(serverTime)) {
    globalServerOffsetMs = serverTime - Date.now();
  }
}

/**
 * Trả về thời gian hiện tại chuẩn hóa theo máy chủ
 */
export function getCorrectedNow(): number {
  return Date.now() + globalServerOffsetMs;
}

export function useServerClock() {
  const [now, setNow] = useState<number>(getCorrectedNow());

  useEffect(() => {
    const timer = setInterval(() => {
      setNow(getCorrectedNow());
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  return {
    now,
    serverOffsetMs: globalServerOffsetMs,
    setServerOffset,
  };
}
