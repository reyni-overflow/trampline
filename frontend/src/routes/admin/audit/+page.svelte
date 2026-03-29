<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Pagination from '$lib/components/ui/Pagination.svelte';
    import { untrack, onMount } from 'svelte';
    import { adminApi, type AuditLogEntry } from '$lib/api/admin';
    import { handleApiError } from '$lib/api/client';
    import { t, getLocaleDateString } from '$lib/i18n';

    let search = $state('');
    let actionFilter = $state('');
    let entityFilter = $state('');
    let page = $state(1);
    let loading = $state(true);
    let logs = $state<AuditLogEntry[]>([]);
    let total = $state(0);
    const pageSize = 20;

    let totalPages = $derived(Math.max(1, Math.ceil(total / pageSize)));

    let actionOptions = $derived([
        { value: '', label: $t('audit.allActions') },
        { value: 'block_user', label: $t('audit.block_user') },
        { value: 'unblock_user', label: $t('audit.unblock_user') },
        { value: 'change_role', label: $t('audit.change_role') },
        { value: 'approve_verification', label: $t('audit.approve_verification') },
        { value: 'reject_verification', label: $t('audit.reject_verification') },
        { value: 'approve_job', label: $t('audit.approve_job') },
        { value: 'reject_job', label: $t('audit.reject_job') },
        { value: 'approve_event', label: $t('audit.approve_event') },
        { value: 'reject_event', label: $t('audit.reject_event') },
        { value: 'create_curator', label: $t('audit.create_curator') },
        { value: 'delete_curator', label: $t('audit.delete_curator') },
        { value: 'create_tag', label: $t('audit.create_tag') },
        { value: 'delete_tag', label: $t('audit.delete_tag') }
    ]);

    let entityOptions = $derived([
        { value: '', label: $t('audit.allEntities') },
        { value: 'User', label: 'User' },
        { value: 'Job', label: 'Job' },
        { value: 'Event', label: 'Event' },
        { value: 'Tag', label: 'Tag' },
        { value: 'EmployeeProfile', label: 'EmployeeProfile' }
    ]);

    let filtered = $derived.by(() => {
        if (!search) return logs;
        const q = search.toLowerCase();
        return logs.filter(
            (l) =>
                l.userName.toLowerCase().includes(q) ||
                (l.details && l.details.toLowerCase().includes(q))
        );
    });

    type BadgeVariant = 'success' | 'error' | 'info' | 'warning' | 'default';

    function actionVariant(action: string): BadgeVariant {
        if (action.startsWith('approve')) return 'success';
        if (action.startsWith('reject') || action.startsWith('delete') || action === 'block_user') return 'error';
        if (action.startsWith('create') || action === 'unblock_user') return 'info';
        if (action.startsWith('change')) return 'warning';
        return 'default';
    }

    function actionLabel(action: string): string {
        const key = `audit.${action}`;
        const translated = $t(key);
        return translated !== key ? translated : action;
    }

    async function loadLogs() {
        loading = true;
        try {
            const data = await adminApi.getAuditLogs(
                page,
                pageSize,
                actionFilter || undefined,
                entityFilter || undefined
            );
            logs = data.items;
            total = data.total;
        } catch (err) {
            handleApiError(err);
        } finally {
            loading = false;
        }
    }

    function handlePageChange() {
        loadLogs();
    }

    let mounted = false;
    onMount(() => { mounted = true; loadLogs(); });

    $effect(() => {
        void actionFilter;
        void entityFilter;
        if (mounted) {
            untrack(() => { page = 1; loadLogs(); });
        }
    });
</script>

<svelte:head><title>{$t('audit.pageTitle')}</title></svelte:head>

