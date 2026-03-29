import { readable } from 'svelte/store';
import { vi } from 'vitest';

export const page = readable({
    url: new URL('http://localhost:4173'),
    params: {},
    route: { id: '/' },
    status: 200,
    error: null,
    data: {},
    form: null
});

export const navigating = readable(null);

export const updated = {
    subscribe: readable(false).subscribe,
    check: vi.fn()
};
