import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import NotificationBell from '$lib/components/ui/NotificationBell.svelte';

describe('NotificationBell', () => {
    it('renders bell button', () => {
        const { container } = render(NotificationBell);
        const btn = container.querySelector('button');
        expect(btn).toBeInTheDocument();
    });

    it('has SVG icon', () => {
        const { container } = render(NotificationBell);
        const svg = container.querySelector('svg');
        expect(svg).toBeInTheDocument();
    });

    it('click does not crash', async () => {
        const { container } = render(NotificationBell);
        const btn = container.querySelector('button')!;
        await fireEvent.click(btn);
        expect(container).toBeInTheDocument();
    });
});
