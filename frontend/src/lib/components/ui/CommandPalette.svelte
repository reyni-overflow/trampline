<script lang="ts">
    import { goto } from '$app/navigation';
    import { SvelteMap } from 'svelte/reactivity';
    import { t } from '$lib/i18n';
    import { jobsApi } from '$lib/api/jobs';
    import { eventsApi } from '$lib/api/events';
    import { employeesApi } from '$lib/api/employees';

    interface Props {
        open: boolean;
    }

    let { open = $bindable(false) }: Props = $props();

    let query = $state('');
    let selectedIndex = $state(0);
    let inputEl = $state<HTMLInputElement>();
    let listEl = $state<HTMLDivElement>();
    let recentSearches = $state<string[]>([]);
    let liveJobs = $state<ResultItem[]>([]);
    let liveEvents = $state<ResultItem[]>([]);
    let liveCompanies = $state<ResultItem[]>([]);
    let searchLoading = $state(false);
    let debounceTimer: ReturnType<typeof setTimeout> | undefined;

    const RECENT_KEY = 'trampline-recent-searches';
    const MAX_RECENT = 5;

    interface ResultItem {
        id: string;
        title: string;
        subtitle?: string;
        category: string;
        icon: string;
        route: string;
    }

    const pages: ResultItem[] = $derived([
        {
            id: 'p-home',
            title: $t('cmdPalette.pageHome'),
            category: $t('cmdPalette.pages'),
            icon: 'home',
            route: '/'
        },
        {
            id: 'p-jobs',
            title: $t('nav.jobs'),
            category: $t('cmdPalette.pages'),
            icon: 'briefcase',
            route: '/jobs'
        },
        {
            id: 'p-companies',
            title: $t('nav.companies'),
            category: $t('cmdPalette.pages'),
            icon: 'building',
            route: '/companies'
        },
        {
            id: 'p-events',
            title: $t('nav.events'),
            category: $t('cmdPalette.pages'),
            icon: 'calendar',
            route: '/events'
        },
        {
            id: 'p-map',
            title: $t('nav.map'),
            category: $t('cmdPalette.pages'),
            icon: 'map',
            route: '/map'
        },
        {
            id: 'p-settings',
            title: $t('cmdPalette.pageSettings'),
            category: $t('cmdPalette.pages'),
            icon: 'settings',
            route: '/settings'
        },
        {
            id: 'p-profile',
            title: $t('cmdPalette.pageProfile'),
            category: $t('cmdPalette.pages'),
            icon: 'user',
            route: '/dashboard'
        }
    ]);

    async function searchLive(q: string) {
        if (q.length < 2) {
            liveJobs = [];
            liveEvents = [];
            liveCompanies = [];
            return;
        }

        searchLoading = true;

        try {
            const [jobsData, eventsData, companiesData] = await Promise.all([
                jobsApi.getAll(1, 5, { search: q }).catch(() => ({ items: [] })),
                eventsApi.getAll(1, 5, { search: q }).catch(() => ({ items: [] })),
                employeesApi.getAll(1, 5).catch(() => ({ items: [] }))
            ]);

            liveJobs = (jobsData.items || []).map((j) => ({
                id: `j-${j.id}`,
                title: j.title,
                subtitle: j.city,
                category: $t('nav.jobs'),
                icon: 'briefcase',
                route: `/jobs/${j.id}`
            }));

            liveEvents = (eventsData.items || []).map((e) => ({
                id: `e-${e.id}`,
                title: e.title,
                subtitle: e.city,
                category: $t('nav.events'),
                icon: 'calendar',
                route: `/events/${e.id}`
            }));

            const ql = q.toLowerCase();
            liveCompanies = (companiesData.items || [])
                .filter((c) => c.name.toLowerCase().includes(ql))
                .slice(0, 5)
                .map((c) => ({
                    id: `c-${c.id}`,
                    title: c.name,
                    subtitle: c.activity,
                    category: $t('nav.companies'),
                    icon: 'building',
                    route: `/companies/${c.id}`
                }));
        } catch {
            liveJobs = [];
            liveEvents = [];
            liveCompanies = [];
        } finally {
            searchLoading = false;
        }
    }

    $effect(() => {
        const q = query.trim();
        clearTimeout(debounceTimer);
        if (q.length >= 2) {
            debounceTimer = setTimeout(() => searchLive(q), 300);
        } else {
            liveJobs = [];
            liveEvents = [];
            liveCompanies = [];
        }
    });

    const results: ResultItem[] = $derived.by(() => {
        const q = query.trim().toLowerCase();
        if (!q) return [];
        const pageResults = pages.filter(
            (item) =>
                item.title.toLowerCase().includes(q) ||
                (item.subtitle && item.subtitle.toLowerCase().includes(q)) ||
                item.category.toLowerCase().includes(q)
        );
        return [...pageResults, ...liveJobs, ...liveEvents, ...liveCompanies];
    });

    const groups = $derived.by(() => {
        const result: { category: string; items: ResultItem[] }[] = [];
        const map = new SvelteMap<string, ResultItem[]>();
        for (const item of results) {
            if (!map.has(item.category)) map.set(item.category, []);
            map.get(item.category)!.push(item);
        }
        for (const [category, items] of map) {
            result.push({ category, items });
        }
        return result;
    });

    $effect(() => {
        if (open) {
            loadRecent();
            query = '';
            selectedIndex = 0;
            requestAnimationFrame(() => inputEl?.focus());
        }
    });

    $effect(() => {
        void query;
        selectedIndex = 0;
    });

    function loadRecent() {
        try {
            const stored = localStorage.getItem(RECENT_KEY);
            recentSearches = stored ? JSON.parse(stored) : [];
        } catch {
            recentSearches = [];
        }
    }

    function saveRecent(term: string) {
        const trimmed = term.trim();
        if (!trimmed) return;
        recentSearches = [trimmed, ...recentSearches.filter((s) => s !== trimmed)].slice(
            0,
            MAX_RECENT
        );
        try {
            localStorage.setItem(RECENT_KEY, JSON.stringify(recentSearches));
        } catch {
            /* ignored */
        }
    }

    function close() {
        open = false;
    }

    function navigate(item: ResultItem) {
        saveRecent(query.trim() || item.title);
        close();
        goto(item.route);
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === 'Escape') {
            e.preventDefault();
            close();
            return;
        }

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (results.length > 0) {
                selectedIndex = (selectedIndex + 1) % results.length;
                scrollToSelected();
            }
            return;
        }

        if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (results.length > 0) {
                selectedIndex = (selectedIndex - 1 + results.length) % results.length;
                scrollToSelected();
            }
            return;
        }

        if (e.key === 'Enter') {
            e.preventDefault();
            if (results.length > 0 && results[selectedIndex]) {
                navigate(results[selectedIndex]);
            }
            return;
        }
    }

    function scrollToSelected() {
        requestAnimationFrame(() => {
            const el = listEl?.querySelector(`[data-index="${selectedIndex}"]`);
            el?.scrollIntoView({ block: 'nearest' });
        });
    }

    function handleGlobalKeydown(e: KeyboardEvent) {
        if ((e.metaKey || e.ctrlKey) && e.code === 'KeyK') {
            e.preventDefault();
            open = !open;
        }
    }

    function handleOverlayClick(e: MouseEvent) {
        if ((e.target as HTMLElement).classList.contains('cmd-overlay')) close();
    }

    function applyRecent(term: string) {
        query = term;
        inputEl?.focus();
    }

    function getFlatIndex(item: ResultItem): number {
        return results.indexOf(item);
    }

    const icons: Record<string, string> = {
        home: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-4 0a1 1 0 01-1-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 01-1 1',
        briefcase:
            'M20 7H4a1 1 0 00-1 1v10a1 1 0 001 1h16a1 1 0 001-1V8a1 1 0 00-1-1zM16 7V5a2 2 0 00-2-2h-4a2 2 0 00-2 2v2',
        building:
            'M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0H5m14 0h2m-16 0H3m4-8h2m4 0h2m-8 4h2m4 0h2m-8-8h2m4 0h2',
        calendar:
            'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
        map: 'M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7',
        settings:
            'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z',
        user: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z',
        clock: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z',
        search: 'M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z'
    };
