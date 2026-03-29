import { describe, it, expect, beforeEach, vi } from 'vitest';
import { get } from 'svelte/store';
import { notifications, unreadCount } from '$lib/stores/notifications';

vi.mock('$lib/api/notifications', () => ({
    notificationsApi: {
        getAll: vi.fn().mockResolvedValue({ items: [], total: 0 }),
        markAsRead: vi.fn().mockResolvedValue(undefined),
        markAllAsRead: vi.fn().mockResolvedValue(undefined),
        getUnreadCount: vi.fn().mockResolvedValue({ count: 0 })
    }
}));

describe('notifications store', () => {
    beforeEach(() => {
        notifications.clear();
    });

    it('starts empty', () => {
        expect(get(notifications)).toHaveLength(0);
        expect(get(unreadCount)).toBe(0);
    });

    it('adds a notification', () => {
        notifications.add({
            type: 'system',
            title: 'Test',
            message: 'Hello'
        });
        const items = get(notifications);
        expect(items).toHaveLength(1);
        expect(items[0].title).toBe('Test');
        expect(items[0].message).toBe('Hello');
        expect(items[0].read).toBe(false);
    });

    it('adds notification at the beginning', () => {
        notifications.add({ type: 'system', title: 'First', message: '' });
        notifications.add({ type: 'system', title: 'Second', message: '' });
        expect(get(notifications)[0].title).toBe('Second');
    });

    it('assigns unique id and timestamp', () => {
        notifications.add({ type: 'system', title: 'Test', message: '' });
        const item = get(notifications)[0];
        expect(item.id).toBeTruthy();
        expect(item.timestamp).toBeInstanceOf(Date);
    });

    it('marks single notification as read', () => {
        notifications.add({ type: 'system', title: 'Test', message: '' });
        const id = get(notifications)[0].id;
        notifications.markAsRead(id);
        expect(get(notifications)[0].read).toBe(true);
    });

    it('marks all as read', () => {
        notifications.add({ type: 'system', title: '1', message: '' });
        notifications.add({ type: 'system', title: '2', message: '' });
        notifications.markAllAsRead();
        const all = get(notifications);
        expect(all.every(n => n.read)).toBe(true);
    });

    it('unreadCount updates correctly', () => {
        notifications.add({ type: 'system', title: '1', message: '' });
        notifications.add({ type: 'system', title: '2', message: '' });
        expect(get(unreadCount)).toBe(2);
        const id = get(notifications)[0].id;
        notifications.markAsRead(id);
        expect(get(unreadCount)).toBe(1);
    });

    it('clears all notifications', () => {
        notifications.add({ type: 'system', title: '1', message: '' });
        notifications.add({ type: 'system', title: '2', message: '' });
        notifications.clear();
        expect(get(notifications)).toHaveLength(0);
        expect(get(unreadCount)).toBe(0);
    });

    it('persistMarkAsRead calls API', async () => {
        const { notificationsApi } = await import('$lib/api/notifications');
        notifications.add({ type: 'system', title: 'Test', message: '' });
        const id = get(notifications)[0].id;
        await notifications.persistMarkAsRead(id);
        expect(notificationsApi.markAsRead).toHaveBeenCalledWith(id);
        expect(get(notifications)[0].read).toBe(true);
    });

    it('persistMarkAllAsRead calls API', async () => {
        const { notificationsApi } = await import('$lib/api/notifications');
        notifications.add({ type: 'system', title: '1', message: '' });
        await notifications.persistMarkAllAsRead();
        expect(notificationsApi.markAllAsRead).toHaveBeenCalled();
    });

    it('fetchUnreadCount calls API and returns count', async () => {
        const { notificationsApi } = await import('$lib/api/notifications');
        (notificationsApi.getUnreadCount as ReturnType<typeof vi.fn>).mockResolvedValueOnce({ count: 5 });
        const count = await notifications.fetchUnreadCount();
        expect(notificationsApi.getUnreadCount).toHaveBeenCalled();
        expect(count).toBe(5);
    });

    it('fetchUnreadCount returns 0 on error', async () => {
        const { notificationsApi } = await import('$lib/api/notifications');
        (notificationsApi.getUnreadCount as ReturnType<typeof vi.fn>).mockRejectedValueOnce(new Error('fail'));
        const count = await notifications.fetchUnreadCount();
        expect(count).toBe(0);
    });
});
