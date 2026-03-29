<script lang="ts">
    import Tabs from '$lib/components/ui/Tabs.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import { toast } from '$lib/stores/toast';
    import { timeAgo } from '$lib/utils/format';
    import { mentorshipsApi, type MentorshipResponse } from '$lib/api/mentorships';
    import { handleApiError } from '$lib/api/client';
    import { user as userStore } from '$lib/stores/auth';
    import { t } from '$lib/i18n';
    import { onMount, onDestroy } from 'svelte';

    let activeTab = $state('active');
    let search = $state('');
    let loading = $state(true);

    let tabs = $derived([
        { id: 'active', label: $t('dashMentorships.active') },
        { id: 'closed', label: $t('dashMentorships.closed') },
        { id: 'draft', label: $t('dashMentorships.drafts') }
    ]);

    type MentorshipItem = { id: string; title: string; format: string; city: string; views: number; createdAt: string; endedAt?: string; status: string; isActive: boolean; duration?: string };
    let mentorships = $state<MentorshipItem[]>([]);

    let unsubUser: (() => void) | undefined;
    onMount(() => {
        unsubUser = userStore.subscribe((v) => {
            if (v?.id) loadMentorships(v.id);
        });
    });
    onDestroy(() => unsubUser?.());

    async function loadMentorships(userId: string) {
        loading = true;
        try {
            const data = await mentorshipsApi.getByUser(userId, 1, 50);
            mentorships = data.map((m: MentorshipResponse) => ({
                id: m.id,
                title: m.title,
                format: m.format,
                city: m.city,
                views: m.views,
                createdAt: m.createdAt,
                endedAt: m.endedAt,
                isActive: m.isActive,
                duration: m.duration,
                status: m.isActive ? 'active' : (m.deletedAt ? 'closed' : 'draft')
            }));
        } catch {
            mentorships = [];
        } finally {
            loading = false;
        }
    }

    let filtered = $derived.by(() => {
        let list = mentorships.filter((m) => m.status === activeTab);
        if (search) {
            const q = search.toLowerCase();
            list = list.filter((m) => m.title.toLowerCase().includes(q));
        }
        return list;
    });

    async function toggleStatus(id: string) {
        const mentorship = mentorships.find((m) => m.id === id);
        if (!mentorship) return;
        try {
            await mentorshipsApi.update(id, { isActive: !mentorship.isActive });
            mentorship.isActive = !mentorship.isActive;
            mentorship.status = mentorship.isActive ? 'active' : 'closed';
            mentorships = [...mentorships];
            toast.success($t('dashMentorships.statusChanged'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteMentorship(id: string) {
        try {
            await mentorshipsApi.delete(id);
            mentorships = mentorships.filter((m) => m.id !== id);
            toast.success($t('dashMentorships.mentorshipDeleted'));
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('dashMentorships.pageTitle')}</title>
</svelte:head>

<div class="my-mentorships">
    <div class="page-header">
        <h1 class="page-heading">{$t('dashMentorships.title')}</h1>
        <Button href="/dashboard/mentorships/create">{$t('dashMentorships.createMentorship')}</Button>
    </div>

    <Tabs {tabs} bind:active={activeTab} />

    <div class="search-row">
        <SearchInput placeholder={$t('dashMentorships.searchPlaceholder')} bind:value={search} />
    </div>

    {#if loading}
        <div class="mentorships-list">
            {#each Array(4) as _, i (i)}
                <div class="mentorship-row">
                    <div class="mentorship-info">
                        <div class="mentorship-title-row">
                            <Skeleton width="50%" height="0.875rem" />
                            <Skeleton width="3.5rem" height="1.25rem" radius="var(--radius-full)" />
                        </div>
                        <div class="mentorship-meta">
                            <Skeleton width="3rem" height="0.75rem" />
                            <Skeleton width="4rem" height="0.75rem" />
                            <Skeleton width="5rem" height="0.75rem" />
                        </div>
                    </div>
                    <div class="mentorship-stats">
                        <Skeleton width="4rem" height="1.25rem" radius="var(--radius-full)" />
                    </div>
                    <div class="mentorship-actions">
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                    </div>
                </div>
            {/each}
        </div>
    {:else if filtered.length === 0}
        <div class="empty">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
            <p>{activeTab === 'active' ? $t('dashMentorships.noActive') : activeTab === 'closed' ? $t('dashMentorships.noClosed') : $t('dashMentorships.noDrafts')}</p>
            <p>{activeTab === 'active' ? $t('dashMentorships.noActiveHint') : activeTab === 'closed' ? $t('dashMentorships.noClosedHint') : $t('dashMentorships.noDraftsHint')}</p>
            {#if activeTab === 'active'}
                <Button href="/dashboard/mentorships/create">{$t('dashMentorships.createFirst')}</Button>
            {/if}
        </div>
    {:else}
        <div class="mentorships-list">
            {#each filtered as mentorship (mentorship.id)}
                <div class="mentorship-row">
                    <div class="mentorship-info">
                        <div class="mentorship-title-row">
                            <a href="/mentorships/{mentorship.id}" class="mentorship-title">{mentorship.title}</a>
                            <Badge variant={mentorship.format === 'Remote' ? 'success' : mentorship.format === 'Office' ? 'warning' : 'default'} size="sm">{mentorship.format === 'Remote' ? $t('dashJobs.remote') : mentorship.format === 'Hybrid' ? $t('dashJobs.hybrid') : $t('dashJobs.office')}</Badge>
                        </div>
                        <div class="mentorship-meta">
                            <span>{mentorship.city}</span>
                            <span>&middot;</span>
                            <span>{timeAgo(mentorship.createdAt)}</span>
                            {#if mentorship.duration}
                                <span>&middot;</span>
                                <span>{mentorship.duration}</span>
                            {/if}
                        </div>
                    </div>
                    <div class="mentorship-stats">
                        <a href="/dashboard/mentorships/{mentorship.id}/responses" class="responses-link">
                            <Badge variant="accent">{mentorship.views} {$t('dashMentorships.responses')}</Badge>
                        </a>
                    </div>
                    <div class="mentorship-actions">
                        <Button size="sm" variant="ghost" href="/dashboard/mentorships/{mentorship.id}/edit">
                            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/></svg>
                        </Button>
                        <Button size="sm" variant="ghost" onclick={() => toggleStatus(mentorship.id)}>
                            {#if mentorship.status === 'active'}
                                <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="3" rx="2"/><line x1="9" y1="9" x2="15" y2="15"/><line x1="15" y1="9" x2="9" y2="15"/></svg>
                            {:else}
                                <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                            {/if}
                        </Button>
                        <Button size="sm" variant="ghost" onclick={() => deleteMentorship(mentorship.id)}>
                            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="var(--color-error)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/></svg>
                        </Button>
                    </div>
                </div>
            {/each}
        </div>
    {/if}
</div>

<style>
    .my-mentorships { display: flex; flex-direction: column; gap: var(--space-6); }
    .page-header { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: var(--space-4); }
    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }
    .search-row { max-width: 20rem; margin-top: var(--space-2); }

    .mentorships-list { display: flex; flex-direction: column; gap: var(--space-2); }

    .mentorship-row {
        display: flex; align-items: center; gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary); border: 1px solid var(--border-default); border-radius: var(--radius-lg);
        transition: var(--transition-colors);
    }
    .mentorship-row:hover { border-color: var(--border-hover); }

    .mentorship-info { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: var(--space-1); }
    .mentorship-title-row { display: flex; align-items: center; gap: var(--space-2); flex-wrap: wrap; }
    .mentorship-title { font-size: var(--font-sm); font-weight: var(--weight-semibold); transition: var(--transition-colors); }
    .mentorship-title:hover { color: var(--accent); }
    .mentorship-meta { display: flex; gap: var(--space-2); font-size: var(--font-xs); color: var(--text-tertiary); flex-wrap: wrap; }

    .mentorship-stats { flex-shrink: 0; }
    .responses-link { text-decoration: none; }

    .mentorship-actions { display: flex; gap: var(--space-1); flex-shrink: 0; }

    .empty { display: flex; flex-direction: column; align-items: center; gap: var(--space-3); padding: var(--space-16) var(--space-4); text-align: center; color: var(--text-tertiary); }
    .empty svg { opacity: 0.5; }
    .empty p { max-width: 20rem; }

    @media (max-width: 640px) {
        .search-row { max-width: 100%; }
        .mentorship-row { flex-direction: column; align-items: stretch; }
        .mentorship-actions { justify-content: flex-end; }
    }
</style>
