<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Tabs from '$lib/components/ui/Tabs.svelte';
    import Pagination from '$lib/components/ui/Pagination.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import { workersApi } from '$lib/api/workers';
    import { handleApiError } from '$lib/api/client';
    import { notifications } from '$lib/stores/notifications';
    import { onMount } from 'svelte';
    import { t, getLocaleDateString } from '$lib/i18n';

    const APP_STATUSES_KEY = 'trampline-app-statuses';

    let activeTab = $state('jobs');
    let statusFilter = $state('');
    let page = $state(1);
    let loading = $state(true);

    let appTabs = $derived([
        { id: 'jobs', label: $t('dashApps.jobApplications') },
        { id: 'events', label: $t('dashApps.eventApplications') },
        { id: 'mentorships', label: $t('dashApps.mentorshipApplications') }
    ]);

    interface AppItem {
        id: string;
        jobTitle: string;
        company: string;
        companyId: string;
        jobId: string;
        date: string;
        status: string;
        kind: 'job' | 'event' | 'mentorship';
    }

    let applications = $state<AppItem[]>([]);
    let eventApplications = $state<AppItem[]>([]);
    let mentorshipApplications = $state<AppItem[]>([]);

    const STATUS_MAP: Record<string, string> = {
        Pending: 'pending', Viewed: 'pending', Rejected: 'rejected',
        Invited: 'accepted', InProgress: 'accepted', Hired: 'accepted', Withdrawn: 'rejected'
    };

    function checkStatusChanges(apps: AppItem[]) {
        try {
            const stored = localStorage.getItem(APP_STATUSES_KEY);
            const prev: Record<string, string> = stored ? JSON.parse(stored) : {};
            for (const app of apps) {
                const old = prev[app.id];
                if (old && old !== app.status) {
                    const link = app.kind === 'event' ? `/events/${app.jobId}` : app.kind === 'mentorship' ? `/mentorships/${app.jobId}` : `/jobs/${app.jobId}`;
                    notifications.add({
                        type: 'application_status',
                        title: app.jobTitle,
                        message: `${app.jobTitle}: ${old} → ${app.status}`,
                        link
                    });
                }
            }
            const next: Record<string, string> = {};
            for (const app of apps) next[app.id] = app.status;
            localStorage.setItem(APP_STATUSES_KEY, JSON.stringify(next));
        } catch { /* ignored */ }
    }

    onMount(async () => {
        try {
            const [jobData, eventData, mentorshipData] = await Promise.all([
                workersApi.getApplications(),
                workersApi.getEventApplications().catch(() => []),
                workersApi.getMentorshipApplications().catch(() => [])
            ]);
            applications = jobData.map(a => ({
                id: a.id,
                jobTitle: a.job?.title ?? '—',
                company: a.job?.city ?? '',
                companyId: '',
                jobId: a.jobId,
                date: a.createdAt?.split('T')[0] ?? '',
                status: STATUS_MAP[a.status] ?? 'pending',
                kind: 'job' as const
            }));
            eventApplications = eventData.map(a => ({
                id: a.id,
                jobTitle: a.event?.title ?? '—',
                company: a.event?.city ?? '',
                companyId: '',
                jobId: a.eventId,
                date: a.createdAt?.split('T')[0] ?? '',
                status: STATUS_MAP[a.status] ?? 'pending',
                kind: 'event' as const
            }));
            mentorshipApplications = mentorshipData.map(a => ({
                id: a.id,
                jobTitle: a.mentorship?.title ?? '—',
                company: a.mentorship?.city ?? '',
                companyId: '',
                jobId: a.mentorshipId,
                date: a.createdAt?.split('T')[0] ?? '',
                status: STATUS_MAP[a.status] ?? 'pending',
                kind: 'mentorship' as const
            }));
            checkStatusChanges([...applications, ...eventApplications, ...mentorshipApplications]);
        } catch (err) {
            handleApiError(err);
            applications = [];
            eventApplications = [];
        } finally {
            loading = false;
        }
    });

    let filterOptions = $derived([
        { value: '', label: $t('dashApps.allStatuses') },
        { value: 'pending', label: $t('dashOverview.underReview') },
        { value: 'accepted', label: $t('dashOverview.acceptedStatus') },
        { value: 'rejected', label: $t('dashOverview.rejected') },
        { value: 'reserve', label: $t('dashOverview.inReserve') }
    ]);

    let statusMap = $derived<Record<string, { label: string; variant: 'warning' | 'success' | 'error' | 'info' }>>({
        pending: { label: $t('dashOverview.underReview'), variant: 'warning' },
        accepted: { label: $t('dashOverview.acceptedStatus'), variant: 'success' },
        rejected: { label: $t('dashOverview.rejected'), variant: 'error' },
        reserve: { label: $t('dashOverview.inReserve'), variant: 'info' }
    });

    let currentList = $derived(
        activeTab === 'events' ? eventApplications :
        activeTab === 'mentorships' ? mentorshipApplications :
        applications
    );

    let filtered = $derived(
        statusFilter ? currentList.filter((a) => a.status === statusFilter) : currentList
    );
