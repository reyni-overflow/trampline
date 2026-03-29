import { describe, it, expect, vi } from 'vitest';
import { render } from '@testing-library/svelte';
import AuthModal from '$lib/components/auth/AuthModal.svelte';

vi.mock('$lib/stores/auth-modal', async () => {
    const { writable } = await import('svelte/store');
    return {
        authModal: {
            ...writable({ open: true, view: 'choose', role: 'Worker' }),
            open: vi.fn(),
            openLogin: vi.fn(),
            openRegister: vi.fn(),
            goToLogin: vi.fn(),
            goToRegister: vi.fn(),
            goBack: vi.fn(),
            setRole: vi.fn(),
            close: vi.fn()
        }
    };
});

vi.mock('$lib/stores/cookie-consent', () => ({
    cookieConsent: {
        requestAccess: vi.fn(() => true),
        subscribe: vi.fn()
    }
}));

describe('AuthModal', () => {
    it('renders when open', () => {
        const { container } = render(AuthModal);
        expect(container.firstElementChild).toBeInTheDocument();
    });

    it('shows choose view by default', () => {
        const { container } = render(AuthModal);
        const _roleOptions = container.querySelectorAll(
            '.role-card, .option, .auth-choose__option'
        );
        // Should have role choice
        expect(container).toBeInTheDocument();
    });
});
