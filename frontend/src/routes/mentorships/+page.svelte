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
    import { mentorshipsApi, type MentorshipResponse } from '$lib/api/mentorships';
    import { handleApiError } from '$lib/api/client';
    import { onMount, onDestroy } from 'svelte';
    import { t } from '$lib/i18n';

    let isAuth = $state(false);
    const unsubAuth = isAuthenticated.subscribe((v) => (isAuth = v));
    onDestroy(unsubAuth);

    let favMentorshipIds = $state<string[]>([]);
    const unsubFavs = favorites.subscribe((s) => (favMentorshipIds = s.mentorships));
    onDestroy(unsubFavs);

    let loading = $state(true);
    let mentorships = $state<MentorshipResponse[]>([]);

    let search = $state('');
    let formatOnline = $state(false);
    let formatOffline = $state(false);
    let city = $state('');
    let sortBy = $state('date');

    interface MentorshipItem {
        id: string;
        title: string;
        organizer: string;
        organizerId: string;
        format: 'online' | 'offline';
        date: string;
        city: string;
        address: string;
        description: string;
        tags: string[];
        participants: number;
        maxParticipants?: number;
        duration?: string;
    }

    function mapMentorship(m: MentorshipResponse): MentorshipItem {
        return {
            id: m.id,
            title: m.title,
            organizer: m.companyName || '',
            organizerId: m.employeeId,
            format: m.format === 'Remote' ? 'online' : 'offline',
            date: m.createdAt?.split('T')[0] ?? '',
            city: m.city || '',
            address: m.address || '',
            description: m.description || '',
            tags: (m.tags || []).map((tag) => tag.name),
            participants: m.views,
            maxParticipants: m.maxParticipants,
            duration: m.duration
        };
    }

    onMount(async () => {
        try {
            const data = await mentorshipsApi.getAll(1, 50);
            mentorships = data.items;
        } catch {
            mentorships = [];
        } finally {
            loading = false;
        }
    });

    let displayMentorships = $derived(mentorships.map(mapMentorship));

    let sortOptions = $derived([
        { value: 'date', label: $t('events.sortByDate') },
        { value: 'name', label: $t('events.sortByName') }
    ]);

    let filtered = $derived.by(() => {
        let list = displayMentorships;
        if (search) {
            const q = search.toLowerCase();
            list = list.filter(
                (m) =>
                    m.title.toLowerCase().includes(q) ||
                    m.organizer.toLowerCase().includes(q) ||
                    m.tags.some((t) => t.toLowerCase().includes(q))
            );
        }
        if (formatOnline && !formatOffline) list = list.filter((m) => m.format === 'online');
        if (formatOffline && !formatOnline) list = list.filter((m) => m.format === 'offline');
        if (city) {
            const q = city.toLowerCase();
            list = list.filter((m) => m.city.toLowerCase().includes(q));
        }
        if (sortBy === 'date')
            list = [...list].sort(
                (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
            );
        if (sortBy === 'name') list = [...list].sort((a, b) => a.title.localeCompare(b.title));
        return list;
    });

    let showApplyModal = $state(false);
    let applyTarget = $state<MentorshipItem | null>(null);
    let coverLetter = $state('');
    let applyLoading = $state(false);
    let coverLetterError = $state('');

    function handleRegister(m: MentorshipItem) {
        if (!isAuth) {
            authModal.open();
            return;
        }
        applyTarget = m;
        coverLetter = '';
        coverLetterError = '';
        showApplyModal = true;
    }

    async function submitApplication() {
        if (coverLetter.length < 50) {
            coverLetterError = $t('mentorship.coverLetterMinLength');
            return;
        }
        coverLetterError = '';
        applyLoading = true;
        try {
            await mentorshipsApi.apply(applyTarget!.id, coverLetter);
            toast.success($t('mentorships.registered', { title: applyTarget!.title }));
            showApplyModal = false;
            coverLetter = '';
            applyTarget = null;
        } catch (err) {
            handleApiError(err);
        } finally {
            applyLoading = false;
        }
    }

    function isMentorshipFav(id: string): boolean {
        return favMentorshipIds.includes(id);
    }

    async function toggleMentorshipFavorite(m: MentorshipItem) {
        favorites.toggleMentorship(m.id);
        toast.success(
            favorites.isMentorshipFavorite(m.id)
                ? $t('mentorship.addToFavorites')
                : $t('mentorship.removeFromFavorites')
        );
        try {
            if (isAuth) {
                await favoritesApi.toggle(m.id, 'Mentorship');
            }
        } catch {
            favorites.toggleMentorship(m.id);
            toast.error($t('common.error'));
        }
    }
</script>

<svelte:head>
    <title>{$t('seo.mentorshipsTitle')}</title>
    <meta name="description" content={$t('seo.mentorshipsDesc')} />
    <meta property="og:title" content={$t('seo.mentorshipsTitle')} />
    <meta property="og:description" content={$t('seo.mentorshipsDesc')} />
    <meta property="og:type" content="website" />
</svelte:head>

<div class="mentorships-page container--wide">
    <div class="page-header">
        <h1 class="page-title">{$t('mentorships.title')}</h1>
        <div class="page-controls">
            <SearchInput placeholder={$t('mentorships.searchPlaceholder')} bind:value={search} />
            <Select
                options={sortOptions}
                bind:value={sortBy}
                placeholder={$t('jobs.sortPlaceholder')}
            />
        </div>
    </div>

    <div class="mentorships-layout">
        <aside class="mentorships-filters">
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

        <div class="mentorships-content">
            {#if loading}
                <div class="mentorships-list">
                    {#each Array(4) as _, i (i)}
                        <div class="mentorship-card-skeleton">
                            <div class="sk-icon-col">
                                <Skeleton
                                    width="2.5rem"
                                    height="2.5rem"
                                    radius="var(--radius-md)"
                                />
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
                        <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" /><circle
                            cx="9"
                            cy="7"
                            r="4"
                        /><path d="M22 21v-2a4 4 0 0 0-3-3.87" /><path
                            d="M16 3.13a4 4 0 0 1 0 7.75"
                        />
                    </svg>
                    <p>{$t('mentorships.notFound')}</p>
                </div>
            {:else}
                <div class="mentorships-list content-fade-in">
                    {#each filtered as mentorship, i (mentorship.id)}
                        <article
                            class="mentorship-card stagger-item"
                            style="animation-delay: {Math.min(i * 50, 500)}ms"
                        >
                            <div class="mentorship-icon-col">
                                <svg
                                    viewBox="0 0 24 24"
                                    width="24"
                                    height="24"
                                    fill="none"
                                    stroke="var(--accent)"
                                    stroke-width="1.75"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                >
                                    <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" /><circle
                                        cx="9"
                                        cy="7"
                                        r="4"
                                    /><path d="M22 21v-2a4 4 0 0 0-3-3.87" /><path
                                        d="M16 3.13a4 4 0 0 1 0 7.75"
                                    />
                                </svg>
                            </div>
                            <div class="mentorship-main">
                                <div class="mentorship-top">
                                    <div class="mentorship-badges">
                                        <Badge variant="warning">{$t('mentorships.badge')}</Badge>
                                        <Badge
                                            variant={mentorship.format === 'online'
                                                ? 'success'
                                                : 'default'}
                                            >{mentorship.format === 'online'
                                                ? $t('events.online')
                                                : $t('events.offline')}</Badge
                                        >
                                    </div>
                                    <span class="mentorship-participants"
                                        >{formatViews(mentorship.participants)}</span
                                    >
                                </div>
                                <a href="/mentorships/{mentorship.id}" class="mentorship-title-link"
                                    ><h3 class="mentorship-title">{mentorship.title}</h3></a
                                >
                                <div class="mentorship-meta">
                                    <a
                                        href="/companies/{mentorship.organizerId}"
                                        class="mentorship-organizer"
                                    >
                                        <Avatar name={mentorship.organizer} size={20} />
                                        {mentorship.organizer}
                                    </a>
                                    {#if mentorship.address}
                                        <span class="mentorship-location">{mentorship.address}</span
                                        >
                                    {:else}
                                        <span class="mentorship-location">{mentorship.city}</span>
                                    {/if}
                                    {#if mentorship.duration}
                                        <span class="mentorship-duration"
                                            >{mentorship.duration}</span
                                        >
                                    {/if}
                                </div>
                                <p class="mentorship-description">{mentorship.description}</p>
                                <div class="mentorship-bottom">
                                    <div class="mentorship-tags">
                                        {#each mentorship.tags as tag, _ki (tag + _ki)}
                                            <Tag>{tag}</Tag>
                                        {/each}
                                    </div>
                                    <div class="mentorship-actions">
                                        <button
                                            class="fav-btn"
                                            class:active={isMentorshipFav(mentorship.id)}
                                            onclick={() => toggleMentorshipFavorite(mentorship)}
                                            title={isMentorshipFav(mentorship.id)
                                                ? $t('mentorship.removeFromFavorites')
                                                : $t('mentorship.addToFavorites')}
                                            type="button"
                                        >
                                            <svg
                                                viewBox="0 0 24 24"
                                                width="20"
                                                height="20"
                                                fill={isMentorshipFav(mentorship.id)
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
                                            title={mentorship.title}
                                            url={`${typeof window !== 'undefined' ? window.location.origin : ''}/mentorships/${mentorship.id}`}
                                        />
                                        <Button size="sm" onclick={() => handleRegister(mentorship)}
                                            >{$t('mentorships.apply')}</Button
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

<Modal bind:open={showApplyModal} title={$t('mentorship.applyTitle')} maxWidth="480px">
    <form
        class="apply-form"
        onsubmit={(e) => {
            e.preventDefault();
            submitApplication();
        }}
    >
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
            <Button
                variant="outline"
                onclick={() => {
                    showApplyModal = false;
                }}>{$t('common.cancel')}</Button
            >
            <Button type="submit" disabled={applyLoading}>
                {applyLoading ? $t('common.loading') : $t('mentorship.submitApplication')}
            </Button>
        </div>
    </form>
</Modal>

<style>
    .mentorships-page {
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

    .mentorships-layout {
        display: flex;
        gap: var(--space-6);
    }

    .mentorships-filters {
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

    .mentorships-content {
        flex: 1;
        min-width: 0;
    }

    .mentorships-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .mentorship-card {
        display: flex;
        gap: var(--space-5);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors), var(--transition-shadow);
    }

    .mentorship-card:hover {
        border-color: var(--border-hover);
        box-shadow: var(--shadow-md);
    }

    .mentorship-icon-col {
        display: flex;
        align-items: center;
        justify-content: center;
        min-width: 3.5rem;
        padding: var(--space-3);
        background: var(--accent-subtle);
        border-radius: var(--radius-md);
        flex-shrink: 0;
        height: fit-content;
    }

    .mentorship-main {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        min-width: 0;
    }

    .mentorship-top {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .mentorship-badges {
        display: flex;
        gap: var(--space-1);
    }

    .mentorship-participants {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .mentorship-title {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
    }

    .mentorship-meta {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        flex-wrap: wrap;
    }

    .mentorship-organizer {
        display: flex;
        align-items: center;
        gap: var(--space-1);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }

    .mentorship-organizer:hover {
        color: var(--accent);
    }

    .mentorship-location {
        font-size: var(--font-sm);
        color: var(--text-tertiary);
    }

    .mentorship-duration {
        font-size: var(--font-sm);
        color: var(--accent);
        font-weight: var(--weight-medium);
    }

    .mentorship-description {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        margin-top: var(--space-1);
    }

    .mentorship-bottom {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-3);
        margin-top: var(--space-2);
    }

    .mentorship-tags {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-1);
    }

    .mentorship-card-skeleton {
        display: flex;
        gap: var(--space-5);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .sk-icon-col {
        display: flex;
        align-items: center;
        justify-content: center;
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

    .mentorship-title-link {
        text-decoration: none;
        color: inherit;
        transition: var(--transition-colors);
    }

    .mentorship-title-link:hover {
        color: var(--accent);
    }

    .mentorship-actions {
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

    @media (max-width: 768px) {
        .mentorships-layout {
            flex-direction: column;
        }
        .mentorships-filters {
            width: 100%;
            position: static;
            flex-direction: row;
            flex-wrap: wrap;
        }
        .mentorship-card {
            flex-direction: column;
        }
        .mentorship-icon-col {
            flex-direction: row;
            gap: var(--space-2);
            width: fit-content;
        }
        .mentorship-bottom {
            flex-direction: column;
            align-items: stretch;
        }
        .page-controls {
            width: 100%;
        }
    }
</style>
