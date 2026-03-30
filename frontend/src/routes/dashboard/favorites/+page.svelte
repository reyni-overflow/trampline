<script lang="ts">
    import Tabs from '$lib/components/ui/Tabs.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import JobCard from '$lib/components/ui/JobCard.svelte';
    import CompanyCard from '$lib/components/ui/CompanyCard.svelte';
    import { favoritesApi } from '$lib/api/favorites';
    import { jobsApi, type JobResponse } from '$lib/api/jobs';
    import { eventsApi, type EventResponse } from '$lib/api/events';
    import { employeesApi } from '$lib/api/employees';
    import { favorites } from '$lib/stores/favorites';
    import { toast } from '$lib/stores/toast';
    import { handleApiError } from '$lib/api/client';
    import { onMount } from 'svelte';
    import { t } from '$lib/i18n';

    let activeTab = $state('jobs');
    let tabs = $derived([
        { id: 'jobs', label: $t('dashFavs.jobs') },
        { id: 'companies', label: $t('dashFavs.companies') },
        { id: 'events', label: $t('dashFavs.events') }
    ]);

    let favJobs = $state<JobResponse[]>([]);
    let favCompanies = $state<
        {
            id: string;
            name: string;
            activity: string;
            isVerified: boolean;
            jobCount: number;
            link: string | null;
        }[]
    >([]);
    let favEvents = $state<EventResponse[]>([]);

    onMount(async () => {
        try {
            const serverFavs = await favoritesApi.getAll();
            const jobIds = serverFavs.filter((f) => f.type === 'Job').map((f) => f.targetId);
            const companyIds = serverFavs
                .filter((f) => f.type === 'Company')
                .map((f) => f.targetId);
            const eventIds = serverFavs.filter((f) => f.type === 'Event').map((f) => f.targetId);

            const [jobResults, companyResults, eventResults] = await Promise.all([
                jobIds.length > 0 ? jobsApi.getByIds(jobIds).catch(() => []) : Promise.resolve([]),
                companyIds.length > 0
                    ? employeesApi.getByIds(companyIds).catch(() => [])
                    : Promise.resolve([]),
                eventIds.length > 0
                    ? eventsApi.getByIds(eventIds).catch(() => [])
                    : Promise.resolve([])
            ]);

            favJobs = jobResults;
            favCompanies = companyResults.map((c) => ({
                id: c.id,
                name: c.name,
                activity: c.activity,
                isVerified: c.isVerified,
                jobCount: 0,
                link: c.link
            }));
            favEvents = eventResults;
        } catch (err) {
            handleApiError(err);
        }
    });

    async function removeEventFavorite(id: string) {
        favEvents = favEvents.filter((e) => e.id !== id);
        favorites.toggleEvent(id);
        try {
            await favoritesApi.toggle(id, 'Event');
            toast.success($t('event.removeFromFavorites'));
        } catch {
            toast.error($t('common.error'));
        }
    }
</script>

<svelte:head>
    <title>{$t('dashFavs.pageTitle')}</title>
</svelte:head>

