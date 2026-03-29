<script lang="ts">
    import { page } from '$app/state';
    import Button from '$lib/components/ui/Button.svelte';
    import { t } from '$lib/i18n';

    const errors = $derived<Record<number, { title: string; description: string; icon: string }>>({
        400: {
            title: $t('errors.400Title'),
            description: $t('errors.400Desc'),
            icon: '<circle cx="12" cy="12" r="10"/><path d="m15 9-6 6"/><path d="m9 9 6 6"/>'
        },
        401: {
            title: $t('errors.401Title'),
            description: $t('errors.401Desc'),
            icon: '<rect width="18" height="11" x="3" y="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>'
        },
        403: {
            title: $t('errors.403Title'),
            description: $t('errors.403Desc'),
            icon: '<circle cx="12" cy="12" r="10"/><path d="m4.9 4.9 14.2 14.2"/>'
        },
        404: {
            title: $t('errors.404Title'),
            description: $t('errors.404Desc'),
            icon: '<circle cx="12" cy="12" r="10"/><path d="M12 16v.01"/><path d="M12 8v4"/>'
        },
        408: {
            title: $t('errors.408Title'),
            description: $t('errors.408Desc'),
            icon: '<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>'
        },
        500: {
            title: $t('errors.500Title'),
            description: $t('errors.500Desc'),
            icon: '<path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>'
        },
        503: {
            title: $t('errors.503Title'),
            description: $t('errors.503Desc'),
            icon: '<path d="M12 2v4"/><path d="m6.34 6.34 2.83 2.83"/><path d="M2 12h4"/><path d="m6.34 17.66 2.83-2.83"/><path d="M12 18v4"/><path d="m17.66 17.66-2.83-2.83"/><path d="M18 12h4"/><path d="m17.66 6.34-2.83 2.83"/>'
        }
    });

    const status = $derived(page.status);
    const msg = $derived(page.error?.message || '');

    const info = $derived(errors[status] || {
        title: $t('errors.fallbackTitle'),
        description: msg || $t('errors.fallbackDesc'),
        icon: '<circle cx="12" cy="12" r="10"/><path d="M12 16v.01"/><path d="M12 8v4"/>'
    });

    function goBack() {
        if (window.history.length > 1) {
            window.history.back();
        } else {
            window.location.href = '/';
        }
    }
</script>

<svelte:head>
    <title>{status} · {info.title} | {$t('brand.name')}</title>
</svelte:head>

<div class="error-page">
    <div class="error-content">
        <div class="error-icon">
            <svg viewBox="0 0 24 24" width="64" height="64" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                {@html info.icon}
            </svg>
        </div>
        <span class="error-code">{status}</span>
        <h1 class="error-title">{info.title}</h1>
        <p class="error-description">{info.description}</p>
        {#if msg && msg !== info.description}
            <p class="error-detail">{msg}</p>
        {/if}
        <div class="error-actions">
            <Button onclick={goBack} variant="outline" size="lg">{$t('errors.back')}</Button>
            <Button href="/" size="lg">{$t('errors.home')}</Button>
        </div>
    </div>
</div>

<style>
    .error-page {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: calc(100dvh - var(--header-height) - 3.25rem);
        padding: var(--space-8);
    }

    .error-content {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        max-width: 30rem;
    }

    .error-icon {
        color: var(--text-tertiary);
        margin-bottom: var(--space-4);
        animation: fade-in var(--duration-moderate) var(--ease-out);
    }

    .error-code {
        font-size: 5rem;
        font-weight: var(--weight-extrabold);
        line-height: 1;
        color: var(--accent);
        letter-spacing: -0.04em;
        animation: slide-up var(--duration-slow) var(--ease-out);
    }

    .error-title {
        font-size: var(--font-xl);
        font-weight: var(--weight-bold);
        margin-top: var(--space-3);
        margin-bottom: var(--space-2);
        animation: slide-up var(--duration-slow) var(--ease-out) 50ms both;
    }

    .error-description {
        font-size: var(--font-base);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        animation: slide-up var(--duration-slow) var(--ease-out) 100ms both;
    }

    .error-detail {
        margin-top: var(--space-3);
        padding: var(--space-3) var(--space-4);
        font-size: var(--font-sm);
        font-family: var(--font-mono);
        color: var(--text-tertiary);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        word-break: break-word;
        animation: slide-up var(--duration-slow) var(--ease-out) 150ms both;
    }

    .error-actions {
        display: flex;
        gap: var(--space-3);
        margin-top: var(--space-8);
        animation: slide-up var(--duration-slow) var(--ease-out) 200ms both;
    }
</style>
