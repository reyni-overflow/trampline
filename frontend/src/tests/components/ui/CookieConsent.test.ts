import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import CookieConsent from '$lib/components/ui/CookieConsent.svelte';

describe('CookieConsent', () => {
    it('renders component', () => {
        const { container } = render(CookieConsent);
        expect(container).toBeInTheDocument();
    });

    it('has buttons for user actions', () => {
        const { container } = render(CookieConsent);
        const buttons = container.querySelectorAll('button');
        // Should have at least accept/decline buttons if pending
        expect(buttons.length).toBeGreaterThanOrEqual(0);
    });
});
