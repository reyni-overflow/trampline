import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import ViewToggle from '$lib/components/ui/ViewToggle.svelte';

describe('ViewToggle', () => {
    it('renders two toggle buttons', () => {
        const { container } = render(ViewToggle, { props: { mode: 'grid', onchange: vi.fn() } });
        const buttons = container.querySelectorAll('button');
        expect(buttons.length).toBe(2);
    });

    it('highlights active mode', () => {
        const { container } = render(ViewToggle, { props: { mode: 'list', onchange: vi.fn() } });
        const activeBtn = container.querySelector('.active, [aria-pressed="true"]');
        expect(activeBtn).toBeInTheDocument();
    });

    it('fires onchange when toggled', async () => {
        const onchange = vi.fn();
        const { container } = render(ViewToggle, { props: { mode: 'grid', onchange } });
        const buttons = container.querySelectorAll('button');
        await fireEvent.click(buttons[1]);
        expect(onchange).toHaveBeenCalled();
    });
});
