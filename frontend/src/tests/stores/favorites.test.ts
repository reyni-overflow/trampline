import { describe, it, expect, beforeEach, vi } from 'vitest';
import { get } from 'svelte/store';
import { favorites } from '$lib/stores/favorites';

vi.mock('$lib/api/favorites', () => ({
    favoritesApi: {
        toggle: vi.fn().mockResolvedValue({ added: true }),
        getAll: vi.fn().mockResolvedValue([])
    }
}));

vi.mock('$lib/stores/auth', async () => {
    const { writable } = await import('svelte/store');
    return {
        user: writable(null)
    };
});

describe('favorites store', () => {
    beforeEach(() => {
        favorites.clear();
        localStorage.clear();
    });

    describe('jobs', () => {
        it('toggles job favorite on', () => {
            favorites.toggleJob('job-1');
            expect(favorites.isJobFavorite('job-1')).toBe(true);
        });

        it('toggles job favorite off', () => {
            favorites.toggleJob('job-1');
            favorites.toggleJob('job-1');
            expect(favorites.isJobFavorite('job-1')).toBe(false);
        });

        it('handles multiple job favorites', () => {
            favorites.toggleJob('job-1');
            favorites.toggleJob('job-2');
            expect(favorites.isJobFavorite('job-1')).toBe(true);
            expect(favorites.isJobFavorite('job-2')).toBe(true);
        });
    });

    describe('companies', () => {
        it('toggles company favorite on', () => {
            favorites.toggleCompany('comp-1');
            expect(favorites.isCompanyFavorite('comp-1')).toBe(true);
        });

        it('toggles company favorite off', () => {
            favorites.toggleCompany('comp-1');
            favorites.toggleCompany('comp-1');
            expect(favorites.isCompanyFavorite('comp-1')).toBe(false);
        });
    });

    describe('events', () => {
        it('toggles event favorite on', () => {
            favorites.toggleEvent('evt-1');
            expect(favorites.isEventFavorite('evt-1')).toBe(true);
        });

        it('toggles event favorite off', () => {
            favorites.toggleEvent('evt-1');
            favorites.toggleEvent('evt-1');
            expect(favorites.isEventFavorite('evt-1')).toBe(false);
        });
    });

    describe('persistence', () => {
        it('saves to localStorage on toggle', () => {
            favorites.toggleJob('job-1');
            expect(localStorage.setItem).toHaveBeenCalled();
        });

        it('clears all favorites', () => {
            favorites.toggleJob('job-1');
            favorites.toggleCompany('comp-1');
            favorites.toggleEvent('evt-1');
            favorites.clear();
            expect(favorites.isJobFavorite('job-1')).toBe(false);
            expect(favorites.isCompanyFavorite('comp-1')).toBe(false);
            expect(favorites.isEventFavorite('evt-1')).toBe(false);
        });
    });

    describe('store subscription', () => {
        it('provides full state via subscribe', () => {
            favorites.toggleJob('j1');
            favorites.toggleCompany('c1');
            favorites.toggleEvent('e1');
            const state = get(favorites);
            expect(state.jobs).toContain('j1');
            expect(state.companies).toContain('c1');
            expect(state.events).toContain('e1');
        });
    });
});
