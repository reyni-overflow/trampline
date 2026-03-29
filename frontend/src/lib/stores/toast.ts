import { writable } from 'svelte/store';

export type ToastType = 'info' | 'success' | 'warning' | 'error' | 'danger' | 'tip';

export interface ToastItem {
    id: string;
    type: ToastType;
    message: string;
    persistent: boolean;
    duration: number;
}

const MAX_TOASTS = 3;

const DURATIONS: Record<ToastType, number> = {
    info: 5000,
    success: 4000,
    warning: 7000,
    error: 10000,
    danger: 0,
    tip: 6000
};

function createToastStore() {
    const { subscribe, update } = writable<ToastItem[]>([]);

    function add(type: ToastType, message: string, options?: { persistent?: boolean; duration?: number }) {
        const persistent = options?.persistent ?? type === 'danger';
        const duration = options?.duration ?? DURATIONS[type];

        const item: ToastItem = {
            id: crypto.randomUUID(),
            type,
            message,
            persistent,
            duration
        };

        update((toasts) => {
            const next = [...toasts, item];
            while (next.length > MAX_TOASTS) next.shift();
            return next;
        });

        if (!persistent && duration > 0) {
            setTimeout(() => remove(item.id), duration);
        }

        return item.id;
    }

    function remove(id: string) {
        update((toasts) => toasts.filter((t) => t.id !== id));
    }

    function clear() {
        update(() => []);
    }

    return {
        subscribe,
        remove,
        clear,
        info: (message: string, options?: { persistent?: boolean; duration?: number }) =>
            add('info', message, options),
        success: (message: string, options?: { persistent?: boolean; duration?: number }) =>
            add('success', message, options),
        warning: (message: string, options?: { persistent?: boolean; duration?: number }) =>
            add('warning', message, options),
        error: (message: string, options?: { persistent?: boolean; duration?: number }) =>
            add('error', message, options),
        danger: (message: string, options?: { persistent?: boolean; duration?: number }) =>
            add('danger', message, options),
        tip: (message: string, options?: { persistent?: boolean; duration?: number }) =>
            add('tip', message, options)
    };
}

export const toast = createToastStore();
