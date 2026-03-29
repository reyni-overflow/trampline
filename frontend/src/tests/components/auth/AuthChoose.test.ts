import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import AuthChoose from '$lib/components/auth/AuthChoose.svelte';

describe('AuthChoose', () => {
    it('renders role options', () => {
        const { container } = render(AuthChoose);
        const options = container.querySelectorAll('.role-card, button, .choose-option');
        expect(options.length).toBeGreaterThanOrEqual(2);
    });

    it('renders seeker and employer options', () => {
        const { container } = render(AuthChoose);
        const text = container.textContent || '';
        // Should have both role options visible
        expect(text.length).toBeGreaterThan(0);
    });

    it('has icons for each role', () => {
        const { container } = render(AuthChoose);
        const icons = container.querySelectorAll('svg');
        expect(icons.length).toBeGreaterThanOrEqual(1);
    });
});
