<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import { t } from '$lib/i18n';
    import { user as userStore, authReady } from '$lib/stores/auth';
    import { authModal } from '$lib/stores/auth-modal';
    import { goto } from '$app/navigation';
    import { onDestroy } from 'svelte';

    let { children } = $props();
    let sidebarOpen = $state(false);
    let isAdmin = $state(false);
    let ready = $state(false);

    const unsub = userStore.subscribe((v) => {
        isAdmin = v?.role === 'Admin';
    });
    const unsubReady = authReady.subscribe((v) => {
        ready = v;
    });
    onDestroy(() => {
        unsub();
        unsubReady();
    });

    $effect(() => {
        if (ready && !isAdmin && typeof window !== 'undefined') {
            goto('/');
            authModal.openLogin();
        }
    });

    let navItems = $derived([
        {
            href: '/admin',
            label: $t('admin.dashboard'),
            icon: '<rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18"/><path d="M9 21V9"/>'
        },
        {
            href: '/admin/users',
            label: $t('admin.users'),
            icon: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>'
        },
        {
            href: '/admin/verification',
            label: $t('admin.verification'),
            icon: '<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>'
        },
        {
            href: '/admin/moderation',
            label: $t('admin.moderation'),
            icon: '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10"/>'
        },
        {
            href: '/admin/tags',
            label: $t('admin.tags'),
            icon: '<path d="M12 2 2 7l10 5 10-5-10-5Z"/><path d="m2 17 10 5 10-5"/><path d="m2 12 10 5 10-5"/>'
        },
        {
            href: '/admin/curators',
            label: $t('admin.curators'),
            icon: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><line x1="19" y1="8" x2="19" y2="14"/><line x1="22" y1="11" x2="16" y2="11"/>'
        },
        {
            href: '/admin/audit',
            label: $t('admin.audit'),
            icon: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/>'
        },
        {
            href: '/admin/reviews',
            label: $t('admin.reviews'),
            icon: '<path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>'
        }
    ]);
</script>

<div class="admin">
    <button
        class="sidebar-toggle"
        type="button"
        onclick={() => (sidebarOpen = !sidebarOpen)}
        aria-label={$t('admin.menu')}
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

    <aside class="sidebar" class:open={sidebarOpen}>
        <div class="sidebar-header">
            <span class="sidebar-title">{$t('admin.title')}</span>
            <Badge variant="error" size="sm">Admin</Badge>
        </div>
        <nav class="sidebar-nav">
            {#each navItems as item (item.href)}
                <a href={item.href} class="sidebar-link" onclick={() => (sidebarOpen = false)}>
                    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
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

    <main class="admin-content">
        {@render children()}
    </main>
</div>

<style>
    .admin {
        display: flex;
        min-height: calc(100dvh - var(--header-height));
    }

    .sidebar {
        width: var(--sidebar-width);
        flex-shrink: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-5);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border-right: 1px solid var(--border-default);
        position: sticky;
        top: var(--header-height);
        height: calc(100dvh - var(--header-height));
        overflow-y: auto;
    }

    .sidebar-header {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        padding-bottom: var(--space-4);
        border-bottom: 1px solid var(--border-default);
    }
    .sidebar-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-bold);
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

    .admin-content {
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
        background: var(--color-error);
        color: var(--text-inverse);
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
        .admin-content {
            padding: var(--space-5);
        }
    }
</style>
