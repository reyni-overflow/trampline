import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import PasswordInput from '$lib/components/ui/PasswordInput.svelte';

describe('PasswordInput', () => {
    it('renders password input', () => {
        const { container } = render(PasswordInput);
        const input = container.querySelector('input');
        expect(input).toBeInTheDocument();
        expect(input?.getAttribute('type')).toBe('password');
    });

    it('toggles password visibility', async () => {
        const { container } = render(PasswordInput);
        const toggleBtn = container.querySelector('button');
        if (toggleBtn) {
            await fireEvent.click(toggleBtn);
            const input = container.querySelector('input');
            expect(input?.getAttribute('type')).toBe('text');
        }
    });

    it('renders with label', () => {
        const { container } = render(PasswordInput, { props: { label: 'Password' } });
        expect(container.textContent).toContain('Password');
    });

    it('shows error state', () => {
        const { container } = render(PasswordInput, { props: { error: 'Too weak' } });
        expect(container.textContent).toContain('Too weak');
    });

    it('has eye icon button', () => {
        const { container } = render(PasswordInput);
        const btn = container.querySelector('button');
        expect(btn).toBeInTheDocument();
    });
});
