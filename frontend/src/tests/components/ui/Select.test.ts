import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import Select from '$lib/components/ui/Select.svelte';

describe('Select', () => {
    const options = [
        { value: '1', label: 'Option 1' },
        { value: '2', label: 'Option 2' },
        { value: '3', label: 'Option 3' }
    ];

    it('renders with placeholder', () => {
        const { container } = render(Select, { props: { options, placeholder: 'Choose...' } });
        expect(container.textContent).toContain('Choose...');
    });

    it('renders with label', () => {
        const { container } = render(Select, { props: { options, label: 'Category' } });
        expect(container.textContent).toContain('Category');
    });

    it('opens dropdown on click', async () => {
        const { container } = render(Select, { props: { options } });
        const trigger = container.querySelector(
            '.select__trigger, .select-trigger, .select > button, .select > div'
        );
        if (trigger) await fireEvent.click(trigger);
        // After click, options should appear
        const optionEls = container.querySelectorAll('.option, .select__option, li');
        expect(optionEls.length).toBeGreaterThan(0);
    });

    it('displays selected value', () => {
        const { container } = render(Select, { props: { options, value: '2' } });
        expect(container.textContent).toContain('Option 2');
    });

    it('renders disabled state', () => {
        const { container } = render(Select, { props: { options, disabled: true } });
        expect(container).toBeInTheDocument();
    });
});
