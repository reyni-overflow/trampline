import { describe, it, expect, beforeEach } from 'vitest';
import { get } from 'svelte/store';
import { forgotPasswordModal } from '$lib/stores/forgot-password-modal';

describe('forgot-password-modal store', () => {
    beforeEach(() => {
        forgotPasswordModal.close();
    });

    it('starts closed', () => {
        expect(get(forgotPasswordModal).open).toBe(false);
    });

    it('opens', () => {
        forgotPasswordModal.open();
        expect(get(forgotPasswordModal).open).toBe(true);
    });

    it('closes', () => {
        forgotPasswordModal.open();
        forgotPasswordModal.close();
        expect(get(forgotPasswordModal).open).toBe(false);
    });
});
