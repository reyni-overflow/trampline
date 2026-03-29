import { writable, derived } from 'svelte/store';
import { browser } from '$app/environment';
import { notificationsApi, type NotificationItem } from '$lib/api/notifications';

export type NotificationType =
    | 'application_status'
    | 'new_application'
    | 'new_job'
    | 'contact_request'
    | 'job_recommendation'
    | 'verification_status'
    | 'job_moderation'
    | 'event_moderation'
    | 'event_reminder'
    | 'system';

export interface Notification {
    id: string;
    type: NotificationType;
    title: string;
    message: string;
    timestamp: Date;
    read: boolean;
    link?: string;
}

function mapServerNotification(item: NotificationItem): Notification {
    return {
        id: item.id,
        type: item.type as NotificationType,
        title: item.title,
        message: item.message,
        timestamp: new Date(item.createdAt),
        read: item.isRead,
        link: item.link
    };
}

function createNotificationStore() {
    const { subscribe, update, set } = writable<Notification[]>([]);

    let totalFromServer = 0;

    function add(notification: Omit<Notification, 'id' | 'timestamp' | 'read'>) {
        update((items) => [
            {
                ...notification,
                id: crypto.randomUUID(),
                timestamp: new Date(),
                read: false
            },
            ...items
        ]);
    }

    function markAsRead(id: string) {
        update((items) => items.map((n) => (n.id === id ? { ...n, read: true } : n)));
    }

    function markAllAsRead() {
        update((items) => items.map((n) => ({ ...n, read: true })));
    }

    function clear() {
        set([]);
    }

    async function loadFromServer(page = 1, size = 20) {
        if (!browser) return;
        try {
            const response = await notificationsApi.getAll(page, size);
            totalFromServer = response.totalCount;
            const mapped = response.items.map(mapServerNotification);
            if (page === 1) {
                set(mapped);
            } else {
                update((items) => {
                    const existingIds = new Set(items.map((n) => n.id));
                    const newItems = mapped.filter((n) => !existingIds.has(n.id));
                    return [...items, ...newItems];
                });
            }
        } catch {
            // silent fail — store keeps current state
        }
    }

    async function fetchUnreadCount(): Promise<number> {
        if (!browser) return 0;
        try {
            const { count } = await notificationsApi.getUnreadCount();
            return count;
        } catch {
            return 0;
        }
    }

    async function persistMarkAsRead(id: string) {
        markAsRead(id);
        if (!browser) return;
        try {
            await notificationsApi.markAsRead(id);
        } catch {
            // local state already updated
        }
    }

    async function persistMarkAllAsRead() {
        markAllAsRead();
        if (!browser) return;
        try {
            await notificationsApi.markAllAsRead();
        } catch {
            // local state already updated
        }
    }

    return {
        subscribe,
        add,
        markAsRead,
        markAllAsRead,
        clear,
        loadFromServer,
        fetchUnreadCount,
        persistMarkAsRead,
        persistMarkAllAsRead,
        get total() {
            return totalFromServer;
        }
    };
}

export const notifications = createNotificationStore();

export const unreadCount = derived(
    notifications,
    ($notifications) => $notifications.filter((n) => !n.read).length
);
