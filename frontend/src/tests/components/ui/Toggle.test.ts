import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import Toggle from '$lib/components/ui/Toggle.svelte';

describe('Toggle', () => {
    it('renders toggle element', () => {
        const { container } = render(Toggle);
        const toggle = container.querySelector('.toggle');
        expect(toggle).toBeInTheDocument();
    });

    it('renders with label', () => {
        const { container } = render(Toggle, { props: { label: 'Dark Mode' } });
        expect(container.textContent).toContain('Dark Mode');
    });

    it('renders track and thumb', () => {
        const { container } = render(Toggle);
        const track = container.querySelector('.track');
        const thumb = container.querySelector('.thumb');
        expect(track).toBeInTheDocument();
        expect(thumb).toBeInTheDocument();
    });

    it('has track.on class when checked', () => {
        const { container } = render(Toggle, { props: { checked: true } });
        const track = container.querySelector('.track');
        expect(track?.className).toContain('on');
    });

    it('is disabled when disabled prop set', () => {
        const { container } = render(Toggle, { props: { disabled: true } });
        const toggle = container.querySelector('.toggle');
        expect(toggle?.className).toContain('disabled');
    });

    it('has hidden checkbox input', () => {
        const { container } = render(Toggle);
        const input = container.querySelector('input[type="checkbox"]');
        expect(input).toBeInTheDocument();
    });
});
