<script lang="ts">
    import Tabs from '$lib/components/ui/Tabs.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import { toast } from '$lib/stores/toast';
    import { timeAgo } from '$lib/utils/format';
    import { eventsApi, type EventResponse } from '$lib/api/events';
    import { handleApiError } from '$lib/api/client';
    import { user as userStore } from '$lib/stores/auth';
    import { t } from '$lib/i18n';
    import { onMount, onDestroy } from 'svelte';

    let activeTab = $state('active');
    let search = $state('');
    let loading = $state(true);

    let tabs = $derived([
        { id: 'active', label: $t('dashEvents.active') },
        { id: 'closed', label: $t('dashEvents.closed') },
        { id: 'draft', label: $t('dashEvents.drafts') }
    ]);

    type EventItem = {
        id: string;
        title: string;
        format: string;
        city: string;
        views: number;
        createdAt: string;
        endedAt: string;
        status: string;
        isActive: boolean;
    };
    let events = $state<EventItem[]>([]);

    let unsubUser: (() => void) | undefined;
    onMount(() => {
        unsubUser = userStore.subscribe((v) => {
            if (v?.id) loadEvents(v.id);
        });
    });
    onDestroy(() => unsubUser?.());

    async function loadEvents(userId: string) {
        loading = true;
        try {
            const data = await eventsApi.getByUser(userId, 1, 50);
            events = data.map((e: EventResponse) => ({
                id: e.id,
                title: e.title,
                format: e.format,
                city: e.city,
                views: e.views,
                createdAt: e.createdAt,
                endedAt: e.endedAt,
                isActive: e.isActive,
                status: e.isActive ? 'active' : e.deletedAt ? 'closed' : 'draft'
            }));
        } catch {
            events = [];
        } finally {
            loading = false;
        }
    }

    let filtered = $derived.by(() => {
        let list = events.filter((e) => e.status === activeTab);
        if (search) {
            const q = search.toLowerCase();
            list = list.filter((e) => e.title.toLowerCase().includes(q));
        }
        return list;
    });

    async function toggleStatus(id: string) {
        const event = events.find((e) => e.id === id);
        if (!event) return;
        try {
            await eventsApi.update(id, { isActive: !event.isActive });
            event.isActive = !event.isActive;
            event.status = event.isActive ? 'active' : 'closed';
            events = [...events];
            toast.success($t('dashEvents.statusChanged'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteEvent(id: string) {
        try {
            await eventsApi.delete(id);
            events = events.filter((e) => e.id !== id);
            toast.success($t('dashEvents.eventDeleted'));
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('dashEvents.pageTitle')}</title>
</svelte:head>

<div class="my-events">
    <div class="page-header">
        <h1 class="page-heading">{$t('dashEvents.title')}</h1>
        <Button href="/dashboard/events/create">{$t('dashEvents.createEvent')}</Button>
    </div>

    <Tabs {tabs} bind:active={activeTab} />

    <div class="search-row">
        <SearchInput placeholder={$t('dashEvents.searchPlaceholder')} bind:value={search} />
    </div>

    {#if loading}
        <div class="events-list">
            {#each Array(4) as _, i (i)}
                <div class="event-row">
                    <div class="event-info">
                        <div class="event-title-row">
                            <Skeleton width="50%" height="0.875rem" />
                            <Skeleton width="3.5rem" height="1.25rem" radius="var(--radius-full)" />
                        </div>
                        <div class="event-meta">
                            <Skeleton width="3rem" height="0.75rem" />
                            <Skeleton width="4rem" height="0.75rem" />
                            <Skeleton width="5rem" height="0.75rem" />
                        </div>
                    </div>
                    <div class="event-stats">
                        <Skeleton width="4rem" height="1.25rem" radius="var(--radius-full)" />
                    </div>
                    <div class="event-actions">
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                    </div>
                </div>
            {/each}
        </div>
    {:else if filtered.length === 0}
        <div class="empty">
            <svg
                width="48"
                height="48"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
                ><rect x="3" y="4" width="18" height="18" rx="2" ry="2" /><line
                    x1="16"
                    y1="2"
                    x2="16"
                    y2="6"
                /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" /></svg
            >
            <p>
                {activeTab === 'active'
                    ? $t('dashEvents.noActive')
                    : activeTab === 'closed'
                      ? $t('dashEvents.noClosed')
                      : $t('dashEvents.noDrafts')}
            </p>
            <p>
                {activeTab === 'active'
                    ? $t('dashEvents.noActiveHint')
                    : activeTab === 'closed'
                      ? $t('dashEvents.noClosedHint')
                      : $t('dashEvents.noDraftsHint')}
            </p>
            {#if activeTab === 'active'}
                <Button href="/dashboard/events/create">{$t('dashEvents.createFirst')}</Button>
            {/if}
        </div>
    {:else}
        <div class="events-list">
            {#each filtered as event (event.id)}
                <div class="event-row">
                    <div class="event-info">
                        <div class="event-title-row">
                            <a href="/events/{event.id}" class="event-title">{event.title}</a>
                            <Badge
                                variant={event.format === 'Remote'
                                    ? 'success'
                                    : event.format === 'Office'
                                      ? 'warning'
                                      : 'default'}
                                size="sm"
                                >{event.format === 'Remote'
                                    ? $t('dashJobs.remote')
                                    : event.format === 'Hybrid'
                                      ? $t('dashJobs.hybrid')
                                      : $t('dashJobs.office')}</Badge
                            >
                        </div>
                        <div class="event-meta">
                            <span>{event.city}</span>
                            <span>&middot;</span>
                            <span>{timeAgo(event.createdAt)}</span>
                            {#if event.endedAt}
                                <span>&middot;</span>
                                <span
                                    >{$t('dashEvents.endsAt')}
                                    {new Date(event.endedAt).toLocaleDateString()}</span
                                >
                            {/if}
                        </div>
                    </div>
                    <div class="event-stats">
                        <a href="/dashboard/events/{event.id}/responses" class="responses-link">
                            <Badge variant="accent"
                                >{event.views} {$t('dashEvents.responses')}</Badge
                            >
                        </a>
                    </div>
                    <div class="event-actions">
                        <Button size="sm" variant="ghost" href="/dashboard/events/{event.id}/edit">
                            <svg
                                viewBox="0 0 24 24"
                                width="14"
                                height="14"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                ><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z" /></svg
                            >
                        </Button>
                        <Button size="sm" variant="ghost" onclick={() => toggleStatus(event.id)}>
                            {#if event.status === 'active'}
                                <svg
                                    viewBox="0 0 24 24"
                                    width="14"
                                    height="14"
                                    fill="none"
                                    stroke="currentColor"
                                    stroke-width="2"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                    ><rect width="18" height="18" x="3" y="3" rx="2" /><line
                                        x1="9"
                                        y1="9"
                                        x2="15"
                                        y2="15"
                                    /><line x1="15" y1="9" x2="9" y2="15" /></svg
                                >
                            {:else}
                                <svg
                                    viewBox="0 0 24 24"
                                    width="14"
                                    height="14"
                                    fill="none"
                                    stroke="currentColor"
                                    stroke-width="2"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                    ><polyline points="20 6 9 17 4 12" /></svg
                                >
                            {/if}
                        </Button>
                        <Button size="sm" variant="ghost" onclick={() => deleteEvent(event.id)}>
                            <svg
                                viewBox="0 0 24 24"
                                width="14"
                                height="14"
                                fill="none"
                                stroke="var(--color-error)"
                                stroke-width="2"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                ><path d="M3 6h18" /><path
                                    d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"
                                /><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2" /></svg
                            >
                        </Button>
                    </div>
                </div>
            {/each}
        </div>
    {/if}
</div>

<style>
    .my-events {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }
    .page-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        flex-wrap: wrap;
        gap: var(--space-4);
    }
    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }
    .search-row {
        max-width: 20rem;
        margin-top: var(--space-2);
    }

    .events-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .event-row {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors);
    }
    .event-row:hover {
        border-color: var(--border-hover);
    }

    .event-info {
        flex: 1;
        min-width: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }
    .event-title-row {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        flex-wrap: wrap;
    }
    .event-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        transition: var(--transition-colors);
    }
    .event-title:hover {
        color: var(--accent);
    }
    .event-meta {
        display: flex;
        gap: var(--space-2);
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        flex-wrap: wrap;
    }

    .event-stats {
        flex-shrink: 0;
    }
    .responses-link {
        text-decoration: none;
    }

    .event-actions {
        display: flex;
        gap: var(--space-1);
        flex-shrink: 0;
    }

    .empty {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-16) var(--space-4);
        text-align: center;
        color: var(--text-tertiary);
    }
    .empty svg {
        opacity: 0.5;
    }
    .empty p {
        max-width: 20rem;
    }

    @media (max-width: 640px) {
        .search-row {
            max-width: 100%;
        }
        .event-row {
            flex-direction: column;
            align-items: stretch;
        }
        .event-actions {
            justify-content: flex-end;
        }
    }
</style>
