<script lang="ts">
    import MapView from '$lib/components/ui/MapView.svelte';
    import Checkbox from '$lib/components/ui/Checkbox.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import { formatSalary } from '$lib/utils/format';
    import { getCityCoords } from '$lib/utils/geo';
    import { toast } from '$lib/stores/toast';
    import { jobsApi, type JobResponse } from '$lib/api/jobs';
    import { eventsApi, type EventResponse } from '$lib/api/events';
    import { mentorshipsApi, type MentorshipResponse } from '$lib/api/mentorships';
    import { favorites } from '$lib/stores/favorites';
    import { onMount } from 'svelte';
    import { t } from '$lib/i18n';

    let geoLoading = $state(false);

    let filtersOpen = $state(true);
    let search = $state('');
    let showJobs = $state(true);
    let showEvents = $state(true);
    let showInternships = $state(true);
    let showMentorship = $state(true);

    interface MapItem {
        id: string;
        lat: number;
        lng: number;
        title: string;
        company: string;
        salary?: string;
        tags?: string[];
        type: string;
        isFavorite?: boolean;
    }

    let allMarkers = $state<MapItem[]>([]);

    onMount(async () => {
        try {
            const [jobsData, eventsData, mentorshipsData] = await Promise.all([
                jobsApi.getAll(1, 500).catch(() => ({ items: [] as JobResponse[] })),
                eventsApi.getAll(1, 500).catch(() => ({ items: [] as EventResponse[] })),
                mentorshipsApi.getAll(1, 500).catch(() => ({ items: [] as MentorshipResponse[] }))
            ]);

            const jobMarkers: MapItem[] = (jobsData.items || []).map((j, i) => {
                const coords = (j.geoLat && j.geoLon) ? [j.geoLat, j.geoLon] : getCityCoords(j.city, i);
                return {
                    id: j.id, lat: coords[0], lng: coords[1], title: j.title, company: j.companyName || j.city,
                    salary: formatSalary(j.salaryFrom, j.salaryTo),
                    tags: j.tags?.map(t => typeof t === 'string' ? t : t.name),
                    type: j.type, isFavorite: favorites.isJobFavorite(j.id)
                };
            });

            const eventMarkers: MapItem[] = (eventsData.items || []).map((e, i) => {
                const coords = (e.geoLat && e.geoLon) ? [e.geoLat, e.geoLon] : getCityCoords(e.city, i);
                return {
                    id: e.id, lat: coords[0], lng: coords[1], title: e.title, company: e.companyName || e.city,
                    tags: (e.tags || []).map(t => t.name), type: 'Event',
                    isFavorite: favorites.isEventFavorite(e.id)
                };
            });

            const mentorshipMarkers: MapItem[] = (mentorshipsData.items || []).map((m, i) => {
                const coords = (m.geoLat && m.geoLon) ? [Number(m.geoLat), Number(m.geoLon)] : getCityCoords(m.city, i);
                return {
                    id: m.id, lat: coords[0], lng: coords[1], title: m.title, company: m.companyName || m.city,
                    salary: formatSalary(m.salaryFrom ?? null, m.salaryTo ?? null),
                    tags: (m.tags || []).map(t => t.name), type: 'Mentorship',
                    isFavorite: favorites.isMentorshipFavorite(m.id)
                };
            });

            allMarkers = [...jobMarkers, ...eventMarkers, ...mentorshipMarkers];
        } catch { /* ignored */ }
    });

    let filtered = $derived.by(() => {
        let list = allMarkers;
        if (!showJobs) list = list.filter((m) => m.type !== 'Work');
        if (!showEvents) list = list.filter((m) => m.type !== 'Event');
        if (!showInternships) list = list.filter((m) => m.type !== 'Internship');
        if (!showMentorship) list = list.filter((m) => m.type !== 'Mentorship');
        if (search) {
            const q = search.toLowerCase();
            list = list.filter((m) => m.title.toLowerCase().includes(q) || m.company.toLowerCase().includes(q));
        }
        return list;
    });

    let selectedId = $state<string | null>(null);
    let selectedItem = $derived(allMarkers.find((m) => m.id === selectedId) || null);

    function handleMarkerClick(id: string) {
        selectedId = id;
    }

    function handleGeolocate() {
        if (!navigator.geolocation) {
            toast.error($t('map.locationError'));
            return;
        }
        geoLoading = true;
        navigator.geolocation.getCurrentPosition(
            (pos) => {
                geoLoading = false;
                const event = new CustomEvent('flyto', { detail: { lat: pos.coords.latitude, lng: pos.coords.longitude } });
                document.dispatchEvent(event);
            },
            (err) => {
                geoLoading = false;
                toast.error(err.code === 1 ? $t('map.locationDenied') : $t('map.locationError'));
            },
            { enableHighAccuracy: true, timeout: 10000 }
        );
    }
