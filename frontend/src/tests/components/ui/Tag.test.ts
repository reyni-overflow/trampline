import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import SnippetWrapper from '../../helpers/SnippetWrapper.svelte';
import Tag from '$lib/components/ui/Tag.svelte';

function renderTag(props: Record<string, unknown> = {}, text = 'TypeScript') {
    return render(SnippetWrapper, { props: { component: Tag, props, text } });
}

describe('Tag', () => {
    it('renders with text content', () => {
        const { container } = renderTag();
        expect(container.textContent).toContain('TypeScript');
    });

    it('applies selected class when selected', () => {
        const { container } = renderTag({ selected: true });
        const tag = container.querySelector('.tag');
        expect(tag?.className).toContain('selected');
    });

    it('renders as button when clickable', () => {
        const { container } = renderTag({ clickable: true });
        const btn = container.querySelector('button.tag');
        expect(btn).toBeInTheDocument();
    });

    it('shows remove button when removable and clickable', () => {
        const { container } = renderTag({ removable: true, clickable: true });
        const removeBtn = container.querySelector('.remove');
        expect(removeBtn).toBeTruthy();
    });

    it('fires onclick when clickable tag clicked', async () => {
        const onclick = vi.fn();
        const { container } = renderTag({ clickable: true, onclick });
        const tag = container.querySelector('button.tag');
        if (tag) await fireEvent.click(tag);
        expect(onclick).toHaveBeenCalled();
    });
});
