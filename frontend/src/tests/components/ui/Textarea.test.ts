import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import Textarea from '$lib/components/ui/Textarea.svelte';

describe('Textarea', () => {
    it('renders textarea element', () => {
        const { container } = render(Textarea);
        const textarea = container.querySelector('textarea');
        expect(textarea).toBeInTheDocument();
    });

    it('renders with label', () => {
        const { container } = render(Textarea, { props: { label: 'Description' } });
        expect(container.textContent).toContain('Description');
    });

    it('renders with placeholder', () => {
        const { container } = render(Textarea, { props: { placeholder: 'Enter text...' } });
        const textarea = container.querySelector('textarea');
        expect(textarea?.getAttribute('placeholder')).toBe('Enter text...');
    });

    it('shows error message', () => {
        const { container } = render(Textarea, { props: { error: 'Too short' } });
        expect(container.textContent).toContain('Too short');
    });

    it('handles disabled state', () => {
        const { container } = render(Textarea, { props: { disabled: true } });
        const textarea = container.querySelector('textarea');
        expect(textarea?.disabled).toBe(true);
    });

    it('handles input event', async () => {
        const { container } = render(Textarea);
        const textarea = container.querySelector('textarea')!;
        await fireEvent.input(textarea, { target: { value: 'Hello world' } });
    });
});
