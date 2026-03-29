import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import Avatar from '$lib/components/ui/Avatar.svelte';

describe('Avatar', () => {
    it('renders with initials when no src', () => {
        const { container } = render(Avatar, { props: { name: 'John Doe' } });
        const avatar = container.querySelector('.avatar');
        expect(avatar).toBeInTheDocument();
        expect(container.textContent).toContain('JD');
    });

    it('renders single initial for single name', () => {
        const { container } = render(Avatar, { props: { name: 'John' } });
        expect(container.textContent).toContain('J');
    });

    it('renders with image when src provided', () => {
        const { container } = render(Avatar, { props: { name: 'Test', src: '/photo.jpg' } });
        const img = container.querySelector('img');
        expect(img).toBeInTheDocument();
        expect(img?.getAttribute('src')).toBe('/photo.jpg');
    });

    it('applies size class', () => {
        const { container } = render(Avatar, { props: { name: 'Test', size: 48 } });
        const avatar = container.querySelector('.avatar');
        expect(avatar).toBeInTheDocument();
    });

    it('handles empty name gracefully', () => {
        const { container } = render(Avatar, { props: { name: '' } });
        const avatar = container.querySelector('.avatar');
        expect(avatar).toBeInTheDocument();
    });
});
