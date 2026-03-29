<script lang="ts">
    import Badge from './Badge.svelte';
    import Tag from './Tag.svelte';
    import Avatar from './Avatar.svelte';
    import { formatSalary, timeAgo, jobTypeLabel, workFormatLabel } from '$lib/utils/format';
    import { toast } from '$lib/stores/toast';
    import { comparison } from '$lib/stores/comparison';
    import { favorites } from '$lib/stores/favorites';
    import { isAuthenticated } from '$lib/stores/auth';
    import { favoritesApi } from '$lib/api/favorites';
    import { onDestroy } from 'svelte';
    import { t } from '$lib/i18n';
    import type { JobResponse } from '$lib/api/jobs';

    interface Props {
        job: JobResponse;
        mode?: 'grid' | 'list';
    }

    let { job, mode = 'grid' }: Props = $props();

    let isAuth = $state(false);
    const unsubAuth = isAuthenticated.subscribe((v) => (isAuth = v));
    onDestroy(unsubAuth);

    let favJobIds = $state<string[]>([]);
    const unsubFavs = favorites.subscribe((s) => (favJobIds = s.jobs));
    onDestroy(unsubFavs);

    let isFav = $derived(favJobIds.includes(job.id));

    function handleFavorite(e: Event) {
        e.preventDefault();
        e.stopPropagation();
        favorites.toggleJob(job.id);
        toast.success(
            favorites.isJobFavorite(job.id)
                ? $t('job.addToFavorites')
                : $t('job.removeFromFavorites')
        );
        if (isAuth) {
            favoritesApi.toggle(job.id, 'Job').catch(() => {
                favorites.toggleJob(job.id);
                toast.error($t('common.error'));
            });
        }
    }

    function handleCompare(e: Event) {
        e.preventDefault();
        e.stopPropagation();
        comparison.add({
            id: job.id,
            title: job.title,
            company: job.companyName || job.city,
            salary: formatSalary(job.salaryFrom, job.salaryTo),
            format: job.format,
            type: job.type,
            city: job.city,
            tags: job.tags?.map((t) => (typeof t === 'string' ? t : t.name))
        });
    }

    let typeBadge = $derived(
        job.type === 'Internship'
            ? ('info' as const)
            : job.type === 'Mentorship'
              ? ('warning' as const)
              : job.type === 'Event'
                ? ('accent' as const)
                : ('default' as const)
    );

    let formatBadge = $derived(
        job.format === 'Remote'
            ? ('success' as const)
            : job.format === 'Office'
              ? ('warning' as const)
              : ('default' as const)
    );
</script>

