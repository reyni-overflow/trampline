import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import SnippetWrapper from '../../helpers/SnippetWrapper.svelte';
import Modal from '$lib/components/ui/Modal.svelte';

function renderModal(props: Record<string, unknown> = {}, text = 'Modal content') {
    return render(SnippetWrapper, { props: { component: Modal, props, text } });
}

describe('Modal', () => {
    it('renders content when open', () => {
        const { container } = renderModal({ open: true, onclose: vi.fn() });
        expect(container.textContent).toContain('Modal content');
    });

    it('does not render when closed', () => {
        const { container } = renderModal({ open: false, onclose: vi.fn() });
        const overlay = container.querySelector('.modal-overlay');
        expect(overlay).not.toBeInTheDocument();
    });

    it('calls onclose when backdrop clicked', async () => {
        const onclose = vi.fn();
        const { container } = renderModal({ open: true, onclose });
        const overlay = container.querySelector('.modal-overlay');
        if (overlay) await fireEvent.click(overlay);
        expect(onclose).toHaveBeenCalled();
    });

    it('calls onclose on Escape key', async () => {
        const onclose = vi.fn();
        renderModal({ open: true, onclose });
        await fireEvent.keyDown(document, { key: 'Escape' });
        expect(onclose).toHaveBeenCalled();
    });

    it('renders title when provided', () => {
        const { container } = renderModal({ open: true, title: 'Test Title', onclose: vi.fn() });
        expect(container.textContent).toContain('Test Title');
    });
});
