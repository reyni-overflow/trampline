<script lang="ts">
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import { user as userStore } from '$lib/stores/auth';
    import { jobsApi, type JobResponse, type TagStatsResponse } from '$lib/api/jobs';
    import { eventsApi, type EventResponse } from '$lib/api/events';
    import { onMount, onDestroy } from 'svelte';
    import { t, getLocaleDateString } from '$lib/i18n';
    let userId = $state('');
    let loading = $state(true);

    const unsub = userStore.subscribe((v) => {
        if (v) userId = v.id;
    });
    onDestroy(unsub);

    let totalViews = $state(0);
    let totalResponses = $state(0);
    let pendingResponses = $state(0);
    let activeJobsCount = $state(0);

    let jobs = $state<JobResponse[]>([]);
    let events = $state<EventResponse[]>([]);
    let tagStats = $state<TagStatsResponse[]>([]);
    let jobResponses = $state<Record<string, number>>({});

    onMount(async () => {
        if (!userId) return;
        try {
            const [jobsList, eventsList, responseStats, tags] = await Promise.all([
                jobsApi.getByUser(userId, 1, 50).catch(() => []),
                eventsApi.getByUser(userId, 1, 50).catch(() => []),
                jobsApi
                    .getMyResponseStats()
                    .catch(() => ({ totalResponses: 0, pendingResponses: 0 })),
                jobsApi.getTagStats().catch(() => [])
            ]);

            jobs = jobsList;
            events = eventsList;
            tagStats = tags.sort((a, b) => b.totalCount - a.totalCount).slice(0, 15);

            totalViews =
                jobsList.reduce((s, j) => s + (j.views || 0), 0) +
                eventsList.reduce((s, e) => s + (e.views || 0), 0);
            totalResponses = responseStats.totalResponses;
            pendingResponses = responseStats.pendingResponses;
            activeJobsCount = jobsList.filter((j) => j.isActive).length;

            const responseCounts: Record<string, number> = {};
            await Promise.all(
                jobsList.map(async (job) => {
                    try {
                        const resps = await jobsApi.getResponses(job.id);
                        responseCounts[job.id] = resps.length;
                    } catch {
                        responseCounts[job.id] = 0;
                    }
                })
            );
            jobResponses = responseCounts;
        } catch {
            /* ignored */
        }
        loading = false;
    });

    let maxResponses = $derived(Math.max(1, ...Object.values(jobResponses)));

    let stats = $derived([
        {
            label: $t('dashStats.views'),
            value: totalViews,
            icon: '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>'
        },
        {
            label: $t('dashStats.totalResponses'),
            value: totalResponses,
            icon: '<path d="m22 2-7 20-4-9-9-4Z"/>'
        },
        {
            label: $t('dashStats.pendingResponses'),
            value: pendingResponses,
            icon: '<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>'
        },
        {
            label: $t('dashStats.activeJobs'),
            value: activeJobsCount,
            icon: '<path d="M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/><rect width="20" height="14" x="2" y="6" rx="2"/>'
        }
    ]);
</script>

<svelte:head>
    <title>{$t('dashStats.pageTitle')}</title>
</svelte:head>

