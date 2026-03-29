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
    import { mentorshipsApi, type MentorshipResponse } from '$lib/api/mentorships';
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
    let mentorship = $state<MentorshipResponse | null>(null);
    let company = $state<EmployeeProfileResponse | null>(null);
    let similarMentorships = $state<MentorshipResponse[]>([]);

    let showApplyModal = $state(false);
    let coverLetter = $state('');
    let applyLoading = $state(false);
    let coverLetterError = $state('');

    let data = $derived(mentorship ?? {
        id: page.params.id ?? '', employeeId: '', userId: '', title: '...', description: '',
        address: '', city: '', country: '', street: '', geoLat: '0', geoLon: '0', salaryFrom: undefined, salaryTo: undefined,
        tags: [], format: 'Remote' as const, createdAt: '', updatedAt: '', deletedAt: undefined, endedAt: undefined, startDate: undefined,
        isActive: true, views: 0, photos: [], videos: []
    });

    onMount(async () => {
        try {
            const id = page.params.id;
            if (id) {
                isFavorite = favorites.isMentorshipFavorite(id);
                mentorship = await mentorshipsApi.getById(id);
                if (mentorship?.employeeId) {
                    try {
                        company = await employeesApi.getById(mentorship.employeeId);
                    } catch { /* ignored */ }
                }
                try {
                    const allData = await mentorshipsApi.getAll(1, 20);
                    const currentTags = new Set(mentorship?.tags?.map(t => t.name) || []);
                    similarMentorships = allData.items
                        .filter(m => m.id !== id)
                        .map(m => ({ mentorship: m, score: m.tags?.filter(t => currentTags.has(t.name)).length || 0 }))
                        .sort((a, b) => b.score - a.score)
                        .slice(0, 3)
                        .map(r => r.mentorship);
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
        favorites.toggleMentorship(id);
        toast.success(isFavorite ? $t('mentorship.addToFavorites') : $t('mentorship.removeFromFavorites'));

        try {
            if (isAuth) {
                await favoritesApi.toggle(id, 'Mentorship');
            }
        } catch {
            isFavorite = prev;
            favorites.toggleMentorship(id);
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
            coverLetterError = $t('mentorship.coverLetterMinLength');
            return;
        }
        if (trimmed.length > 5000) {
            coverLetterError = $t('mentorship.coverLetterMax');
            return;
        }
        coverLetterError = '';
        applyLoading = true;
        try {
            await mentorshipsApi.apply(page.params.id ?? '', trimmed);
            toast.success($t('mentorship.applicationSent'));
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

<div class="mentorship-page container">
    {#if loading}
        <div class="mentorship-skeleton">
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
    <div class="mentorship-layout content-fade-in">
        <main class="mentorship-main">
            <div class="mentorship-header">
                <div class="mentorship-header-top">
                    <a href="/mentorships" class="back-link">
                        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                            <path d="m15 18-6-6 6-6"/>
                        </svg>
                        {$t('mentorship.backToMentorships')}
                    </a>
                    <div class="header-actions">
                        <ShareButton title={data.title} />
                        <button class="fav-btn" class:active={isFavorite} onclick={toggleFavorite} title={isFavorite ? $t('mentorship.removeFromFavorites') : $t('mentorship.addToFavorites')} type="button">
                            <svg viewBox="0 0 24 24" width="22" height="22" fill={isFavorite ? 'currentColor' : 'none'} stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                                <path d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"/>
                            </svg>
                        </button>
                    </div>
                </div>

                <div class="mentorship-title-row">
                    <Avatar name={data.title} size={56} />
                    <div class="mentorship-title-info">
                        <h1 class="mentorship-title">{data.title}</h1>
                        <p class="mentorship-location">{data.city}{#if data.country}, {data.country}{/if} {#if data.address}&middot; {data.address}{/if}</p>
                    </div>
                </div>

                <div class="mentorship-meta">
                    <Badge variant="warning">{$t('mentorships.badge')}</Badge>
                    <Badge variant={data.format === 'Remote' ? 'success' : data.format === 'Office' ? 'warning' : 'default'}>{workFormatLabel(data.format)}</Badge>
                    {#if mentorship?.duration}
                        <span class="meta-dot">&middot;</span>
                        <span class="meta-text meta-duration">{mentorship.duration}</span>
                    {/if}
                    {#if mentorship?.maxParticipants}
                        <span class="meta-dot">&middot;</span>
                        <span class="meta-text">{$t('mentorship.maxParticipants')}: {mentorship.maxParticipants}</span>
                    {/if}
                    <span class="meta-dot">&middot;</span>
                    <span class="meta-text">{timeAgo(data.createdAt)}</span>
                    <span class="meta-dot">&middot;</span>
                    <span class="meta-text">{formatViews(data.views)}</span>
                </div>
            </div>

            {#if data.tags?.length}
                <div class="mentorship-tags">
                    {#each data.tags as tag (tag.name)}
                        <Tag clickable onclick={() => {}}>{tag.name}</Tag>
                    {/each}
                </div>
            {/if}

            <div class="mentorship-description">
                <MarkdownRenderer source={data.description} />
            </div>

            <div class="mentorship-actions">
                <Button size="lg" onclick={handleApply}>
                    {isAuth ? $t('mentorships.apply') : $t('job.loginToRespond')}
                </Button>
                <Button size="lg" variant="outline" onclick={toggleFavorite}>
                    {isFavorite ? $t('mentorship.removeFromFavorites') : $t('mentorship.addToFavorites')}
                </Button>
            </div>

            <div class="mentorship-footer-info">
                <span class="footer-item">{$t('job.published')} {formatDate(data.createdAt)}</span>
                {#if data.endedAt}
                    <span class="footer-item">&middot; {$t('event.deadline')} {formatDate(data.endedAt)}</span>
                {/if}
            </div>
        </main>

        <aside class="mentorship-sidebar">
            {#if data.city}
                <div class="sidebar-card minimap-card">
                    <h3 class="sidebar-title">{$t('job.location')}</h3>
                    <div class="minimap-wrap">
                        <MapView
                            markers={[{ id: data.id, lat: Number(data.geoLat) || 55.757, lng: Number(data.geoLon) || 37.617, title: data.title, company: company?.name ?? '', type: 'Mentorship' }]}
                            center={[Number(data.geoLat) || 55.757, Number(data.geoLon) || 37.617]}
                            zoom={14}
                            height="12rem"
                        />
                    </div>
                    <p class="minimap-address">{data.address ? `${data.address}, ` : ''}{data.city}{#if data.country}, {data.country}{/if}</p>
                </div>
            {/if}

            <div class="sidebar-card">
                <h3 class="sidebar-title">{$t('mentorship.mentor')}</h3>
                <div class="sidebar-company">
                    <Avatar name={company?.name ?? $t('mentorship.mentor')} size={48} />
                    <div>
                        <a href="/companies/{data.employeeId}" class="company-link">{company?.name ?? $t('mentorship.mentor')}</a>
                        <p class="company-activity">{company?.activity ?? ''}</p>
                    </div>
                </div>
                {#if company?.isVerified}
                    <Badge variant="success">{$t('companies.verified')}</Badge>
                {/if}
            </div>

            {#if similarMentorships.length > 0}
            <div class="sidebar-card">
                <h3 class="sidebar-title">{$t('mentorship.similarMentorships')}</h3>
                <div class="similar-list">
                    {#each similarMentorships as sm (sm.id)}
                        <a href="/mentorships/{sm.id}" class="similar-item">
                            <span class="similar-title">{sm.title}</span>
                            <span class="similar-format">{sm.format === 'Remote' ? $t('events.online') : $t('events.offline')}</span>
                            {#if sm.tags?.length}
                                <span class="similar-tags">{sm.tags.slice(0, 3).map(t => t.name).join(', ')}</span>
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

<Modal bind:open={showApplyModal} title={$t('mentorship.applyTitle')} maxWidth="480px">
    <form class="apply-form" onsubmit={(e) => { e.preventDefault(); submitApplication(); }}>
        <p class="apply-hint">{$t('mentorship.applyHint')}</p>
        <Textarea
            label={$t('mentorship.coverLetter')}
            placeholder={$t('mentorship.coverLetterPlaceholder')}
            bind:value={coverLetter}
            error={coverLetterError}
            hint={$t('mentorship.coverLetterHintCount', { count: coverLetter.length })}
            rows={5}
        />
        <div class="apply-actions">
            <Button variant="outline" onclick={() => { showApplyModal = false; }}>{$t('common.cancel')}</Button>
            <Button type="submit" disabled={applyLoading}>
                {applyLoading ? $t('common.loading') : $t('mentorship.submitApplication')}
            </Button>
        </div>
    </form>
</Modal>

<style>
    .mentorship-skeleton {
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
        .mentorship-skeleton { grid-template-columns: 1fr; }
    }

    .mentorship-page {
        padding-top: var(--space-6);
        padding-bottom: var(--space-16);
        min-height: calc(100dvh - var(--header-height));
    }

    .mentorship-layout {
        display: grid;
        grid-template-columns: 1fr 20rem;
        gap: var(--space-8);
        align-items: start;
    }

    .mentorship-main {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }

    .mentorship-header {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .mentorship-header-top {
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

    .mentorship-title-row {
        display: flex;
        align-items: center;
        gap: var(--space-4);
    }

    .mentorship-title-info {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .mentorship-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        line-height: var(--leading-tight);
    }

    .mentorship-location {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .mentorship-meta {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        flex-wrap: wrap;
    }

    .meta-dot { color: var(--text-tertiary); }
    .meta-text { font-size: var(--font-sm); color: var(--text-secondary); }
    .meta-duration { font-weight: var(--weight-medium); color: var(--accent); }

    .mentorship-tags {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-2);
    }

    .mentorship-description {
        line-height: var(--leading-normal);
    }

    .mentorship-actions {
        display: flex;
        gap: var(--space-3);
        padding-top: var(--space-4);
        border-top: 1px solid var(--border-default);
    }

    .mentorship-footer-info {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .mentorship-sidebar {
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
        .mentorship-layout {
            grid-template-columns: 1fr;
        }

        .mentorship-sidebar {
            position: static;
        }
    }

    @media (max-width: 640px) {
        .mentorship-actions {
            flex-direction: column;
        }
    }
</style>
