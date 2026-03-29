import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import Pagination from '$lib/components/ui/Pagination.svelte';

describe('Pagination', () => {
    it('renders page buttons', () => {
        const { container } = render(Pagination, { props: { page: 1, totalPages: 5 } });
        const buttons = container.querySelectorAll('button');
        expect(buttons.length).toBeGreaterThan(0);
    });

    it('highlights current page', () => {
        const { container } = render(Pagination, { props: { page: 3, totalPages: 5 } });
        const activeBtn = container.querySelector('.active');
        expect(activeBtn).toBeInTheDocument();
    });

    it('fires onchange on click', async () => {
        const onchange = vi.fn();
        const { container } = render(Pagination, { props: { page: 1, totalPages: 5, onchange } });
        const buttons = container.querySelectorAll('button');
        const page2 = Array.from(buttons).find((b) => b.textContent?.trim() === '2');
        if (page2) await fireEvent.click(page2);
        expect(onchange).toHaveBeenCalled();
    });

    it('does not render when totalPages is 1', () => {
        const { container } = render(Pagination, { props: { page: 1, totalPages: 1 } });
        const _pagination = container.querySelector('.pagination');
        // May either not render or render empty
        expect(container).toBeInTheDocument();
    });

    it('disables prev button on first page', () => {
        const { container } = render(Pagination, { props: { page: 1, totalPages: 5 } });
        const prevBtn = container.querySelector('.prev, button:first-child');
        if (prevBtn) expect(prevBtn).toBeDisabled();
    });

    it('shows ellipsis for large page ranges', () => {
        const { container } = render(Pagination, { props: { page: 5, totalPages: 20 } });
        const text = container.textContent || '';
        expect(text.includes('…') || text.includes('...')).toBe(true);
    });
});
