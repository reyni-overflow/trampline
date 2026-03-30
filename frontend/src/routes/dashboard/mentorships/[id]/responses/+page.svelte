<script lang="ts">
    import { page } from '$app/state';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import { toast } from '$lib/stores/toast';
    import { mentorshipsApi, type MentorshipApplicationResponse } from '$lib/api/mentorships';
    import { handleApiError } from '$lib/api/client';
    import { onMount } from 'svelte';
    import { t, getLocaleDateString } from '$lib/i18n';

    let statusFilter = $state('');
    let search = $state('');

    const STATUS_TO_BACKEND: Record<string, string> = {
        pending: 'Pending',
        accepted: 'Invited',
        rejected: 'Rejected',
        reserve: 'Reserved'
    };
    const BACKEND_TO_STATUS: Record<string, string> = {
        Pending: 'pending',
        Viewed: 'reserve',
        Rejected: 'rejected',
        Invited: 'accepted',
        InProgress: 'accepted',
        Hired: 'accepted',
        Withdrawn: 'rejected',
        Reserved: 'reserve'
    };

    let statusOptions = $derived([
        { value: '', label: $t('dashApps.allStatuses') },
        { value: 'pending', label: $t('dashOverview.underReview') },
        { value: 'accepted', label: $t('dashOverview.acceptedStatus') },
        { value: 'rejected', label: $t('dashOverview.rejected') },
        { value: 'reserve', label: $t('dashOverview.inReserve') }
    ]);

    let statusMap = $derived<
        Record<string, { label: string; variant: 'warning' | 'success' | 'error' | 'info' }>
    >({
        pending: { label: $t('dashOverview.underReview'), variant: 'warning' },
        accepted: { label: $t('dashOverview.acceptedStatus'), variant: 'success' },
        rejected: { label: $t('dashOverview.rejected'), variant: 'error' },
        reserve: { label: $t('dashOverview.inReserve'), variant: 'info' }
    });

    interface ResponseItem {
        id: string;
        userId: string;
        name: string;
        skills: string[];
        university: string;
        hasResume: boolean;
        date: string;
        status: string;
    }

    let responses = $state<ResponseItem[]>([]);

    onMount(async () => {
        try {
            const data = await mentorshipsApi.getResponses(page.params.id ?? '');
            responses = data.map((r: MentorshipApplicationResponse) => ({
                id: r.id,
                userId: r.workerProfileId,
                name: `${r.profile?.lastName ?? ''} ${r.profile?.name ?? ''}`.trim() || '—',
                skills: [],
                university: r.profile?.info?.university ?? '',
                hasResume: !!r.profile?.resume,
                date: r.createdAt?.split('T')[0] ?? '',
                status: BACKEND_TO_STATUS[r.status] ?? 'pending'
            }));
        } catch (err) {
            handleApiError(err);
            responses = [];
        }
    });

    let filtered = $derived.by(() => {
        let list = responses;
        if (statusFilter) list = list.filter((r) => r.status === statusFilter);
        if (search) {
            const q = search.toLowerCase();
            list = list.filter(
                (r) =>
                    r.name.toLowerCase().includes(q) ||
                    r.skills.some((s) => s.toLowerCase().includes(q))
            );
        }
        return list;
    });

    async function changeStatus(id: string, status: string) {
        const backendStatus = STATUS_TO_BACKEND[status];
        if (!backendStatus) return;
        try {
            await mentorshipsApi.updateApplicationStatus(id, backendStatus);
            responses = responses.map((r) => (r.id === id ? { ...r, status } : r));
            toast.success(
                $t('responses.statusChanged', { status: statusMap[status]?.label || status })
            );
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('mentorshipResponses.pageTitle')}</title>
</svelte:head>

<div class="responses">
    <a href="/dashboard/mentorships" class="back-link">
        <svg
            viewBox="0 0 24 24"
            width="18"
            height="18"
            fill="none"
            stroke="currentColor"
            stroke-width="1.75"
            stroke-linecap="round"
            stroke-linejoin="round"><path d="m15 18-6-6 6-6" /></svg
        >
        {$t('editMentorship.backToMentorships')}
    </a>

    <div class="page-header">
        <h1 class="page-heading">{$t('mentorshipResponses.title')}</h1>
        <Badge variant="accent">{responses.length}</Badge>
    </div>

    <div class="controls-row">
        <div class="search-box">
            <Input placeholder={$t('mentorshipResponses.searchPlaceholder')} bind:value={search} />
        </div>
        <Select
            options={statusOptions}
            bind:value={statusFilter}
            placeholder={$t('common.status')}
        />
    </div>

    {#if filtered.length === 0}
        <div class="empty"><p>{$t('mentorshipResponses.noResponses')}</p></div>
    {:else}
        <div class="responses-list">
            {#each filtered as resp (resp.id)}
                <div class="response-card">
                    <div class="resp-main">
                        <a href="/profile/{resp.userId}" class="resp-user">
                            <Avatar name={resp.name} size={44} />
                            <div class="resp-user-info">
                                <span class="resp-name">{resp.name}</span>
                                <span class="resp-uni">{resp.university}</span>
                            </div>
                        </a>
                        <div class="resp-skills">
                            {#each resp.skills as skill, _ki (skill + _ki)}
                                <Tag>{skill}</Tag>
                            {/each}
                        </div>
                    </div>
                    <div class="resp-actions">
                        <span class="resp-date"
                            >{new Date(resp.date).toLocaleDateString(getLocaleDateString(), {
                                day: 'numeric',
                                month: 'short'
                            })}</span
                        >
                        {#if resp.hasResume}
                            <Button size="sm" variant="ghost"
                                >{$t('mentorshipResponses.resume')}</Button
                            >
                        {/if}
                        <Select
                            options={statusOptions.filter((o) => o.value)}
                            bind:value={resp.status}
                            placeholder={$t('responses.statusPlaceholder')}
                            onchange={(s) => changeStatus(resp.id, s)}
                        />
                    </div>
                </div>
            {/each}
        </div>
    {/if}
</div>

<style>
    .responses {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }
    .back-link {
        display: inline-flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }
    .back-link:hover {
        color: var(--accent);
    }
    .page-header {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }
    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .controls-row {
        display: flex;
        gap: var(--space-3);
        align-items: flex-end;
        flex-wrap: wrap;
    }
    .search-box {
        width: 16rem;
    }
    .controls-row :global(.select-trigger) {
        height: 2.25rem;
        font-size: var(--font-xs);
        padding: 0 0.75rem;
    }
    .controls-row :global(.option) {
        font-size: var(--font-xs);
    }

    .responses-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .response-card {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors);
    }
    .response-card:hover {
        border-color: var(--border-hover);
    }

    .resp-main {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        flex: 1;
        min-width: 0;
    }
    .resp-user {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        text-decoration: none;
        color: inherit;
        min-width: 12rem;
    }
    .resp-user:hover .resp-name {
        color: var(--accent);
    }
    .resp-user-info {
        display: flex;
        flex-direction: column;
        gap: 2px;
    }
    .resp-name {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        transition: var(--transition-colors);
    }
    .resp-uni {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }
    .resp-skills {
        display: flex;
        gap: var(--space-1);
        flex-wrap: wrap;
    }

    .resp-actions {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        flex-shrink: 0;
    }
    .resp-actions :global(.select-trigger) {
        height: 2rem;
        font-size: var(--font-xs);
        padding: 0 0.5rem;
        min-width: 9rem;
    }
    .resp-actions :global(.option) {
        font-size: var(--font-xs);
    }
    .resp-date {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .empty {
        padding: var(--space-12);
        text-align: center;
        color: var(--text-tertiary);
    }

    @media (max-width: 640px) {
        .search-box {
            width: 100%;
        }
        .controls-row {
            flex-direction: column;
            align-items: stretch;
        }
    }

    @media (max-width: 768px) {
        .response-card {
            flex-direction: column;
            align-items: stretch;
        }
        .resp-main {
            flex-direction: column;
            align-items: stretch;
        }
        .resp-user {
            min-width: 0;
        }
        .resp-actions {
            flex-wrap: wrap;
        }
    }
</style>
