<script lang="ts">
    import { page } from '$app/state';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import ShareButton from '$lib/components/ui/ShareButton.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import Modal from '$lib/components/ui/Modal.svelte';
    import Textarea from '$lib/components/ui/Textarea.svelte';
    import MapView from '$lib/components/ui/MapView.svelte';
    import MarkdownRenderer from '$lib/components/ui/MarkdownRenderer.svelte';
    import { formatDate, timeAgo, workFormatLabel, formatViews } from '$lib/utils/format';
    import { isAuthenticated } from '$lib/stores/auth';
    import { authModal } from '$lib/stores/auth-modal';
    import { toast } from '$lib/stores/toast';
    import { favorites } from '$lib/stores/favorites';
    import { eventsApi, type EventResponse } from '$lib/api/events';
    import { employeesApi } from '$lib/api/employees';
    import { favoritesApi } from '$lib/api/favorites';
    import { handleApiError } from '$lib/api/client';
    import type { EmployeeProfileResponse } from '$lib/api/auth';
    import { onMount, onDestroy } from 'svelte';
    import { t } from '$lib/i18n';

    let isAuth = $state(false);
    const unsubAuth = isAuthenticated.subscribe((v) => (isAuth = v));
    onDestroy(unsubAuth);

    let loading = $state(true);
    let isFavorite = $state(false);
    let event = $state<EventResponse | null>(null);
    let company = $state<EmployeeProfileResponse | null>(null);
    let similarEvents = $state<EventResponse[]>([]);

    let showApplyModal = $state(false);
    let coverLetter = $state('');
    let applyLoading = $state(false);
    let coverLetterError = $state('');

    let data = $derived(event ?? {
        id: page.params.id ?? '', employeeId: '', userId: '', title: '...', description: '',
        address: '', city: '', country: '', street: '', geoLat: 0, geoLon: 0, salaryFrom: null, salaryTo: null,
        tags: [], format: 'Remote' as const, createdAt: '', updatedAt: '', deletedAt: null, endedAt: '', startDate: null,
        isActive: true, views: 0
    });

    onMount(async () => {
        try {
            const id = page.params.id;
            if (id) {
                isFavorite = favorites.isEventFavorite(id);
                event = await eventsApi.getById(id);
                if (event?.employeeId) {
                    try {
                        company = await employeesApi.getById(event.employeeId);
                    } catch { /* ignored */ }
                }
                try {
                    const allData = await eventsApi.getAll(1, 20);
                    const currentTags = new Set(event?.tags?.map(t => t.name) || []);
                    similarEvents = allData.items
                        .filter(e => e.id !== id)
                        .map(e => ({ event: e, score: e.tags?.filter(t => currentTags.has(t.name)).length || 0 }))
                        .sort((a, b) => b.score - a.score)
                        .slice(0, 3)
                        .map(r => r.event);
                } catch { /* ignored */ }
            }
        } catch (err) {
            handleApiError(err);
        } finally {
            loading = false;
        }
    });

    async function toggleFavorite() {
        const id = page.params.id ?? '';
        const prev = isFavorite;
        isFavorite = !isFavorite;
        favorites.toggleEvent(id);
        toast.success(isFavorite ? $t('event.addToFavorites') : $t('event.removeFromFavorites'));

        try {
            if (isAuth) {
                await favoritesApi.toggle(id, 'Event');
            }
        } catch {
            isFavorite = prev;
            favorites.toggleEvent(id);
            toast.error($t('common.error'));
        }
    }

    function handleApply() {
        if (!isAuth) {
            authModal.open();
            return;
        }
        showApplyModal = true;
    }

    async function submitApplication() {
        const trimmed = coverLetter.trim();
        if (trimmed.length < 50) {
            coverLetterError = $t('event.coverLetterMinLength');
            return;
        }
        if (trimmed.length > 5000) {
            coverLetterError = $t('event.coverLetterMax');
            return;
        }
        coverLetterError = '';
        applyLoading = true;
        try {
            await eventsApi.apply(page.params.id ?? '', trimmed);
            toast.success($t('event.applicationSent'));
            showApplyModal = false;
            coverLetter = '';
        } catch (err) {
            handleApiError(err);
        } finally {
            applyLoading = false;
        }
    }
</script>

<svelte:head>
    <title>{data.title} · {$t('brand.name')}</title>
    <meta name="description" content={data.description?.slice(0, 160)} />
    <meta property="og:title" content="{data.title} · {$t('brand.name')}" />
    <meta property="og:description" content={data.description?.slice(0, 160)} />
    <meta property="og:type" content="website" />
</svelte:head>

