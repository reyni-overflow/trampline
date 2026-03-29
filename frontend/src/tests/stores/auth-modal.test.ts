import { describe, it, expect, beforeEach, vi } from 'vitest';
import { get } from 'svelte/store';
import { authModal } from '$lib/stores/auth-modal';

vi.mock('$lib/stores/cookie-consent', () => ({
    cookieConsent: {
        requestAccess: vi.fn(() => true),
        subscribe: vi.fn()
    }
}));

describe('auth-modal store', () => {
    beforeEach(() => {
        authModal.close();
    });

    it('starts closed', () => {
        const state = get(authModal);
        expect(state.open).toBe(false);
        expect(state.view).toBe('choose');
        expect(state.role).toBe('Worker');
    });

    it('opens with choose view', () => {
        authModal.open();
        const state = get(authModal);
        expect(state.open).toBe(true);
        expect(state.view).toBe('choose');
    });

    it('opens login directly', () => {
        authModal.openLogin();
        const state = get(authModal);
        expect(state.open).toBe(true);
        expect(state.view).toBe('login');
    });

    it('opens login with specific role', () => {
        authModal.openLogin('Employee');
        expect(get(authModal).role).toBe('Employee');
    });

    it('opens register directly', () => {
        authModal.openRegister();
        const state = get(authModal);
        expect(state.open).toBe(true);
        expect(state.view).toBe('register');
    });

    it('opens register with specific role', () => {
        authModal.openRegister('Employee');
        expect(get(authModal).role).toBe('Employee');
    });

    it('navigates to login from choose', () => {
        authModal.open();
        authModal.goToLogin();
        expect(get(authModal).view).toBe('login');
    });

    it('navigates to register from choose', () => {
        authModal.open();
        authModal.goToRegister();
        expect(get(authModal).view).toBe('register');
    });

    it('navigates back to choose', () => {
        authModal.openLogin();
        authModal.goBack();
        expect(get(authModal).view).toBe('choose');
    });

    it('sets role', () => {
        authModal.open();
        authModal.setRole('Employee');
        expect(get(authModal).role).toBe('Employee');
    });

    it('closes and resets to initial state', () => {
        authModal.openLogin('Employee');
        authModal.close();
        const state = get(authModal);
        expect(state.open).toBe(false);
        expect(state.view).toBe('choose');
        expect(state.role).toBe('Worker');
    });

    it('does not open when cookie consent denied', async () => {
        const { cookieConsent } = await import('$lib/stores/cookie-consent');
        (cookieConsent.requestAccess as ReturnType<typeof vi.fn>).mockReturnValueOnce(false);
        authModal.open();
        expect(get(authModal).open).toBe(false);
    });
});
