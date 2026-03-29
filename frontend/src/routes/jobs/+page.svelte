<script lang="ts">
    import JobCard from '$lib/components/ui/JobCard.svelte';
    import ViewToggle from '$lib/components/ui/ViewToggle.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import RangeSlider from '$lib/components/ui/RangeSlider.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Checkbox from '$lib/components/ui/Checkbox.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Pagination from '$lib/components/ui/Pagination.svelte';
    import JobCardSkeleton from '$lib/components/ui/JobCardSkeleton.svelte';
    import ComparisonBar from '$lib/components/ui/ComparisonBar.svelte';
    import MapView from '$lib/components/ui/MapView.svelte';
    import { jobsApi, type JobResponse } from '$lib/api/jobs';
    import { toast } from '$lib/stores/toast';
    import { formatSalary } from '$lib/utils/format';
    import { getCityCoords } from '$lib/utils/geo';
    import { favorites } from '$lib/stores/favorites';
    import { t } from '$lib/i18n';

    let viewMode = $state<'grid' | 'list'>('grid');
    let splitView = $state(false);
    let jobs = $state<JobResponse[]>([]);
    let loading = $state(true);
    let page = $state(1);
    let totalPages = $state(1);
    let filtersOpen = $state(true);

    let search = $state('');
    let typeWork = $state(true);
    let typeInternship = $state(true);
    let typeMentorship = $state(false);
    let typeEvent = $state(false);
    let formatRemote = $state(false);
    let formatHybrid = $state(false);
    let formatOffice = $state(false);
    let sortBy = $state('date');
    let city = $state('');
    let salaryMin = $state(0);
    let salaryMax = $state(1500000);

    const sortOptions = $derived([
        { value: 'date', label: $t('jobs.sortByDate') },
        { value: 'salary', label: $t('jobs.sortBySalary') },
        { value: 'responses', label: $t('jobs.sortByResponses') }
    ]);

    let activeFiltersCount = $derived.by(() => {
        let count = 0;
        if (!typeWork || !typeInternship) count++;
        if (typeMentorship) count++;
        if (typeEvent) count++;
        if (formatRemote || formatHybrid || formatOffice) count++;
        if (city) count++;
        if (search) count++;
        return count;
    });

    let filtersLabel = $derived(
        activeFiltersCount > 0
            ? `${$t('jobs.filters')} (${activeFiltersCount})`
            : $t('jobs.filters')
    );

    function buildTypeFilter(): string | undefined {
        const types: string[] = [];
        if (typeWork) types.push('Work');
        if (typeInternship) types.push('Internship');
        if (typeMentorship) types.push('Mentorship');
        if (typeEvent) types.push('Event');
        if (types.length === 0 || types.length === 4) return undefined;
        return types.join(',');
    }

    function buildFormatFilter(): string | undefined {
        const formats: string[] = [];
        if (formatRemote) formats.push('Remote');
        if (formatHybrid) formats.push('Hybrid');
        if (formatOffice) formats.push('Office');
        if (formats.length === 0 || formats.length === 3) return undefined;
        return formats.join(',');
    }

    async function loadJobs() {
        loading = true;
        try {
            const data = await jobsApi.getAll(page, 12, {
                city: city || undefined,
                salaryMin: salaryMin || undefined,
                salaryMax: salaryMax < 1500000 ? salaryMax : undefined,
                search: search || undefined,
                type: buildTypeFilter(),
                format: buildFormatFilter()
            });
            jobs = data.items;
            totalPages = Math.max(1, data.totalPages);
        } catch {
            toast.error($t('jobs.loadError'));
            jobs = [];
        } finally {
            loading = false;
        }
    }

    function resetFilters() {
        search = '';
        typeWork = true;
        typeInternship = true;
        typeMentorship = false;
        typeEvent = false;
        formatRemote = false;
        formatHybrid = false;
        formatOffice = false;
        city = '';
        sortBy = 'date';
        salaryMin = 0;
        salaryMax = 1500000;
    }

    let debounceTimer: ReturnType<typeof setTimeout>;

    $effect(() => {
        void search;
        void typeWork;
        void typeInternship;
        void typeMentorship;
        void typeEvent;
        void formatRemote;
        void formatHybrid;
        void formatOffice;
        void city;
        void salaryMin;
        void salaryMax;
        void sortBy;

        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            loadJobs();
        }, 300);
    });

    let displayJobs = $derived.by(() => {
        let list = jobs;

        if (sortBy === 'salary')
            list = [...list].sort((a, b) => (b.salaryFrom ?? 0) - (a.salaryFrom ?? 0));
        if (sortBy === 'date')
            list = [...list].sort(
                (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
            );

        return list;
    });

    let mapMarkers = $derived(
        displayJobs.map((j, i) => {
            const coords = j.geoLat && j.geoLon ? [j.geoLat, j.geoLon] : getCityCoords(j.city, i);
            return {
                id: j.id,
                lat: coords[0],
                lng: coords[1],
                title: j.title,
                company: j.companyName || j.city,
                salary: formatSalary(j.salaryFrom, j.salaryTo),
                tags: j.tags?.map((t) => (typeof t === 'string' ? t : t.name)),
                type: j.type,
                isFavorite:
                    favorites.isJobFavorite(j.id) || favorites.isCompanyFavorite(j.employeeId)
            };
        })
    );
</script>

<svelte:head>
    <title>{$t('seo.jobsTitle')}</title>
    <meta name="description" content={$t('seo.jobsDesc')} />
    <meta property="og:title" content={$t('seo.jobsTitle')} />
    <meta property="og:description" content={$t('seo.jobsDesc')} />
    <meta property="og:type" content="website" />
</svelte:head>

<div class="jobs-page container--wide">
    <div class="jobs-header">
        <div class="jobs-title-row">
            <h1 class="jobs-title">{$t('jobs.title')}</h1>
        </div>
        <div class="jobs-controls">
            <button
                class="filter-toggle"
                type="button"
                onclick={() => (filtersOpen = !filtersOpen)}
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
                >
                    <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
                </svg>
                {filtersLabel}
            </button>
            <Select
                options={sortOptions}
                bind:value={sortBy}
                placeholder={$t('jobs.sortPlaceholder')}
            />
            <ViewToggle bind:mode={viewMode} />
            <button
                class="split-toggle"
                class:active={splitView}
                type="button"
                onclick={() => (splitView = !splitView)}
                title={$t('map.title')}
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
                    ><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z" /><circle
                        cx="12"
                        cy="10"
                        r="3"
                    /></svg
                >
            </button>
        </div>
    </div>

    <div class="jobs-layout" class:filters-open={filtersOpen}>
        <aside class="filters-panel" class:collapsed={!filtersOpen}>
            <div class="filters-header">
                <h3 class="filters-title">{$t('jobs.filters')}</h3>
                <button class="filters-reset" type="button" onclick={resetFilters}
                    >{$t('jobs.resetFilters')}</button
                >
            </div>

            <div class="filter-group">
                <SearchInput placeholder={$t('jobs.searchByTitle')} bind:value={search} />
            </div>

            <div class="filter-group">
                <span class="filter-label">{$t('jobs.type')}</span>
                <Checkbox label={$t('jobs.checkJobs')} bind:checked={typeWork} />
                <Checkbox label={$t('jobs.checkInternships')} bind:checked={typeInternship} />
                <Checkbox label={$t('jobs.checkMentorship')} bind:checked={typeMentorship} />
                <Checkbox label={$t('jobs.checkEvents')} bind:checked={typeEvent} />
            </div>

            <div class="filter-group">
                <span class="filter-label">{$t('jobs.format')}</span>
                <Checkbox label={$t('jobs.formatRemote')} bind:checked={formatRemote} />
                <Checkbox label={$t('jobs.formatHybrid')} bind:checked={formatHybrid} />
                <Checkbox label={$t('jobs.formatOffice')} bind:checked={formatOffice} />
            </div>

            <div class="filter-group">
                <RangeSlider
                    label={$t('jobs.salary')}
                    min={0}
                    max={1500000}
                    step={10000}
                    bind:valueMin={salaryMin}
                    bind:valueMax={salaryMax}
                    formatValue={(v) =>
                        v === 0
                            ? '0'
                            : v >= 1000000
                              ? `${(v / 1000000).toFixed(1)}м`
                              : `${(v / 1000).toFixed(0)}к`}
                />
            </div>

            <div class="filter-group">
                <Input
                    label={$t('jobs.city')}
                    placeholder={$t('jobs.cityPlaceholder')}
                    bind:value={city}
                />
            </div>
        </aside>

        <div class="jobs-content" class:split-active={splitView}>
            {#if loading}
                <div class="jobs-grid" class:list={viewMode === 'list'}>
                    {#each Array(6) as _, i (i)}
                        <JobCardSkeleton mode={viewMode} />
                    {/each}
                </div>
            {:else if displayJobs.length === 0}
                <div class="jobs-empty">
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
                        <circle cx="11" cy="11" r="8" /><path d="m21 21-4.3-4.3" />
                    </svg>
                    <p>{$t('jobs.notFound')}</p>
                    <Button variant="ghost" onclick={resetFilters}>{$t('jobs.resetFilters')}</Button
                    >
                </div>
            {:else}
                <div class="jobs-grid" class:list={viewMode === 'list'}>
                    {#each displayJobs as job, i (job.id)}
                        <div
                            class="stagger-item job-card-wrap"
                            style="animation-delay: {Math.min(i * 50, 500)}ms"
                        >
                            <JobCard {job} mode={viewMode} />
                        </div>
                    {/each}
                </div>
                <div class="jobs-pagination">
                    <Pagination bind:page {totalPages} onchange={loadJobs} />
                </div>
            {/if}

            {#if splitView}
                <div class="split-map">
                    <MapView
                        markers={mapMarkers}
                        center={[55.751, 37.618]}
                        zoom={5}
                        height="100%"
                    />
                </div>
            {/if}
        </div>
    </div>
</div>

<ComparisonBar />

<style>
    .jobs-page {
        padding: var(--space-8) var(--space-8);
        min-height: calc(100dvh - var(--header-height));
    }

    .jobs-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--space-6);
        flex-wrap: wrap;
        gap: var(--space-4);
    }

    .jobs-title-row {
        display: flex;
        align-items: baseline;
        gap: var(--space-3);
    }

    .jobs-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .jobs-title-row :global(.badge) {
        position: relative;
        top: -0.1em;
    }

    .jobs-controls {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .jobs-controls :global(.select-group) {
        font-size: var(--font-xs);
    }

    .jobs-controls :global(.select-trigger) {
        height: 2.25rem;
        font-size: var(--font-xs);
        padding: 0 0.75rem;
        min-width: 14rem;
    }

    .jobs-controls :global(.option) {
        font-size: var(--font-xs);
    }

    .filter-toggle {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        cursor: pointer;
    }

    .filter-toggle:hover {
        color: var(--text-primary);
        border-color: var(--border-hover);
    }

    .jobs-layout {
        display: flex;
        gap: var(--space-6);
    }

    .filters-panel {
        width: 16rem;
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
        overflow: hidden;
        opacity: 1;
        transform: translateX(0);
        transition:
            opacity var(--duration-moderate) var(--ease-out),
            transform var(--duration-moderate) var(--ease-out),
            width var(--duration-moderate) var(--ease-out),
            padding var(--duration-moderate) var(--ease-out),
            margin var(--duration-moderate) var(--ease-out),
            border-width var(--duration-moderate) var(--ease-out);
    }

    .filters-panel.collapsed {
        width: 0;
        padding: 0;
        border-width: 0;
        opacity: 0;
        transform: translateX(-1rem);
        pointer-events: none;
    }

    .filters-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .filters-title {
        font-size: var(--font-base);
        font-weight: var(--weight-semibold);
    }

    .filters-reset {
        font-size: var(--font-xs);
        color: var(--accent);
        cursor: pointer;
    }

    .filters-reset:hover {
        text-decoration: underline;
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
        margin-bottom: var(--space-1);
    }

    .jobs-content {
        flex: 1;
        min-width: 0;
    }

    .jobs-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(18rem, 1fr));
        gap: var(--space-4);
    }

    .jobs-grid.list {
        grid-template-columns: 1fr;
        gap: var(--space-3);
    }

    .jobs-pagination {
        display: flex;
        justify-content: center;
        margin-top: var(--space-8);
    }

    .jobs-empty {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: var(--space-4);
        padding: var(--space-16);
        color: var(--text-tertiary);
        text-align: center;
    }

    .jobs-empty p {
        font-size: var(--font-md);
    }

    @media (max-width: 768px) {
        .filters-panel {
            position: fixed;
            top: var(--header-height);
            left: 0;
            right: 0;
            bottom: 0;
            width: 100%;
            border-radius: 0;
            z-index: var(--z-overlay);
            overflow-y: auto;
        }

        .jobs-grid {
            grid-template-columns: 1fr;
        }
    }

    .job-card-wrap {
        position: relative;
    }

    .split-toggle {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.25rem;
        height: 2.25rem;
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        color: var(--text-tertiary);
        transition: var(--transition-colors);
        flex-shrink: 0;
    }

    .split-toggle:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }
    .split-toggle.active {
        background: var(--accent-subtle);
        color: var(--accent);
        border-color: var(--accent);
    }

    .jobs-content.split-active {
        display: grid;
        grid-template-columns: 1fr 1fr;
        grid-template-rows: 1fr auto;
        gap: var(--space-4);
    }

    .jobs-content.split-active .jobs-grid {
        grid-column: 1;
        max-height: calc(100dvh - var(--header-height) - 10rem);
        overflow-y: auto;
        align-content: start;
    }

    .jobs-content.split-active .jobs-pagination {
        grid-column: 1;
    }

    .split-map {
        grid-column: 2;
        grid-row: 1 / -1;
        border-radius: var(--radius-lg);
        overflow: hidden;
        min-height: 24rem;
        position: sticky;
        top: var(--space-4);
    }

    @media (max-width: 1024px) {
        .jobs-content.split-active {
            grid-template-columns: 1fr;
        }

        .split-map {
            grid-column: 1;
            grid-row: auto;
            position: static;
            height: 20rem;
        }
    }
</style>
