<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import { onMount } from 'svelte';
    import { adminApi } from '$lib/api/admin';
    import { jobsApi } from '$lib/api/jobs';
    import { employeesApi } from '$lib/api/employees';
    import { handleApiError } from '$lib/api/client';
    import { t, getLocaleDateString } from '$lib/i18n';

    let loading = $state(true);
    let usersCount = $state('...');
    let companiesCount = $state('...');
    let jobsCount = $state('...');
    let pendingJobs = $state('...');
    let pendingEvents = $state('...');
    let apiUsers = $state<{ name: string; role: string; date: string }[]>([]);

    let recentUsers = $derived(apiUsers);

    let pendingVerifications = $state('...');

    let stats = $derived([
        { label: $t('adminDash.users'), value: usersCount, icon: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/>' },
        { label: $t('adminDash.companies'), value: companiesCount, icon: '<path d="M6 22V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v18Z"/>' },
        { label: $t('adminDash.jobs'), value: jobsCount, icon: '<path d="M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/><rect width="20" height="14" x="2" y="6" rx="2"/>' },
        { label: $t('adminDash.pendingModeration'), value: String(Number(pendingJobs) + Number(pendingEvents)), icon: '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>' }
    ]);

    onMount(async () => {
        try {
            const [users, verifications, jobs, events, companiesData, jobsData] = await Promise.all([
                adminApi.getUsers(1, 20),
                adminApi.getPendingVerifications(),
                adminApi.getPendingJobs(),
                adminApi.getPendingEvents(),
                employeesApi.getAll(1, 1).catch(() => ({ items: [], total: 0 })),
                jobsApi.getAll(1, 1).catch(() => ({ items: [], totalCount: 0 }))
            ]);
            usersCount = String(users.length);
            companiesCount = String(companiesData.total);
            jobsCount = String('totalCount' in jobsData ? jobsData.totalCount : 0);
            pendingVerifications = String(verifications.length);
            pendingJobs = String((jobs as unknown[]).length);
            pendingEvents = String((events as unknown[]).length);
            apiUsers = users.slice(0, 4).map((u) => ({
                name: u.nickname,
                role: u.role,
                date: new Date(u.createdAt).toLocaleDateString(getLocaleDateString(), { day: 'numeric', month: 'short', year: 'numeric' })
            }));
        } catch (err) {
            handleApiError(err);
        } finally {
            loading = false;
        }
    });
</script>

<svelte:head><title>{$t('adminDash.pageTitle')}</title></svelte:head>

<div class="admin-dash">
    <h1 class="page-heading">{$t('adminDash.title')}</h1>

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
        <div class="sections-row">
            <section class="dash-section">
                <div class="section-header">
                    <Skeleton width="12rem" height="1.125rem" />
                    <Skeleton width="5rem" height="0.875rem" />
                </div>
                <div class="recent-list">
                    {#each Array(4) as _, i (i)}
                        <div class="recent-row">
                            <Skeleton circle height="2rem" />
                            <Skeleton width="8rem" height="0.875rem" />
                            <Skeleton width="4rem" height="1.25rem" radius="var(--radius-full)" />
                            <Skeleton width="5rem" height="0.75rem" />
                        </div>
                    {/each}
                </div>
            </section>
            <section class="dash-section">
                <Skeleton width="8rem" height="1.125rem" />
                <div class="quick-actions" style="margin-top: var(--space-4)">
                    <Skeleton width="100%" height="2.25rem" radius="var(--radius-md)" />
                    <Skeleton width="100%" height="2.25rem" radius="var(--radius-md)" />
                    <Skeleton width="100%" height="2.25rem" radius="var(--radius-md)" />
                </div>
            </section>
        </div>
    {:else}
        <div class="stats-row">
            {#each stats as stat (stat.label)}
                <div class="stat-card">
                    <span class="stat-icon">
                        <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                        <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">{@html stat.icon}</svg>
                    </span>
                    <div class="stat-info">
                        <span class="stat-value">{stat.value}</span>
                        <span class="stat-label">{stat.label}</span>
                    </div>
                </div>
            {/each}
        </div>

        <div class="sections-row">
            <section class="dash-section">
                <div class="section-header">
                    <h2>{$t('adminDash.latestRegistrations')}</h2>
                    <a href="/admin/users" class="section-link">{$t('adminDash.allUsers')}</a>
                </div>
                <div class="recent-list">
                    {#each recentUsers as u (u.name)}
                        <div class="recent-row">
                            <Avatar name={u.name} size={32} />
                            <span class="recent-name">{u.name}</span>
                            <Badge variant={u.role === 'Admin' ? 'error' : u.role === 'Employee' ? 'info' : 'default'} size="sm">{u.role === 'Admin' ? $t('adminUsers.curator') : u.role === 'Employee' ? $t('adminDash.company') : $t('adminDash.worker')}</Badge>
                            <span class="recent-date">{u.date}</span>
                        </div>
                    {/each}
                </div>
            </section>

            <section class="dash-section">
                <h2>{$t('adminDash.quickActions')}</h2>
                <div class="quick-actions">
                    <Button href="/admin/verification" variant="outline">{$t('adminDash.verificationCount', { count: pendingVerifications })}</Button>
                    <Button href="/admin/moderation" variant="outline">{$t('adminDash.moderationCount', { count: String(Number(pendingJobs) + Number(pendingEvents)) })}</Button>
                    <Button href="/admin/users" variant="outline">{$t('adminDash.usersLink')}</Button>
                    <Button href="/admin/tags" variant="outline">{$t('adminDash.tagsLink')}</Button>
                </div>
            </section>
        </div>
    {/if}
</div>

<style>
    .admin-dash { display: flex; flex-direction: column; gap: var(--space-8); }
    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }
    .stats-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: var(--space-4); }
    .stat-card { display: flex; align-items: center; gap: var(--space-3); padding: var(--space-4) var(--space-5); background: var(--bg-secondary); border: 1px solid var(--border-default); border-radius: var(--radius-lg); }
    .stat-icon { display: flex; align-items: center; justify-content: center; width: 2.5rem; height: 2.5rem; background: var(--accent-subtle); color: var(--accent); border-radius: var(--radius-md); flex-shrink: 0; }
    .stat-info { display: flex; flex-direction: column; }
    .stat-value { font-size: var(--font-xl); font-weight: var(--weight-bold); line-height: 1; }
    .stat-label { font-size: var(--font-xs); color: var(--text-secondary); }

    .sections-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-6); }
    .dash-section h2 { font-size: var(--font-md); font-weight: var(--weight-semibold); margin-bottom: var(--space-4); }
    .section-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--space-4); }
    .section-link { font-size: var(--font-sm); color: var(--accent); font-weight: var(--weight-medium); }
    .section-link:hover { text-decoration: underline; }
    .recent-list { display: flex; flex-direction: column; gap: var(--space-2); }
    .recent-row { display: flex; align-items: center; gap: var(--space-3); padding: var(--space-2) var(--space-3); background: var(--bg-secondary); border-radius: var(--radius-md); }
    .recent-name { flex: 1; font-size: var(--font-sm); font-weight: var(--weight-medium); }
    .recent-date { font-size: var(--font-xs); color: var(--text-tertiary); }
    .quick-actions { display: flex; flex-direction: column; gap: var(--space-2); }

    @media (max-width: 768px) { .stats-row { grid-template-columns: repeat(2, 1fr); } .sections-row { grid-template-columns: 1fr; } }
</style>
