import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import ComparisonBar from '$lib/components/ui/ComparisonBar.svelte';

describe('ComparisonBar', () => {
    it('renders component without crash', () => {
        const { container } = render(ComparisonBar);
        expect(container).toBeInTheDocument();
    });
});
