import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import CompanyCard from '$lib/components/ui/CompanyCard.svelte';

describe('CompanyCard', () => {
    const props = {
        id: 'comp-1',
        name: 'TechCorp',
        activity: 'IT',
        isVerified: true,
        jobCount: 5,
        link: 'https://techcorp.com'
    };

    it('renders company name', () => {
        const { container } = render(CompanyCard, { props });
        expect(container.textContent).toContain('TechCorp');
    });

    it('renders activity field', () => {
        const { container } = render(CompanyCard, { props });
        expect(container.textContent).toContain('IT');
    });

    it('shows verified badge when verified', () => {
        const { container } = render(CompanyCard, { props });
        const badge = container.querySelector('.badge--success');
        expect(badge).toBeTruthy();
    });

    it('has link to company details', () => {
        const { container } = render(CompanyCard, { props });
        const link = container.querySelector('a[href*="comp-1"]');
        expect(link).toBeTruthy();
    });

    it('renders job count', () => {
        const { container } = render(CompanyCard, { props });
        expect(container.textContent).toContain('5');
    });

    it('renders avatar', () => {
        const { container } = render(CompanyCard, { props });
        const avatar = container.querySelector('.avatar');
        expect(avatar).toBeTruthy();
    });

    it('renders without link when not provided', () => {
        const { container } = render(CompanyCard, { props: { ...props, link: null } });
        expect(container.textContent).toContain('TechCorp');
    });
});
