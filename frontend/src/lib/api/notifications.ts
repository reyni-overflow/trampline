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
    total: number;
}

export const notificationsApi = {
    async getAll(page = 1, size = 20): Promise<NotificationsResponse> {
        return api.get<NotificationsResponse>(`/notification?page=${page}&size=${size}`);
    },

    async getUnreadCount(): Promise<{ count: number }> {
        return api.get<{ count: number }>('/notification/unread-count');
    },

    async markAsRead(id: string): Promise<void> {
        await api.put('/notification/' + id + '/read');
    },

    async markAllAsRead(): Promise<void> {
        await api.put('/notification/read-all');
    }
};
