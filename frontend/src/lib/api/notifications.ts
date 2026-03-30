import { api } from './client';

export interface NotificationItem {
    id: string;
    type: string;
    title: string;
    message: string;
    link?: string;
    isRead: boolean;
    createdAt: string;
}

export interface NotificationsResponse {
    items: NotificationItem[];
    totalCount: number;
}

export const notificationsApi = {
    async getAll(pageNumber = 1, pageSize = 20): Promise<NotificationsResponse> {
        return api.get<NotificationsResponse>(
            `/notification?pageNumber=${pageNumber}&pageSize=${pageSize}`
        );
    },

    async getUnreadCount(): Promise<{ count: number }> {
        return api.get<{ count: number }>('/notification/unread-count');
    },

    async markAsRead(id: string): Promise<void> {
        await api.put('/notification/' + id + '/read');
    },

    async markAllAsRead(): Promise<void> {
        await api.put('/notification/read-all');
    },

    async getVapidKey(): Promise<{ publicKey: string }> {
        return api.get<{ publicKey: string }>('/notification/vapid-key');
    },

    async pushSubscribe(subscription: PushSubscription): Promise<void> {
        const json = subscription.toJSON();
        await api.post('/notification/push-subscribe', {
            endpoint: json.endpoint,
            p256dh: json.keys?.p256dh ?? '',
            auth: json.keys?.auth ?? ''
        });
    },

    async pushUnsubscribe(endpoint: string): Promise<void> {
        await api.post('/notification/push-unsubscribe', { endpoint });
    }
};