<div class="audit-page">
    <h1 class="page-heading">{$t('audit.title')}</h1>

    <div class="controls">
        <SearchInput placeholder={$t('common.search')} bind:value={search} />
        <Select options={actionOptions} bind:value={actionFilter} placeholder={$t('audit.filterAction')} />
        <Select options={entityOptions} bind:value={entityFilter} placeholder={$t('audit.filterEntity')} />
    </div>

    {#if loading}
        <div class="skeleton-table">
            <div class="skeleton-header"></div>
            {#each { length: 8 } as _, i (i)}
                <div class="skeleton-row">
                    <div class="skeleton-cell wide"></div>
                    <div class="skeleton-cell"></div>
                    <div class="skeleton-cell"></div>
                    <div class="skeleton-cell narrow"></div>
                </div>
            {/each}
        </div>
    {:else if filtered.length === 0}
        <div class="empty-state">
            <svg viewBox="0 0 24 24" width="48" height="48" fill="none" stroke="currentColor" stroke-width="1.25" stroke-linecap="round" stroke-linejoin="round">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z"/>
                <polyline points="14 2 14 8 20 8"/>
                <line x1="16" y1="13" x2="8" y2="13"/>
                <line x1="16" y1="17" x2="8" y2="17"/>
                <polyline points="10 9 9 9 8 9"/>
            </svg>
            <p class="empty-title">{$t('audit.noLogs')}</p>
            <p class="empty-hint">{$t('audit.noLogsHint')}</p>
        </div>
    {:else}
        <div class="audit-table">
            <div class="table-header">
                <span class="col-date">{$t('audit.date')}</span>
                <span class="col-user">{$t('audit.user')}</span>
                <span class="col-action">{$t('audit.action')}</span>
                <span class="col-entity">{$t('audit.entity')}</span>
                <span class="col-details">{$t('audit.details')}</span>
                <span class="col-ip">{$t('audit.ip')}</span>
            </div>
            {#each filtered as entry (entry.id)}
                <div class="table-row">
                    <span class="col-date">{new Date(entry.createdAt).toLocaleDateString(getLocaleDateString(), { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</span>
                    <div class="col-user">
                        <span class="user-name">{entry.userName}</span>
                        <span class="user-role">{entry.userRole}</span>
                    </div>
                    <span class="col-action"><Badge variant={actionVariant(entry.action)} size="sm">{actionLabel(entry.action)}</Badge></span>
                    <span class="col-entity">
                        {#if entry.entityId}
                            <span class="entity-type">{entry.entityType}</span>
                            <span class="entity-id">{entry.entityId.slice(0, 8)}</span>
                        {:else}
                            <span class="entity-type">{entry.entityType}</span>
                        {/if}
                    </span>
                    <span class="col-details" title={entry.details || ''}>{entry.details || '—'}</span>
                    <span class="col-ip">{entry.ipAddress || '—'}</span>
                </div>
            {/each}
        </div>
        <div class="pagination-row">
            <Pagination bind:page {totalPages} onchange={handlePageChange} />
        </div>
    {/if}
</div>

<style>
    .audit-page { display: flex; flex-direction: column; gap: var(--space-6); }
    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }
    .controls { display: flex; gap: var(--space-3); flex-wrap: wrap; align-items: center; }
    .controls :global(.select-group) { min-width: 12rem; }
    .controls :global(.select-trigger) { height: 2.5rem; font-size: var(--font-sm); padding: 0 0.75rem; }
    .controls :global(.option) { font-size: var(--font-sm); }

    .audit-table { border: 1px solid var(--border-default); border-radius: var(--radius-lg); overflow: hidden; }
    .table-header, .table-row {
        display: grid;
        grid-template-columns: 10rem 9rem 10rem 7rem 1fr 6rem;
        align-items: center;
        padding: var(--space-3) var(--space-4);
        gap: var(--space-3);
    }
    .table-header {
        font-size: var(--font-xs); font-weight: var(--weight-semibold);
        color: var(--text-tertiary); text-transform: uppercase; letter-spacing: 0.05em;
        background: var(--bg-secondary); border-bottom: 1px solid var(--border-default);
    }
    .table-row {
        font-size: var(--font-sm);
        border-bottom: 1px solid var(--border-default);
        transition: var(--transition-colors);
    }
    .table-row:last-child { border-bottom: none; }
    .table-row:hover { background: var(--bg-secondary); }

    .col-date { font-size: var(--font-xs); color: var(--text-tertiary); white-space: nowrap; }
    .col-user { display: flex; flex-direction: column; gap: 1px; min-width: 0; }
    .user-name { font-weight: var(--weight-medium); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .user-role { font-size: var(--font-xs); color: var(--text-tertiary); }
    .col-action { white-space: nowrap; }
    .col-entity { display: flex; flex-direction: column; gap: 1px; min-width: 0; }
    .entity-type { font-size: var(--font-xs); font-weight: var(--weight-medium); }
    .entity-id { font-size: var(--font-xs); color: var(--text-tertiary); font-family: monospace; }
    .col-details { font-size: var(--font-xs); color: var(--text-secondary); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .col-ip { font-size: var(--font-xs); color: var(--text-tertiary); font-family: monospace; }

    .empty-state {
        display: flex; flex-direction: column; align-items: center; gap: var(--space-3);
        padding: var(--space-12) var(--space-4); color: var(--text-tertiary);
    }
    .empty-title { font-size: var(--font-base); font-weight: var(--weight-semibold); color: var(--text-secondary); }
    .empty-hint { font-size: var(--font-sm); }

    .skeleton-table { display: flex; flex-direction: column; gap: 1px; border: 1px solid var(--border-default); border-radius: var(--radius-lg); overflow: hidden; }
    .skeleton-header { height: 2.5rem; background: var(--bg-secondary); }
    .skeleton-row { display: flex; gap: var(--space-4); padding: var(--space-3) var(--space-4); }
    .skeleton-cell { height: 1rem; border-radius: var(--radius-sm); background: var(--bg-tertiary); flex: 1; animation: pulse 1.5s ease-in-out infinite; }
    .skeleton-cell.wide { flex: 2; }
    .skeleton-cell.narrow { flex: 0.5; }

    @keyframes pulse {
        0%, 100% { opacity: 1; }
        50% { opacity: 0.4; }
    }

    .pagination-row { display: flex; justify-content: center; }

    @media (max-width: 768px) {
        .table-header { display: none; }
        .table-row {
            grid-template-columns: 1fr 1fr;
            grid-template-rows: auto auto auto;
            gap: var(--space-1) var(--space-3);
        }
        .col-details { grid-column: 1 / -1; }
        .col-ip { display: none; }
    }
</style>
