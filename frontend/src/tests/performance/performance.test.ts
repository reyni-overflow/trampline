import { describe, it, expect, beforeEach, vi } from 'vitest';
import { api } from '$lib/api/client';
import { get } from 'svelte/store';
import { favorites } from '$lib/stores/favorites';

vi.mock('$lib/api/client', () => ({
    api: {
        get: vi.fn().mockResolvedValue({}),
        post: vi.fn().mockResolvedValue({}),
        put: vi.fn().mockResolvedValue({}),
        delete: vi.fn().mockResolvedValue(undefined),
        upload: vi.fn().mockResolvedValue([])
    },
    handleApiError: vi.fn()
}));

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

describe('Performance', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        favorites.clear();
        localStorage.clear();
    });

    describe('API client call efficiency', () => {
        it('single getAll call makes exactly one fetch', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getAll(1, 10);
            expect(api.get).toHaveBeenCalledTimes(1);
        });

        it('getById makes exactly one fetch', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getById('test-id');
            expect(api.get).toHaveBeenCalledTimes(1);
        });

        it('mentorship getAll makes exactly one fetch', async () => {
            const { mentorshipsApi } = await import('$lib/api/mentorships');
            await mentorshipsApi.getAll(1, 10);
            expect(api.get).toHaveBeenCalledTimes(1);
        });

        it('multiple sequential getAll calls make separate fetches', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getAll(1, 10);
            await jobsApi.getAll(2, 10);
            await jobsApi.getAll(3, 10);
            expect(api.get).toHaveBeenCalledTimes(3);
        });

        it('create sends exactly one POST', async () => {
            const { mentorshipsApi } = await import('$lib/api/mentorships');
            await mentorshipsApi.create({ title: 'T', description: 'D', address: 'A' });
            expect(api.post).toHaveBeenCalledTimes(1);
        });

        it('delete sends exactly one DELETE', async () => {
            const { mentorshipsApi } = await import('$lib/api/mentorships');
            await mentorshipsApi.delete('id-1');
            expect(api.delete).toHaveBeenCalledTimes(1);
        });
    });

    describe('Favorites store efficiency', () => {
        it('toggle updates store synchronously without waiting for API', () => {
            favorites.toggleJob('job-1');
            expect(favorites.isJobFavorite('job-1')).toBe(true);
        });

        it('rapid toggles are all reflected in state', () => {
            for (let i = 0; i < 100; i++) {
                favorites.toggleJob(`job-${i}`);
            }
            const state = get(favorites);
            expect(state.jobs).toHaveLength(100);
        });

        it('toggling same item is idempotent pair', () => {
            favorites.toggleJob('job-1');
            favorites.toggleJob('job-1');
            expect(favorites.isJobFavorite('job-1')).toBe(false);
        });

        it('clear resets all collections at once', () => {
            favorites.toggleJob('j1');
            favorites.toggleCompany('c1');
            favorites.toggleEvent('e1');
            favorites.toggleMentorship('m1');
            favorites.clear();
            const state = get(favorites);
            expect(state.jobs).toHaveLength(0);
            expect(state.companies).toHaveLength(0);
            expect(state.events).toHaveLength(0);
            expect(state.mentorships).toHaveLength(0);
        });

        it('localStorage writes happen on each toggle', () => {
            const before = (localStorage.setItem as ReturnType<typeof vi.fn>).mock.calls.length;
            favorites.toggleJob('j1');
            favorites.toggleJob('j2');
            favorites.toggleJob('j3');
            const after = (localStorage.setItem as ReturnType<typeof vi.fn>).mock.calls.length;
            expect(after - before).toBe(3);
        });
    });

    describe('Query parameter construction efficiency', () => {
        it('omits undefined filter params to reduce URL size', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getAll(1, 10, {});
            const callUrl = (api.get as ReturnType<typeof vi.fn>).mock.calls[0][0];
            expect(callUrl).not.toContain('city=');
            expect(callUrl).not.toContain('search=');
        });

        it('omits undefined mentorship filter params', async () => {
            const { mentorshipsApi } = await import('$lib/api/mentorships');
            await mentorshipsApi.getAll(1, 10, {});
            const callUrl = (api.get as ReturnType<typeof vi.fn>).mock.calls[0][0];
            expect(callUrl).not.toContain('city=');
            expect(callUrl).not.toContain('format=');
            expect(callUrl).not.toContain('tags=');
        });

        it('includes only provided filter params', async () => {
            const { mentorshipsApi } = await import('$lib/api/mentorships');
            await mentorshipsApi.getAll(1, 10, { city: 'Moscow' });
            const callUrl = (api.get as ReturnType<typeof vi.fn>).mock.calls[0][0];
            expect(callUrl).toContain('city=Moscow');
            expect(callUrl).not.toContain('search=');
            expect(callUrl).not.toContain('format=');
        });
    });
});
