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

describe('favorites store - mentorships', () => {
    beforeEach(() => {
        favorites.clear();
        localStorage.clear();
    });

    it('toggles mentorship favorite on', () => {
        favorites.toggleMentorship('m-1');
        expect(favorites.isMentorshipFavorite('m-1')).toBe(true);
    });

    it('toggles mentorship favorite off', () => {
        favorites.toggleMentorship('m-1');
        favorites.toggleMentorship('m-1');
        expect(favorites.isMentorshipFavorite('m-1')).toBe(false);
    });

    it('handles multiple mentorship favorites', () => {
        favorites.toggleMentorship('m-1');
        favorites.toggleMentorship('m-2');
        expect(favorites.isMentorshipFavorite('m-1')).toBe(true);
        expect(favorites.isMentorshipFavorite('m-2')).toBe(true);
    });

    it('does not affect other favorite types', () => {
        favorites.toggleJob('job-1');
        favorites.toggleMentorship('m-1');
        expect(favorites.isJobFavorite('job-1')).toBe(true);
        expect(favorites.isMentorshipFavorite('m-1')).toBe(true);
    });

    it('clears mentorships with clear()', () => {
        favorites.toggleMentorship('m-1');
        favorites.toggleMentorship('m-2');
        favorites.clear();
        expect(favorites.isMentorshipFavorite('m-1')).toBe(false);
        expect(favorites.isMentorshipFavorite('m-2')).toBe(false);
    });

    it('includes mentorships in store state', () => {
        favorites.toggleMentorship('m-1');
        const state = get(favorites);
        expect(state.mentorships).toContain('m-1');
    });

    it('saves mentorships to localStorage', () => {
        favorites.toggleMentorship('m-1');
        expect(localStorage.setItem).toHaveBeenCalled();
    });

    it('mentorship state is independent from events', () => {
        favorites.toggleEvent('e-1');
        favorites.toggleMentorship('m-1');
        favorites.toggleEvent('e-1');
        expect(favorites.isEventFavorite('e-1')).toBe(false);
        expect(favorites.isMentorshipFavorite('m-1')).toBe(true);
    });
});
