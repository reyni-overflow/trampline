<script lang="ts">
    import { user as userStore, authReady } from '$lib/stores/auth';
    import { authModal } from '$lib/stores/auth-modal';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import { t } from '$lib/i18n';
    import { onMount, onDestroy } from 'svelte';
    import { goto } from '$app/navigation';

    let { children } = $props();

    let currentUser = $state<{ nickname: string; avatar: string | null; role: string } | null>(
        null
    );
    let authenticated = $state(false);
    let ready = $state(false);
    const unsub = userStore.subscribe((v) => {
        if (v) {
            currentUser = { nickname: v.nickname, avatar: v.avatar, role: v.role };
            authenticated = true;
        } else {
            authenticated = false;
        }
    });
    const unsubReady = authReady.subscribe((v) => {
        ready = v;
    });
    onDestroy(() => {
        unsub();
        unsubReady();
    });

    $effect(() => {
        if (ready && !authenticated && typeof window !== 'undefined') {
            goto('/');
            authModal.openLogin();
        }
        if (
            ready &&
            authenticated &&
            currentUser?.role === 'Admin' &&
            typeof window !== 'undefined'
        ) {
            goto('/admin');
        }
    });

    let headerHeight = $state('4rem');
    let observer: MutationObserver | undefined;

    onMount(() => {
        const header = document.querySelector('header.header');
        if (!header) return;
        function updateHeight() {
            const h = header!.getBoundingClientRect().height;
            headerHeight = `${h}px`;
        }
        updateHeight();
        observer = new MutationObserver(updateHeight);
        observer.observe(header, { attributes: true, attributeFilter: ['class'] });
        window.addEventListener('scroll', updateHeight, { passive: true });
        return () => {
            observer?.disconnect();
            window.removeEventListener('scroll', updateHeight);
        };
    });

    let role = $derived(currentUser?.role || 'Worker');

    const settingsIcon =
        '<path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z"/><circle cx="12" cy="12" r="3"/>';

    let workerNav = $derived([
        {
            href: '/dashboard',
            label: $t('dash.overview'),
            icon: '<rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18"/><path d="M9 21V9"/>'
        },
        {
            href: '/dashboard/profile',
            label: $t('dash.myProfile'),
            icon: '<circle cx="12" cy="7" r="4"/><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>'
        },
        {
            href: '/dashboard/applications',
            label: $t('dash.myApplications'),
            icon: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="m22 2-7 20-4-9-9-4Z"/>'
        },
        {
            href: '/dashboard/favorites',
            label: $t('dash.favorites'),
            icon: '<path d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"/>'
        },
        {
            href: '/dashboard/contacts',
            label: $t('dashContacts.title'),
            icon: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>'
        },
        {
            href: '/dashboard/search',
            label: $t('workerSearch.nav'),
            icon: '<circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/>'
        },
        { href: '/settings', label: $t('dash.settings'), icon: settingsIcon }
    ]);

    let employeeNav = $derived([
        {
            href: '/dashboard',
            label: $t('dash.overview'),
            icon: '<rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18"/><path d="M9 21V9"/>'
        },
        {
            href: '/dashboard/profile',
            label: $t('dash.companyProfile'),
            icon: '<path d="M6 22V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v18Z"/><path d="M6 12H4a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h2"/><path d="M18 9h2a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-2"/>'
        },
        {
            href: '/dashboard/jobs',
            label: $t('dash.myJobs'),
            icon: '<path d="M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/><rect width="20" height="14" x="2" y="6" rx="2"/>'
        },
        {
            href: '/dashboard/events',
            label: $t('dash.myEvents'),
            icon: '<rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>'
        },
        {
            href: '/dashboard/mentorships',
            label: $t('dash.myMentorships'),
            icon: '<path d="M22 10v6M2 10l10-5 10 5-10 5z"/><path d="M6 12v5c0 2 3 3 6 3s6-1 6-3v-5"/>'
        },
        {
            href: '/dashboard/search',
            label: $t('workerSearch.nav'),
            icon: '<circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/>'
        },
        {
            href: '/dashboard/stats',
            label: 'Статистика',
            icon: '<path d="M18 20V10"/><path d="M12 20V4"/><path d="M6 20v-6"/>'
        },
        { href: '/settings', label: $t('dash.settings'), icon: settingsIcon }
    ]);

    let navItems = $derived(role === 'Employee' ? employeeNav : workerNav);
    let sidebarOpen = $state(false);
</script>

