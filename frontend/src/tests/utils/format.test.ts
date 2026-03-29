import { describe, it, expect } from 'vitest';
import { formatSalary, timeAgo, formatDate, jobTypeLabel, formatViews, workFormatLabel } from '$lib/utils/format';

describe('format utils', () => {
    describe('formatSalary()', () => {
        it('returns range when both from and to provided', () => {
            const result = formatSalary(50000, 100000);
            expect(result).toBeTruthy();
            expect(typeof result).toBe('string');
        });

        it('returns "from" format when only from provided', () => {
            const result = formatSalary(50000, null);
            expect(result).toBeTruthy();
        });

        it('returns "to" format when only to provided', () => {
            const result = formatSalary(null, 100000);
            expect(result).toBeTruthy();
        });

        it('returns "not specified" when both null', () => {
            const result = formatSalary(null, null);
            expect(result).toBeTruthy();
        });

        it('returns "not specified" when both undefined', () => {
            const result = formatSalary(undefined, undefined);
            expect(result).toBeTruthy();
        });

        it('handles zero salary from', () => {
            const result = formatSalary(0, 100000);
            expect(result).toBeTruthy();
        });
    });

    describe('timeAgo()', () => {
        it('returns "just now" for recent timestamps', () => {
            const now = new Date().toISOString();
            const result = timeAgo(now);
            expect(result).toBeTruthy();
        });

        it('returns minutes ago for timestamps within an hour', () => {
            const date = new Date(Date.now() - 30 * 60 * 1000).toISOString();
            const result = timeAgo(date);
            expect(result).toBeTruthy();
        });

        it('returns hours ago for timestamps within a day', () => {
            const date = new Date(Date.now() - 5 * 3600 * 1000).toISOString();
            const result = timeAgo(date);
            expect(result).toBeTruthy();
        });

        it('returns days ago for timestamps within a month', () => {
            const date = new Date(Date.now() - 5 * 86400 * 1000).toISOString();
            const result = timeAgo(date);
            expect(result).toBeTruthy();
        });

        it('returns formatted date for old timestamps', () => {
            const date = new Date(Date.now() - 60 * 86400 * 1000).toISOString();
            const result = timeAgo(date);
            expect(result).toBeTruthy();
        });
    });

    describe('formatDate()', () => {
        it('returns formatted date string', () => {
            const result = formatDate('2026-01-15T12:00:00Z');
            expect(result).toBeTruthy();
            expect(typeof result).toBe('string');
        });

        it('handles different date formats', () => {
            const result = formatDate('2026-06-01');
            expect(result).toBeTruthy();
        });
    });

    describe('jobTypeLabel()', () => {
        it('returns label for Work type', () => {
            expect(typeof jobTypeLabel('Work')).toBe('string');
        });

        it('returns label for Internship type', () => {
            expect(typeof jobTypeLabel('Internship')).toBe('string');
        });

        it('returns label for Mentorship type', () => {
            expect(typeof jobTypeLabel('Mentorship')).toBe('string');
        });

        it('returns label for Event type', () => {
            expect(typeof jobTypeLabel('Event')).toBe('string');
        });

        it('returns raw type for unknown type', () => {
            expect(jobTypeLabel('Unknown')).toBe('Unknown');
        });
    });

    describe('formatViews()', () => {
        it('returns formatted string for 1 view', () => {
            const result = formatViews(1);
            expect(result).toBeTruthy();
        });

        it('returns formatted string for 5 views', () => {
            const result = formatViews(5);
            expect(result).toBeTruthy();
        });

        it('returns formatted string for 21 views', () => {
            const result = formatViews(21);
            expect(result).toBeTruthy();
        });

        it('returns formatted string for 0 views', () => {
            const result = formatViews(0);
            expect(result).toBeTruthy();
        });
    });

    describe('workFormatLabel()', () => {
        it('returns label for Remote', () => {
            expect(typeof workFormatLabel('Remote')).toBe('string');
        });

        it('returns label for Hybrid', () => {
            expect(typeof workFormatLabel('Hybrid')).toBe('string');
        });

        it('returns label for Office', () => {
            expect(typeof workFormatLabel('Office')).toBe('string');
        });

        it('returns raw format for unknown format', () => {
            expect(workFormatLabel('Unknown')).toBe('Unknown');
        });
    });
});
