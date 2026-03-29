import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import Input from '$lib/components/ui/Input.svelte';

describe('Input', () => {
    it('renders text input', () => {
        const { container } = render(Input);
        const input = container.querySelector('input');
        expect(input).toBeInTheDocument();
    });

    it('renders with label', () => {
        const { container } = render(Input, { props: { label: 'Name' } });
        expect(container.textContent).toContain('Name');
    });

    it('renders with placeholder', () => {
        const { container } = render(Input, { props: { placeholder: 'Enter...' } });
        const input = container.querySelector('input');
        expect(input?.getAttribute('placeholder')).toBe('Enter...');
    });

    it('shows error message', () => {
        const { container } = render(Input, { props: { error: 'Required field' } });
        expect(container.textContent).toContain('Required field');
    });

    it('shows hint message', () => {
        const { container } = render(Input, { props: { hint: 'Enter your name' } });
        expect(container.textContent).toContain('Enter your name');
    });

    it('applies disabled state', () => {
        const { container } = render(Input, { props: { disabled: true } });
        const input = container.querySelector('input');
        expect(input?.disabled).toBe(true);
    });

    it('handles input event', async () => {
        const { container } = render(Input);
        const input = container.querySelector('input')!;
        await fireEvent.input(input, { target: { value: 'Hello' } });
    });

    it('renders as number type', () => {
        const { container } = render(Input, { props: { type: 'number' } });
        const input = container.querySelector('input');
        expect(input?.getAttribute('type')).toBe('text');
        expect(input?.getAttribute('inputmode')).toBe('numeric');
    });

    it('renders as password type', () => {
        const { container } = render(Input, { props: { type: 'password' } });
        const input = container.querySelector('input');
        expect(input?.getAttribute('type')).toBe('password');
    });
});
