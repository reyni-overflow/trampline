import { writable } from 'svelte/store';
import { cookieConsent } from '$lib/stores/cookie-consent';

export type AuthModalView = 'choose' | 'login' | 'register';
export type AuthRole = 'Worker' | 'Employee';

interface AuthModalState {
    open: boolean;
    view: AuthModalView;
    role: AuthRole;
}

const INITIAL: AuthModalState = {
    open: false,
    view: 'choose',
    role: 'Worker'
};

function createAuthModalStore() {
    const { subscribe, set, update } = writable<AuthModalState>(INITIAL);

    return {
        subscribe,

        open() {
            if (!cookieConsent.requestAccess()) return;
            set({ ...INITIAL, open: true });
        },

        openLogin(role?: AuthRole) {
            if (!cookieConsent.requestAccess()) return;
            set({ open: true, view: 'login', role: role || 'Worker' });
        },

        openRegister(role?: AuthRole) {
            if (!cookieConsent.requestAccess()) return;
            set({ open: true, view: 'register', role: role || 'Worker' });
        },

        goToLogin() {
            update((s) => ({ ...s, view: 'login' }));
        },

        goToRegister() {
            update((s) => ({ ...s, view: 'register' }));
        },

        goBack() {
            update((s) => ({ ...s, view: 'choose' }));
        },

        setRole(role: AuthRole) {
            update((s) => ({ ...s, role }));
        },

        close() {
            set(INITIAL);
        }
    };
}

export const authModal = createAuthModalStore();
