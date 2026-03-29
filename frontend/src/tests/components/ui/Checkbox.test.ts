import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import Checkbox from '$lib/components/ui/Checkbox.svelte';

describe('Checkbox', () => {
    it('renders checkbox element', () => {
        const { container } = render(Checkbox);
        const checkbox = container.querySelector('.checkbox');
        expect(checkbox).toBeInTheDocument();
    });

    it('renders with label', () => {
        const { container } = render(Checkbox, { props: { label: 'Accept terms' } });
        expect(container.textContent).toContain('Accept terms');
    });

    it('has hidden input', () => {
        const { container } = render(Checkbox);
        const input = container.querySelector('input[type="checkbox"]');
        expect(input).toBeInTheDocument();
    });

    it('has box with checked class when checked', () => {
        const { container } = render(Checkbox, { props: { checked: true } });
        const box = container.querySelector('.box');
        expect(box?.className).toContain('checked');
    });

    it('is disabled when disabled prop set', () => {
        const { container } = render(Checkbox, { props: { disabled: true } });
        const checkbox = container.querySelector('.checkbox');
        expect(checkbox?.className).toContain('disabled');
    });

    it('has SVG checkmark', () => {
        const { container } = render(Checkbox);
        const svg = container.querySelector('svg');
        expect(svg).toBeInTheDocument();
    });
});
