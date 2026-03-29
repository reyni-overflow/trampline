import { describe, it, expect, beforeEach } from 'vitest';
import { get } from 'svelte/store';
import { comparison, comparisonCount } from '$lib/stores/comparison';
import type { ComparisonJob } from '$lib/stores/comparison';

const mockJob = (id: string): ComparisonJob => ({
    id,
    title: `Job ${id}`,
    company: `Company ${id}`,
    salary: '100k',
    format: 'Remote',
    type: 'Work',
    city: 'Moscow',
    tags: ['TypeScript']
});

describe('comparison store', () => {
    beforeEach(() => {
        comparison.clear();
        localStorage.clear();
    });

    it('starts empty', () => {
        expect(get(comparison)).toHaveLength(0);
        expect(get(comparisonCount)).toBe(0);
    });

    it('adds a job', () => {
        comparison.add(mockJob('1'));
        expect(get(comparison)).toHaveLength(1);
        expect(get(comparisonCount)).toBe(1);
    });

    it('does not add duplicate job', () => {
        comparison.add(mockJob('1'));
        comparison.add(mockJob('1'));
        expect(get(comparison)).toHaveLength(1);
    });

    it('limits to 3 items', () => {
        comparison.add(mockJob('1'));
        comparison.add(mockJob('2'));
        comparison.add(mockJob('3'));
        comparison.add(mockJob('4'));
        expect(get(comparison)).toHaveLength(3);
        expect(get(comparison).map(j => j.id)).not.toContain('4');
    });

    it('removes a job by id', () => {
        comparison.add(mockJob('1'));
        comparison.add(mockJob('2'));
        comparison.remove('1');
        expect(get(comparison)).toHaveLength(1);
        expect(get(comparison)[0].id).toBe('2');
    });

    it('clears all items', () => {
        comparison.add(mockJob('1'));
        comparison.add(mockJob('2'));
        comparison.clear();
        expect(get(comparison)).toHaveLength(0);
    });

    it('checks if job exists via has()', () => {
        comparison.add(mockJob('1'));
        expect(comparison.has('1')).toBe(true);
        expect(comparison.has('2')).toBe(false);
    });

    it('persists to localStorage', () => {
        comparison.add(mockJob('1'));
        expect(localStorage.setItem).toHaveBeenCalled();
    });

    it('comparisonCount is derived correctly', () => {
        expect(get(comparisonCount)).toBe(0);
        comparison.add(mockJob('1'));
        expect(get(comparisonCount)).toBe(1);
        comparison.add(mockJob('2'));
        expect(get(comparisonCount)).toBe(2);
        comparison.remove('1');
        expect(get(comparisonCount)).toBe(1);
    });
});