<div class="favorites">
    <h1 class="page-heading">{$t('dashFavs.title')}</h1>

    <Tabs {tabs} bind:active={activeTab} />

    <div class="tab-content">
        {#if activeTab === 'jobs'}
            {#if favJobs.length === 0}
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
                        ><path
                            d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"
                        /></svg
                    >
                    <p>{$t('dashFavs.noJobs')}</p>
                    <p>{$t('dashFavs.noJobsHint')}</p>
                    <Button href="/jobs">{$t('dashFavs.browseJobs')}</Button>
                </div>
            {:else}
                <div class="fav-grid">
                    {#each favJobs as job, i (job.id)}
                        <div
                            class="stagger-item"
                            style="animation-delay: {Math.min(i * 50, 500)}ms"
                        >
                            <JobCard {job} mode="grid" />
                        </div>
                    {/each}
                </div>
            {/if}
        {:else if activeTab === 'companies'}
            {#if favCompanies.length === 0}
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
                        ><path
                            d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"
                        /></svg
                    >
                    <p>{$t('dashFavs.noCompanies')}</p>
                    <p>{$t('dashFavs.noCompaniesHint')}</p>
                    <Button href="/companies">{$t('dashFavs.browseCompanies')}</Button>
                </div>
            {:else}
                <div class="fav-grid">
                    {#each favCompanies as company, i (company.id)}
                        <div
                            class="stagger-item"
                            style="animation-delay: {Math.min(i * 50, 500)}ms"
                        >
                            <CompanyCard {...company} />
                        </div>
                    {/each}
                </div>
            {/if}
        {:else if activeTab === 'events'}
            {#if favEvents.length === 0}
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
                        ><rect width="18" height="18" x="3" y="4" rx="2" /><line
                            x1="16"
                            y1="2"
                            x2="16"
                            y2="6"
                        /><line x1="8" y1="2" x2="8" y2="6" /><line
                            x1="3"
                            y1="10"
                            x2="21"
                            y2="10"
                        /></svg
                    >
                    <p>{$t('dashFavs.noEvents')}</p>
                    <p>{$t('dashFavs.noEventsHint')}</p>
                    <Button href="/events">{$t('dashFavs.browseEvents')}</Button>
                </div>
            {:else}
                <div class="fav-grid">
                    {#each favEvents as event, i (event.id)}
                        <div
                            class="event-fav-card stagger-item"
                            style="animation-delay: {Math.min(i * 50, 500)}ms"
                        >
                            <div class="event-fav-top">
                                <div class="event-fav-badges">
                                    <Badge variant="accent"
                                        >{event.format === 'Remote'
                                            ? $t('events.online')
                                            : $t('events.offline')}</Badge
                                    >
                                </div>
                                <button
                                    class="remove-btn"
                                    onclick={() => removeEventFavorite(event.id)}
                                    title={$t('event.removeFromFavorites')}
                                    type="button"
                                >
                                    <svg
                                        viewBox="0 0 24 24"
                                        width="18"
                                        height="18"
                                        fill="currentColor"
                                        stroke="currentColor"
                                        stroke-width="1.75"
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                    >
                                        <path
                                            d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"
                                        />
                                    </svg>
                                </button>
                            </div>
                            <a href="/events/{event.id}" class="event-fav-title">{event.title}</a>
                            <p class="event-fav-location">
                                {event.city}{#if event.country}, {event.country}{/if}
                            </p>
                            {#if event.tags?.length}
                                <div class="event-fav-tags">
                                    {#each event.tags.slice(0, 3) as tag (tag.name)}
                                        <Tag>{tag.name}</Tag>
                                    {/each}
                                </div>
                            {/if}
                        </div>
                    {/each}
                </div>
            {/if}
        {/if}
    </div>
</div>

<style>
    .favorites {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }

    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .tab-content {
        margin-top: var(--space-4);
    }

    .fav-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(18rem, 1fr));
        gap: var(--space-4);
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

    .event-fav-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        padding: var(--space-4);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors), var(--transition-shadow);
    }

    .event-fav-card:hover {
        border-color: var(--border-hover);
        box-shadow: var(--shadow-md);
    }

    .event-fav-top {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .event-fav-badges {
        display: flex;
        gap: var(--space-1);
    }

    .remove-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        color: var(--color-error);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        flex-shrink: 0;
    }

    .remove-btn:hover {
        background: var(--color-error-subtle);
    }

    .event-fav-title {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
        transition: var(--transition-colors);
    }

    .event-fav-title:hover {
        color: var(--accent);
    }

    .event-fav-location {
        font-size: var(--font-sm);
        color: var(--text-tertiary);
    }

    .event-fav-tags {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-1);
        margin-top: var(--space-1);
    }
</style>
