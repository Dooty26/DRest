/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          DEFAULT: "var(--brand-primary)",
          hover: "var(--brand-hover)",
          light: "var(--brand-light)",
          ink: "var(--brand-ink)",
        },
        surface: {
          DEFAULT: "var(--surface)",
          subtle: "var(--surface-subtle)",
          card: "var(--surface-card)",
          overlay: "var(--surface-overlay)",
        },
        ink: {
          DEFAULT: "var(--ink)",
          muted: "var(--ink-muted)",
          subtle: "var(--ink-subtle)",
          inverse: "var(--ink-inverse)",
        },
        border: {
          DEFAULT: "var(--border)",
          subtle: "var(--border-subtle)",
          strong: "var(--border-strong)",
        },
        status: {
          available: {
            DEFAULT: "var(--status-available)",
            bg: "var(--status-available-bg)",
            ink: "var(--status-available-ink)",
          },
          reserved: {
            DEFAULT: "var(--status-reserved)",
            bg: "var(--status-reserved-bg)",
            ink: "var(--status-reserved-ink)",
          },
          occupied: {
            DEFAULT: "var(--status-occupied)",
            bg: "var(--status-occupied-bg)",
            ink: "var(--status-occupied-ink)",
          },
          cleaning: {
            DEFAULT: "var(--status-cleaning)",
            bg: "var(--status-cleaning-bg)",
            ink: "var(--status-cleaning-ink)",
          },
          held: {
            DEFAULT: "var(--status-held)",
            bg: "var(--status-held-bg)",
            ink: "var(--status-held-ink)",
          },
        },
      },
      fontFamily: {
        sans: ['"Be Vietnam Pro"', 'system-ui', '-apple-system', 'sans-serif'],
      },
      borderRadius: {
        sm: '4px',
        DEFAULT: '8px',
        md: '8px',
        lg: '12px',
        xl: '16px',
        full: '9999px',
      },
      boxShadow: {
        sm: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
        DEFAULT: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px -1px rgba(0, 0, 0, 0.1)',
        md: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1)',
        lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1)',
      },
    },
  },
  plugins: [],
};
