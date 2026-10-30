import React from 'react';
import { BrowserRouter, Routes, Route, Link, Navigate } from 'react-router-dom';
import { ToastProvider } from './components/ui/Toast';
import { DevUiKitPage } from './pages/DevUiKitPage';

export const App: React.FC = () => {
  const isDevTools = import.meta.env.VITE_ENABLE_DEV_TOOLS === 'true';

  return (
    <ToastProvider>
      <BrowserRouter>
        <header className="bg-surface border-b border-border sticky top-0 z-40">
          <div className="max-w-4xl mx-auto px-4 h-14 flex items-center justify-between">
            <Link to="/" className="flex items-center gap-2">
              <span className="w-8 h-8 rounded-lg bg-brand text-brand-ink flex items-center justify-center font-bold text-sm shadow-sm">
                D
              </span>
              <span className="font-bold text-base text-ink tracking-tight">
                Dooty Culinary
              </span>
            </Link>
            {isDevTools && (
              <nav className="flex items-center gap-3 text-xs">
                {/* copy-ok: Liên kết công cụ dev chỉ hiển thị khi bật VITE_ENABLE_DEV_TOOLS */}
                <Link
                  to="/dev/ui-kit"
                  className="px-3 py-1.5 font-medium rounded text-ink-muted hover:text-brand hover:bg-brand-light transition-colors"
                >
                  Giao diện mẫu
                </Link>
              </nav>
            )}
          </div>
        </header>

        <main>
          <Routes>
            <Route path="/" element={<Navigate to="/dev/ui-kit" replace />} />
            <Route path="/dev/ui-kit" element={<DevUiKitPage />} />
            <Route path="*" element={<Navigate to="/dev/ui-kit" replace />} />
          </Routes>
        </main>
      </BrowserRouter>
    </ToastProvider>
  );
};

export default App;
