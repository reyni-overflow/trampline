import { writable, derived } from 'svelte/store';
import { browser } from '$app/environment';

export interface ComparisonJob {
    id: string;
    title: string;
    company: string;
    salary?: string;
    format?: string;
    type?: string;
    city?: string;
    tags?: string[];
}

const MAX_ITEMS = 3;
const STORAGE_KEY = 'trampline-comparison';

function loadFromStorage(): ComparisonJob[] {
    if (!browser) return [];
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        return raw ? JSON.parse(raw) : [];
    } catch {
        return [];
    }
}

function saveToStorage(items: ComparisonJob[]) {
    if (!browser) return;
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
    } catch {
        /* ignored */
    }
}

function createComparisonStore() {
    const { subscribe, update, set } = writable<ComparisonJob[]>(loadFromStorage());

    function persist(fn: (items: ComparisonJob[]) => ComparisonJob[]) {
        update((items) => {
            const next = fn(items);
            saveToStorage(next);
            return next;
        });
    }

    return {
        subscribe,
        add(job: ComparisonJob) {
            persist((items) => {
                if (items.length >= MAX_ITEMS) return items;
                if (items.some((j) => j.id === job.id)) return items;
                return [...items, job];
            });
        },
        remove(id: string) {
            persist((items) => items.filter((j) => j.id !== id));
        },
        clear() {
            set([]);
            saveToStorage([]);
        },
        has(id: string): boolean {
            let found = false;
            const unsub = subscribe((items) => {
                found = items.some((j) => j.id === id);
            });
            unsub();
            return found;
        }
    };
}

export const comparison = createComparisonStore();
export const comparisonCount = derived(comparison, ($c) => $c.length);