<div class="analytics">
    <h1 class="page-heading">{$t('dashStats.title')}</h1>

    {#if loading}
        <div class="stats-row">
            {#each Array(4) as _, i (i)}
                <div class="stat-card">
                    <Skeleton width="2.5rem" height="2.5rem" radius="var(--radius-md)" />
                    <div class="stat-info">
                        <Skeleton width="2rem" height="1.25rem" />
                        <Skeleton width="4rem" height="0.75rem" />
                    </div>
                </div>
            {/each}
        </div>
        <div class="section-card">
            <Skeleton width="12rem" height="1.25rem" />
            <div style="margin-top: 1rem; display: flex; flex-direction: column; gap: 0.75rem;">
                {#each Array(3) as _, i (i)}
                    <Skeleton width="100%" height="2rem" />
                {/each}
            </div>
        </div>
    {:else}
        <div class="stats-row content-fade-in">
            {#each stats as stat (stat.label)}
                <div class="stat-card">
                    <span class="stat-icon">
                        <svg
                            viewBox="0 0 24 24"
                            width="20"
                            height="20"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="1.75"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                        >
                            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                            {@html stat.icon}
                        </svg>
                    </span>
                    <div class="stat-info">
                        <span class="stat-value">{stat.value}</span>
                        <span class="stat-label">{stat.label}</span>
                    </div>
                </div>
            {/each}
        </div>

        {#if jobs.length > 0}
            <section class="section-card content-fade-in">
                <h2 class="section-title">{$t('dashStats.responsesByJob')}</h2>
                <div class="chart">
                    {#each jobs as job (job.id)}
                        {@const count = jobResponses[job.id] || 0}
                        <div class="chart-row">
                            <span class="chart-label" title={job.title}>
                                {job.title.length > 30 ? job.title.slice(0, 30) + '...' : job.title}
                            </span>
                            <div class="chart-bar-track">
                                <div
                                    class="chart-bar-fill"
                                    style="width: {(count / maxResponses) * 100}%"
                                ></div>
                            </div>
                            <span class="chart-count">{count}</span>
                        </div>
                    {/each}
                </div>
            </section>
        {/if}

        {#if tagStats.length > 0}
            <section class="section-card content-fade-in">
                <h2 class="section-title">{$t('dashStats.popularTags')}</h2>
                <div class="tags-grid">
                    {#each tagStats as tag (tag.id)}
                        <div class="tag-stat">
                            <span class="tag-name">{tag.name}</span>
                            <div class="tag-counts">
                                {#if tag.jobCount > 0}
                                    <Badge variant="accent"
                                        >{tag.jobCount} {$t('dashStats.jobsLabel')}</Badge
                                    >
                                {/if}
                                {#if tag.eventCount > 0}
                                    <Badge variant="info"
                                        >{tag.eventCount} {$t('dashStats.eventsLabel')}</Badge
                                    >
                                {/if}
                            </div>
                        </div>
                    {/each}
                </div>
            </section>
        {/if}

        {#if jobs.length > 0}
            <section class="section-card content-fade-in">
                <h2 class="section-title">{$t('dashStats.jobsViewsResponses')}</h2>
                <div class="table-wrap">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>{$t('dashStats.colJob')}</th>
                                <th>{$t('dashStats.colStatus')}</th>
                                <th>{$t('dashStats.colViews')}</th>
                                <th>{$t('dashStats.colResponses')}</th>
                                <th>{$t('dashStats.colCreated')}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {#each jobs as job (job.id)}
                                <tr>
                                    <td class="cell-title">{job.title}</td>
                                    <td>
                                        {#if job.isActive}
                                            <Badge variant="success">{$t('dashStats.active')}</Badge
                                            >
                                        {:else}
                                            <Badge variant="warning"
                                                >{$t('dashStats.inactive')}</Badge
                                            >
                                        {/if}
                                    </td>
                                    <td>{job.views || 0}</td>
                                    <td>{jobResponses[job.id] || 0}</td>
                                    <td class="cell-date">
                                        {new Date(job.createdAt).toLocaleDateString(
                                            getLocaleDateString(),
                                            {
                                                day: 'numeric',
                                                month: 'short',
                                                year: 'numeric'
                                            }
                                        )}
                                    </td>
                                </tr>
                            {/each}
                        </tbody>
                    </table>
                </div>
            </section>
        {/if}

        {#if jobs.length === 0 && events.length === 0}
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
                    <path d="M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16" />
                    <rect width="20" height="14" x="2" y="6" rx="2" />
                </svg>
                <p>{$t('dashStats.emptyState')}</p>
            </div>
        {/if}
    {/if}
</div>

<style>
    .analytics {
        display: flex;
        flex-direction: column;
        gap: var(--space-8);
    }

    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .stats-row {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: var(--space-4);
    }

    .stat-card {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .stat-icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        background: var(--accent-subtle);
        color: var(--accent);
        border-radius: var(--radius-md);
        flex-shrink: 0;
    }

    .stat-info {
        display: flex;
        flex-direction: column;
    }

    .stat-value {
        font-size: var(--font-xl);
        font-weight: var(--weight-bold);
        line-height: 1;
    }

    .stat-label {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .section-card {
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        padding: var(--space-5) var(--space-6);
    }

    .section-title {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
        margin-bottom: var(--space-5);
    }

    .chart {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .chart-row {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .chart-label {
        width: 12rem;
        flex-shrink: 0;
        font-size: var(--font-sm);
        color: var(--text-secondary);
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .chart-bar-track {
        flex: 1;
        height: 1.25rem;
        background: var(--bg-tertiary);
        border-radius: var(--radius-sm);
        overflow: hidden;
    }

    .chart-bar-fill {
        height: 100%;
        background: var(--accent);
        border-radius: var(--radius-sm);
        min-width: 2px;
        transition: width var(--duration-moderate) var(--ease-out);
    }

    .chart-count {
        width: 2.5rem;
        text-align: right;
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        flex-shrink: 0;
    }

    .tags-grid {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .tag-stat {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-2) var(--space-3);
        border-radius: var(--radius-md);
        background: var(--bg-primary);
    }

    .tag-name {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
    }

    .tag-counts {
        display: flex;
        gap: var(--space-2);
    }

    .table-wrap {
        overflow-x: auto;
    }

    .data-table {
        width: 100%;
        border-collapse: collapse;
        font-size: var(--font-sm);
    }

    .data-table th {
        text-align: left;
        padding: var(--space-2) var(--space-3);
        font-weight: var(--weight-semibold);
        color: var(--text-secondary);
        border-bottom: 1px solid var(--border-default);
        white-space: nowrap;
    }

    .data-table td {
        padding: var(--space-3);
        border-bottom: 1px solid var(--border-subtle);
        vertical-align: middle;
    }

    .data-table tbody tr:hover {
        background: var(--bg-tertiary);
    }

    .cell-title {
        font-weight: var(--weight-medium);
        max-width: 16rem;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .cell-date {
        color: var(--text-tertiary);
        white-space: nowrap;
    }

    .empty-state {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-12) var(--space-6);
        color: var(--text-tertiary);
        text-align: center;
    }

    .empty-state p {
        font-size: var(--font-sm);
        max-width: 20rem;
    }

    @media (max-width: 768px) {
        .stats-row {
            grid-template-columns: repeat(2, 1fr);
        }

        .chart-label {
            width: 6rem;
            font-size: var(--font-xs);
        }

        .data-table {
            font-size: var(--font-xs);
        }

        .cell-title {
            max-width: 8rem;
        }
    }
</style>