</script>

<svelte:head>
    <title>{$t('seo.mapTitle')}</title>
    <meta name="description" content={$t('seo.mapDesc')} />
    <meta property="og:title" content={$t('seo.mapTitle')} />
    <meta property="og:description" content={$t('seo.mapDesc')} />
    <meta property="og:type" content="website" />
</svelte:head>

<div class="map-page">
    <div class="map-sidebar" class:collapsed={!filtersOpen}>
        <div class="sidebar-header">
            <h2 class="sidebar-title">{$t('map.title')}</h2>
            <button class="sidebar-close" type="button" onclick={() => (filtersOpen = false)} title={$t('map.hide')}>
                <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
            </button>
        </div>

        <div class="sidebar-search">
            <SearchInput placeholder={$t('common.search')} bind:value={search} />
        </div>

        <div class="sidebar-filters">
            <span class="filter-label">{$t('map.show')}</span>
            <Checkbox label={$t('map.jobs')} bind:checked={showJobs} />
            <Checkbox label={$t('map.internships')} bind:checked={showInternships} />
            <Checkbox label={$t('map.mentorship')} bind:checked={showMentorship} />
            <Checkbox label={$t('map.events')} bind:checked={showEvents} />
        </div>

        <div class="sidebar-legend">
            <span class="filter-label">{$t('map.legend')}</span>
            <div class="legend-item"><span class="legend-dot" style="background: #3B82F6"></span> {$t('map.legendJob')}</div>
            <div class="legend-item"><span class="legend-dot" style="background: #10B981"></span> {$t('map.legendMentorship')}</div>
            <div class="legend-item"><span class="legend-dot" style="background: #8B5CF6"></span> {$t('map.legendEvent')}</div>
            <div class="legend-item"><span class="legend-dot" style="background: var(--accent)"></span> {$t('map.legendFavorite')}</div>
        </div>

        <div class="sidebar-count">
            <Badge variant="accent">{filtered.length}</Badge>
            <span class="count-label">{$t('map.onMap')}</span>
        </div>

        {#if selectedItem}
            <div class="sidebar-selected">
                <div class="selected-header">
                    <h3 class="selected-title">{selectedItem.title}</h3>
                    <button class="selected-close" type="button" aria-label={$t('common.close')} onclick={() => (selectedId = null)}>
                        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                    </button>
                </div>
                <span class="selected-company">{selectedItem.company}</span>
                {#if selectedItem.salary}
                    <span class="selected-salary">{selectedItem.salary}</span>
                {/if}
                {#if selectedItem.tags?.length}
                    <div class="selected-tags">
                        {#each selectedItem.tags as tag (tag)}
                            <span class="selected-tag">{tag}</span>
                        {/each}
                    </div>
                {/if}
                <Button size="sm" href={selectedItem.type === 'Event' ? `/events/${selectedItem.id}` : selectedItem.type === 'Mentorship' ? `/mentorships/${selectedItem.id}` : `/jobs/${selectedItem.id}`}>{$t('map.details')}</Button>
            </div>
        {/if}
    </div>

    {#if !filtersOpen}
        <button class="sidebar-open" type="button" onclick={() => (filtersOpen = true)} title={$t('map.showPanel')}>
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
        </button>
    {/if}

    <div class="map-area">
        <MapView
            markers={filtered}
            center={[55.751, 37.618]}
            zoom={5}
            rounded={false}
            onmarkerclick={handleMarkerClick}
        />
        <button class="geo-btn" type="button" onclick={handleGeolocate} disabled={geoLoading} title={$t('map.myLocation')}>
            {#if geoLoading}
                <svg class="geo-spin" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M21 12a9 9 0 1 1-6.219-8.56"/></svg>
            {:else}
                <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M12 2v4m0 12v4m10-10h-4M6 12H2"/></svg>
            {/if}
        </button>
    </div>
</div>

<style>
    .map-page {
        display: flex;
        height: calc(100dvh - var(--header-height));
        overflow: hidden;
    }

    .map-sidebar {
        width: 20rem;
        flex-shrink: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border-right: 1px solid var(--border-default);
        overflow-y: auto;
        transition: width var(--duration-moderate) var(--ease-out),
            padding var(--duration-moderate) var(--ease-out),
            opacity var(--duration-moderate) var(--ease-out);
    }

    .map-sidebar.collapsed {
        width: 0;
        padding: 0;
        opacity: 0;
        overflow: hidden;
        pointer-events: none;
    }

    .sidebar-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .sidebar-title {
        font-size: var(--font-lg);
        font-weight: var(--weight-bold);
    }

    .sidebar-close {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }

    .sidebar-close:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .sidebar-open {
        position: absolute;
        top: var(--space-4);
        left: var(--space-4);
        z-index: var(--z-sticky);
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        color: var(--text-secondary);
        box-shadow: var(--shadow-md);
        transition: var(--transition-colors);
    }

    .sidebar-open:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .sidebar-filters {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .filter-label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        margin-bottom: var(--space-1);
    }

    .sidebar-legend {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        padding-top: var(--space-3);
        border-top: 1px solid var(--border-default);
    }

    .legend-item {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .legend-dot {
        width: 0.625rem;
        height: 0.625rem;
        border-radius: 50%;
        flex-shrink: 0;
    }

    .sidebar-count {
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }

    .count-label {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .sidebar-selected {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        padding: var(--space-4);
        background: var(--bg-tertiary);
        border-radius: var(--radius-lg);
        animation: slide-up var(--duration-fast) var(--ease-out);
    }

    .selected-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: var(--space-2);
    }

    .selected-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }

    .selected-close {
        display: flex;
        color: var(--text-tertiary);
        flex-shrink: 0;
    }

    .selected-close:hover { color: var(--text-primary); }

    .selected-company {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .selected-salary {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--accent);
    }

    .selected-tags {
        display: flex;
        gap: var(--space-1);
        flex-wrap: wrap;
    }

    .selected-tag {
        font-size: var(--font-xs);
        padding: 0.125rem 0.5rem;
        background: var(--bg-secondary);
        border-radius: var(--radius-full);
        color: var(--text-secondary);
    }

    .map-area {
        flex: 1;
        position: relative;
    }

    .geo-btn {
        position: absolute;
        bottom: var(--space-16);
        right: var(--space-4);
        z-index: var(--z-sticky);
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        color: var(--text-secondary);
        box-shadow: var(--shadow-md);
        transition: var(--transition-colors);
        cursor: pointer;
    }

    .geo-btn:hover { color: var(--accent); background: var(--bg-tertiary); }
    .geo-btn:disabled { opacity: 0.6; cursor: wait; }
    .geo-spin { animation: spin 1s linear infinite; }

    @media (max-width: 768px) {
        .map-sidebar {
            position: absolute;
            top: 0;
            left: 0;
            bottom: 0;
            z-index: var(--z-sticky);
            width: 18rem;
            box-shadow: var(--shadow-xl);
        }
    }
</style>
