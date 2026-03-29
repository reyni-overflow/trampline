import { describe, it, expect, vi } from 'vitest';
import { render } from '@testing-library/svelte';
import CommandPalette from '$lib/components/ui/CommandPalette.svelte';

vi.mock('$lib/api/jobs', () => ({
    jobsApi: { getAll: vi.fn().mockResolvedValue({ items: [], totalCount: 0 }) }
}));
vi.mock('$lib/api/events', () => ({
    eventsApi: { getAll: vi.fn().mockResolvedValue({ items: [], totalCount: 0 }) }
}));
vi.mock('$lib/api/employees', () => ({
    employeesApi: { getAll: vi.fn().mockResolvedValue({ items: [], total: 0 }) }
}));

describe('CommandPalette', () => {
    it('renders when open', () => {
        const { container } = render(CommandPalette, { props: { open: true } });
        const palette = container.querySelector('.command-palette, .palette, input');
        expect(palette).toBeInTheDocument();
    });

    it('does not render when closed', () => {
        const { container } = render(CommandPalette, { props: { open: false } });
        const input = container.querySelector('input');
        expect(input).toBeNull();
    });

    it('has search input', () => {
        const { container } = render(CommandPalette, { props: { open: true } });
        const input = container.querySelector('input');
        expect(input).toBeInTheDocument();
    });
});
