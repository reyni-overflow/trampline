import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import SnippetWrapper from '../../helpers/SnippetWrapper.svelte';
import Button from '$lib/components/ui/Button.svelte';

function renderButton(props: Record<string, unknown> = {}, text = 'Click me') {
    return render(SnippetWrapper, { props: { component: Button, props, text } });
}

describe('Button', () => {
    it('renders with text', () => {
        const { container } = renderButton();
        expect(container.textContent).toContain('Click me');
    });

    it('renders as button by default', () => {
        const { container } = renderButton();
        const btn = container.querySelector('button');
        expect(btn).toBeInTheDocument();
    });

    it('renders as a link when href is provided', () => {
        const { container } = renderButton({ href: '/test' });
        const link = container.querySelector('a');
        expect(link).toBeInTheDocument();
        expect(link?.getAttribute('href')).toBe('/test');
    });

    it('applies variant class', () => {
        const { container } = renderButton({ variant: 'danger' });
        const btn = container.querySelector('button, a');
        expect(btn?.className).toContain('danger');
    });

    it('applies size class', () => {
        const { container } = renderButton({ size: 'sm' });
        const btn = container.querySelector('button, a');
        expect(btn?.className).toContain('sm');
    });

    it('is disabled when disabled prop is true', () => {
        const { container } = renderButton({ disabled: true });
        const btn = container.querySelector('button');
        expect(btn).toBeDisabled();
    });

    it('is disabled when loading is true', () => {
        const { container } = renderButton({ loading: true });
        const btn = container.querySelector('button');
        expect(btn).toBeDisabled();
    });

    it('shows loading class', () => {
        const { container } = renderButton({ loading: true });
        const btn = container.querySelector('button');
        expect(btn?.className).toContain('loading');
    });

    it('fires click event', async () => {
        const onclick = vi.fn();
        const { container } = renderButton({ onclick });
        const btn = container.querySelector('button')!;
        await fireEvent.click(btn);
        expect(onclick).toHaveBeenCalledTimes(1);
    });

    it('button has disabled attribute when disabled', () => {
        const { container } = renderButton({ disabled: true });
        const btn = container.querySelector('button');
        expect(btn).toBeDisabled();
    });
});
