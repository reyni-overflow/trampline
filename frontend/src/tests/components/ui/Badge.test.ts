import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import SnippetWrapper from '../../helpers/SnippetWrapper.svelte';
import Badge from '$lib/components/ui/Badge.svelte';

function renderBadge(props: Record<string, unknown> = {}, text = 'Active') {
    return render(SnippetWrapper, { props: { component: Badge, props, text } });
}

describe('Badge', () => {
    it('renders with text', () => {
        const { container } = renderBadge();
        expect(container.textContent).toContain('Active');
    });

    it('applies default variant', () => {
        const { container } = renderBadge();
        const badge = container.querySelector('.badge');
        expect(badge).toBeInTheDocument();
        expect(badge?.className).toContain('badge--default');
    });

    it('applies success variant', () => {
        const { container } = renderBadge({ variant: 'success' });
        expect(container.querySelector('.badge')?.className).toContain('success');
    });

    it('applies error variant', () => {
        const { container } = renderBadge({ variant: 'error' });
        expect(container.querySelector('.badge')?.className).toContain('error');
    });

    it('applies warning variant', () => {
        const { container } = renderBadge({ variant: 'warning' });
        expect(container.querySelector('.badge')?.className).toContain('warning');
    });

    it('applies info variant', () => {
        const { container } = renderBadge({ variant: 'info' });
        expect(container.querySelector('.badge')?.className).toContain('info');
    });

    it('applies accent variant', () => {
        const { container } = renderBadge({ variant: 'accent' });
        expect(container.querySelector('.badge')?.className).toContain('accent');
    });
});
