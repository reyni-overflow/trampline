import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import SearchInput from '$lib/components/ui/SearchInput.svelte';

describe('SearchInput', () => {
    it('renders search input', () => {
        const { container } = render(SearchInput);
        const input = container.querySelector('input');
        expect(input).toBeInTheDocument();
    });

    it('renders with placeholder', () => {
        const { container } = render(SearchInput, { props: { placeholder: 'Search...' } });
        const input = container.querySelector('input');
        expect(input?.getAttribute('placeholder')).toBe('Search...');
    });

    it('shows clear button when value is not empty', () => {
        const { container } = render(SearchInput, { props: { value: 'test' } });
        const clearBtn = container.querySelector('.clear, button');
        expect(clearBtn).toBeTruthy();
    });

    it('has search icon', () => {
        const { container } = render(SearchInput);
        const icon = container.querySelector('svg, .icon, .search-icon');
        expect(icon).toBeTruthy();
    });
});
