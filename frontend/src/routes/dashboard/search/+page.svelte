<script lang="ts">
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import { workersApi, type WorkerSearchResult } from '$lib/api/workers';
    import { handleApiError } from '$lib/api/client';
    import { onMount } from 'svelte';
    import { t } from '$lib/i18n';

    let search = $state('');
    let skills = $state('');
    let university = $state('');
    let loading = $state(true);
    let workers = $state<WorkerSearchResult[]>([]);
    let totalCount = $state(0);
    let page = $state(1);
    const pageSize = 12;

    async function loadWorkers() {
        loading = true;
        try {
            const data = await workersApi.search({
                search: search || undefined,
                skills: skills || undefined,
                university: university || undefined,
                pageSize,
                pageNumber: page
            });
            workers = data.items || [];
            totalCount = data.totalCount || 0;
        } catch (err) {
            handleApiError(err);
            workers = [];
        } finally {
            loading = false;
        }
    }

    onMount(() => {
        loadWorkers();
    });

    function handleSearch() {
        page = 1;
        loadWorkers();
    }

    let totalPages = $derived(Math.ceil(totalCount / pageSize) || 1);

    function goPage(p: number) {
        if (p < 1 || p > totalPages) return;
        page = p;
        loadWorkers();
    }

    function fullName(w: WorkerSearchResult) {
        return [w.lastName, w.name, w.patronymic].filter(Boolean).join(' ') || w.userId;
    }
</script>

<svelte:head>
    <title>{$t('workerSearch.pageTitle')}</title>
</svelte:head>

<div class="search-page">
    <h1 class="page-heading">{$t('workerSearch.title')}</h1>

    <div class="filters">
        <div class="filter-item">
            <SearchInput placeholder={$t('workerSearch.searchPlaceholder')} bind:value={search} onsearch={handleSearch} />
        </div>
        <div class="filter-item">
            <Input placeholder={$t('workerSearch.skillsPlaceholder')} bind:value={skills} onchange={handleSearch} />
        </div>
        <div class="filter-item">
            <Input placeholder={$t('workerSearch.universityPlaceholder')} bind:value={university} onchange={handleSearch} />
        </div>
        <button class="search-btn" type="button" onclick={handleSearch} aria-label={$t('common.search')}>
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
        </button>
    </div>

    {#if loading}
        <div class="workers-grid">
            {#each Array(6) as _, i (i)}
                <div class="worker-card skeleton-card">
                    <div class="skeleton-avatar"></div>
                    <div class="skeleton-line w60"></div>
                    <div class="skeleton-line w40"></div>
                </div>
            {/each}
        </div>
    {:else if workers.length === 0}
        <div class="empty">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/></svg>
            <p>{$t('workerSearch.noResults')}</p>
            <p>{$t('workerSearch.noResultsHint')}</p>
        </div>
    {:else}
        <div class="workers-grid">
            {#each workers as worker (worker.userId)}
                {#if worker.isPrivate}
                    <div class="worker-card private">
                        <div class="worker-avatar">
                            <Avatar name={fullName(worker)} src={worker.photo} size={56} />
                        </div>
                        <span class="worker-name">{fullName(worker)}</span>
                        <div class="private-badge">
                            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="11" x="3" y="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                            <span>{$t('workerSearch.privateProfile')}</span>
                        </div>
                    </div>
                {:else}
                    <a href="/profile/{worker.userId}" class="worker-card">
                        <div class="worker-avatar">
                            <Avatar name={fullName(worker)} src={worker.photo} size={56} />
                        </div>
                        <span class="worker-name">{fullName(worker)}</span>
                        {#if worker.university}
                            <Badge size="sm">{worker.university}{#if worker.course}, {worker.course} {$t('workerSearch.course')}{/if}</Badge>
                        {/if}
                        {#if worker.skills && worker.skills.length > 0}
                            <div class="worker-skills">
                                {#each worker.skills.slice(0, 4) as skill (skill)}
                                    <Tag>{skill}</Tag>
                                {/each}
                                {#if worker.skills.length > 4}
                                    <Badge size="sm">+{worker.skills.length - 4}</Badge>
                                {/if}
                            </div>
                        {/if}
                    </a>
                {/if}
            {/each}
        </div>

        {#if totalPages > 1}
            <div class="pagination">
                <Button size="sm" variant="ghost" disabled={page <= 1} onclick={() => goPage(page - 1)}>
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
                </Button>
                {#each Array(totalPages) as _, i (i)}
                    <button class="page-btn" class:active={page === i + 1} type="button" onclick={() => goPage(i + 1)}>{i + 1}</button>
                {/each}
                <Button size="sm" variant="ghost" disabled={page >= totalPages} onclick={() => goPage(page + 1)}>
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
                </Button>
            </div>
        {/if}
    {/if}
</div>

<style>
    .search-page { display: flex; flex-direction: column; gap: var(--space-6); }
    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }

    .filters {
        display: flex;
        gap: var(--space-3);
        flex-wrap: wrap;
        align-items: flex-end;
    }

    .filter-item { width: 14rem; }

    .search-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        flex-shrink: 0;
        background: var(--accent);
        color: var(--accent-contrast);
        border-radius: var(--radius-md);
        cursor: pointer;
        transition: var(--transition-colors);
    }

    .search-btn:hover {
        opacity: 0.9;
    }

    .workers-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(14rem, 1fr));
        gap: var(--space-4);
    }

    .worker-card {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-6) var(--space-4);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        text-decoration: none;
        color: inherit;
        text-align: center;
        transition: var(--transition-transform), var(--transition-shadow);
    }

    .worker-card:not(.private):hover {
        transform: translateY(-0.125rem);
        box-shadow: var(--shadow-md);
        border-color: var(--border-hover);
    }

    .worker-card.private {
        opacity: 0.7;
        cursor: default;
    }

    .worker-avatar { flex-shrink: 0; }

    .worker-name {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }

    .worker-skills {
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
        gap: var(--space-1);
    }

    .private-badge {
        display: flex;
        align-items: center;
        gap: var(--space-1);
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .pagination {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: var(--space-1);
        margin-top: var(--space-4);
    }

    .page-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        min-width: 2rem;
        height: 2rem;
        padding: 0 var(--space-2);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
    }

    .page-btn:hover { background: var(--bg-tertiary); color: var(--text-primary); }
    .page-btn.active { background: var(--accent); color: var(--accent-contrast); }

    .skeleton-card { min-height: 10rem; }
    .skeleton-avatar {
        width: 3.5rem;
        height: 3.5rem;
        border-radius: var(--radius-full);
        background: var(--bg-tertiary);
        animation: pulse 1.5s infinite;
    }
    .skeleton-line {
        height: 0.75rem;
        border-radius: var(--radius-sm);
        background: var(--bg-tertiary);
        animation: pulse 1.5s infinite;
    }
    .skeleton-line.w60 { width: 60%; }
    .skeleton-line.w40 { width: 40%; }

    @keyframes pulse {
        0%, 100% { opacity: 1; }
        50% { opacity: 0.5; }
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
    .empty svg { opacity: 0.5; }
    .empty p { max-width: 20rem; }

    @media (max-width: 640px) {
        .filters { flex-direction: column; }
        .filter-item { width: 100%; }
    }
</style>
