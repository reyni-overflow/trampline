import { writable, get } from 'svelte/store';
import { browser } from '$app/environment';
import { favoritesApi } from '$lib/api/favorites';
import { user } from '$lib/stores/auth';

const STORAGE_KEY = 'trampline-favorites';

interface FavoritesState {
    jobs: string[];
    companies: string[];
    events: string[];
    mentorships: string[];
}

function loadFromStorage(): FavoritesState {
    if (!browser) return { jobs: [], companies: [], events: [], mentorships: [] };
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (raw) {
            const parsed = JSON.parse(raw);
            return { jobs: parsed.jobs || [], companies: parsed.companies || [], events: parsed.events || [], mentorships: parsed.mentorships || [] };
        }
    } catch { /* ignored */ }
    return { jobs: [], companies: [], events: [], mentorships: [] };
}

function saveToStorage(state: FavoritesState) {
    if (!browser) return;
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(state)); } catch { /* ignored */ }
}

function isLoggedIn(): boolean {
    return get(user) !== null;
}

const store = writable<FavoritesState>(loadFromStorage());

function persist(fn: (state: FavoritesState) => FavoritesState) {
    store.update((s) => {
        const next = fn(s);
        saveToStorage(next);
        return next;
    });
}

export const favorites = {
    subscribe: store.subscribe,

    toggleJob(id: string) {
        persist((s) => ({
            ...s,
            jobs: s.jobs.includes(id) ? s.jobs.filter((j) => j !== id) : [...s.jobs, id]
        }));
        if (isLoggedIn()) {
            favoritesApi.toggle(id, 'Job').catch(() => {
                persist((s) => ({
                    ...s,
                    jobs: s.jobs.includes(id) ? s.jobs.filter((j) => j !== id) : [...s.jobs, id]
                }));
            });
        }
    },

    toggleCompany(id: string) {
        persist((s) => ({
            ...s,
            companies: s.companies.includes(id) ? s.companies.filter((c) => c !== id) : [...s.companies, id]
        }));
        if (isLoggedIn()) {
            favoritesApi.toggle(id, 'Company').catch(() => {
                persist((s) => ({
                    ...s,
                    companies: s.companies.includes(id) ? s.companies.filter((c) => c !== id) : [...s.companies, id]
                }));
            });
        }
    },

    toggleEvent(id: string) {
        persist((s) => ({
            ...s,
            events: s.events.includes(id) ? s.events.filter((e) => e !== id) : [...s.events, id]
        }));
        if (isLoggedIn()) {
            favoritesApi.toggle(id, 'Event').catch(() => {
                persist((s) => ({
                    ...s,
                    events: s.events.includes(id) ? s.events.filter((e) => e !== id) : [...s.events, id]
                }));
            });
        }
    },

    toggleMentorship(id: string) {
        persist((s) => ({
            ...s,
            mentorships: s.mentorships.includes(id) ? s.mentorships.filter((m) => m !== id) : [...s.mentorships, id]
        }));
        if (isLoggedIn()) {
            favoritesApi.toggle(id, 'Mentorship').catch(() => {
                persist((s) => ({
                    ...s,
                    mentorships: s.mentorships.includes(id) ? s.mentorships.filter((m) => m !== id) : [...s.mentorships, id]
                }));
            });
        }
    },

    isJobFavorite(id: string): boolean {
        return get(store).jobs.includes(id);
    },

    isCompanyFavorite(id: string): boolean {
        return get(store).companies.includes(id);
    },

    isEventFavorite(id: string): boolean {
        return get(store).events.includes(id);
    },

    isMentorshipFavorite(id: string): boolean {
        return get(store).mentorships.includes(id);
    },

    clear() {
        const empty: FavoritesState = { jobs: [], companies: [], events: [], mentorships: [] };
        store.set(empty);
        saveToStorage(empty);
    }
};

export async function syncWithServer() {
    if (!isLoggedIn()) return;

    try {
        const serverFavs = await favoritesApi.getAll();
        const local = loadFromStorage();

        const serverJobs = new Set(serverFavs.filter(f => f.type === 'Job').map(f => f.targetId));
        const serverCompanies = new Set(serverFavs.filter(f => f.type === 'Company').map(f => f.targetId));
        const serverEvents = new Set(serverFavs.filter(f => f.type === 'Event').map(f => f.targetId));
        const serverMentorships = new Set(serverFavs.filter(f => f.type === 'Mentorship').map(f => f.targetId));

        const localOnlyJobs = local.jobs.filter(id => !serverJobs.has(id));
        const localOnlyCompanies = local.companies.filter(id => !serverCompanies.has(id));
        const localOnlyEvents = local.events.filter(id => !serverEvents.has(id));
        const localOnlyMentorships = local.mentorships.filter(id => !serverMentorships.has(id));

        await Promise.all([
            ...localOnlyJobs.map(id => favoritesApi.toggle(id, 'Job').catch(() => { /* best-effort sync */ })),
            ...localOnlyCompanies.map(id => favoritesApi.toggle(id, 'Company').catch(() => { /* best-effort sync */ })),
            ...localOnlyEvents.map(id => favoritesApi.toggle(id, 'Event').catch(() => { /* best-effort sync */ })),
            ...localOnlyMentorships.map(id => favoritesApi.toggle(id, 'Mentorship').catch(() => { /* best-effort sync */ }))
        ]);

        const merged: FavoritesState = {
            jobs: Array.from(new Set([...local.jobs, ...serverJobs])),
            companies: Array.from(new Set([...local.companies, ...serverCompanies])),
            events: Array.from(new Set([...local.events, ...serverEvents])),
            mentorships: Array.from(new Set([...local.mentorships, ...serverMentorships]))
        };
        saveToStorage(merged);
        store.set(merged);
    } catch { /* ignored */ }
}