<a href="/jobs/{job.id}" class="job-card job-card--{mode}">
    {#if mode === 'grid'}
        <div class="card-top">
            <Avatar name={job.title} size={40} />
            <div class="card-badges">
                <Badge variant={typeBadge}>{jobTypeLabel(job.type)}</Badge>
                <Badge variant={formatBadge}>{workFormatLabel(job.format)}</Badge>
            </div>
        </div>
        <h3 class="card-title">{job.title}</h3>
        <p class="card-company">{job.city}, {job.country}</p>
        <p class="card-salary">{formatSalary(job.salaryFrom, job.salaryTo)}</p>
        <div class="card-footer">
            <div class="card-footer-left">
                {#if job.tags?.length}
                    <div class="card-tags">
                        {#each job.tags.slice(0, 4) as tag (tag.name)}
                            <Tag>{tag?.name}</Tag>
                        {/each}
                        {#if job.tags.length > 4}
                            <Tag>+{job.tags.length - 4}</Tag>
                        {/if}
                    </div>
                {/if}
                <span class="card-date">{timeAgo(job.createdAt)}</span>
            </div>
            <div class="card-actions">
                <button
                    class="card-action-btn"
                    type="button"
                    title={$t('comparison.add')}
                    onclick={handleCompare}
                >
                    <svg
                        viewBox="0 0 24 24"
                        width="16"
                        height="16"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        ><line x1="18" y1="20" x2="18" y2="10" /><line
                            x1="12"
                            y1="20"
                            x2="12"
                            y2="4"
                        /><line x1="6" y1="20" x2="6" y2="14" /></svg
                    >
                </button>
                <button
                    class="card-action-btn"
                    class:fav-active={isFav}
                    type="button"
                    title={isFav ? $t('job.removeFromFavorites') : $t('job.addToFavorites')}
                    onclick={handleFavorite}
                >
                    <svg
                        viewBox="0 0 24 24"
                        width="16"
                        height="16"
                        fill={isFav ? 'currentColor' : 'none'}
                        stroke="currentColor"
                        stroke-width="2"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                    >
                        <path
                            d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"
                        />
                    </svg>
                </button>
            </div>
        </div>
    {:else}
        <div class="list-left">
            <Avatar name={job.title} size={44} />
        </div>
        <div class="list-main">
            <div class="list-top-row">
                <h3 class="card-title">{job.title}</h3>
                <div class="card-badges">
                    <Badge variant={typeBadge} size="sm">{jobTypeLabel(job.type)}</Badge>
                    <Badge variant={formatBadge} size="sm">{workFormatLabel(job.format)}</Badge>
                </div>
            </div>
            <p class="card-company">{job.city}, {job.country}</p>
            {#if job.tags?.length}
                <div class="card-tags">
                    {#each job.tags.slice(0, 5) as tag (tag.name)}
                        <Tag>{tag?.name}</Tag>
                    {/each}
                </div>
            {/if}
        </div>
        <div class="list-right">
            <span class="card-salary">{formatSalary(job.salaryFrom, job.salaryTo)}</span>
            <span class="card-date">{timeAgo(job.createdAt)}</span>
            {#if job.views > 0}
                <span class="card-responses">{job.views} {$t('job.views')}</span>
            {/if}
        </div>
        <div class="list-actions">
            <button
                class="card-action-btn"
                type="button"
                title={$t('comparison.add')}
                onclick={handleCompare}
            >
                <svg
                    viewBox="0 0 24 24"
                    width="16"
                    height="16"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    ><line x1="18" y1="20" x2="18" y2="10" /><line
                        x1="12"
                        y1="20"
                        x2="12"
                        y2="4"
                    /><line x1="6" y1="20" x2="6" y2="14" /></svg
                >
            </button>
            <button
                class="card-action-btn"
                class:fav-active={isFav}
                type="button"
                title={isFav ? $t('job.removeFromFavorites') : $t('job.addToFavorites')}
                onclick={handleFavorite}
            >
                <svg
                    viewBox="0 0 24 24"
                    width="16"
                    height="16"
                    fill={isFav ? 'currentColor' : 'none'}
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path
                        d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"
                    />
                </svg>
            </button>
        </div>
    {/if}
</a>

<style>
    .job-card {
        display: flex;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors), var(--transition-transform), var(--transition-shadow);
        text-decoration: none;
        color: inherit;
    }

    .job-card:hover {
        border-color: var(--border-hover);
        box-shadow: var(--shadow-md);
        color: inherit;
    }

    .job-card--grid {
        flex-direction: column;
        gap: var(--space-3);
        padding: var(--space-5);
        height: 100%;
    }

    .job-card--grid:hover {
        transform: translateY(-0.125rem);
    }

    .job-card--list {
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
    }

    .card-top {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
    }

    .card-badges {
        display: flex;
        gap: var(--space-1);
        flex-wrap: wrap;
    }

    .card-title {
        font-size: var(--font-base);
        font-weight: var(--weight-semibold);
        line-height: var(--leading-snug);
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .card-company {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .card-salary {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--accent);
    }

    .card-tags {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-1);
    }

    .card-date {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .card-footer {
        display: flex;
        align-items: flex-end;
        justify-content: space-between;
        gap: var(--space-2);
        margin-top: auto;
    }

    .card-footer-left {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        min-width: 0;
    }

    .card-actions {
        display: flex;
        align-items: center;
        gap: var(--space-1);
        flex-shrink: 0;
    }

    .card-action-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 1.75rem;
        height: 1.75rem;
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        color: var(--text-tertiary);
        opacity: 0;
        transition:
            opacity var(--duration-fast) var(--ease-out),
            var(--transition-colors);
        cursor: pointer;
    }

    .job-card:hover .card-action-btn {
        opacity: 1;
    }

    .card-action-btn:hover {
        color: var(--accent);
        border-color: var(--accent);
    }

    .card-action-btn.fav-active {
        opacity: 1;
        color: var(--color-error);
        border-color: var(--color-error);
        background: var(--color-error-subtle);
    }

    .card-responses {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .list-left {
        flex-shrink: 0;
    }

    .list-main {
        flex: 1;
        min-width: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .list-top-row {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        flex-wrap: wrap;
    }

    .list-right {
        display: flex;
        flex-direction: column;
        align-items: flex-end;
        gap: var(--space-1);
        flex-shrink: 0;
    }

    .list-actions {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-1);
        flex-shrink: 0;
        margin-left: var(--space-2);
    }

    @media (max-width: 640px) {
        .job-card--list {
            flex-direction: column;
            align-items: stretch;
        }

        .list-left {
            display: none;
        }

        .list-right {
            flex-direction: row;
            align-items: center;
            gap: var(--space-3);
        }
    }
</style>
