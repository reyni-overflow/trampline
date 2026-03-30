import { writable, get } from 'svelte/store';
import { browser } from '$app/environment';
import { favoritesApi } from '$lib/api/favorites';
import { user } from '$lib/stores/auth';

const STORAGE_KEY = 'trampline-favorites';
const TOGGLE_DEBOUNCE_MS = 400;

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
            return {
                jobs: parsed.jobs || [],
                companies: parsed.companies || [],
                events: parsed.events || [],
                mentorships: parsed.mentorships || []
            };
        }
    } catch {
        /* ignored */
    }
    return { jobs: [], companies: [], events: [], mentorships: [] };
}

function saveToStorage(state: FavoritesState) {
    if (!browser) return;
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch {
        /* ignored */
    }
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

const pendingToggles = new Map<string, ReturnType<typeof setTimeout>>();

function debouncedApiToggle(id: string, type: 'Job' | 'Company' | 'Event' | 'Mentorship') {
    if (!isLoggedIn()) return;

    const key = `${type}:${id}`;
    const existing = pendingToggles.get(key);
    if (existing) clearTimeout(existing);

    pendingToggles.set(
        key,
        setTimeout(() => {
            pendingToggles.delete(key);
            favoritesApi.toggle(id, type).catch(() => {
                persist((s) => {
                    const field = (type.toLowerCase() + 's') as keyof FavoritesState;
                    const list = s[field] as string[];
                    return {
                        ...s,
                        [field]: list.includes(id) ? list.filter((x) => x !== id) : [...list, id]
                    };
                });
            });
        }, TOGGLE_DEBOUNCE_MS)
    );
}

export const favorites = {
    subscribe: store.subscribe,

    toggleJob(id: string) {
        persist((s) => ({
            ...s,
            jobs: s.jobs.includes(id) ? s.jobs.filter((j) => j !== id) : [...s.jobs, id]
        }));
        debouncedApiToggle(id, 'Job');
    },

    toggleCompany(id: string) {
        persist((s) => ({
            ...s,
            companies: s.companies.includes(id)
                ? s.companies.filter((c) => c !== id)
                : [...s.companies, id]
        }));
        debouncedApiToggle(id, 'Company');
    },

    toggleEvent(id: string) {
        persist((s) => ({
            ...s,
            events: s.events.includes(id) ? s.events.filter((e) => e !== id) : [...s.events, id]
        }));
        debouncedApiToggle(id, 'Event');
    },

    toggleMentorship(id: string) {
        persist((s) => ({
            ...s,
            mentorships: s.mentorships.includes(id)
                ? s.mentorships.filter((m) => m !== id)
                : [...s.mentorships, id]
        }));
        debouncedApiToggle(id, 'Mentorship');
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
        for (const t of pendingToggles.values()) clearTimeout(t);
        pendingToggles.clear();
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

        const serverJobs = new Set(
            serverFavs.filter((f) => f.type === 'Job').map((f) => f.targetId)
        );
        const serverCompanies = new Set(
            serverFavs.filter((f) => f.type === 'Company').map((f) => f.targetId)
        );
        const serverEvents = new Set(
            serverFavs.filter((f) => f.type === 'Event').map((f) => f.targetId)
        );
        const serverMentorships = new Set(
            serverFavs.filter((f) => f.type === 'Mentorship').map((f) => f.targetId)
        );

        const localOnlyJobs = local.jobs.filter((id) => !serverJobs.has(id));
        const localOnlyCompanies = local.companies.filter((id) => !serverCompanies.has(id));
        const localOnlyEvents = local.events.filter((id) => !serverEvents.has(id));
        const localOnlyMentorships = local.mentorships.filter((id) => !serverMentorships.has(id));

        await Promise.allSettled([
            ...localOnlyJobs.map((id) => favoritesApi.toggle(id, 'Job')),
            ...localOnlyCompanies.map((id) => favoritesApi.toggle(id, 'Company')),
            ...localOnlyEvents.map((id) => favoritesApi.toggle(id, 'Event')),
            ...localOnlyMentorships.map((id) => favoritesApi.toggle(id, 'Mentorship'))
        ]);

        const merged: FavoritesState = {
            jobs: Array.from(new Set([...local.jobs, ...serverJobs])),
            companies: Array.from(new Set([...local.companies, ...serverCompanies])),
            events: Array.from(new Set([...local.events, ...serverEvents])),
            mentorships: Array.from(new Set([...local.mentorships, ...serverMentorships]))
        };
        saveToStorage(merged);
        store.set(merged);
    } catch {
        /* ignored */
    }
}
