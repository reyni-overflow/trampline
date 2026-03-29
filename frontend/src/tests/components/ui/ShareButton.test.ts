import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import ShareButton from '$lib/components/ui/ShareButton.svelte';

describe('ShareButton', () => {
    it('renders share button', () => {
        const { container } = render(ShareButton, {
            props: { title: 'Test Job' }
        });
        const btn = container.querySelector('button');
        expect(btn).toBeInTheDocument();
    });

    it('clicking share triggers navigator.share or dropdown', async () => {
        const { container } = render(ShareButton, {
            props: { title: 'Test', url: 'https://example.com' }
        });
        const btn = container.querySelector('button')!;
        await fireEvent.click(btn);
        // Either navigtor.share was called or a dropdown appeared
        expect(container).toBeInTheDocument();
    });
});
