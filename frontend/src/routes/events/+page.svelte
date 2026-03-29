<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Checkbox from '$lib/components/ui/Checkbox.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import ShareButton from '$lib/components/ui/ShareButton.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import Modal from '$lib/components/ui/Modal.svelte';
    import Textarea from '$lib/components/ui/Textarea.svelte';
    import { formatViews } from '$lib/utils/format';
    import { authModal } from '$lib/stores/auth-modal';
    import { isAuthenticated } from '$lib/stores/auth';
    import { toast } from '$lib/stores/toast';
    import { favorites } from '$lib/stores/favorites';
    import { favoritesApi } from '$lib/api/favorites';
    import { eventsApi, type EventResponse } from '$lib/api/events';
    import { handleApiError } from '$lib/api/client';
    import { onMount, onDestroy } from 'svelte';
    import { t } from '$lib/i18n';

    let isAuth = $state(false);
    const unsubAuth = isAuthenticated.subscribe((v) => (isAuth = v));
    onDestroy(unsubAuth);

    let favEventIds = $state<string[]>([]);
    const unsubFavs = favorites.subscribe((s) => (favEventIds = s.events));
    onDestroy(unsubFavs);

    let loading = $state(true);
    let events = $state<EventResponse[]>([]);

    let search = $state('');
    let formatOnline = $state(false);
    let formatOffline = $state(false);
    let city = $state('');
    let sortBy = $state('date');
    let viewMode = $state<'list' | 'calendar'>('list');
    let calendarMonth = $state(new Date().getMonth());
    let calendarYear = $state(new Date().getFullYear());

    let MONTH_NAMES = $derived($t('events.months').split(','));

    interface EventItem {
        id: string;
        title: string;
        organizer: string;
        organizerId: string;
        type: string;
        format: 'online' | 'offline';
        date: string;
        city: string;
        address: string;
        description: string;
        tags: string[];
        participants: number;
    }

    function mapEvent(e: EventResponse): EventItem {
        return {
            id: e.id,
            title: e.title,
            organizer: '',
            organizerId: e.employeeId,
            type: e.format === 'Remote' ? 'Онлайн' : 'Офлайн',
            format: e.format === 'Remote' ? 'online' : 'offline',
            date: e.createdAt?.split('T')[0] ?? '',
            city: e.city || '',
            address: e.address || '',
            description: e.description || '',
            tags: (e.tags || []).map((tag) => tag.name),
            participants: e.views
        };
    }

    onMount(async () => {
        try {
            const data = await eventsApi.getAll(1, 50);
            events = data.items;
        } catch {
            events = [];
        } finally {
            loading = false;
        }
    });

    let displayEvents = $derived(events.map(mapEvent));

    let calendarDays = $derived.by(() => {
        const first = new Date(calendarYear, calendarMonth, 1);
        const startDay = (first.getDay() + 6) % 7;
        const daysInMonth = new Date(calendarYear, calendarMonth + 1, 0).getDate();
        const days: { day: number; events: EventItem[] }[] = [];
        for (let i = 0; i < startDay; i++) days.push({ day: 0, events: [] });
        for (let d = 1; d <= daysInMonth; d++) {
            const dateStr = `${calendarYear}-${String(calendarMonth + 1).padStart(2, '0')}-${String(d).padStart(2, '0')}`;
            days.push({ day: d, events: filtered.filter((e) => e.date === dateStr) });
        }
        return days;
    });

    function prevMonth() {
        if (calendarMonth === 0) {
            calendarMonth = 11;
            calendarYear--;
        } else calendarMonth--;
    }
    function nextMonth() {
        if (calendarMonth === 11) {
            calendarMonth = 0;
            calendarYear++;
        } else calendarMonth++;
    }

    let sortOptions = $derived([
        { value: 'date', label: $t('events.sortByDate') },
        { value: 'name', label: $t('events.sortByName') }
    ]);

    let filtered = $derived.by(() => {
        let list = displayEvents;
        if (search) {
            const q = search.toLowerCase();
            list = list.filter(
                (e) =>
                    e.title.toLowerCase().includes(q) ||
                    e.organizer.toLowerCase().includes(q) ||
                    e.tags.some((t) => t.toLowerCase().includes(q))
            );
        }
        if (formatOnline && !formatOffline) list = list.filter((e) => e.format === 'online');
        if (formatOffline && !formatOnline) list = list.filter((e) => e.format === 'offline');
        if (city) {
            const q = city.toLowerCase();
            list = list.filter((e) => e.city.toLowerCase().includes(q));
        }
        if (sortBy === 'date')
            list = [...list].sort(
                (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
            );
        if (sortBy === 'name') list = [...list].sort((a, b) => a.title.localeCompare(b.title));
        return list;
    });

    let showApplyModal = $state(false);
    let applyTarget = $state<EventItem | null>(null);
    let coverLetter = $state('');
    let applyLoading = $state(false);
    let coverLetterError = $state('');

    function handleRegister(ev: EventItem) {
        if (!isAuth) {
            authModal.open();
            return;
        }
        applyTarget = ev;
        coverLetter = '';
        coverLetterError = '';
        showApplyModal = true;
    }

    async function submitApplication() {
        if (coverLetter.length < 50) {
            coverLetterError = $t('event.coverLetterMinLength');
            return;
        }
        coverLetterError = '';
        applyLoading = true;
        try {
            await eventsApi.apply(applyTarget!.id, coverLetter);
            toast.success($t('events.registered', { title: applyTarget!.title }));
            showApplyModal = false;
            coverLetter = '';
            applyTarget = null;
        } catch (err) {
            handleApiError(err);
        } finally {
            applyLoading = false;
        }
    }

    function isEventFav(id: string): boolean {
        return favEventIds.includes(id);
    }

    async function toggleEventFavorite(ev: EventItem) {
        favorites.toggleEvent(ev.id);
        toast.success(
            favorites.isEventFavorite(ev.id)
                ? $t('event.addToFavorites')
                : $t('event.removeFromFavorites')
        );
        try {
            if (isAuth) {
                await favoritesApi.toggle(ev.id, 'Event');
            }
        } catch {
            favorites.toggleEvent(ev.id);
            toast.error($t('common.error'));
        }
    }
</script>

<svelte:head>
    <title>{$t('seo.eventsTitle')}</title>
    <meta name="description" content={$t('seo.eventsDesc')} />
    <meta property="og:title" content={$t('seo.eventsTitle')} />
    <meta property="og:description" content={$t('seo.eventsDesc')} />
    <meta property="og:type" content="website" />
</svelte:head>

<div class="events-page container--wide">
    <div class="page-header">
        <h1 class="page-title">{$t('events.title')}</h1>
        <div class="page-controls">
            <SearchInput placeholder={$t('events.searchPlaceholder')} bind:value={search} />
            <Select
                options={sortOptions}
                bind:value={sortBy}
                placeholder={$t('jobs.sortPlaceholder')}
            />
            <div class="view-toggle-group">
                <button
                    class="vt-btn"
                    class:active={viewMode === 'list'}
                    type="button"
                    onclick={() => (viewMode = 'list')}
                    title={$t('events.listView')}
                >
                    <svg
                        viewBox="0 0 24 24"
                        width="18"
                        height="18"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="1.75"
                        stroke-linecap="round"
                        ><line x1="8" y1="6" x2="21" y2="6" /><line
                            x1="8"
                            y1="12"
                            x2="21"
                            y2="12"
                        /><line x1="8" y1="18" x2="21" y2="18" /><line
                            x1="3"
                            y1="6"
                            x2="3.01"
                            y2="6"
                        /><line x1="3" y1="12" x2="3.01" y2="12" /><line
                            x1="3"
                            y1="18"
                            x2="3.01"
                            y2="18"
                        /></svg
                    >
                </button>
                <button
                    class="vt-btn"
                    class:active={viewMode === 'calendar'}
                    type="button"
                    onclick={() => (viewMode = 'calendar')}
                    title={$t('events.calendarView')}
                >
                    <svg
                        viewBox="0 0 24 24"
                        width="18"
                        height="18"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="1.75"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        ><rect width="18" height="18" x="3" y="4" rx="2" /><line
                            x1="16"
                            y1="2"
                            x2="16"
                            y2="6"
                        /><line x1="8" y1="2" x2="8" y2="6" /><line
                            x1="3"
                            y1="10"
                            x2="21"
                            y2="10"
                        /></svg
                    >
                </button>
            </div>
        </div>
    </div>

    <div class="events-layout">
        <aside class="events-filters">
            <div class="filter-group">
                <span class="filter-label">{$t('jobs.format')}</span>
                <Checkbox label={$t('events.online')} bind:checked={formatOnline} />
                <Checkbox label={$t('events.offline')} bind:checked={formatOffline} />
            </div>
            <div class="filter-group">
                <Input
                    label={$t('jobs.city')}
                    placeholder={$t('jobs.cityPlaceholder')}
                    bind:value={city}
                />
            </div>
        </aside>

        <div class="events-content">
            {#if viewMode === 'calendar'}
                <div class="calendar">
                    <div class="cal-header">
                        <button
                            class="cal-nav"
                            type="button"
                            onclick={prevMonth}
                            aria-label={$t('common.back')}
                        >
                            <svg
                                viewBox="0 0 24 24"
                                width="18"
                                height="18"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2"
                                stroke-linecap="round"><path d="m15 18-6-6 6-6" /></svg
                            >
                        </button>
                        <span class="cal-month">{MONTH_NAMES[calendarMonth]} {calendarYear}</span>
                        <button
                            class="cal-nav"
                            type="button"
                            onclick={nextMonth}
                            aria-label={$t('common.more')}
                        >
                            <svg
                                viewBox="0 0 24 24"
                                width="18"
                                height="18"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2"
                                stroke-linecap="round"><path d="m9 18 6-6-6-6" /></svg
                            >
                        </button>
                    </div>
                    <div class="cal-weekdays">
                        {#each $t('events.weekdays').split(',') as d (d)}
                            <span class="cal-wd">{d}</span>
                        {/each}
                    </div>
                    <div class="cal-grid">
                        {#each calendarDays as cell, i (i)}
                            <div
                                class="cal-cell"
                                class:empty={cell.day === 0}
                                class:today={cell.day === new Date().getDate() &&
                                    calendarMonth === new Date().getMonth() &&
                                    calendarYear === new Date().getFullYear()}
                            >
                                {#if cell.day}
                                    <span class="cal-day">{cell.day}</span>
                                    {#each cell.events.slice(0, 2) as ev (ev.id)}
                                        <span class="cal-event" title={ev.title}>{ev.title}</span>
                                    {/each}
                                    {#if cell.events.length > 2}
                                        <span class="cal-more">+{cell.events.length - 2}</span>
                                    {/if}
                                {/if}
                            </div>
                        {/each}
                    </div>
                    {#if calendarDays.every((c) => c.events.length === 0)}
                        <p class="cal-empty">{$t('events.noEventsThisMonth')}</p>
                    {/if}
                </div>
            {:else if loading}
                <div class="events-list">
                    {#each Array(4) as _, i (i)}
                        <div class="event-card-skeleton">
                            <div class="sk-date-col">
                                <Skeleton width="2rem" height="1.5rem" />
                                <Skeleton width="2.5rem" height="0.75rem" />
                            </div>
                            <div class="sk-main">
                                <div class="sk-badges-row">
                                    <Skeleton
                                        width="4rem"
                                        height="1.25rem"
                                        radius="var(--radius-full)"
                                    />
                                    <Skeleton
                                        width="3.5rem"
                                        height="1.25rem"
                                        radius="var(--radius-full)"
                                    />
                                </div>
                                <Skeleton width="60%" height="1.125rem" />
                                <div class="sk-meta-row">
                                    <Skeleton circle height="1.25rem" />
                                    <Skeleton width="5rem" height="0.875rem" />
                                    <Skeleton width="8rem" height="0.875rem" />
                                </div>
                                <Skeleton width="90%" height="0.875rem" />
                                <Skeleton width="70%" height="0.875rem" />
                                <div class="sk-bottom-row">
                                    <div class="sk-tags-row">
                                        <Skeleton
                                            width="3.5rem"
                                            height="1.25rem"
                                            radius="var(--radius-full)"
                                        />
                                        <Skeleton
                                            width="4rem"
                                            height="1.25rem"
                                            radius="var(--radius-full)"
                                        />
                                        <Skeleton
                                            width="3rem"
                                            height="1.25rem"
                                            radius="var(--radius-full)"
                                        />
                                    </div>
                                    <Skeleton
                                        width="6rem"
                                        height="2rem"
                                        radius="var(--radius-md)"
                                    />
                                </div>
                            </div>
                        </div>
                    {/each}
                </div>
            {:else if filtered.length === 0}
                <div class="empty-state">
                    <svg
                        viewBox="0 0 24 24"
                        width="48"
                        height="48"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="1.5"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                    >
                        <rect width="18" height="18" x="3" y="4" rx="2" /><line
                            x1="16"
                            y1="2"
                            x2="16"
                            y2="6"
                        /><line x1="8" y1="2" x2="8" y2="6" /><line
                            x1="3"
                            y1="10"
                            x2="21"
                            y2="10"
                        />
                    </svg>
                    <p>{$t('events.notFound')}</p>
                </div>
            {:else}
                <div class="events-list content-fade-in">
                    {#each filtered as event, i (event.id)}
                        <article
                            class="event-card stagger-item"
                            style="animation-delay: {Math.min(i * 50, 500)}ms"
                        >
                            <div class="event-date-col">
                                <span class="event-day">{new Date(event.date).getDate()}</span>
                                <span class="event-month"
                                    >{new Date(event.date).toLocaleDateString('ru-RU', {
                                        month: 'short'
                                    })}</span
                                >
                            </div>
                            <div class="event-main">
                                <div class="event-top">
                                    <div class="event-badges">
                                        <Badge variant="accent">{event.type}</Badge>
                                        <Badge
                                            variant={event.format === 'online'
                                                ? 'success'
                                                : 'warning'}
                                            >{event.format === 'online'
                                                ? $t('events.online')
                                                : $t('events.offline')}</Badge
                                        >
                                    </div>
                                    <span class="event-participants"
                                        >{formatViews(event.participants)}</span
                                    >
                                </div>
                                <a href="/events/{event.id}" class="event-title-link"
                                    ><h3 class="event-title">{event.title}</h3></a
                                >
                                <div class="event-meta">
                                    <a
                                        href="/companies/{event.organizerId}"
                                        class="event-organizer"
                                    >
                                        <Avatar name={event.organizer} size={20} />
                                        {event.organizer}
                                    </a>
                                    {#if event.address}
                                        <span class="event-location">{event.address}</span>
                                    {:else}
                                        <span class="event-location">{event.city}</span>
                                    {/if}
                                </div>
                                <p class="event-description">{event.description}</p>
                                <div class="event-bottom">
                                    <div class="event-tags">
                                        {#each event.tags as tag (tag)}
                                            <Tag>{tag}</Tag>
                                        {/each}
                                    </div>
                                    <div class="event-actions">
                                        <button
                                            class="fav-btn"
                                            class:active={isEventFav(event.id)}
                                            onclick={() => toggleEventFavorite(event)}
                                            title={isEventFav(event.id)
                                                ? $t('event.removeFromFavorites')
                                                : $t('event.addToFavorites')}
                                            type="button"
                                        >
                                            <svg
                                                viewBox="0 0 24 24"
                                                width="20"
                                                height="20"
                                                fill={isEventFav(event.id)
                                                    ? 'currentColor'
                                                    : 'none'}
                                                stroke="currentColor"
                                                stroke-width="1.75"
                                                stroke-linecap="round"
                                                stroke-linejoin="round"
                                            >
                                                <path
                                                    d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"
                                                />
                                            </svg>
                                        </button>
                                        <ShareButton
                                            title={event.title}
                                            url={`${typeof window !== 'undefined' ? window.location.origin : ''}/events/${event.id}`}
                                        />
                                        <Button size="sm" onclick={() => handleRegister(event)}
                                            >{$t('events.participate')}</Button
                                        >
                                    </div>
                                </div>
                            </div>
                        </article>
                    {/each}
                </div>
            {/if}
        </div>
    </div>
</div>

<Modal bind:open={showApplyModal} title={$t('event.applyTitle')} maxWidth="480px">
    <form
        class="apply-form"
        onsubmit={(e) => {
            e.preventDefault();
            submitApplication();
        }}
    >
        <p class="apply-hint">{$t('event.applyHint')}</p>
        <Textarea
            label={$t('event.coverLetter')}
            placeholder={$t('event.coverLetterPlaceholder')}
            bind:value={coverLetter}
            error={coverLetterError}
            hint={$t('event.coverLetterHint', { count: coverLetter.length })}
            rows={5}
        />
        <div class="apply-actions">
            <Button
                variant="outline"
                onclick={() => {
                    showApplyModal = false;
                }}>{$t('common.cancel')}</Button
            >
            <Button type="submit" disabled={applyLoading}>
                {applyLoading ? $t('common.loading') : $t('event.submitApplication')}
            </Button>
        </div>
    </form>
</Modal>

<style>
    .events-page {
        padding: var(--space-8);
        min-height: calc(100dvh - var(--header-height));
    }

    .page-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--space-6);
        flex-wrap: wrap;
        gap: var(--space-4);
    }

    .page-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .page-controls {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .page-controls :global(.select-trigger) {
        height: 2.25rem;
        font-size: var(--font-xs);
        padding: 0 0.75rem;
        min-width: 14rem;
    }
    .page-controls :global(.option) {
        font-size: var(--font-xs);
    }

    .events-layout {
        display: flex;
        gap: var(--space-6);
    }

    .events-filters {
        width: 14rem;
        flex-shrink: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-5);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        height: fit-content;
        position: sticky;
        top: calc(var(--header-height) + var(--space-4));
    }

    .filter-group {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .filter-label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }

    .events-content {
        flex: 1;
        min-width: 0;
    }

    .events-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .event-card {
        display: flex;
        gap: var(--space-5);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors), var(--transition-shadow);
    }

    .event-card:hover {
        border-color: var(--border-hover);
        box-shadow: var(--shadow-md);
    }

    .event-date-col {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        min-width: 3.5rem;
        padding: var(--space-3);
        background: var(--accent-subtle);
        border-radius: var(--radius-md);
        flex-shrink: 0;
        height: fit-content;
    }

    .event-day {
        font-size: var(--font-xl);
        font-weight: var(--weight-extrabold);
        color: var(--accent);
        line-height: 1;
    }

    .event-month {
        font-size: var(--font-xs);
        color: var(--accent);
        font-weight: var(--weight-medium);
        text-transform: lowercase;
    }

    .event-main {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        min-width: 0;
    }

    .event-top {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .event-badges {
        display: flex;
        gap: var(--space-1);
    }

    .event-participants {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .event-title {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
    }

    .event-meta {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        flex-wrap: wrap;
    }

    .event-organizer {
        display: flex;
        align-items: center;
        gap: var(--space-1);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }

    .event-organizer:hover {
        color: var(--accent);
    }

    .event-location {
        font-size: var(--font-sm);
        color: var(--text-tertiary);
    }

    .event-description {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        margin-top: var(--space-1);
    }

    .event-bottom {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-3);
        margin-top: var(--space-2);
    }

    .event-tags {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-1);
    }

    .event-card-skeleton {
        display: flex;
        gap: var(--space-5);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .sk-date-col {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-1);
        min-width: 3.5rem;
        padding: var(--space-3);
        flex-shrink: 0;
    }

    .sk-main {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        min-width: 0;
    }

    .sk-badges-row {
        display: flex;
        gap: var(--space-1);
    }

    .sk-meta-row {
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }

    .sk-bottom-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-3);
        margin-top: var(--space-2);
    }

    .sk-tags-row {
        display: flex;
        gap: var(--space-1);
    }

    .event-title-link {
        text-decoration: none;
        color: inherit;
        transition: var(--transition-colors);
    }

    .event-title-link:hover {
        color: var(--accent);
    }

    .event-actions {
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }

    .fav-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        flex-shrink: 0;
    }

    .fav-btn:hover,
    .fav-btn.active {
        color: var(--color-error);
    }

    .fav-btn.active {
        background: var(--color-error-subtle);
    }

    .apply-form {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .apply-hint {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
    }

    .apply-actions {
        display: flex;
        justify-content: flex-end;
        gap: var(--space-3);
    }

    .empty-state {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-16);
        color: var(--text-tertiary);
    }

    .empty-state p {
        font-size: var(--font-md);
    }

    .view-toggle-group {
        display: flex;
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        overflow: hidden;
        flex-shrink: 0;
    }

    .vt-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.25rem;
        height: 2.25rem;
        color: var(--text-tertiary);
        transition: var(--transition-colors);
    }

    .vt-btn.active {
        background: var(--accent-subtle);
        color: var(--accent);
    }

    .vt-btn:hover:not(.active) {
        background: var(--bg-tertiary);
        color: var(--text-primary);
    }

    .calendar {
        padding: var(--space-4);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .cal-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--space-4);
    }

    .cal-month {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
    }

    .cal-nav {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }

    .cal-nav:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .cal-weekdays {
        display: grid;
        grid-template-columns: repeat(7, 1fr);
        gap: 1px;
        margin-bottom: var(--space-2);
    }

    .cal-wd {
        text-align: center;
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        color: var(--text-tertiary);
        padding: var(--space-1);
    }

    .cal-grid {
        display: grid;
        grid-template-columns: repeat(7, 1fr);
        gap: 1px;
    }

    .cal-cell {
        min-height: 5rem;
        padding: var(--space-1);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        display: flex;
        flex-direction: column;
        gap: 2px;
        overflow: hidden;
    }

    .cal-cell.empty {
        border-color: transparent;
    }

    .cal-cell.today {
        background: var(--accent-subtle);
        border-color: var(--accent);
    }

    .cal-day {
        font-size: var(--font-xs);
        font-weight: var(--weight-semibold);
        color: var(--text-secondary);
    }

    .cal-event {
        font-size: 0.625rem;
        padding: 1px 4px;
        background: var(--accent-subtle);
        color: var(--accent);
        border-radius: var(--radius-sm);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        cursor: default;
    }

    .cal-more {
        font-size: 0.625rem;
        color: var(--text-tertiary);
        padding: 1px 4px;
    }

    .cal-empty {
        text-align: center;
        color: var(--text-tertiary);
        font-size: var(--font-sm);
        padding: var(--space-6);
    }

    @media (max-width: 768px) {
        .events-layout {
            flex-direction: column;
        }
        .events-filters {
            width: 100%;
            position: static;
            flex-direction: row;
            flex-wrap: wrap;
        }
        .event-card {
            flex-direction: column;
        }
        .event-date-col {
            flex-direction: row;
            gap: var(--space-2);
            width: fit-content;
        }
        .event-bottom {
            flex-direction: column;
            align-items: stretch;
        }
        .page-controls {
            width: 100%;
        }
        .cal-cell {
            min-height: 3rem;
        }
        .cal-event {
            display: none;
        }
        .cal-cell.today .cal-day {
            color: var(--accent);
        }
    }
</style>
