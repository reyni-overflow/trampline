import { writable, get } from 'svelte/store';
import { browser } from '$app/environment';

const STORAGE_KEY = 'cookie-consent';

export type ConsentStatus = 'pending' | 'accepted' | 'declined';

export interface CookieCategories {
    required: boolean;
    analytics: boolean;
    marketing: boolean;
}

interface CookieConsentState {
    status: ConsentStatus;
    categories: CookieCategories;
    shaking: boolean;
}

const DEFAULT_CATEGORIES: CookieCategories = {
    required: true,
    analytics: false,
    marketing: false
};

const ALLOWED_PATHS = [
    '/',
    '/privacy',
    '/terms',
    '/help',
    '/about',
    '/contacts',
    '/mobile-app'
];

function loadState(): CookieConsentState {
    if (!browser) return { status: 'pending', categories: DEFAULT_CATEGORIES, shaking: false };
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (raw) {
            const parsed = JSON.parse(raw);
            if (parsed.status === 'accepted') {
                return { status: 'accepted', categories: parsed.categories || DEFAULT_CATEGORIES, shaking: false };
            }
            if (parsed.status === 'declined') {
                return { status: 'declined', categories: DEFAULT_CATEGORIES, shaking: false };
            }
        }
    } catch { /* ignored */ }
    return { status: 'pending', categories: DEFAULT_CATEGORIES, shaking: false };
}

function saveState(status: ConsentStatus, categories: CookieCategories) {
    if (!browser) return;
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify({ status, categories })); } catch { /* ignored */ }
}

const store = writable<CookieConsentState>(loadState());

let shakeTimer: ReturnType<typeof setTimeout> | null = null;

export const cookieConsent = {
    subscribe: store.subscribe,

    accept(categories: CookieCategories) {
        const cats = { ...categories, required: true };
        saveState('accepted', cats);
        store.set({ status: 'accepted', categories: cats, shaking: false });
    },

    decline() {
        saveState('declined', DEFAULT_CATEGORIES);
        store.set({ status: 'declined', categories: DEFAULT_CATEGORIES, shaking: false });
    },

    reconsider() {
        if (!browser) return;
        try { localStorage.removeItem(STORAGE_KEY); } catch { /* ignored */ }
        store.set({ status: 'pending', categories: DEFAULT_CATEGORIES, shaking: false });
    },

    shake() {
        if (shakeTimer) clearTimeout(shakeTimer);
        store.update(s => ({ ...s, shaking: true }));
        shakeTimer = setTimeout(() => {
            store.update(s => ({ ...s, shaking: false }));
            shakeTimer = null;
        }, 500);
    },

    isAccepted(): boolean {
        return get(store).status === 'accepted';
    },

    isPathAllowed(path: string): boolean {
        if (get(store).status === 'accepted') return true;
        return ALLOWED_PATHS.some(p => path === p || path.startsWith(p + '/'));
    },

    requestAccess(path?: string): boolean {
        const state = get(store);
        if (state.status === 'accepted') return true;
        if (path && this.isPathAllowed(path)) return true;
        this.shake();
        return false;
    },

};
