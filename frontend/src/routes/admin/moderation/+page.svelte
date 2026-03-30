<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import Textarea from '$lib/components/ui/Textarea.svelte';
    import Modal from '$lib/components/ui/Modal.svelte';
    import { toast } from '$lib/stores/toast';
    import { formatSalary } from '$lib/utils/format';
    import { onMount } from 'svelte';
    import { adminApi } from '$lib/api/admin';
    import { handleApiError } from '$lib/api/client';
    import { t, getLocaleDateString } from '$lib/i18n';
    import type { TagResponse } from '$lib/api/jobs';
    import type { EventResponse } from '$lib/api/events';
    import type { MentorshipResponse } from '$lib/api/mentorships';

    type Tab = 'jobs' | 'events' | 'mentorships';
    let activeTab = $state<Tab>('jobs');

    interface ModerationJob {
        id: string;
        title: string;
        description: string;
        company: string;
        salary: string | null;
        tags: TagResponse[];
        type: string;
        isPublished: boolean;
        date: string;
    }

    interface ModerationEvent {
        id: string;
        title: string;
        description: string;
        format: string;
        address: string;
        salary: string | null;
        tags: EventResponse['tags'];
        isPublished: boolean;
        date: string;
    }

    interface ModerationMentorship {
        id: string;
        title: string;
        description: string;
        format: string;
        address: string;
        salary: string | null;
        tags: MentorshipResponse['tags'];
        isPublished: boolean;
        date: string;
    }

    let apiJobs = $state<ModerationJob[]>([]);
    let apiEvents = $state<ModerationEvent[]>([]);
    let apiMentorships = $state<ModerationMentorship[]>([]);
    let pendingJobs = $derived(apiJobs);
    let pendingEvents = $derived(apiEvents);
    let pendingMentorships = $derived(apiMentorships);
    let totalPending = $derived(
        pendingJobs.length + pendingEvents.length + pendingMentorships.length
    );

    let editingJob = $state<{
        id: string;
        title: string;
        description: string;
        isPublished: boolean;
    } | null>(null);
    let editingEvent = $state<{
        id: string;
        title: string;
        description: string;
        isPublished: boolean;
    } | null>(null);
    let editingMentorship = $state<{
        id: string;
        title: string;
        description: string;
        isPublished: boolean;
    } | null>(null);
    let saving = $state(false);

    async function approveJob(id: string, title: string) {
        try {
            await adminApi.approveJob(id);
            toast.success($t('adminMod.approvedMsg', { title }));
            apiJobs = apiJobs.filter((j) => j.id !== id);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function rejectJob(id: string, title: string) {
        try {
            await adminApi.rejectJob(id);
            toast.warning($t('adminMod.rejectedMsg', { title }));
            apiJobs = apiJobs.filter((j) => j.id !== id);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function saveJobEdit() {
        if (!editingJob) return;
        saving = true;
        try {
            await adminApi.updateJob(editingJob.id, {
                title: editingJob.title,
                description: editingJob.description,
                isPublished: editingJob.isPublished
            });
            apiJobs = apiJobs.map((j) =>
                j.id === editingJob!.id
                    ? { ...j, title: editingJob!.title, description: editingJob!.description }
                    : j
            );
            toast.success($t('adminMod.editSaved'));
            editingJob = null;
        } catch (err) {
            handleApiError(err);
        } finally {
            saving = false;
        }
    }

    async function approveEvent(id: string, title: string) {
        try {
            await adminApi.approveEvent(id);
            toast.success($t('adminMod.eventApprovedMsg', { title }));
            apiEvents = apiEvents.filter((e) => e.id !== id);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function rejectEvent(id: string, title: string) {
        try {
            await adminApi.rejectEvent(id);
            toast.warning($t('adminMod.eventRejectedMsg', { title }));
            apiEvents = apiEvents.filter((e) => e.id !== id);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function saveEventEdit() {
        if (!editingEvent) return;
        saving = true;
        try {
            await adminApi.updateEvent(editingEvent.id, {
                title: editingEvent.title,
                description: editingEvent.description,
                isPublished: editingEvent.isPublished
            });
            apiEvents = apiEvents.map((e) =>
                e.id === editingEvent!.id
                    ? { ...e, title: editingEvent!.title, description: editingEvent!.description }
                    : e
            );
            toast.success($t('adminMod.editSaved'));
            editingEvent = null;
        } catch (err) {
            handleApiError(err);
        } finally {
            saving = false;
        }
    }

    async function approveMentorship(id: string, title: string) {
        try {
            await adminApi.approveMentorship(id);
            toast.success($t('adminMod.mentorshipApprovedMsg', { title }));
            apiMentorships = apiMentorships.filter((m) => m.id !== id);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function rejectMentorship(id: string, title: string) {
        try {
            await adminApi.rejectMentorship(id);
            toast.warning($t('adminMod.mentorshipRejectedMsg', { title }));
            apiMentorships = apiMentorships.filter((m) => m.id !== id);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function saveMentorshipEdit() {
        if (!editingMentorship) return;
        saving = true;
        try {
            await adminApi.updateMentorship(editingMentorship.id, {
                title: editingMentorship.title,
                description: editingMentorship.description,
                isPublished: editingMentorship.isPublished
            });
            apiMentorships = apiMentorships.map((m) =>
                m.id === editingMentorship!.id
                    ? {
                          ...m,
                          title: editingMentorship!.title,
                          description: editingMentorship!.description
                      }
                    : m
            );
            toast.success($t('adminMod.editSaved'));
            editingMentorship = null;
        } catch (err) {
            handleApiError(err);
        } finally {
            saving = false;
        }
    }

    onMount(async () => {
        try {
            const [jobsData, eventsData, mentorshipsData] = await Promise.all([
                adminApi.getPendingJobs(),
                adminApi.getPendingEvents(),
                adminApi.getPendingMentorships()
            ]);
            apiJobs = jobsData.map((j) => ({
                id: j.id,
                title: j.title,
                description: j.description || '',
                company: j.company || j.companyName || '',
                salary: j.salaryFrom || j.salaryTo ? formatSalary(j.salaryFrom, j.salaryTo) : null,
                tags: j.tags || [],
                type: j.type || 'Work',
                isPublished: j.isPublished ?? true,
                date: j.createdAt || j.date || ''
            }));
            apiEvents = eventsData.map((e) => ({
                id: e.id,
                title: e.title,
                description: e.description || '',
                format: e.format || '',
                address: e.address || '',
                salary: e.salaryFrom || e.salaryTo ? formatSalary(e.salaryFrom, e.salaryTo) : null,
                tags: e.tags || [],
                isPublished: e.isPublished ?? true,
                date: e.createdAt || ''
            }));
            apiMentorships = mentorshipsData.map((m) => ({
                id: m.id,
                title: m.title,
                description: m.description || '',
                format: m.format || '',
                address: m.address || '',
                salary:
                    m.salaryFrom || m.salaryTo
                        ? formatSalary(m.salaryFrom ?? null, m.salaryTo ?? null)
                        : null,
                tags: m.tags || [],
                isPublished: m.isPublished ?? true,
                date: m.createdAt || ''
            }));
        } catch (err) {
            handleApiError(err);
        }
    });
</script>

<svelte:head><title>{$t('adminMod.pageTitle')}</title></svelte:head>

<div class="moderation">
    <div class="page-header">
        <h1 class="page-heading">{$t('adminMod.title')}</h1>
        <Badge variant="warning">{totalPending} {$t('adminMod.pending')}</Badge>
    </div>

    <div class="tabs">
        <button
            class="tab"
            class:active={activeTab === 'jobs'}
            onclick={() => (activeTab = 'jobs')}
        >
            {$t('adminMod.jobsTab')}
            {#if pendingJobs.length}<Badge variant="warning" size="sm">{pendingJobs.length}</Badge
                >{/if}
        </button>
        <button
            class="tab"
            class:active={activeTab === 'events'}
            onclick={() => (activeTab = 'events')}
        >
            {$t('adminMod.eventsTab')}
            {#if pendingEvents.length}<Badge variant="warning" size="sm"
                    >{pendingEvents.length}</Badge
                >{/if}
        </button>
        <button
            class="tab"
            class:active={activeTab === 'mentorships'}
            onclick={() => (activeTab = 'mentorships')}
        >
            {$t('adminMod.mentorshipsTab')}
            {#if pendingMentorships.length}<Badge variant="warning" size="sm"
                    >{pendingMentorships.length}</Badge
                >{/if}
        </button>
    </div>

    {#if activeTab === 'jobs'}
        <div class="mod-list">
            {#each pendingJobs as job (job.id)}
                <div class="mod-card">
                    <div class="mod-main">
                        <div class="mod-title-row">
                            <span class="mod-title">{job.title}</span>
                            <Badge
                                variant={job.type === 'Internship'
                                    ? 'info'
                                    : job.type === 'Event'
                                      ? 'accent'
                                      : 'default'}
                                size="sm"
                                >{job.type === 'Internship'
                                    ? $t('adminMod.internship')
                                    : job.type === 'Event'
                                      ? $t('adminMod.event')
                                      : $t('adminMod.vacancy')}</Badge
                            >
                        </div>
                        <span class="mod-company">{job.company}</span>
                        {#if job.salary}<span class="mod-salary">{job.salary}</span>{/if}
                        <div class="mod-tags">
                            {#each job.tags as tag (tag.name)}<Tag>{tag.name}</Tag>{/each}
                        </div>
                        <span class="mod-date"
                            >{new Date(job.date).toLocaleDateString(getLocaleDateString(), {
                                day: 'numeric',
                                month: 'short'
                            })}</span
                        >
                    </div>
                    <div class="mod-actions">
                        <Button
                            size="sm"
                            variant="ghost"
                            onclick={() =>
                                (editingJob = {
                                    id: job.id,
                                    title: job.title,
                                    description: job.description,
                                    isPublished: job.isPublished
                                })}>{$t('common.edit')}</Button
                        >
                        <Button size="sm" onclick={() => approveJob(job.id, job.title)}
                            >{$t('common.approve')}</Button
                        >
                        <Button
                            size="sm"
                            variant="danger"
                            onclick={() => rejectJob(job.id, job.title)}
                            >{$t('common.reject')}</Button
                        >
                    </div>
                </div>
            {:else}
                <p class="mod-empty">{$t('adminMod.noJobs')}</p>
            {/each}
        </div>
    {:else if activeTab === 'events'}
        <div class="mod-list">
            {#each pendingEvents as event (event.id)}
                <div class="mod-card">
                    <div class="mod-main">
                        <div class="mod-title-row">
                            <span class="mod-title">{event.title}</span>
                            {#if event.format}<Badge variant="accent" size="sm"
                                    >{event.format}</Badge
                                >{/if}
                        </div>
                        {#if event.address}<span class="mod-company">{event.address}</span>{/if}
                        {#if event.salary}<span class="mod-salary">{event.salary}</span>{/if}
                        <div class="mod-tags">
                            {#each event.tags as tag (tag.name)}<Tag>{tag.name}</Tag>{/each}
                        </div>
                        <span class="mod-date"
                            >{new Date(event.date).toLocaleDateString(getLocaleDateString(), {
                                day: 'numeric',
                                month: 'short'
                            })}</span
                        >
                    </div>
                    <div class="mod-actions">
                        <Button
                            size="sm"
                            variant="ghost"
                            onclick={() =>
                                (editingEvent = {
                                    id: event.id,
                                    title: event.title,
                                    description: event.description,
                                    isPublished: event.isPublished
                                })}>{$t('common.edit')}</Button
                        >
                        <Button size="sm" onclick={() => approveEvent(event.id, event.title)}
                            >{$t('common.approve')}</Button
                        >
                        <Button
                            size="sm"
                            variant="danger"
                            onclick={() => rejectEvent(event.id, event.title)}
                            >{$t('common.reject')}</Button
                        >
                    </div>
                </div>
            {:else}
                <p class="mod-empty">{$t('adminMod.noEvents')}</p>
            {/each}
        </div>
    {:else}
        <div class="mod-list">
            {#each pendingMentorships as mentorship (mentorship.id)}
                <div class="mod-card">
                    <div class="mod-main">
                        <div class="mod-title-row">
                            <span class="mod-title">{mentorship.title}</span>
                            {#if mentorship.format}<Badge variant="accent" size="sm"
                                    >{mentorship.format}</Badge
                                >{/if}
                        </div>
                        {#if mentorship.address}<span class="mod-company">{mentorship.address}</span
                            >{/if}
                        {#if mentorship.salary}<span class="mod-salary">{mentorship.salary}</span
                            >{/if}
                        <div class="mod-tags">
                            {#each mentorship.tags as tag (tag.name)}<Tag>{tag.name}</Tag>{/each}
                        </div>
                        <span class="mod-date"
                            >{new Date(mentorship.date).toLocaleDateString(getLocaleDateString(), {
                                day: 'numeric',
                                month: 'short'
                            })}</span
                        >
                    </div>
                    <div class="mod-actions">
                        <Button
                            size="sm"
                            variant="ghost"
                            onclick={() =>
                                (editingMentorship = {
                                    id: mentorship.id,
                                    title: mentorship.title,
                                    description: mentorship.description,
                                    isPublished: mentorship.isPublished
                                })}>{$t('common.edit')}</Button
                        >
                        <Button
                            size="sm"
                            onclick={() => approveMentorship(mentorship.id, mentorship.title)}
                            >{$t('common.approve')}</Button
                        >
                        <Button
                            size="sm"
                            variant="danger"
                            onclick={() => rejectMentorship(mentorship.id, mentorship.title)}
                            >{$t('common.reject')}</Button
                        >
                    </div>
                </div>
            {:else}
                <p class="mod-empty">{$t('adminMod.noMentorships')}</p>
            {/each}
        </div>
    {/if}
</div>

{#if editingJob}
    <Modal open={true} onclose={() => (editingJob = null)} title={$t('adminMod.editJob')}>
        <div class="edit-form">
            <Input label={$t('adminMod.titleLabel')} bind:value={editingJob.title} />
            <Textarea
                label={$t('adminMod.descriptionLabel')}
                bind:value={editingJob.description}
                rows={5}
            />
            <div class="edit-actions">
                <Button variant="ghost" onclick={() => (editingJob = null)}
                    >{$t('common.cancel')}</Button
                >
                <Button onclick={saveJobEdit} disabled={saving}
                    >{saving ? '...' : $t('common.save')}</Button
                >
            </div>
        </div>
    </Modal>
{/if}

{#if editingEvent}
    <Modal open={true} onclose={() => (editingEvent = null)} title={$t('adminMod.editEvent')}>
        <div class="edit-form">
            <Input label={$t('adminMod.titleLabel')} bind:value={editingEvent.title} />
            <Textarea
                label={$t('adminMod.descriptionLabel')}
                bind:value={editingEvent.description}
                rows={5}
            />
            <div class="edit-actions">
                <Button variant="ghost" onclick={() => (editingEvent = null)}
                    >{$t('common.cancel')}</Button
                >
                <Button onclick={saveEventEdit} disabled={saving}
                    >{saving ? '...' : $t('common.save')}</Button
                >
            </div>
        </div>
    </Modal>
{/if}

{#if editingMentorship}
    <Modal
        open={true}
        onclose={() => (editingMentorship = null)}
        title={$t('adminMod.editMentorship')}
    >
        <div class="edit-form">
            <Input label={$t('adminMod.titleLabel')} bind:value={editingMentorship.title} />
            <Textarea
                label={$t('adminMod.descriptionLabel')}
                bind:value={editingMentorship.description}
                rows={5}
            />
            <div class="edit-actions">
                <Button variant="ghost" onclick={() => (editingMentorship = null)}
                    >{$t('common.cancel')}</Button
                >
                <Button onclick={saveMentorshipEdit} disabled={saving}
                    >{saving ? '...' : $t('common.save')}</Button
                >
            </div>
        </div>
    </Modal>
{/if}

<style>
    .moderation {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
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

    .tabs {
        display: flex;
        gap: var(--space-1);
        border-bottom: 1px solid var(--border-default);
    }
    .tab {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        padding: var(--space-2) var(--space-4);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        background: none;
        border: none;
        border-bottom: 2px solid transparent;
        cursor: pointer;
        transition: var(--transition-colors);
    }
    .tab:hover {
        color: var(--text-primary);
    }
    .tab.active {
        color: var(--accent);
        border-bottom-color: var(--accent);
    }

    .mod-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }
    .mod-card {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }
    .mod-main {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
        flex: 1;
        min-width: 0;
    }
    .mod-title-row {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        flex-wrap: wrap;
    }
    .mod-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }
    .mod-company {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }
    .mod-salary {
        font-size: var(--font-xs);
        color: var(--accent);
        font-weight: var(--weight-medium);
    }
    .mod-tags {
        display: flex;
        gap: var(--space-1);
        flex-wrap: wrap;
    }
    .mod-date {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }
    .mod-actions {
        display: flex;
        gap: var(--space-2);
        flex-shrink: 0;
    }
    .mod-empty {
        font-size: var(--font-sm);
        color: var(--text-tertiary);
        text-align: center;
        padding: var(--space-8) 0;
    }

    .edit-form {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
        padding: var(--space-2) 0;
    }
    .edit-actions {
        display: flex;
        justify-content: flex-end;
        gap: var(--space-2);
    }

    @media (max-width: 640px) {
        .mod-card {
            flex-direction: column;
            align-items: stretch;
        }
    }
</style>