{#if !ready || !authenticated}
    <div class="auth-loading">
        <div class="auth-spinner"></div>
    </div>
{:else}
    <div class="dashboard">
        <button
            class="sidebar-toggle"
            type="button"
            onclick={() => (sidebarOpen = !sidebarOpen)}
            aria-label={$t('dash.menu')}
        >
            <svg
                viewBox="0 0 24 24"
                width="20"
                height="20"
                fill="none"
                stroke="currentColor"
                stroke-width="1.75"
                stroke-linecap="round"
                stroke-linejoin="round"
            >
                <line x1="4" y1="6" x2="20" y2="6" /><line x1="4" y1="12" x2="20" y2="12" /><line
                    x1="4"
                    y1="18"
                    x2="20"
                    y2="18"
                />
            </svg>
        </button>

        {#if sidebarOpen}
            <!-- svelte-ignore a11y_click_events_have_key_events -->
            <!-- svelte-ignore a11y_no_static_element_interactions -->
            <div class="sidebar-overlay" onclick={() => (sidebarOpen = false)}></div>
        {/if}

        <aside
            class="sidebar"
            class:open={sidebarOpen}
            style="--actual-header-height: {headerHeight};"
        >
            <div class="sidebar-user">
                <Avatar name={currentUser?.nickname || 'U'} src={currentUser?.avatar} size={40} />
                <div class="sidebar-user-info">
                    <span class="sidebar-user-name">{currentUser?.nickname || $t('dash.user')}</span
                    >
                    <Badge variant="accent" size="sm"
                        >{role === 'Employee' ? $t('dash.employee') : $t('dash.worker')}</Badge
                    >
                </div>
            </div>

            <nav class="sidebar-nav">
                {#each navItems as item (item.href)}
                    <a href={item.href} class="sidebar-link" onclick={() => (sidebarOpen = false)}>
                        <svg
                            viewBox="0 0 24 24"
                            width="18"
                            height="18"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="1.75"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                        >
                            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                            {@html item.icon}
                        </svg>
                        <span>{item.label}</span>
                    </a>
                {/each}
            </nav>
        </aside>

        <main class="dashboard-content">
            {@render children()}
        </main>
    </div>
{/if}

<style>
    .auth-loading {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: calc(100dvh - var(--header-height));
    }

    .auth-spinner {
        width: 2rem;
        height: 2rem;
        border: 2px solid var(--border-default);
        border-top-color: var(--accent);
        border-radius: var(--radius-full);
        animation: spin 0.6s linear infinite;
    }

    @keyframes spin {
        to {
            transform: rotate(360deg);
        }
    }

    .dashboard {
        display: flex;
        min-height: calc(100dvh - var(--header-height));
    }

    .sidebar {
        width: var(--sidebar-width);
        flex-shrink: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border-right: 1px solid var(--border-default);
        position: sticky;
        top: var(--actual-header-height, var(--header-height));
        height: calc(100dvh - var(--actual-header-height, var(--header-height)));
        overflow-y: auto;
        transition:
            top var(--duration-normal) var(--ease-out),
            height var(--duration-normal) var(--ease-out);
    }

    .sidebar-user {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        padding-bottom: var(--space-4);
        border-bottom: 1px solid var(--border-default);
    }

    .sidebar-user-info {
        display: flex;
        flex-direction: column;
        gap: 2px;
        min-width: 0;
        align-items: flex-start;
    }

    .sidebar-user-name {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        max-width: 100%;
    }

    .sidebar-nav {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .sidebar-link {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        text-decoration: none;
    }

    .sidebar-link:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .sidebar-link svg {
        flex-shrink: 0;
    }

    .dashboard-content {
        flex: 1;
        min-width: 0;
        padding: var(--space-6) var(--space-8);
    }

    .sidebar-toggle {
        display: none;
        position: fixed;
        bottom: var(--space-4);
        right: var(--space-4);
        z-index: var(--z-sticky);
        width: 3rem;
        height: 3rem;
        align-items: center;
        justify-content: center;
        background: var(--accent);
        color: var(--accent-contrast);
        border-radius: var(--radius-full);
        box-shadow: var(--shadow-lg);
    }

    .sidebar-overlay {
        display: none;
    }

    @media (max-width: 768px) {
        .sidebar {
            position: fixed;
            top: var(--header-height);
            left: 0;
            bottom: 0;
            z-index: var(--z-overlay);
            transform: translateX(-100%);
            transition: transform var(--duration-moderate) var(--ease-out);
            box-shadow: var(--shadow-xl);
        }

        .sidebar.open {
            transform: translateX(0);
        }

        .sidebar-toggle {
            display: flex;
        }

        .sidebar-overlay {
            display: block;
            position: fixed;
            inset: 0;
            top: var(--header-height);
            background: var(--bg-overlay);
            z-index: calc(var(--z-overlay) - 1);
            animation: fade-in var(--duration-fast) var(--ease-out);
        }

        .dashboard-content {
            padding: var(--space-5);
        }
    }
</style>
