<script lang="ts">
    import Tabs from '$lib/components/ui/Tabs.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import { toast } from '$lib/stores/toast';
    import { formatSalary, timeAgo } from '$lib/utils/format';
    import { jobsApi, type JobResponse } from '$lib/api/jobs';
    import { handleApiError } from '$lib/api/client';
    import { user as userStore } from '$lib/stores/auth';
    import { t } from '$lib/i18n';
    import { onMount, onDestroy } from 'svelte';

    let activeTab = $state('active');
    let search = $state('');
    let loading = $state(true);

    let tabs = $derived([
        { id: 'active', label: $t('dashJobs.active') },
        { id: 'closed', label: $t('dashJobs.closed') },
        { id: 'draft', label: $t('dashJobs.drafts') }
    ]);

    type JobItem = {
        id: string;
        title: string;
        type: string;
        format: string;
        city: string;
        salaryFrom: number | null;
        salaryTo: number | null;
        views: number;
        createdAt: string;
        status: string;
        isActive: boolean;
    };
    let jobs = $state<JobItem[]>([]);

    let unsubUser: (() => void) | undefined;
    onMount(() => {
        unsubUser = userStore.subscribe((v) => {
            if (v?.id) loadJobs(v.id);
        });
    });
    onDestroy(() => unsubUser?.());

    async function loadJobs(userId: string) {
        loading = true;
        try {
            const data = await jobsApi.getByUser(userId, 1, 50);
            jobs = data.map((j: JobResponse) => ({
                id: j.id,
                title: j.title,
                type: j.type,
                format: j.format,
                city: j.city,
                salaryFrom: j.salaryFrom,
                salaryTo: j.salaryTo,
                views: j.views,
                createdAt: j.createdAt,
                isActive: j.isActive,
                status: j.isActive ? 'active' : j.deletedAt ? 'closed' : 'draft'
            }));
        } catch {
            jobs = [];
        } finally {
            loading = false;
        }
    }

    let filtered = $derived.by(() => {
        let list = jobs.filter((j) => j.status === activeTab);
        if (search) {
            const q = search.toLowerCase();
            list = list.filter((j) => j.title.toLowerCase().includes(q));
        }
        return list;
    });

    async function toggleStatus(id: string) {
        const job = jobs.find((j) => j.id === id);
        if (!job) return;
        try {
            await jobsApi.update(id, { isActive: !job.isActive });
            job.isActive = !job.isActive;
            job.status = job.isActive ? 'active' : 'closed';
            jobs = [...jobs];
            toast.success($t('dashJobs.statusChanged'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteJob(id: string) {
        try {
            await jobsApi.delete(id);
            jobs = jobs.filter((j) => j.id !== id);
            toast.success($t('dashJobs.jobDeleted'));
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('dashJobs.pageTitle')}</title>
</svelte:head>

<div class="my-jobs">
    <div class="page-header">
        <h1 class="page-heading">{$t('dashJobs.title')}</h1>
        <Button href="/dashboard/jobs/create">{$t('dashOverview.createJob')}</Button>
    </div>

    <Tabs {tabs} bind:active={activeTab} />

    <div class="search-row">
        <SearchInput placeholder={$t('dashJobs.searchPlaceholder')} bind:value={search} />
    </div>

    {#if loading}
        <div class="jobs-list">
            {#each Array(4) as _, i (i)}
                <div class="job-row">
                    <div class="job-info">
                        <div class="job-title-row">
                            <Skeleton width="50%" height="0.875rem" />
                            <Skeleton width="3.5rem" height="1.25rem" radius="var(--radius-full)" />
                        </div>
                        <div class="job-meta">
                            <Skeleton width="3rem" height="0.75rem" />
                            <Skeleton width="4rem" height="0.75rem" />
                            <Skeleton width="5rem" height="0.75rem" />
                            <Skeleton width="3.5rem" height="0.75rem" />
                        </div>
                    </div>
                    <div class="job-stats">
                        <Skeleton width="4rem" height="1.25rem" radius="var(--radius-full)" />
                    </div>
                    <div class="job-actions">
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                        <Skeleton width="1.75rem" height="1.75rem" radius="var(--radius-md)" />
                    </div>
                </div>
            {/each}
        </div>
    {:else if filtered.length === 0}
        <div class="empty">
            <svg
                width="48"
                height="48"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
                ><rect x="2" y="7" width="20" height="14" rx="2" ry="2" /><path
                    d="M16 7V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v2"
                /><line x1="12" y1="12" x2="12" y2="12.01" /></svg
            >
            <p>
                {activeTab === 'active'
                    ? $t('dashJobs.noActive')
                    : activeTab === 'closed'
                      ? $t('dashJobs.noClosed')
                      : $t('dashJobs.noDrafts')}
            </p>
            <p>
                {activeTab === 'active'
                    ? $t('dashJobs.noActiveHint')
                    : activeTab === 'closed'
                      ? $t('dashJobs.noClosedHint')
                      : $t('dashJobs.noDraftsHint')}
            </p>
            {#if activeTab === 'active'}
                <Button href="/dashboard/jobs/create">{$t('dashJobs.createFirst')}</Button>
            {/if}
        </div>
    {:else}
        <div class="jobs-list">
            {#each filtered as job (job.id)}
                <div class="job-row">
                    <div class="job-info">
                        <div class="job-title-row">
                            <a href="/jobs/{job.id}" class="job-title">{job.title}</a>
                            <Badge
                                variant={job.type === 'Internship' ? 'info' : 'default'}
                                size="sm"
                                >{job.type === 'Internship'
                                    ? $t('dashJobs.internship')
                                    : $t('dashJobs.vacancy')}</Badge
                            >
                        </div>
                        <div class="job-meta">
                            <span>{job.city}</span>
                            <span>&middot;</span>
                            <span
                                >{job.format === 'Remote'
                                    ? $t('dashJobs.remote')
                                    : job.format === 'Hybrid'
                                      ? $t('dashJobs.hybrid')
                                      : $t('dashJobs.office')}</span
                            >
                            <span>&middot;</span>
                            <span>{formatSalary(job.salaryFrom, job.salaryTo)}</span>
                            <span>&middot;</span>
                            <span>{timeAgo(job.createdAt)}</span>
                        </div>
                    </div>
                    <div class="job-stats">
                        <a href="/dashboard/jobs/{job.id}/responses" class="responses-link">
                            <Badge variant="accent">{job.views} {$t('dashJobs.responses')}</Badge>
                        </a>
                    </div>
                    <div class="job-actions">
                        <Button size="sm" variant="ghost" href="/dashboard/jobs/{job.id}/edit">
                            <svg
                                viewBox="0 0 24 24"
                                width="14"
                                height="14"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                ><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z" /></svg
                            >
                        </Button>
                        <Button size="sm" variant="ghost" onclick={() => toggleStatus(job.id)}>
                            {#if job.status === 'active'}
                                <svg
                                    viewBox="0 0 24 24"
                                    width="14"
                                    height="14"
                                    fill="none"
                                    stroke="currentColor"
                                    stroke-width="2"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                    ><rect width="18" height="18" x="3" y="3" rx="2" /><line
                                        x1="9"
                                        y1="9"
                                        x2="15"
                                        y2="15"
                                    /><line x1="15" y1="9" x2="9" y2="15" /></svg
                                >
                            {:else}
                                <svg
                                    viewBox="0 0 24 24"
                                    width="14"
                                    height="14"
                                    fill="none"
                                    stroke="currentColor"
                                    stroke-width="2"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                    ><polyline points="20 6 9 17 4 12" /></svg
                                >
                            {/if}
                        </Button>
                        <Button size="sm" variant="ghost" onclick={() => deleteJob(job.id)}>
                            <svg
                                viewBox="0 0 24 24"
                                width="14"
                                height="14"
                                fill="none"
                                stroke="var(--color-error)"
                                stroke-width="2"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                ><path d="M3 6h18" /><path
                                    d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"
                                /><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2" /></svg
                            >
                        </Button>
                    </div>
                </div>
            {/each}
        </div>
    {/if}
</div>

<style>
    .my-jobs {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }
    .page-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        flex-wrap: wrap;
        gap: var(--space-4);
    }
    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }
    .search-row {
        max-width: 20rem;
        margin-top: var(--space-2);
    }

    .jobs-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .job-row {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors);
    }
    .job-row:hover {
        border-color: var(--border-hover);
    }

    .job-info {
        flex: 1;
        min-width: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }
    .job-title-row {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        flex-wrap: wrap;
    }
    .job-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        transition: var(--transition-colors);
    }
    .job-title:hover {
        color: var(--accent);
    }
    .job-meta {
        display: flex;
        gap: var(--space-2);
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        flex-wrap: wrap;
    }

    .job-stats {
        flex-shrink: 0;
    }
    .responses-link {
        text-decoration: none;
    }

    .job-actions {
        display: flex;
        gap: var(--space-1);
        flex-shrink: 0;
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
    .empty svg {
        opacity: 0.5;
    }
    .empty p {
        max-width: 20rem;
    }

    @media (max-width: 640px) {
        .search-row {
            max-width: 100%;
        }
        .job-row {
            flex-direction: column;
            align-items: stretch;
        }
        .job-actions {
            justify-content: flex-end;
        }
    }
</style>
