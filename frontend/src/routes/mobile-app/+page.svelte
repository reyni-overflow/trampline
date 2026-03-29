<script lang="ts">
    import { env } from '$env/dynamic/public';
    import { t } from '$lib/i18n';
    import Button from '$lib/components/ui/Button.svelte';
    import { onMount } from 'svelte';

    const targetUrl = env.PUBLIC_MOBILE_APP_LINK || '';

    onMount(() => {
        if (targetUrl) {
            window.location.href = targetUrl;
        }
    });
</script>

<svelte:head>
    <title>{$t('mobileApp.title')}</title>
    {#if targetUrl}
        <meta http-equiv="refresh" content="2;url={targetUrl}" />
    {/if}
</svelte:head>

<div class="mobile-app-page container--narrow">
    <div class="redirect-card">
        <svg class="redirect-icon" viewBox="0 0 24 24" width="48" height="48" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <rect x="5" y="2" width="14" height="20" rx="2" ry="2" />
            <line x1="12" y1="18" x2="12.01" y2="18" />
        </svg>

        <h1 class="redirect-title">{$t('mobileApp.redirecting')}</h1>
        <p class="redirect-desc">{$t('mobileApp.description')}</p>

        <div class="spinner-wrap">
            <svg class="spinner" viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10" opacity="0.25" />
                <path d="M12 2a10 10 0 0 1 10 10" stroke-linecap="round" />
            </svg>
        </div>

        <p class="redirect-fallback">{$t('mobileApp.manual')}</p>

        {#if targetUrl}
            <a href={targetUrl} rel="noopener noreferrer">
                <Button variant="primary" size="lg">{$t('mobileApp.open')}</Button>
            </a>
        {/if}
    </div>
</div>

<style>
    .mobile-app-page {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 60vh;
        padding: var(--space-8) var(--space-4);
    }

    .redirect-card {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        gap: var(--space-4);
        padding: var(--space-10);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        box-shadow: var(--shadow-lg);
        max-width: 28rem;
        width: 100%;
    }

    .redirect-icon {
        color: var(--accent);
    }

    .redirect-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        color: var(--text-primary);
    }

    .redirect-desc {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: 1.6;
    }

    .spinner-wrap {
        padding: var(--space-2);
    }

    .spinner {
        color: var(--accent);
        animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
        to { transform: rotate(360deg); }
    }

    .redirect-fallback {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }
</style>
