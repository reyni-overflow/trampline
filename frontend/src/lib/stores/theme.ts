import { writable } from 'svelte/store';
import { browser } from '$app/environment';

export type Theme = 'dark' | 'light' | 'system';
export type AccentColor =
    | 'neutral'
    | 'yellow'
    | 'blue'
    | 'purple'
    | 'teal'
    | 'green'
    | 'pink'
    | 'orange'
    | 'red';

export const ACCENT_COLORS: { id: AccentColor; labelKey: string; color: string }[] = [
    { id: 'neutral', labelKey: 'accent.neutral', color: '' },
    { id: 'yellow', labelKey: 'accent.yellow', color: '#FACC15' },
    { id: 'blue', labelKey: 'accent.blue', color: '#3B82F6' },
    { id: 'purple', labelKey: 'accent.purple', color: '#8B5CF6' },
    { id: 'teal', labelKey: 'accent.teal', color: '#06B6D4' },
    { id: 'green', labelKey: 'accent.green', color: '#10B981' },
    { id: 'pink', labelKey: 'accent.pink', color: '#EC4899' },
    { id: 'orange', labelKey: 'accent.orange', color: '#F97316' },
    { id: 'red', labelKey: 'accent.red', color: '#EF4444' }
];

const THEME_KEY = 'trampline-theme';
const ACCENT_KEY = 'trampline-accent';

function getInitialTheme(): Theme {
    if (!browser) return 'dark';
    return (localStorage.getItem(THEME_KEY) as Theme) || 'system';
}

function getInitialAccent(): AccentColor {
    if (!browser) return 'neutral';
    return (localStorage.getItem(ACCENT_KEY) as AccentColor) || 'neutral';
}

function resolveTheme(theme: Theme): 'dark' | 'light' {
    if (theme !== 'system') return theme;
    if (!browser) return 'dark';
    return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
}

function applyTheme(resolved: 'dark' | 'light') {
    if (!browser) return;
    document.documentElement.setAttribute('data-theme', resolved);
    const metaTheme = document.querySelector('meta[name="theme-color"]');
    if (metaTheme) {
        metaTheme.setAttribute('content', resolved === 'dark' ? '#000000' : '#FFFFFF');
    }
}

function applyAccent(accent: AccentColor) {
    if (!browser) return;
    document.documentElement.setAttribute('data-accent', accent);
}

function createThemeStore() {
    const { subscribe, set, update } = writable<Theme>(getInitialTheme());

    return {
        subscribe,
        set(value: Theme) {
            set(value);
            if (browser) try { localStorage.setItem(THEME_KEY, value); } catch { /* ignored */ }
            applyTheme(resolveTheme(value));
        },
        toggle() {
            update((current) => {
                const resolved = resolveTheme(current);
                const next = resolved === 'dark' ? 'light' : 'dark';
                if (browser) try { localStorage.setItem(THEME_KEY, next); } catch { /* ignored */ }
                applyTheme(next);
                return next;
            });
        },
        init() {
            const theme = getInitialTheme();
            const resolved = resolveTheme(theme);
            applyTheme(resolved);

            if (browser) {
                const mq = window.matchMedia('(prefers-color-scheme: light)');
                mq.addEventListener('change', () => {
                    let current: Theme = 'system';
                    subscribe((v) => (current = v))();
                    if (current === 'system') {
                        applyTheme(resolveTheme('system'));
                    }
                });
            }
        }
    };
}

function createAccentStore() {
    const { subscribe, set } = writable<AccentColor>(getInitialAccent());

    return {
        subscribe,
        set(value: AccentColor) {
            set(value);
            if (browser) try { localStorage.setItem(ACCENT_KEY, value); } catch { /* ignored */ }
            applyAccent(value);
        },
        init() {
            const accent = getInitialAccent();
            applyAccent(accent);
        }
    };
}

export const theme = createThemeStore();
export const accent = createAccentStore();
