import { describe, it, expect, beforeEach, vi } from 'vitest';
import { get } from 'svelte/store';
import { cookieConsent } from '$lib/stores/cookie-consent';

describe('cookie-consent store', () => {
    beforeEach(() => {
        localStorage.clear();
        cookieConsent.reconsider();
    });

    it('starts with pending status', () => {
        const state = get(cookieConsent);
        expect(state.status).toBe('pending');
        expect(state.categories.required).toBe(true);
        expect(state.categories.analytics).toBe(false);
        expect(state.categories.marketing).toBe(false);
    });

    it('accepts cookies with categories', () => {
        cookieConsent.accept({ required: true, analytics: true, marketing: false });
        const state = get(cookieConsent);
        expect(state.status).toBe('accepted');
        expect(state.categories.analytics).toBe(true);
        expect(state.categories.marketing).toBe(false);
    });

    it('always keeps required true', () => {
        cookieConsent.accept({ required: false, analytics: true, marketing: true });
        expect(get(cookieConsent).categories.required).toBe(true);
    });

    it('declines cookies', () => {
        cookieConsent.decline();
        const state = get(cookieConsent);
        expect(state.status).toBe('declined');
        expect(state.categories.analytics).toBe(false);
    });

    it('reconsidering resets to pending', () => {
        cookieConsent.accept({ required: true, analytics: true, marketing: true });
        cookieConsent.reconsider();
        expect(get(cookieConsent).status).toBe('pending');
    });

    it('isAccepted returns correct value', () => {
        expect(cookieConsent.isAccepted()).toBe(false);
        cookieConsent.accept({ required: true, analytics: false, marketing: false });
        expect(cookieConsent.isAccepted()).toBe(true);
    });

    it('isPathAllowed allows public paths when not accepted', () => {
        expect(cookieConsent.isPathAllowed('/')).toBe(true);
        expect(cookieConsent.isPathAllowed('/privacy')).toBe(true);
        expect(cookieConsent.isPathAllowed('/terms')).toBe(true);
        expect(cookieConsent.isPathAllowed('/help')).toBe(true);
    });

    it('isPathAllowed blocks non-public paths when not accepted', () => {
        expect(cookieConsent.isPathAllowed('/dashboard')).toBe(false);
        expect(cookieConsent.isPathAllowed('/jobs')).toBe(false);
    });

    it('isPathAllowed allows all paths when accepted', () => {
        cookieConsent.accept({ required: true, analytics: false, marketing: false });
        expect(cookieConsent.isPathAllowed('/dashboard')).toBe(true);
        expect(cookieConsent.isPathAllowed('/jobs')).toBe(true);
    });

    it('requestAccess returns true when accepted', () => {
        cookieConsent.accept({ required: true, analytics: false, marketing: false });
        expect(cookieConsent.requestAccess()).toBe(true);
    });

    it('requestAccess returns false when pending', () => {
        expect(cookieConsent.requestAccess()).toBe(false);
    });

    it('requestAccess allows public paths even when pending', () => {
        expect(cookieConsent.requestAccess('/')).toBe(true);
        expect(cookieConsent.requestAccess('/privacy')).toBe(true);
    });

    it('shake sets shaking to true temporarily', () => {
        vi.useFakeTimers();
        cookieConsent.shake();
        expect(get(cookieConsent).shaking).toBe(true);
        vi.advanceTimersByTime(600);
        expect(get(cookieConsent).shaking).toBe(false);
        vi.useRealTimers();
    });

    it('persists to localStorage on accept', () => {
        cookieConsent.accept({ required: true, analytics: true, marketing: false });
        expect(localStorage.setItem).toHaveBeenCalled();
    });

    it('persists to localStorage on decline', () => {
        cookieConsent.decline();
        expect(localStorage.setItem).toHaveBeenCalled();
    });
});
