<script lang="ts">
    import { notifications, unreadCount, type Notification } from '$lib/stores/notifications';
    import { t } from '$lib/i18n';
    import { goto } from '$app/navigation';
    import { onDestroy } from 'svelte';

    let open = $state(false);
    let count = $state(0);
    let items = $state<Notification[]>([]);

    const unsubCount = unreadCount.subscribe((v) => (count = v));
    const unsubItems = notifications.subscribe((v) => (items = v));
    onDestroy(() => { unsubCount(); unsubItems(); });

    function toggle() {
        open = !open;
    }

    function handleClickOutside(e: MouseEvent) {
        const target = e.target as HTMLElement;
        if (!target.closest('.notification-bell')) open = false;
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === 'Escape') open = false;
    }

    function handleNotificationClick(id: string, link?: string) {
        notifications.persistMarkAsRead(id);
        if (link) goto(link);
        open = false;
    }

    function handleMarkAllRead() {
        notifications.persistMarkAllAsRead();
    }

    function typeIcon(type: Notification['type']): string {
        switch (type) {
            case 'application_status':
                return '📋';
            case 'new_job':
                return '💼';
            case 'contact_request':
                return '👤';
            case 'event_reminder':
                return '📅';
            case 'system':
            default:
                return '🔔';
        }
    }

    function relativeTime(date: Date): string {
        const now = Date.now();
        const diff = now - date.getTime();
        const minutes = Math.floor(diff / 60000);
        const hours = Math.floor(diff / 3600000);
        const days = Math.floor(diff / 86400000);

        if (minutes < 1) return $t('notifications.justNow');
        if (minutes < 60) return $t('notifications.minutesAgo', { n: minutes });
        if (hours < 24) return $t('notifications.hoursAgo', { n: hours });
        return $t('notifications.daysAgo', { n: days });
    }
</script>

<svelte:window onclick={handleClickOutside} onkeydown={handleKeydown} />

<div class="notification-bell">
    <button
        class="bell-btn"
        onclick={toggle}
        aria-label={$t('notifications.title')}
        title={$t('notifications.title')}
    >
        <svg viewBox="0 0 24 24" width="20" height="20" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round">
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
            <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        {#if count > 0}
            <span class="badge">{count > 9 ? '9+' : count}</span>
        {/if}
    </button>

    {#if open}
        <div class="panel" role="menu">
            <div class="panel-header">
                <span class="panel-title">{$t('notifications.title')}</span>
                {#if count > 0}
                    <button class="mark-all-btn" type="button" onclick={handleMarkAllRead}>
                        {$t('notifications.markAllRead')}
                    </button>
                {/if}
            </div>

            <div class="panel-list">
                {#each items as item (item.id)}
                    <button
                        class="notification-item"
                        class:unread={!item.read}
                        type="button"
                        onclick={() => handleNotificationClick(item.id, item.link)}
                    >
                        <span class="notification-icon">{typeIcon(item.type)}</span>
                        <div class="notification-content">
                            <span class="notification-title">{item.title}</span>
                            <span class="notification-message">{item.message}</span>
                            <span class="notification-time">{relativeTime(item.timestamp)}</span>
                        </div>
                        {#if !item.read}
                            <span class="unread-dot"></span>
                        {/if}
                    </button>
                {:else}
                    <div class="empty-state">{$t('notifications.empty')}</div>
                {/each}
            </div>
        </div>
    {/if}
</div>

<style>
    .notification-bell {
        position: relative;
        display: inline-flex;
    }

    .bell-btn {
        position: relative;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        border-radius: var(--radius-md);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }

    .bell-btn:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .badge {
        position: absolute;
        top: 0.25rem;
        right: 0.25rem;
        min-width: 1.125rem;
        height: 1.125rem;
        padding: 0 0.3rem;
        font-size: 0.625rem;
        font-weight: var(--weight-bold);
        line-height: 1.125rem;
        text-align: center;
        color: var(--text-inverse);
        background: var(--color-error);
        border-radius: var(--radius-full);
        pointer-events: none;
    }

    .panel {
        position: absolute;
        top: calc(100% + 0.5rem);
        right: 0;
        width: 22rem;
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        box-shadow: var(--shadow-lg);
        z-index: var(--z-dropdown);
        animation: scale-in var(--duration-fast) var(--ease-out);
        overflow: hidden;
    }

    .panel-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-3) var(--space-4);
        border-bottom: 1px solid var(--border-subtle);
    }

    .panel-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
    }

    .mark-all-btn {
        font-size: var(--font-xs);
        color: var(--accent);
        font-weight: var(--weight-medium);
        transition: var(--transition-colors);
    }

    .mark-all-btn:hover {
        color: var(--accent-hover);
    }

    .panel-list {
        max-height: 25rem;
        overflow-y: auto;
    }

    .notification-item {
        display: flex;
        align-items: flex-start;
        gap: var(--space-3);
        width: 100%;
        padding: var(--space-3) var(--space-4);
        text-align: left;
        transition: var(--transition-colors);
        cursor: pointer;
    }

    .notification-item:hover {
        background: var(--bg-tertiary);
    }

    .notification-item.unread {
        background: color-mix(in srgb, var(--accent) 5%, transparent);
    }

    .notification-item.unread:hover {
        background: color-mix(in srgb, var(--accent) 10%, transparent);
    }

    .notification-icon {
        flex-shrink: 0;
        font-size: 1.25rem;
        line-height: 1;
        margin-top: 0.125rem;
    }

    .notification-content {
        flex: 1;
        min-width: 0;
        display: flex;
        flex-direction: column;
        gap: 0.125rem;
    }

    .notification-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
    }

    .notification-message {
        font-size: var(--font-xs);
        color: var(--text-secondary);
        display: -webkit-box;
        -webkit-line-clamp: 2;
        line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
    }

    .notification-time {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        margin-top: 0.125rem;
    }

    .unread-dot {
        flex-shrink: 0;
        width: 0.5rem;
        height: 0.5rem;
        border-radius: var(--radius-full);
        background: var(--accent);
        margin-top: 0.375rem;
    }

    .empty-state {
        padding: var(--space-8) var(--space-4);
        text-align: center;
        font-size: var(--font-sm);
        color: var(--text-tertiary);
    }

    @media (max-width: 480px) {
        .panel {
            position: fixed;
            top: var(--header-height);
            right: 0;
            left: 0;
            width: auto;
            border-radius: 0;
            border-left: none;
            border-right: none;
        }
    }
</style>
