import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import JobCard from '$lib/components/ui/JobCard.svelte';

const mockJob = {
    id: 'job-1',
    title: 'Frontend Developer',
    description: 'Build amazing UIs',
    type: 'Work' as const,
    format: 'Remote' as const,
    city: 'Москва',
    address: 'Москва',
    country: 'Россия',
    street: '',
    geoLat: 55.75,
    geoLon: 37.62,
    salaryFrom: 80000,
    salaryTo: 150000,
    tags: [{ id: '1', name: 'TypeScript', category: 'language', lvl: 0 }, { id: '2', name: 'Svelte', category: 'framework', lvl: 0 }],
    photos: [],
    videos: [],
    createdAt: '2026-03-01T00:00:00Z',
    updatedAt: '2026-03-01T00:00:00Z',
    endedAt: '2026-06-01T00:00:00Z',
    deletedAt: null,
    isActive: true,
    views: 42,
    employeeId: 'emp-1',
    userId: 'user-1',
    companyName: 'TechCorp'
};

describe('JobCard', () => {
    it('renders job title', () => {
        const { container } = render(JobCard, { props: { job: mockJob } });
        expect(container.textContent).toContain('Frontend Developer');
    });

    it('renders tags', () => {
        const { container } = render(JobCard, { props: { job: mockJob } });
        expect(container.textContent).toContain('TypeScript');
    });

    it('renders city', () => {
        const { container } = render(JobCard, { props: { job: mockJob } });
        expect(container.textContent).toContain('Москва');
    });

    it('renders in grid mode by default', () => {
        const { container } = render(JobCard, { props: { job: mockJob } });
        const card = container.querySelector('.job-card');
        expect(card).toBeInTheDocument();
    });

    it('renders in list mode', () => {
        const { container } = render(JobCard, { props: { job: mockJob, mode: 'list' } });
        const card = container.querySelector('.job-card');
        expect(card?.className).toContain('list');
    });

    it('has link to job details', () => {
        const { container } = render(JobCard, { props: { job: mockJob } });
        const link = container.querySelector('a[href*="job-1"]');
        expect(link).toBeTruthy();
    });

    it('renders without salary when not provided', () => {
        const jobNoSalary = { ...mockJob, salaryFrom: null, salaryTo: null };
        const { container } = render(JobCard, { props: { job: jobNoSalary } });
        expect(container).toBeInTheDocument();
    });
});
