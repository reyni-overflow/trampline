import { writable, derived, get } from 'svelte/store';
import { browser } from '$app/environment';
import { ru } from './ru';

export type Locale = 'ru' | 'en' | 'zh' | 'pirate';

export const LOCALES: { id: Locale; label: string; flag: string }[] = [
    { id: 'ru', label: 'Русский', flag: '🇷🇺' },
    { id: 'en', label: 'English', flag: '🇺🇸' },
    { id: 'zh', label: '中文', flag: '🇨🇳' },
    { id: 'pirate', label: 'Пиратский', flag: '🏴‍☠️' }
];

const LOCALE_KEY = 'trampline-locale';
const VALID_LOCALES: Locale[] = ['ru', 'en', 'zh', 'pirate'];

const loadedDictionaries: Partial<Record<Locale, Record<string, string>>> = { ru };

const loaders: Record<Locale, () => Promise<Record<string, string>>> = {
    ru: async () => ru,
    en: async () => (await import('./en')).en,
    zh: async () => (await import('./zh')).zh,
    pirate: async () => (await import('./pirate')).pirate
};

async function loadDictionary(loc: Locale): Promise<Record<string, string>> {
    if (loadedDictionaries[loc]) return loadedDictionaries[loc]!;
    const dict = await loaders[loc]();
    loadedDictionaries[loc] = dict;
    return dict;
}

function getInitialLocale(): Locale {
    if (!browser) return 'ru';
    const saved = localStorage.getItem(LOCALE_KEY) as Locale;

    if (saved && VALID_LOCALES.includes(saved)) return saved;
    const lang = navigator.language.slice(0, 2);

    if (lang === 'zh') return 'zh';
    if (lang === 'en') return 'en';

    return 'ru';
}

const dictStore = writable<Record<string, string>>(ru);

function createLocaleStore() {
    const initialLocale = getInitialLocale();
    const { subscribe, set: rawSet } = writable<Locale>(initialLocale);

    if (initialLocale !== 'ru') {
        loadDictionary(initialLocale).then(dict => dictStore.set(dict));
    }

    return {
        subscribe,
        set(value: Locale) {
            rawSet(value);
            if (browser) {
                try { localStorage.setItem(LOCALE_KEY, value); } catch { /* ignored */ }
                document.documentElement.lang = value === 'zh' ? 'zh-CN' : value === 'pirate' ? 'ru' : value;
            }
            loadDictionary(value).then(dict => dictStore.set(dict));
        },
        init() {
            const loc = getInitialLocale();
            if (browser) {
                document.documentElement.lang = loc === 'zh' ? 'zh-CN' : loc === 'pirate' ? 'ru' : loc;
            }
        }
    };
}

export const locale = createLocaleStore();

export const t = derived(dictStore, ($dict) => {
    const fallback = ru;

    return (key: string, params?: Record<string, string | number>): string => {
        let text = $dict[key] ?? fallback[key] ?? key;

        if (params) {
            for (const [k, v] of Object.entries(params)) {
                text = text.replaceAll(`{${k}}`, String(v));
            }
        }

        return text;
    };
});


export function tGet(key: string, params?: Record<string, string | number>): string {
    return get(t)(key, params);
}

export function getLocale(): Locale {
    return get(locale);
}


export function getLocaleDateString(): string {
    const loc = get(locale);

    if (loc === 'zh') return 'zh-CN';
    if (loc === 'en') return 'en-US';
    if (loc === 'pirate') return 'ru-RU';

    return 'ru-RU';
}


export function pluralForm(n: number, one: string, few: string, many: string, other?: string): string {
    const loc = get(locale);

    if (loc === 'zh') {
        return (other ?? many).replaceAll('{n}', String(n));
    }

    if (loc === 'en') {
        const form = Math.abs(n) === 1 ? one : (other ?? many);
        return form.replaceAll('{n}', String(n));
    }

    const abs = Math.abs(n) % 100;
    const last = abs % 10;

    let form: string;

    if (abs > 10 && abs < 20) form = many;
    else if (last > 1 && last < 5) form = few;
    else if (last === 1) form = one;
    else form = many;

    return form.replaceAll('{n}', String(n));
}
