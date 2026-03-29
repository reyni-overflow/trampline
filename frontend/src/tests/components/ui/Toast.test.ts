import { describe, it, expect, vi } from 'vitest';
import { render } from '@testing-library/svelte';
import Toast from '$lib/components/ui/Toast.svelte';

describe('Toast', () => {
    const defaultProps = {
        id: 'test-1',
        type: 'info' as const,
        message: 'Test message',
        onclose: vi.fn()
    };

    it('renders message', () => {
        const { container } = render(Toast, { props: defaultProps });
        expect(container.textContent).toContain('Test message');
    });

    it('applies info type class', () => {
        const { container } = render(Toast, { props: defaultProps });
        const toast = container.querySelector('.toast');
        expect(toast?.className).toContain('info');
    });

    it('applies success type class', () => {
        const { container } = render(Toast, { props: { ...defaultProps, type: 'success' } });
        const toast = container.querySelector('.toast');
        expect(toast?.className).toContain('success');
    });

    it('applies error type class', () => {
        const { container } = render(Toast, { props: { ...defaultProps, type: 'error' } });
        const toast = container.querySelector('.toast');
        expect(toast?.className).toContain('error');
    });

    it('applies warning type class', () => {
        const { container } = render(Toast, { props: { ...defaultProps, type: 'warning' } });
        const toast = container.querySelector('.toast');
        expect(toast?.className).toContain('warning');
    });

    it('has close button', () => {
        const { container } = render(Toast, { props: defaultProps });
        const closeBtn = container.querySelector('.close, button');
        expect(closeBtn).toBeTruthy();
    });

    it('has a close button element', () => {
        const { container } = render(Toast, { props: defaultProps });
        const btns = container.querySelectorAll('button, .close');
        expect(btns.length).toBeGreaterThan(0);
    });

    it('has icon', () => {
        const { container } = render(Toast, { props: defaultProps });
        const icon = container.querySelector('svg, .icon');
        expect(icon).toBeTruthy();
    });
});
