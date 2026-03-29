import '@testing-library/jest-dom/vitest';
import { vi } from 'vitest';

// Mock crypto.randomUUID
if (!globalThis.crypto?.randomUUID) {
    let counter = 0;
    Object.defineProperty(globalThis, 'crypto', {
        value: {
            ...globalThis.crypto,
            randomUUID: () => `test-uuid-${++counter}`
        }
    });
}

// Mock localStorage
const localStorageMock = (() => {
    let store: Record<string, string> = {};
    return {
        getItem: vi.fn((key: string) => store[key] ?? null),
        setItem: vi.fn((key: string, value: string) => {
            store[key] = value;
        }),
        removeItem: vi.fn((key: string) => {
            delete store[key];
        }),
        clear: vi.fn(() => {
            store = {};
        }),
        get length() {
            return Object.keys(store).length;
        },
        key: vi.fn((index: number) => Object.keys(store)[index] ?? null)
    };
})();

Object.defineProperty(globalThis, 'localStorage', { value: localStorageMock });

// Mock matchMedia
Object.defineProperty(globalThis, 'matchMedia', {
    value: vi.fn().mockImplementation((query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn()
    }))
});

// Mock IntersectionObserver
globalThis.IntersectionObserver = vi.fn().mockImplementation(() => ({
    observe: vi.fn(),
    unobserve: vi.fn(),
    disconnect: vi.fn(),
    root: null,
    rootMargin: '',
    thresholds: []
})) as unknown as typeof IntersectionObserver;

// Mock ResizeObserver
globalThis.ResizeObserver = vi.fn().mockImplementation(() => ({
    observe: vi.fn(),
    unobserve: vi.fn(),
    disconnect: vi.fn()
})) as unknown as typeof ResizeObserver;

// Mock scrollTo
globalThis.scrollTo = vi.fn() as unknown as typeof globalThis.scrollTo;

// Mock HTMLElement.scrollIntoView
Element.prototype.scrollIntoView = vi.fn();

// Mock navigator.share
Object.defineProperty(navigator, 'share', {
    value: vi.fn(),
    writable: true,
    configurable: true
});

// Mock navigator.clipboard
Object.defineProperty(navigator, 'clipboard', {
    value: {
        writeText: vi.fn().mockResolvedValue(undefined),
        readText: vi.fn().mockResolvedValue('')
    },
    writable: true,
    configurable: true
});
