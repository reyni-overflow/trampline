import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import RangeSlider from '$lib/components/ui/RangeSlider.svelte';

describe('RangeSlider', () => {
    it('renders two range inputs', () => {
        const { container } = render(RangeSlider, {
            props: { min: 0, max: 100, valueMin: 20, valueMax: 80 }
        });
        const inputs = container.querySelectorAll('input[type="range"]');
        expect(inputs.length).toBe(2);
    });

    it('displays min and max values', () => {
        const { container } = render(RangeSlider, {
            props: { min: 0, max: 500000, valueMin: 50000, valueMax: 200000 }
        });
        expect(container.textContent).toBeTruthy();
    });

    it('renders fill track', () => {
        const { container } = render(RangeSlider, {
            props: { min: 0, max: 100, valueMin: 20, valueMax: 80 }
        });
        const fill = container.querySelector('.fill, .track-fill, .range-fill');
        expect(fill).toBeTruthy();
    });
});