</script>

<svelte:head>
    <title>{$t('dashApps.pageTitle')}</title>
</svelte:head>

<div class="applications">
    <div class="page-header">
        <h1 class="page-heading">{$t('dashApps.title')}</h1>
        <Select options={filterOptions} bind:value={statusFilter} placeholder={$t('common.status')} />
    </div>

    <Tabs tabs={appTabs} bind:active={activeTab} />

    {#if loading}
        <div class="applications-table">
            {#each Array(4) as _, i (i)}
                <div class="table-row">
                    <span class="col-job"><Skeleton width="70%" height="0.875rem" /></span>
                    <span class="col-company"><Skeleton width="50%" height="0.875rem" /></span>
                    <span class="col-date"><Skeleton width="4rem" height="0.75rem" /></span>
                    <span class="col-status"><Skeleton width="5rem" height="1.25rem" radius="var(--radius-full)" /></span>
                </div>
            {/each}
        </div>
    {:else if filtered.length === 0}
        <div class="empty">
            {#if activeTab === 'events'}
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="4" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                <p>{$t('dashApps.emptyEventsStatus')}</p>
                <p>{$t('dashApps.emptyEventsHint')}</p>
                <Button href="/events">{$t('dashApps.browseEvents')}</Button>
            {:else}
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z"/><path d="M14 2v6h6"/><path d="m9 15 2 2 4-4"/></svg>
                <p>{$t('dashApps.emptyStatus')}</p>
                <p>{$t('dashApps.emptyHint')}</p>
                <Button href="/jobs">{$t('dashApps.browseJobs')}</Button>
            {/if}
        </div>
    {:else}
        <div class="applications-table">
            <div class="table-header">
                <span class="col-job">{activeTab === 'events' ? $t('dashApps.event') : $t('dashApps.vacancy')}</span>
                <span class="col-company">{activeTab === 'events' ? $t('dashApps.city') : $t('dashApps.company')}</span>
                <span class="col-date">{$t('dashApps.date')}</span>
                <span class="col-status">{$t('dashApps.status')}</span>
            </div>
            {#each filtered as app (app.id)}
                <div class="table-row">
                    <a href="{app.kind === 'event' ? '/events' : app.kind === 'mentorship' ? '/mentorships' : '/jobs'}/{app.jobId}" class="col-job row-link">{app.jobTitle}</a>
                    <span class="col-company">{app.company}</span>
                    <span class="col-date">{new Date(app.date).toLocaleDateString(getLocaleDateString(), { day: 'numeric', month: 'short' })}</span>
                    <span class="col-status"><Badge variant={statusMap[app.status].variant}>{statusMap[app.status].label}</Badge></span>
                </div>
            {/each}
        </div>
        <div class="pagination-row">
            <Pagination bind:page totalPages={1} />
        </div>
    {/if}
</div>

<style>
    .applications {
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

    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }

    .page-header :global(.select-trigger) { height: 2.25rem; font-size: var(--font-xs); padding: 0 0.75rem; }
    .page-header :global(.option) { font-size: var(--font-xs); }

    .applications-table {
        display: flex;
        flex-direction: column;
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        overflow: hidden;
    }

    .table-header, .table-row {
        display: grid;
        grid-template-columns: 2fr 1.5fr 6rem 8rem;
        align-items: center;
        padding: var(--space-3) var(--space-4);
        gap: var(--space-3);
    }

    .table-header {
        font-size: var(--font-xs);
        font-weight: var(--weight-semibold);
        color: var(--text-tertiary);
        text-transform: uppercase;
        letter-spacing: 0.05em;
        background: var(--bg-secondary);
        border-bottom: 1px solid var(--border-default);
    }

    .table-row {
        font-size: var(--font-sm);
        border-bottom: 1px solid var(--border-default);
        transition: var(--transition-colors);
    }

    .table-row:last-child { border-bottom: none; }
    .table-row:hover { background: var(--bg-secondary); }

    .row-link {
        color: var(--text-primary);
        font-weight: var(--weight-medium);
        transition: var(--transition-colors);
    }

    .row-link:hover { color: var(--accent); }

    .col-date { font-size: var(--font-xs); color: var(--text-tertiary); }

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

    .pagination-row { display: flex; justify-content: center; }

    @media (max-width: 640px) {
        .table-header { display: none; }

        .applications-table {
            border: none;
            overflow: visible;
        }

        .table-row {
            grid-template-columns: 1fr 1fr;
            gap: var(--space-2);
            padding: var(--space-4);
            border: 1px solid var(--border-default);
            border-radius: var(--radius-lg);
            margin-bottom: var(--space-3);
        }

        .table-row:last-child { margin-bottom: 0; }

        .col-job { grid-column: 1 / -1; font-weight: var(--weight-semibold); }
        .col-status { justify-self: end; }
    }
</style>
