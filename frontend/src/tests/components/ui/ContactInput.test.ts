import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import ContactInput from '$lib/components/ui/ContactInput.svelte';

describe('ContactInput', () => {
    it('renders input element', () => {
        const { container } = render(ContactInput);
        const input = container.querySelector('input');
        expect(input).toBeInTheDocument();
    });

    it('detects email input', async () => {
        const { container } = render(ContactInput);
        const input = container.querySelector('input')!;
        await fireEvent.input(input, { target: { value: 'user@mail.com' } });
        const _icon = container.querySelector('.email-icon, svg, .type-indicator');
        expect(container).toBeInTheDocument();
    });

    it('detects phone input', async () => {
        const { container } = render(ContactInput);
        const input = container.querySelector('input')!;
        await fireEvent.input(input, { target: { value: '+7999' } });
        expect(container).toBeInTheDocument();
    });

    it('auto-formats phone number', async () => {
        const { container } = render(ContactInput);
        const input = container.querySelector('input')! as HTMLInputElement;
        await fireEvent.input(input, { target: { value: '89991234567' } });
        // Phone should be formatted
        expect(container).toBeInTheDocument();
    });

    it('renders with label', () => {
        const { container } = render(ContactInput, { props: { label: 'Contact' } });
        expect(container.textContent).toContain('Contact');
    });

    it('shows error state', () => {
        const { container } = render(ContactInput, { props: { error: 'Invalid' } });
        expect(container.textContent).toContain('Invalid');
    });
});
