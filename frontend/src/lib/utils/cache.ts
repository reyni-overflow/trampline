interface CacheEntry<T> {
    data: T;
    expiresAt: number;
}

const store = new Map<string, CacheEntry<unknown>>();

export function cached<T>(
    key: string,
    fetcher: () => Promise<T>,
    ttlMs = 60_000
): () => Promise<T> {
    return async () => {
        const existing = store.get(key) as CacheEntry<T> | undefined;
        if (existing && existing.expiresAt > Date.now()) {
            return existing.data;
        }

        const data = await fetcher();
        store.set(key, { data, expiresAt: Date.now() + ttlMs });
        return data;
    };
}

export function invalidate(key: string) {
    store.delete(key);
}

export function invalidateAll() {
    store.clear();
}
