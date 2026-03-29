import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import ThemeToggle from '$lib/components/ui/ThemeToggle.svelte';

vi.mock('$lib/stores/theme', () => ({
    theme: {
        subscribe: vi.fn((fn: (value: string) => void) => { fn('dark'); return () => {}; }),
        toggle: vi.fn(),
        set: vi.fn()
    }
}));

describe('ThemeToggle', () => {
    it('renders toggle button', () => {
        const { container } = render(ThemeToggle);
        const btn = container.querySelector('button');
        expect(btn).toBeInTheDocument();
    });

    it('has sun/moon icon', () => {
        const { container } = render(ThemeToggle);
        const icon = container.querySelector('svg');
        expect(icon).toBeInTheDocument();
    });

    it('toggles theme on click', async () => {
        const { theme } = await import('$lib/stores/theme');
        const { container } = render(ThemeToggle);
        const btn = container.querySelector('button')!;
        await fireEvent.click(btn);
        expect(theme.toggle).toHaveBeenCalled();
    });
});
