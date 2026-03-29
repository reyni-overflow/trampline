<script lang="ts">
    import '$styles/global.css';
    import Header from '$lib/components/layout/Header.svelte';
    import Footer from '$lib/components/layout/Footer.svelte';
    import ToastManager from '$lib/components/ui/ToastManager.svelte';
    import AuthModal from '$lib/components/auth/AuthModal.svelte';
    import ForgotPasswordModal from '$lib/components/auth/ForgotPasswordModal.svelte';
    import CommandPalette from '$lib/components/ui/CommandPalette.svelte';
    import CookieConsent from '$lib/components/ui/CookieConsent.svelte';
    import { browser } from '$app/environment';
    import { beforeNavigate } from '$app/navigation';
    import { theme, accent } from '$lib/stores/theme';
    import { user } from '$lib/stores/auth';
    import { cookieConsent } from '$lib/stores/cookie-consent';
    import { locale, t } from '$lib/i18n';
    import { startConnection, stopConnection } from '$lib/api/signalr';
    import { onMount, onDestroy } from 'svelte';

    let { children } = $props();
    let commandPaletteOpen = $state(false);
    beforeNavigate(({ to, cancel }) => {
        if (to?.url?.pathname && !cookieConsent.isPathAllowed(to.url.pathname)) {
            cancel();
            cookieConsent.shake();
        }
    });

    onMount(async () => {
        theme.init();
        accent.init();
        locale.init();
        const u = await user.fetchUser();
        if (u) startConnection();

        if (browser && 'serviceWorker' in navigator && location.hostname !== 'localhost' && !location.hostname.endsWith('.localhost')) {
            navigator.serviceWorker.register('/sw.js');
        }
    });

    onDestroy(() => {
        stopConnection();
    });
</script>

<svelte:head>
    <link rel="icon" href="/favicon.svg" type="image/svg+xml" />
    <link rel="icon" href="/favicon.ico" sizes="48x48" />
    <link rel="apple-touch-icon" href="/apple-touch-icon.png" />
    <meta property="og:image" content="/og-image.png" />
    <meta property="og:image:width" content="1200" />
    <meta property="og:image:height" content="630" />
    <title>{$t('brand.name')} · {$t('brand.tagline')}</title>
</svelte:head>

<a href="#main-content" class="skip-link">{$t('a11y.skipToContent')}</a>

<div class="app">
    <Header onOpenCommandPalette={() => { commandPaletteOpen = true; }} />
    <main id="main-content" class="main">
        {@render children()}
    </main>
    <Footer />
</div>

<CommandPalette bind:open={commandPaletteOpen} />
<AuthModal />
<ForgotPasswordModal />
<CookieConsent />
<ToastManager />

<style>
    .app {
        display: flex;
        flex-direction: column;
        min-height: 100dvh;
    }

    .main {
        flex: 1;
    }


    :global(.skip-link) {
        position: fixed;
        top: -100%;
        left: var(--space-4);
        z-index: 1100;
        padding: var(--space-2) var(--space-4);
        background: var(--accent);
        color: var(--text-inverse);
        border-radius: var(--radius-md);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        text-decoration: none;
        transition: top var(--duration-fast) var(--ease-out);
    }

    :global(.skip-link:focus) {
        top: var(--space-4);
    }
</style>
