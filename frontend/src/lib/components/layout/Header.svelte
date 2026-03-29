<script lang="ts">
    import ThemeToggle from '$lib/components/ui/ThemeToggle.svelte';
    import AccentPicker from '$lib/components/ui/AccentPicker.svelte';
    import NotificationBell from '$lib/components/ui/NotificationBell.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Dropdown from '$lib/components/ui/Dropdown.svelte';
    import { user as userStore, isAuthenticated as isAuthStore } from '$lib/stores/auth';
    import { authModal } from '$lib/stores/auth-modal';
    import { cookieConsent } from '$lib/stores/cookie-consent';
    import { favorites } from '$lib/stores/favorites';
    import { comparison } from '$lib/stores/comparison';
    import { notifications } from '$lib/stores/notifications';
    import { authApi } from '$lib/api/auth';
    import { stopConnection } from '$lib/api/signalr';
    import { toast } from '$lib/stores/toast';
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';

    interface Props {
        onOpenCommandPalette?: () => void;
    }

    let { onOpenCommandPalette }: Props = $props();

    let scrolled = $state(false);
    let mobileMenuOpen = $state(false);

    let isAuthenticated = $state(false);
    let currentUser = $state<{ nickname: string; avatar: string | null; role: string }>({
        nickname: '',
        avatar: null,
        role: ''
    });

    const unsubAuth = isAuthStore.subscribe((v) => (isAuthenticated = v));
    const unsubUser = userStore.subscribe((v) => {
        if (v) currentUser = { nickname: v.nickname, avatar: v.avatar, role: v.role };
    });

    onDestroy(() => {
        unsubAuth();
        unsubUser();
    });

    async function handleLogout() {
        try {
            await authApi.logout();
        } catch {
            /* ignored */
        }

        stopConnection();
        userStore.clear();
        favorites.clear();
        comparison.clear();
        notifications.clear();
        toast.success($t('auth.loggedOut'));
        window.location.href = '/';
    }

    function handleScroll() {
        scrolled = window.scrollY > 20;
    }

    const navLinks = $derived([
        { href: '/jobs', label: $t('nav.jobs') },
        { href: '/companies', label: $t('nav.companies') },
        { href: '/events', label: $t('nav.events') },
        { href: '/mentorships', label: $t('nav.mentorships') },
        { href: '/map', label: $t('nav.map') }
    ]);

    function guardNav(e: MouseEvent, href: string) {
        if (!cookieConsent.isPathAllowed(href)) {
            e.preventDefault();
            cookieConsent.shake();
        }
    }
</script>

<svelte:window onscroll={handleScroll} />