<div class="event-page container">
    {#if loading}
        <div class="event-skeleton">
            <div class="sk-main">
                <Skeleton width="8rem" height="0.875rem" />
                <div class="sk-title-row">
                    <Skeleton circle height="3.5rem" />
                    <div class="sk-title-info">
                        <Skeleton width="60%" height="1.5rem" />
                        <Skeleton width="40%" height="0.875rem" />
                    </div>
                </div>
                <div class="sk-badges-row">
                    <Skeleton width="5rem" height="1.25rem" radius="var(--radius-full)" />
                    <Skeleton width="4rem" height="1.25rem" radius="var(--radius-full)" />
                    <Skeleton width="6rem" height="0.875rem" />
                </div>
                <div class="sk-tags-row">
                    {#each Array(5) as _, i (i)}
                        <Skeleton width="4rem" height="1.5rem" radius="var(--radius-full)" />
                    {/each}
                </div>
                <Skeleton width="100%" height="1rem" />
                <Skeleton width="100%" height="1rem" />
                <Skeleton width="80%" height="1rem" />
                <Skeleton width="100%" height="1rem" />
                <Skeleton width="60%" height="1rem" />
            </div>
            <div class="sk-sidebar">
                <Skeleton width="100%" height="10rem" radius="var(--radius-lg)" />
                <Skeleton width="100%" height="8rem" radius="var(--radius-lg)" />
            </div>
        </div>
    {:else}
    <div class="event-layout content-fade-in">
        <main class="event-main">
            <div class="event-header">
                <div class="event-header-top">
                    <a href="/events" class="back-link">
                        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                            <path d="m15 18-6-6 6-6"/>
                        </svg>
                        {$t('event.backToEvents')}
                    </a>
                    <div class="header-actions">
                        <ShareButton title={data.title} />
                        <button class="fav-btn" class:active={isFavorite} onclick={toggleFavorite} title={isFavorite ? $t('event.removeFromFavorites') : $t('event.addToFavorites')} type="button">
                            <svg viewBox="0 0 24 24" width="22" height="22" fill={isFavorite ? 'currentColor' : 'none'} stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                                <path d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"/>
                            </svg>
                        </button>
                    </div>
                </div>

                <div class="event-title-row">
                    <Avatar name={data.title} size={56} />
                    <div class="event-title-info">
                        <h1 class="event-title">{data.title}</h1>
                        <p class="event-location">{data.city}{#if data.country}, {data.country}{/if} {#if data.address}&middot; {data.address}{/if}</p>
                    </div>
                </div>

                <div class="event-meta">
                    <Badge variant="accent">{data.format === 'Remote' ? $t('events.online') : $t('events.offline')}</Badge>
                    <Badge variant={data.format === 'Remote' ? 'success' : data.format === 'Office' ? 'warning' : 'default'}>{workFormatLabel(data.format)}</Badge>
                    {#if data.startDate}
                        <span class="meta-dot">&middot;</span>
                        <span class="meta-text meta-date">
                            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="4" rx="2"/><path d="M16 2v4"/><path d="M8 2v4"/><path d="M3 10h18"/></svg>
                            {new Date(data.startDate).toLocaleDateString(undefined, { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
                        </span>
                    {/if}
                    <span class="meta-dot">&middot;</span>
                    <span class="meta-text">{timeAgo(data.createdAt)}</span>
                    <span class="meta-dot">&middot;</span>
                    <span class="meta-text">{formatViews(data.views)}</span>
                </div>
            </div>

            {#if data.tags?.length}
                <div class="event-tags">
                    {#each data.tags as tag (tag.name)}
                        <Tag clickable onclick={() => {}}>{tag.name}</Tag>
                    {/each}
                </div>
            {/if}

            <div class="event-description">
                <MarkdownRenderer source={data.description} />
            </div>

            <div class="event-actions">
                <Button size="lg" onclick={handleApply}>
                    {isAuth ? $t('events.participate') : $t('job.loginToRespond')}
                </Button>
                <Button size="lg" variant="outline" onclick={toggleFavorite}>
                    {isFavorite ? $t('event.removeFromFavorites') : $t('event.addToFavorites')}
                </Button>
            </div>

            <div class="event-footer-info">
                <span class="footer-item">{$t('job.published')} {formatDate(data.createdAt)}</span>
                {#if data.endedAt}
                    <span class="footer-item">&middot; {$t('event.deadline')} {formatDate(data.endedAt)}</span>
                {/if}
            </div>
        </main>

        <aside class="event-sidebar">
            {#if data.city}
                <div class="sidebar-card minimap-card">
                    <h3 class="sidebar-title">{$t('job.location')}</h3>
                    <div class="minimap-wrap">
                        <MapView
                            markers={[{ id: data.id, lat: data.geoLat || 55.757, lng: data.geoLon || 37.617, title: data.title, company: company?.name ?? '', type: 'Event' }]}
                            center={[data.geoLat || 55.757, data.geoLon || 37.617]}
                            zoom={14}
                            height="12rem"
                        />
                    </div>
                    <p class="minimap-address">{data.address ? `${data.address}, ` : ''}{data.city}{#if data.country}, {data.country}{/if}</p>
                </div>
            {/if}

            <div class="sidebar-card">
                <h3 class="sidebar-title">{$t('event.organizer')}</h3>
                <div class="sidebar-company">
                    <Avatar name={company?.name ?? $t('event.organizer')} size={48} />
                    <div>
                        <a href="/companies/{data.employeeId}" class="company-link">{company?.name ?? $t('event.organizer')}</a>
                        <p class="company-activity">{company?.activity ?? ''}</p>
                    </div>
                </div>
                {#if company?.isVerified}
                    <Badge variant="success">{$t('companies.verified')}</Badge>
                {/if}
            </div>

            {#if similarEvents.length > 0}
            <div class="sidebar-card">
                <h3 class="sidebar-title">{$t('event.similarEvents')}</h3>
                <div class="similar-list">
                    {#each similarEvents as se (se.id)}
                        <a href="/events/{se.id}" class="similar-item">
                            <span class="similar-title">{se.title}</span>
                            <span class="similar-format">{se.format === 'Remote' ? $t('events.online') : $t('events.offline')}</span>
                            {#if se.tags?.length}
                                <span class="similar-tags">{se.tags.slice(0, 3).map(t => t.name).join(', ')}</span>
                            {/if}
                        </a>
                    {/each}
                </div>
            </div>
            {/if}
        </aside>
    </div>
    {/if}
</div>

<Modal bind:open={showApplyModal} title={$t('event.applyTitle')} maxWidth="480px">
    <form class="apply-form" onsubmit={(e) => { e.preventDefault(); submitApplication(); }}>
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
            <Button variant="outline" onclick={() => { showApplyModal = false; }}>{$t('common.cancel')}</Button>
            <Button type="submit" disabled={applyLoading}>
                {applyLoading ? $t('common.loading') : $t('event.submitApplication')}
            </Button>
        </div>
    </form>
</Modal>

<style>
    .event-skeleton {
        display: grid;
        grid-template-columns: 1fr 20rem;
        gap: var(--space-8);
        align-items: start;
    }

    .sk-main {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .sk-title-row {
        display: flex;
        align-items: center;
        gap: var(--space-4);
    }

    .sk-title-info {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .sk-badges-row, .sk-tags-row {
        display: flex;
        gap: var(--space-2);
        flex-wrap: wrap;
    }

    .sk-sidebar {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    @media (max-width: 1024px) {
        .event-skeleton { grid-template-columns: 1fr; }
    }

    .event-page {
        padding-top: var(--space-6);
        padding-bottom: var(--space-16);
        min-height: calc(100dvh - var(--header-height));
    }

    .event-layout {
        display: grid;
        grid-template-columns: 1fr 20rem;
        gap: var(--space-8);
        align-items: start;
    }

    .event-main {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }

    .event-header {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .event-header-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .back-link {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }

    .back-link:hover {
        color: var(--accent);
    }

    .header-actions {
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }

    .fav-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
    }

    .fav-btn:hover, .fav-btn.active {
        color: var(--color-error);
    }

    .fav-btn.active {
        background: var(--color-error-subtle);
    }

    .event-title-row {
        display: flex;
        align-items: center;
        gap: var(--space-4);
    }

    .event-title-info {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .event-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        line-height: var(--leading-tight);
    }

    .event-location {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .event-meta {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        flex-wrap: wrap;
    }

    .meta-dot { color: var(--text-tertiary); }
    .meta-text { font-size: var(--font-sm); color: var(--text-secondary); }
    .meta-date { display: inline-flex; align-items: center; gap: 0.25rem; font-weight: var(--weight-medium); color: var(--accent); }

    .event-tags {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-2);
    }

    .event-description {
        line-height: var(--leading-normal);
    }

    .event-actions {
        display: flex;
        gap: var(--space-3);
        padding-top: var(--space-4);
        border-top: 1px solid var(--border-default);
    }

    .event-footer-info {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .event-sidebar {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
        position: sticky;
        top: calc(var(--header-height) + var(--space-4));
    }

    .sidebar-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .sidebar-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.05em;
    }

    .sidebar-company {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .company-link {
        font-size: var(--font-base);
        font-weight: var(--weight-semibold);
        transition: var(--transition-colors);
    }

    .company-link:hover {
        color: var(--accent);
    }

    .company-activity {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .similar-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .similar-item {
        display: flex;
        flex-direction: column;
        gap: 2px;
        padding: var(--space-3);
        background: var(--bg-tertiary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        text-decoration: none;
    }

    .similar-item:hover {
        background: var(--border-hover);
        color: inherit;
    }

    .similar-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-primary);
    }

    .similar-format {
        font-size: var(--font-xs);
        color: var(--accent);
    }

    .similar-tags {
        font-size: 0.6875rem;
        color: var(--text-tertiary);
    }

    .minimap-card {
        padding: 0;
        overflow: hidden;
    }

    .minimap-card .sidebar-title {
        padding: var(--space-4) var(--space-4) var(--space-2);
    }

    .minimap-wrap :global(.map-wrapper) {
        border-radius: 0;
    }

    .minimap-address {
        padding: var(--space-2) var(--space-4) var(--space-4);
        font-size: var(--font-xs);
        color: var(--text-secondary);
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

    @media (max-width: 1024px) {
        .event-layout {
            grid-template-columns: 1fr;
        }

        .event-sidebar {
            position: static;
        }
    }

    @media (max-width: 640px) {
        .event-actions {
            flex-direction: column;
        }
    }
</style>
