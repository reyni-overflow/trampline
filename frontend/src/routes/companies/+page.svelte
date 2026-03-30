<script lang="ts">
    import { onMount } from 'svelte';
    import CompanyCard from '$lib/components/ui/CompanyCard.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Pagination from '$lib/components/ui/Pagination.svelte';
    import CompanyCardSkeleton from '$lib/components/ui/CompanyCardSkeleton.svelte';
    import { employeesApi } from '$lib/api/employees';
    import { t } from '$lib/i18n';

    let search = $state('');
    let activityFilter = $state('');
    let page = $state(1);
    let totalPages = $state(1);
    const pageSize = 12;

    let companies = $state<
        {
            id: string;
            name: string;
            activity: string;
            isVerified: boolean;
            verificationLevel: number;
            jobCount: number;
            link: string | null;
        }[]
    >([]);
    let loading = $state(true);
    let _totalCompanies = $state(0);

    const activityOptions = $derived([
        { value: '', label: $t('companies.allFields') },
        { value: 'dev', label: $t('companies.fieldSoftware') },
        { value: 'fintech', label: $t('companies.fieldFintech') },
        { value: 'gamedev', label: $t('companies.fieldGamedev') },
        { value: 'ai', label: $t('companies.fieldAI') },
        { value: 'security', label: $t('companies.fieldCyber') },
        { value: 'cloud', label: $t('companies.fieldCloud') },
        { value: 'edtech', label: $t('companies.fieldEdtech') }
    ]);

    let debounceTimer: ReturnType<typeof setTimeout>;

    async function loadCompanies(p = page) {
        loading = true;
        try {
            const activityMap: Record<string, string> = {
                dev: $t('companies.fieldSoftware'),
                fintech: $t('companies.fieldFintech'),
                gamedev: $t('companies.fieldGamedev'),
                ai: $t('companies.fieldAI'),
                security: $t('companies.fieldCyber'),
                cloud: $t('companies.fieldCloud'),
                edtech: $t('companies.fieldEdtech')
            };
            const data = await employeesApi.getAll(p, pageSize, {
                search: search || undefined,
                activity: activityFilter ? activityMap[activityFilter] : undefined
            });
            companies = data.items.map((e) => ({
                id: e.id,
                name: e.name,
                activity: e.activity,
                isVerified: e.isVerified,
                verificationLevel: e.verificationLevel ?? 0,
                jobCount: e.jobs?.filter((j) => j.isActive).length ?? 0,
                link: e.link
            }));
            _totalCompanies = data.totalCount;
            totalPages = Math.max(1, Math.ceil(data.totalCount / pageSize));
        } catch {
            companies = [];
        } finally {
            loading = false;
        }
    }

    function onFilterChange() {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            page = 1;
            loadCompanies(1);
        }, 300);
    }

    $effect(() => {
        search;
        activityFilter;
        onFilterChange();
    });

    onMount(() => loadCompanies());
</script>

<svelte:head>
    <title>{$t('seo.companiesTitle')}</title>
    <meta name="description" content={$t('seo.companiesDesc')} />
    <meta property="og:title" content={$t('seo.companiesTitle')} />
    <meta property="og:description" content={$t('seo.companiesDesc')} />
    <meta property="og:type" content="website" />
</svelte:head>

<div class="companies-page container--wide">
    <div class="page-header">
        <h1 class="page-title">{$t('companies.title')}</h1>
        <div class="page-controls">
            <SearchInput placeholder={$t('common.search')} bind:value={search} />
            <Select
                options={activityOptions}
                bind:value={activityFilter}
                placeholder={$t('common.industry')}
            />
        </div>
    </div>

    {#if loading}
        <div class="companies-grid">
            {#each Array(8) as _, i (i)}
                <CompanyCardSkeleton />
            {/each}
        </div>
    {:else if companies.length === 0}
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
                <path d="M6 22V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v18Z" /><path
                    d="M6 12H4a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h2"
                /><path d="M18 9h2a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-2" />
            </svg>
            <p>{$t('companies.notFound')}</p>
        </div>
    {:else}
        <div class="companies-grid content-fade-in">
            {#each companies as company, i (company.id)}
                <div class="stagger-item" style="animation-delay: {Math.min(i * 50, 500)}ms">
                    <CompanyCard {...company} />
                </div>
            {/each}
        </div>
        <div class="page-pagination">
            <Pagination bind:page {totalPages} onchange={() => loadCompanies(page)} />
        </div>
    {/if}
</div>

<style>
    .companies-page {
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

    .companies-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(17rem, 1fr));
        gap: var(--space-4);
    }

    .page-pagination {
        display: flex;
        justify-content: center;
        margin-top: var(--space-8);
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

    @media (max-width: 640px) {
        .page-controls {
            width: 100%;
            flex-direction: column;
        }
    }
</style>
