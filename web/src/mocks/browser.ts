import { setupWorker } from 'msw/browser';
import { handlers } from './handlers';

export const worker = setupWorker(...handlers);

export async function enableMocking() {
  if (import.meta.env.VITE_USE_MOCK !== 'true') {
    return;
  }

  // Khởi chạy MSW worker trong trình duyệt với timeout an toàn
  // nhằm tránh treo render trong môi trường headless/CI không có ServiceWorker
  try {
    const startPromise = worker.start({
      onUnhandledRequest: 'bypass',
      serviceWorker: {
        url: '/mockServiceWorker.js',
      },
    });

    const timeoutPromise = new Promise<void>((resolve) =>
      setTimeout(() => {
        console.warn('MSW worker.start timed out after 1.5s, proceeding to render.');
        resolve();
      }, 1500)
    );

    await Promise.race([startPromise, timeoutPromise]);
  } catch (err) {
    console.warn('MSW worker failed to start:', err);
  }
}