<header class="header" class:scrolled>
    <div class="header-inner container">
        <a href="/" class="logo-link" aria-label={$t('header.logoAria')}>
            <span class="logo-icon" aria-hidden="true">
                <svg viewBox="0 0 200 200" width="32" height="32">
                    <path
                        d="M 90 160 L 90 80"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="20"
                        stroke-linecap="round"
                    />
                    <path
                        d="M 30 100 L 135 65"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="20"
                        stroke-linecap="round"
                    />
                    <circle cx="165" cy="55" r="14" fill="currentColor" />
                </svg>
            </span>
            <span class="logo-text">{$t('brand.name')}</span>
        </a>

        <nav class="nav-desktop" aria-label={$t('header.mainNav')}>
            {#each navLinks as link (link.href)}
                <a href={link.href} class="nav-link" onclick={(e) => guardNav(e, link.href)}
                    >{link.label}</a
                >
            {/each}
        </nav>

        <div class="actions">
            <button
                class="search-trigger"
                onclick={() => onOpenCommandPalette?.()}
                aria-label={$t('header.searchAria')}
                title={$t('header.searchAria')}
            >
                <svg
                    viewBox="0 0 24 24"
                    width="20"
                    height="20"
                    stroke="currentColor"
                    stroke-width="2"
                    fill="none"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <circle cx="11" cy="11" r="8" /><line x1="21" y1="21" x2="16.65" y2="16.65" />
                </svg>
                <kbd class="search-kbd">⌘K</kbd>
            </button>

            <ThemeToggle />
            <AccentPicker />

            {#if isAuthenticated}
                <NotificationBell />
                <Dropdown align="right">
                    {#snippet trigger()}
                        <Avatar
                            src={currentUser.avatar}
                            name={currentUser.nickname}
                            size={36}
                            clickable
                        />
                    {/snippet}
                    <div class="user-dropdown">
                        <div class="user-info">
                            <span class="user-name">{currentUser.nickname}</span>
                            <span class="user-role"
                                >{currentUser.role === 'Worker'
                                    ? $t('roles.worker')
                                    : currentUser.role === 'Employee'
                                      ? $t('roles.employee')
                                      : currentUser.role === 'Admin'
                                        ? $t('roles.admin')
                                        : currentUser.role}</span
                            >
                        </div>
                        <div class="dropdown-divider"></div>
                        <a href="/dashboard" class="dropdown-item">{$t('header.myProfile')}</a>
                        <a href="/settings" class="dropdown-item">{$t('header.settings')}</a>
                        <div class="dropdown-divider"></div>
                        <button class="dropdown-item danger" type="button" onclick={handleLogout}
                            >{$t('auth.logout')}</button
                        >
                    </div>
                </Dropdown>
            {:else}
                <button class="login-btn" type="button" onclick={() => authModal.open()}
                    >{$t('auth.login')}</button
                >
            {/if}

            <button
                class="burger"
                onclick={() => (mobileMenuOpen = !mobileMenuOpen)}
                aria-label={$t('header.menuAria')}
                aria-expanded={mobileMenuOpen}
            >
                <span class="burger-line" class:open={mobileMenuOpen}></span>
                <span class="burger-line" class:open={mobileMenuOpen}></span>
                <span class="burger-line" class:open={mobileMenuOpen}></span>
            </button>
        </div>
    </div>

    {#if mobileMenuOpen}
        <!-- svelte-ignore a11y_click_events_have_key_events -->
        <!-- svelte-ignore a11y_no_static_element_interactions -->
        <div class="mobile-overlay" onclick={() => (mobileMenuOpen = false)}></div>
        <nav class="mobile-menu" aria-label={$t('header.mobileNav')}>
            {#each navLinks as link (link.href)}
                <a
                    href={link.href}
                    class="mobile-link"
                    onclick={(e) => {
                        if (!cookieConsent.isPathAllowed(link.href)) {
                            e.preventDefault();
                            cookieConsent.shake();
                            return;
                        }
                        mobileMenuOpen = false;
                    }}>{link.label}</a
                >
            {/each}
            {#if !isAuthenticated}
                <button
                    class="mobile-link login"
                    type="button"
                    onclick={() => {
                        mobileMenuOpen = false;
                        authModal.open();
                    }}>{$t('auth.login')}</button
                >
            {/if}
        </nav>
    {/if}
</header>

<style>
    .header {
        position: sticky;
        top: 0;
        z-index: var(--z-sticky);
        background: color-mix(in srgb, var(--bg-primary) 80%, transparent);
        backdrop-filter: blur(0.75rem);
        -webkit-backdrop-filter: blur(0.75rem);
        border-bottom: 1px solid var(--border-subtle);
        transition:
            height var(--duration-normal) var(--ease-out),
            border-color var(--duration-normal) var(--ease-in-out);
    }

    .header.scrolled {
        border-bottom-color: var(--border-default);
    }

    .header-inner {
        display: flex;
        align-items: center;
        justify-content: space-between;
        height: var(--header-height);
        gap: var(--space-4);
        transition: height var(--duration-normal) var(--ease-out);
    }

    .header.scrolled .header-inner {
        height: var(--header-height-scrolled);
    }

    .logo-link {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        text-decoration: none;
        color: var(--text-primary);
        flex-shrink: 0;
    }

    .logo-link:hover {
        color: var(--accent);
    }

    .logo-text {
        font-size: var(--font-lg);
        font-weight: var(--weight-extrabold);
        letter-spacing: -0.02em;
    }

    .nav-desktop {
        display: flex;
        align-items: center;
        gap: var(--space-1);
    }

    .nav-link {
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        text-decoration: none;
    }

    .nav-link:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .actions {
        display: flex;
        align-items: center;
        gap: var(--space-1);
    }

    .search-trigger {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        height: 2.25rem;
        padding: 0 var(--space-3);
        border-radius: var(--radius-md);
        color: var(--text-secondary);
        transition: var(--transition-colors);
        cursor: pointer;
    }

    .search-trigger:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .search-kbd {
        padding: 0.125rem var(--space-2);
        font-size: 0.6875rem;
        font-family: inherit;
        color: var(--text-tertiary);
        background: var(--bg-tertiary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        line-height: 1.4;
    }

    .login-btn {
        display: flex;
        align-items: center;
        height: 2.25rem;
        padding: 0 var(--space-4);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        background: var(--accent);
        color: var(--accent-contrast);
        border-radius: var(--radius-md);
        transition: var(--transition-colors), var(--transition-transform);
        text-decoration: none;
    }

    .login-btn:hover {
        background: var(--accent-hover);
        color: var(--accent-contrast);
    }

    .login-btn:active {
        transform: scale(0.97);
    }

    .user-dropdown {
        min-width: 11.25rem;
    }

    .user-info {
        padding: var(--space-2) var(--space-3);
    }

    .user-name {
        display: block;
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
    }

    .user-role {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .dropdown-divider {
        height: 1px;
        background: var(--border-default);
        margin: var(--space-1) 0;
    }

    .dropdown-item {
        display: block;
        width: 100%;
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-sm);
        color: var(--text-primary);
        border-radius: var(--radius-md);
        text-decoration: none;
        text-align: left;
        transition: var(--transition-colors);
        cursor: pointer;
    }

    .dropdown-item:hover {
        background: var(--bg-tertiary);
    }

    .dropdown-item.danger {
        color: var(--color-error);
    }

    .dropdown-item.danger:hover {
        background: var(--color-error-subtle);
    }

    .burger {
        display: none;
        flex-direction: column;
        justify-content: center;
        gap: 0.3125rem;
        width: 2.5rem;
        height: 2.5rem;
        padding: 0.625rem;
    }

    .burger-line {
        display: block;
        width: 100%;
        height: 2px;
        background: var(--text-primary);
        border-radius: 1px;
        transition:
            transform var(--duration-moderate) var(--ease-spring),
            opacity var(--duration-fast) var(--ease-in-out);
    }

    .burger-line.open:nth-child(1) {
        transform: translateY(0.4375rem) rotate(45deg);
    }
    .burger-line.open:nth-child(2) {
        opacity: 0;
    }
    .burger-line.open:nth-child(3) {
        transform: translateY(-0.4375rem) rotate(-45deg);
    }

    .mobile-overlay {
        position: fixed;
        inset: 0;
        top: var(--header-height);
        background: var(--bg-overlay);
        z-index: var(--z-overlay);
        animation: fade-in var(--duration-fast) var(--ease-out);
    }

    .mobile-menu {
        position: fixed;
        top: var(--header-height);
        right: 0;
        width: min(17.5rem, 80vw);
        height: calc(100dvh - var(--header-height));
        padding: var(--space-4);
        background: var(--bg-elevated);
        border-left: 1px solid var(--border-default);
        z-index: var(--z-overlay);
        overflow-y: auto;
        animation: slide-in-right var(--duration-moderate) var(--ease-spring);
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .mobile-link {
        display: block;
        padding: var(--space-3) var(--space-4);
        font-size: var(--font-base);
        font-weight: var(--weight-medium);
        color: var(--text-primary);
        border-radius: var(--radius-md);
        text-decoration: none;
        transition: var(--transition-colors);
    }

    .mobile-link:hover {
        background: var(--bg-tertiary);
    }

    .mobile-link.login {
        margin-top: auto;
        background: var(--accent);
        color: var(--accent-contrast);
        text-align: center;
    }

    @media (max-width: 768px) {
        .nav-desktop {
            display: none;
        }
        .login-btn {
            display: none;
        }
        .search-trigger {
            display: none;
        }
        .burger {
            display: flex;
        }
    }

    @media (min-width: 769px) {
        .burger {
            display: none;
        }
        .mobile-overlay,
        .mobile-menu {
            display: none;
        }
    }
</style>
