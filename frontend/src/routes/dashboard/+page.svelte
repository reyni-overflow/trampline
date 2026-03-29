<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import JobCard from '$lib/components/ui/JobCard.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import { user as userStore } from '$lib/stores/auth';
    import { jobsApi, type JobResponse } from '$lib/api/jobs';
    import { eventsApi } from '$lib/api/events';
    import { workersApi } from '$lib/api/workers';
    import { favoritesApi } from '$lib/api/favorites';
    import { onMount, onDestroy } from 'svelte';
    import { t } from '$lib/i18n';

    let onboardingDismissed = $state(false);

    function dismissOnboarding() {
        onboardingDismissed = true;
        try { localStorage.setItem('onboarding-dismissed', 'true'); } catch { /* ignored */ }
    }

    let role = $state('Worker');
    let nickname = $state('');
    let userId = $state('');
    let loading = $state(true);
    const unsubUser = userStore.subscribe((v) => {
        if (v) { role = v.role; nickname = v.nickname; userId = v.id; }
    });
    onDestroy(unsubUser);

    let workerStatsValues = $state({ applications: '0', pending: '0', accepted: '0', favorites: '0' });
    let employeeStatsValues = $state({ jobs: '0', responses: '0', newOnes: '0', views: '0' });
    let recentApplications = $state<{ job: string; company: string; date: string; status: 'pending' | 'accepted' | 'rejected' | 'reserve' }[]>([]);
    let recommendedJobs = $state<JobResponse[]>([]);

    const BACKEND_STATUS: Record<string, 'pending' | 'accepted' | 'rejected' | 'reserve'> = {
        Pending: 'pending', Viewed: 'reserve', Rejected: 'rejected',
        Invited: 'accepted', InProgress: 'accepted', Hired: 'accepted', Withdrawn: 'rejected'
    };

    onMount(async () => {
        if (localStorage.getItem('onboarding-dismissed') === 'true') onboardingDismissed = true;

        try {
            if (role === 'Worker') {
                const [apps, favs, jobsData] = await Promise.all([
                    workersApi.getApplications().catch(() => []),
                    favoritesApi.getAll().catch(() => []),
                    jobsApi.getAll(1, 4).catch(() => ({ items: [] }))
                ]);
                const pending = apps.filter(a => a.status === 'Pending' || a.status === 'Viewed').length;
                const accepted = apps.filter(a => a.status === 'Invited' || a.status === 'Hired' || a.status === 'InProgress').length;
                workerStatsValues = {
                    applications: String(apps.length),
                    pending: String(pending),
                    accepted: String(accepted),
                    favorites: String(favs.length)
                };
                recentApplications = apps.slice(0, 3).map(a => ({
                    job: a.job?.title ?? '—',
                    company: a.job?.city ?? '',
                    date: a.createdAt ? new Date(a.createdAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' }) : '',
                    status: BACKEND_STATUS[a.status] ?? 'pending'
                }));
                recommendedJobs = jobsData.items?.slice(0, 3) ?? [];
            } else if (role === 'Employee' && userId) {
                const [jobs, events] = await Promise.all([
                    jobsApi.getByUser(userId, 1, 50).catch(() => []),
                    eventsApi.getByUser(userId, 1, 50).catch(() => [])
                ]);
                const totalViews = jobs.reduce((sum, j) => sum + (j.views || 0), 0);

                const responseLists = await Promise.all(
                    jobs.map(j => jobsApi.getResponses(j.id).catch(() => []))
                );
                const allResponses = responseLists.flat();
                const totalResponses = allResponses.length;
                const newResponses = allResponses.filter(r => r.status === 'Pending').length;

                employeeStatsValues = {
                    jobs: String(jobs.length + events.length),
                    responses: String(totalResponses),
                    newOnes: String(newResponses),
                    views: String(totalViews)
                };
            }
        } catch { /* ignored */ }
        loading = false;
    });

    let workerStats = $derived([
        { label: $t('dashOverview.applications'), value: workerStatsValues.applications, icon: '<path d="m22 2-7 20-4-9-9-4Z"/>' },
        { label: $t('dashOverview.pending'), value: workerStatsValues.pending, icon: '<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>' },
        { label: $t('dashOverview.accepted'), value: workerStatsValues.accepted, icon: '<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>' },
        { label: $t('dashOverview.favorites'), value: workerStatsValues.favorites, icon: '<path d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"/>' }
    ]);

    let employeeStats = $derived([
        { label: $t('dashOverview.jobs'), value: employeeStatsValues.jobs, icon: '<path d="M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/><rect width="20" height="14" x="2" y="6" rx="2"/>' },
        { label: $t('dashOverview.responses'), value: employeeStatsValues.responses, icon: '<path d="m22 2-7 20-4-9-9-4Z"/>' },
        { label: $t('dashOverview.newOnes'), value: employeeStatsValues.newOnes, icon: '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="16"/><line x1="8" y1="12" x2="16" y2="12"/>' },
        { label: $t('dashOverview.views'), value: employeeStatsValues.views, icon: '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>' }
    ]);

    let stats = $derived(role === 'Employee' ? employeeStats : workerStats);

    let statusMap = $derived({
        pending: { label: $t('dashOverview.underReview'), variant: 'warning' as const },
        accepted: { label: $t('dashOverview.acceptedStatus'), variant: 'success' as const },
        rejected: { label: $t('dashOverview.rejected'), variant: 'error' as const },
        reserve: { label: $t('dashOverview.inReserve'), variant: 'info' as const }
    });
</script>

<svelte:head>
    <title>{$t('dash.pageTitle')}</title>
</svelte:head>

<div class="overview">
    <h1 class="overview-greeting">
        {role === 'Employee' ? $t('dashOverview.companyWelcome', { name: nickname }) : $t('dashOverview.welcome', { name: nickname })}
    </h1>

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
        <section class="overview-section">
            <div class="section-header">
                <Skeleton width="10rem" height="1.25rem" />
            </div>
            <div class="applications-list">
                {#each Array(3) as _, i (i)}
                    <div class="application-row">
                        <div class="app-info">
                            <Skeleton width="60%" height="0.875rem" />
                            <Skeleton width="30%" height="0.75rem" />
                        </div>
                        <Skeleton width="4rem" height="0.75rem" />
                        <Skeleton width="5rem" height="1.25rem" radius="var(--radius-full)" />
                    </div>
                {/each}
            </div>
        </section>
    {:else}
        <div class="stats-row content-fade-in">
            {#each stats as stat (stat.label)}
                <div class="stat-card">
                    <span class="stat-icon">
                        <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
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

        {#if role === 'Worker' && !onboardingDismissed}
            <div class="onboarding-card">
                <div class="onboarding-header">
                    <h3 class="onboarding-title">{$t('onboarding.title')}</h3>
                    <button class="onboarding-dismiss" type="button" onclick={dismissOnboarding} aria-label={$t('onboarding.dismiss')}>
                        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                    </button>
                </div>
                <div class="onboarding-steps">
                    <a href="/dashboard/profile" class="onboarding-step">
                        <span class="ob-icon">
                            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                        </span>
                        <span class="ob-text">{$t('onboarding.createProfile')}</span>
                    </a>
                    <a href="/dashboard/profile" class="onboarding-step">
                        <span class="ob-icon">
                            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/></svg>
                        </span>
                        <span class="ob-text">{$t('onboarding.addSkills')}</span>
                    </a>
                    <a href="/jobs" class="onboarding-step">
                        <span class="ob-icon">
                            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                        </span>
                        <span class="ob-text">{$t('onboarding.browseJobs')}</span>
                    </a>
                    <a href="/jobs" class="onboarding-step">
                        <span class="ob-icon">
                            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="m22 2-7 20-4-9-9-4Z"/></svg>
                        </span>
                        <span class="ob-text">{$t('onboarding.applyFirst')}</span>
                    </a>
                </div>
            </div>
        {/if}

        {#if role === 'Worker'}
        <section class="overview-section">
            <div class="section-header">
                <h2>{$t('dashOverview.latestApps')}</h2>
                <a href="/dashboard/applications" class="section-link">{$t('dashOverview.allApps')}</a>
            </div>
            <div class="applications-list">
                {#each recentApplications as app (app.job)}
                    <div class="application-row">
                        <div class="app-info">
                            <span class="app-job">{app.job}</span>
                            <span class="app-company">{app.company}</span>
                        </div>
                        <span class="app-date">{app.date}</span>
                        <Badge variant={statusMap[app.status].variant}>{statusMap[app.status].label}</Badge>
                    </div>
                {/each}
            </div>
        </section>

        <section class="overview-section">
            <div class="section-header">
                <h2>{$t('dashOverview.recommended')}</h2>
                <a href="/jobs" class="section-link">{$t('dashOverview.allJobs')}</a>
            </div>
            <div class="recommended-grid">
                {#each recommendedJobs as job (job.id)}
                    <JobCard {job} mode="list" />
                {/each}
            </div>
        </section>
    {:else}
        <section class="overview-section">
            <div class="section-header">
                <h2>{$t('dashOverview.quickActions')}</h2>
            </div>
            <div class="quick-actions">
                <Button href="/dashboard/jobs/create">{$t('dashOverview.createJob')}</Button>
                <Button href="/dashboard/jobs" variant="outline">{$t('dashOverview.myJobs')}</Button>
                <Button href="/dashboard/profile" variant="ghost">{$t('dashOverview.editProfile')}</Button>
            </div>
        </section>
    {/if}
    {/if}
</div>

<style>
    .overview {
        display: flex;
        flex-direction: column;
        gap: var(--space-8);
    }

    .overview-greeting {
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

    .section-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--space-4);
    }

    .section-header h2 {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
    }

    .section-link {
        font-size: var(--font-sm);
        color: var(--accent);
        font-weight: var(--weight-medium);
    }

    .section-link:hover { text-decoration: underline; }

    .applications-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .application-row {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-3) var(--space-4);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
    }

    .app-info {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 2px;
        min-width: 0;
    }

    .app-job {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
    }

    .app-company {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .app-date {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        flex-shrink: 0;
    }

    .recommended-grid {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .quick-actions {
        display: flex;
        gap: var(--space-3);
        flex-wrap: wrap;
    }

    .onboarding-card {
        padding: var(--space-5);
        background: var(--accent-subtle);
        border: 1px solid var(--accent);
        border-radius: var(--radius-lg);
    }

    .onboarding-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--space-4);
    }

    .onboarding-title {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
        color: var(--accent);
    }

    .onboarding-dismiss {
        display: flex;
        color: var(--text-tertiary);
        padding: var(--space-1);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }
    .onboarding-dismiss:hover { color: var(--text-primary); }

    .onboarding-steps {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(10rem, 1fr));
        gap: var(--space-3);
    }

    .onboarding-step {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-3) var(--space-4);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        text-decoration: none;
        color: var(--text-primary);
        transition: var(--transition-colors), border-color var(--duration-normal) var(--ease-in-out);
    }
    .onboarding-step:hover { border-color: var(--accent); }

    .ob-icon {
        display: flex;
        color: var(--accent);
        flex-shrink: 0;
    }

    .ob-text {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
    }

    @media (max-width: 768px) {
        .stats-row { grid-template-columns: repeat(2, 1fr); }
        .application-row { flex-wrap: wrap; }
        .onboarding-steps { grid-template-columns: 1fr; }
    }
</style>
