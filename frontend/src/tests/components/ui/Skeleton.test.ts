import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import Skeleton from '$lib/components/ui/Skeleton.svelte';
import JobCardSkeleton from '$lib/components/ui/JobCardSkeleton.svelte';
import CompanyCardSkeleton from '$lib/components/ui/CompanyCardSkeleton.svelte';

describe('Skeleton', () => {
    it('renders skeleton element', () => {
        const { container } = render(Skeleton);
        const skeleton = container.querySelector('.skeleton');
        expect(skeleton).toBeInTheDocument();
    });

    it('applies width and height', () => {
        const { container } = render(Skeleton, { props: { width: '200px', height: '20px' } });
        const skeleton = container.querySelector('.skeleton');
        expect(skeleton).toBeInTheDocument();
    });

    it('applies circle variant', () => {
        const { container } = render(Skeleton, { props: { circle: true } });
        const skeleton = container.querySelector('.skeleton');
        expect(skeleton?.className).toContain('circle');
    });
});

describe('JobCardSkeleton', () => {
    it('renders skeleton placeholders', () => {
        const { container } = render(JobCardSkeleton);
        const skeletons = container.querySelectorAll('.skeleton');
        expect(skeletons.length).toBeGreaterThan(0);
    });

    it('renders in list mode', () => {
        const { container } = render(JobCardSkeleton, { props: { mode: 'list' } });
        expect(container.querySelector('.skeleton')).toBeInTheDocument();
    });

    it('renders in grid mode', () => {
        const { container } = render(JobCardSkeleton, { props: { mode: 'grid' } });
        expect(container.querySelector('.skeleton')).toBeInTheDocument();
    });
});

describe('CompanyCardSkeleton', () => {
    it('renders skeleton placeholders', () => {
        const { container } = render(CompanyCardSkeleton);
        const skeletons = container.querySelectorAll('.skeleton');
        expect(skeletons.length).toBeGreaterThan(0);
    });
});