</script>

<svelte:window onkeydown={handleGlobalKeydown} />

{#if open}
    <!-- svelte-ignore a11y_click_events_have_key_events -->
    <!-- svelte-ignore a11y_no_static_element_interactions -->
    <div class="cmd-overlay" onclick={handleOverlayClick}>
        <div class="cmd-palette" role="dialog" aria-modal="true" aria-label="Command palette">
            <div class="cmd-input-wrapper">
                <svg
                    class="cmd-search-icon"
                    viewBox="0 0 24 24"
                    width="20"
                    height="20"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path d={icons.search} />
                </svg>
                <input
                    bind:this={inputEl}
                    bind:value={query}
                    class="cmd-input"
                    type="text"
                    placeholder={$t('cmdPalette.placeholder')}
                    onkeydown={handleKeydown}
                />
                <kbd class="cmd-kbd">ESC</kbd>
            </div>

            <div class="cmd-results" bind:this={listEl}>
                {#if searchLoading}
                    <div class="cmd-loading">
                        <svg
                            class="cmd-spinner"
                            viewBox="0 0 24 24"
                            width="20"
                            height="20"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2"
                            ><circle cx="12" cy="12" r="10" opacity="0.25" /><path
                                d="M12 2a10 10 0 0 1 10 10"
                                stroke-linecap="round"
                            /></svg
                        >
                        <span>{$t('cmdPalette.searching')}</span>
                    </div>
                {:else if query.trim() && results.length === 0}
                    <div class="cmd-empty">{$t('cmdPalette.noResults')}</div>
                {:else if query.trim() && results.length > 0}
                    {#each groups as group (group.category)}
                        <div class="cmd-group">
                            <div class="cmd-group-label">{group.category}</div>
                            {#each group.items as item (item.id)}
                                {@const idx = getFlatIndex(item)}
                                <button
                                    class="cmd-item"
                                    class:active={idx === selectedIndex}
                                    data-index={idx}
                                    type="button"
                                    onclick={() => navigate(item)}
                                    onmouseenter={() => {
                                        selectedIndex = idx;
                                    }}
                                >
                                    <svg
                                        class="cmd-item-icon"
                                        viewBox="0 0 24 24"
                                        width="18"
                                        height="18"
                                        fill="none"
                                        stroke="currentColor"
                                        stroke-width="1.5"
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                    >
                                        <path d={icons[item.icon]} />
                                    </svg>
                                    <div class="cmd-item-text">
                                        <span class="cmd-item-title">{item.title}</span>
                                        {#if item.subtitle}
                                            <span class="cmd-item-subtitle">{item.subtitle}</span>
                                        {/if}
                                    </div>
                                    <span class="cmd-item-badge">{item.category}</span>
                                </button>
                            {/each}
                        </div>
                    {/each}
                {:else if recentSearches.length > 0}
                    <div class="cmd-group">
                        <div class="cmd-group-label">{$t('cmdPalette.recentSearches')}</div>
                        {#each recentSearches as term, _ki (term + _ki)}
                            <button
                                class="cmd-item"
                                type="button"
                                onclick={() => applyRecent(term)}
                            >
                                <svg
                                    class="cmd-item-icon"
                                    viewBox="0 0 24 24"
                                    width="18"
                                    height="18"
                                    fill="none"
                                    stroke="currentColor"
                                    stroke-width="1.5"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                >
                                    <path d={icons.clock} />
                                </svg>
                                <div class="cmd-item-text">
                                    <span class="cmd-item-title">{term}</span>
                                </div>
                            </button>
                        {/each}
                    </div>
                {/if}
            </div>

            <div class="cmd-footer">
                <span class="cmd-hint">{$t('cmdPalette.hint')}</span>
            </div>
        </div>
    </div>
{/if}

<style>
    .cmd-overlay {
        position: fixed;
        inset: 0;
        display: flex;
        align-items: flex-start;
        justify-content: center;
        padding-top: 12vh;
        background: var(--bg-overlay);
        backdrop-filter: blur(0.5rem);
        z-index: var(--z-modal);
        animation: fade-in var(--duration-fast) var(--ease-out);
    }

    .cmd-palette {
        width: 100%;
        max-width: 640px;
        margin: 0 var(--space-4);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        box-shadow: var(--shadow-xl);
        overflow: hidden;
        display: flex;
        flex-direction: column;
        animation: scale-in var(--duration-moderate) var(--ease-spring);
    }

    .cmd-input-wrapper {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-4) var(--space-5);
        border-bottom: 1px solid var(--border-default);
    }

    .cmd-search-icon {
        flex-shrink: 0;
        color: var(--text-tertiary);
    }

    .cmd-input {
        flex: 1;
        font-size: var(--font-lg);
        font-weight: var(--weight-normal);
        color: var(--text-primary);
        background: transparent;
        border: none;
        outline: none;
    }

    .cmd-input::placeholder {
        color: var(--text-tertiary);
    }

    .cmd-kbd {
        flex-shrink: 0;
        padding: var(--space-1) var(--space-2);
        font-size: var(--font-xs);
        font-family: inherit;
        color: var(--text-tertiary);
        background: var(--bg-tertiary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        line-height: 1;
    }

    .cmd-results {
        max-height: 20rem;
        overflow-y: auto;
        overscroll-behavior: contain;
    }

    .cmd-loading {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: var(--space-2);
        padding: var(--space-8) var(--space-5);
        color: var(--text-tertiary);
        font-size: var(--font-sm);
    }

    .cmd-spinner {
        animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
        to {
            transform: rotate(360deg);
        }
    }

    .cmd-empty {
        padding: var(--space-8) var(--space-5);
        text-align: center;
        color: var(--text-tertiary);
        font-size: var(--font-sm);
    }

    .cmd-group {
        padding: var(--space-2) 0;
    }

    .cmd-group-label {
        padding: var(--space-2) var(--space-5);
        font-size: var(--font-xs);
        font-weight: var(--weight-semibold);
        color: var(--text-tertiary);
        text-transform: uppercase;
        letter-spacing: 0.05em;
    }

    .cmd-item {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-2) var(--space-5);
        width: 100%;
        border: none;
        background: none;
        color: inherit;
        font: inherit;
        text-align: left;
        cursor: pointer;
        transition: background-color var(--duration-fast) var(--ease-in-out);
    }

    .cmd-item:hover,
    .cmd-item.active {
        background: var(--bg-tertiary);
    }

    .cmd-item.active {
        background: var(--accent-subtle, var(--bg-tertiary));
    }

    .cmd-item-icon {
        flex-shrink: 0;
        color: var(--text-tertiary);
    }

    .cmd-item.active .cmd-item-icon {
        color: var(--accent);
    }

    .cmd-item-text {
        flex: 1;
        min-width: 0;
        display: flex;
        align-items: baseline;
        gap: var(--space-2);
    }

    .cmd-item-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-primary);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .cmd-item-subtitle {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .cmd-item-badge {
        flex-shrink: 0;
        padding: 0.125rem var(--space-2);
        font-size: 0.6875rem;
        font-weight: var(--weight-medium);
        color: var(--text-tertiary);
        background: var(--bg-secondary);
        border: 1px solid var(--border-subtle);
        border-radius: var(--radius-full);
        line-height: 1.4;
    }

    .cmd-footer {
        padding: var(--space-3) var(--space-5);
        border-top: 1px solid var(--border-default);
        display: flex;
        justify-content: center;
    }

    .cmd-hint {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    @media (max-width: 640px) {
        .cmd-overlay {
            padding-top: var(--space-4);
        }

        .cmd-palette {
            border-radius: var(--radius-lg);
            max-height: 80vh;
        }
    }
</style>
