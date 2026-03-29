import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import AccentPicker from '$lib/components/ui/AccentPicker.svelte';

vi.mock('$lib/stores/theme', () => ({
    accent: {
        subscribe: vi.fn((fn: (value: string) => void) => { fn('neutral'); return () => {}; }),
        set: vi.fn()
    },
    ACCENT_COLORS: [
        { id: 'neutral', labelKey: 'accent.neutral', color: '' },
        { id: 'blue', labelKey: 'accent.blue', color: '#3B82F6' },
        { id: 'purple', labelKey: 'accent.purple', color: '#8B5CF6' }
    ]
}));

describe('AccentPicker', () => {
    it('renders picker button', () => {
        const { container } = render(AccentPicker);
        const btn = container.querySelector('button');
        expect(btn).toBeInTheDocument();
    });

    it('opens dropdown on click', async () => {
        const { container } = render(AccentPicker);
        const btn = container.querySelector('button')!;
        await fireEvent.click(btn);
        const dropdown = container.querySelector('.dropdown, .accent-dropdown, .picker');
        expect(dropdown).toBeTruthy();
    });

    it('renders color swatches', async () => {
        const { container } = render(AccentPicker);
        const btn = container.querySelector('button')!;
        await fireEvent.click(btn);
        const swatches = container.querySelectorAll('.swatch, .color, [data-accent]');
        expect(swatches.length).toBeGreaterThan(0);
    });
});
