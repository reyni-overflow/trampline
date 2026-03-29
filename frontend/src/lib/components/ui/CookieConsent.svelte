<script lang="ts">
    import { onMount } from 'svelte';
    import Button from './Button.svelte';
    import Toggle from './Toggle.svelte';
    import { t } from '$lib/i18n';
    import { cookieConsent, type CookieCategories } from '$lib/stores/cookie-consent';

    let status = $state<'pending' | 'accepted' | 'declined'>('accepted');
    let shaking = $state(false);
    let expanded = $state(false);
    let categories = $state<CookieCategories>({
        required: true,
        analytics: false,
        marketing: false
    });

    const unsub = cookieConsent.subscribe((s) => {
        status = s.status;
        shaking = s.shaking;
        if (s.status === 'accepted') {
            categories = { ...s.categories };
        }
    });

    onMount(() => {
        return unsub;
    });

    function accept() {
        cookieConsent.accept(categories);
        expanded = false;
    }

    function acceptAll() {
        categories = { required: true, analytics: true, marketing: true };
        cookieConsent.accept({ required: true, analytics: true, marketing: true });
        expanded = false;
    }

    function decline() {
        cookieConsent.decline();
        expanded = false;
    }

    function reconsider() {
        cookieConsent.reconsider();
    }
</script>

{#if status === 'pending'}
    <div class="cookie-banner" class:shaking>
        <div class="cookie-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24" width="28" height="28" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10" />
                <circle cx="8" cy="9" r="1.5" fill="currentColor" stroke="none" />
                <circle cx="15" cy="7" r="1" fill="currentColor" stroke="none" />
                <circle cx="10" cy="14" r="1" fill="currentColor" stroke="none" />
                <circle cx="16" cy="13" r="1.5" fill="currentColor" stroke="none" />
                <circle cx="13" cy="17" r="1" fill="currentColor" stroke="none" />
                <path d="M21.5 10.5c-2 0-3-1.5-3-3 0-1-.5-1.5-1.5-1.5" />
            </svg>
        </div>
        <div class="cookie-body">
            <p class="cookie-title">{$t('cookie.title')}</p>
            <p class="cookie-text">
                {$t('cookie.message')}
                <a href="/privacy" class="cookie-link">{$t('cookie.learnMore')}</a>
            </p>

            {#if expanded}
                <div class="cookie-categories">
                    <div class="category">
                        <div class="category-header">
                            <Toggle checked={true} disabled={true} />
                            <div class="category-info">
                                <span class="category-name">{$t('cookie.catRequired')}</span>
                                <span class="category-desc">{$t('cookie.catRequiredDesc')}</span>
                            </div>
                        </div>
                    </div>
                    <div class="category">
                        <div class="category-header">
                            <Toggle bind:checked={categories.analytics} />
                            <div class="category-info">
                                <span class="category-name">{$t('cookie.catAnalytics')}</span>
                                <span class="category-desc">{$t('cookie.catAnalyticsDesc')}</span>
                            </div>
                        </div>
                    </div>
                    <div class="category">
                        <div class="category-header">
                            <Toggle bind:checked={categories.marketing} />
                            <div class="category-info">
                                <span class="category-name">{$t('cookie.catMarketing')}</span>
                                <span class="category-desc">{$t('cookie.catMarketingDesc')}</span>
                            </div>
                        </div>
                    </div>
                </div>
            {/if}

            <div class="cookie-actions">
                <Button size="sm" variant="ghost" onclick={decline}>{$t('cookie.decline')}</Button>
                <Button size="sm" variant="ghost" onclick={() => { expanded = !expanded; }}>
                    {expanded ? $t('cookie.hideSettings') : $t('cookie.showSettings')}
                </Button>
                {#if expanded}
                    <Button size="sm" onclick={accept}>{$t('cookie.acceptSelected')}</Button>
                {/if}
                <Button size="sm" onclick={acceptAll}>{$t('cookie.acceptAll')}</Button>
            </div>
        </div>
    </div>
{/if}

{#if status === 'declined'}
    <div class="cookie-overlay">
        <div class="cookie-overlay-card">
            <div class="overlay-icon" aria-hidden="true">
                <svg viewBox="0 0 24 24" width="48" height="48" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10" />
                    <circle cx="8" cy="9" r="1.5" fill="currentColor" stroke="none" />
                    <circle cx="15" cy="7" r="1" fill="currentColor" stroke="none" />
                    <circle cx="10" cy="14" r="1" fill="currentColor" stroke="none" />
                    <circle cx="16" cy="13" r="1.5" fill="currentColor" stroke="none" />
                    <circle cx="13" cy="17" r="1" fill="currentColor" stroke="none" />
                    <path d="M21.5 10.5c-2 0-3-1.5-3-3 0-1-.5-1.5-1.5-1.5" />
                </svg>
            </div>
            <h2 class="overlay-title">{$t('cookie.declinedTitle')}</h2>
            <p class="overlay-text">{$t('cookie.declinedMessage')}</p>
            <div class="overlay-actions">
                <Button onclick={reconsider}>{$t('cookie.reconsider')}</Button>
            </div>
        </div>
    </div>
{/if}

<style>
    .cookie-banner {
        position: fixed;
        bottom: var(--space-4);
        left: var(--space-4);
        right: var(--space-4);
        max-width: 28rem;
        display: flex;
        gap: var(--space-3);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        box-shadow: var(--shadow-xl);
        z-index: 10001;
        animation: slide-up var(--duration-moderate) var(--ease-out) both;
    }

    .cookie-banner.shaking {
        animation: slide-up var(--duration-moderate) var(--ease-out) both, cookie-shake 0.5s var(--ease-out);
    }

    @keyframes cookie-shake {
        0%, 100% { transform: translateX(0); }
        10% { transform: translateX(-8px); }
        20% { transform: translateX(8px); }
        30% { transform: translateX(-6px); }
        40% { transform: translateX(6px); }
        50% { transform: translateX(-4px); }
        60% { transform: translateX(4px); }
        70% { transform: translateX(-2px); }
        80% { transform: translateX(2px); }
        90% { transform: translateX(-1px); }
    }

    .cookie-icon {
        flex-shrink: 0;
        color: var(--text-secondary);
        margin-top: var(--space-1);
    }

    .cookie-body {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .cookie-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
    }

    .cookie-text {
        font-size: var(--font-xs);
        color: var(--text-secondary);
        line-height: var(--leading-relaxed);
    }

    .cookie-link {
        color: var(--accent);
        text-decoration: underline;
    }

    .cookie-categories {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
        padding: var(--space-3);
        background: var(--bg-secondary);
        border-radius: var(--radius-lg);
        animation: fade-in var(--duration-fast) var(--ease-out);
    }

    .category-header {
        display: flex;
        align-items: flex-start;
        gap: var(--space-3);
    }

    .category-info {
        display: flex;
        flex-direction: column;
        gap: 0.125rem;
    }

    .category-name {
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        color: var(--text-primary);
    }

    .category-desc {
        font-size: 0.6875rem;
        color: var(--text-tertiary);
        line-height: var(--leading-relaxed);
    }

    .cookie-actions {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-2);
        margin-top: var(--space-1);
    }

    /* Overlay for declined state */
    .cookie-overlay {
        position: fixed;
        inset: 0;
        z-index: 10000;
        display: flex;
        align-items: center;
        justify-content: center;
        background: rgba(0, 0, 0, 0.6);
        backdrop-filter: blur(8px);
        -webkit-backdrop-filter: blur(8px);
        animation: fade-in var(--duration-moderate) var(--ease-out);
    }

    .cookie-overlay-card {
        max-width: 26rem;
        margin: var(--space-4);
        padding: var(--space-8);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        box-shadow: var(--shadow-xl);
        text-align: center;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-3);
        animation: scale-in var(--duration-moderate) var(--ease-out);
    }

    .overlay-icon {
        color: var(--text-secondary);
    }

    .overlay-title {
        font-size: var(--font-lg);
        font-weight: var(--weight-bold);
        color: var(--text-primary);
    }

    .overlay-text {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-relaxed);
    }

    .overlay-actions {
        margin-top: var(--space-2);
    }

    @media (max-width: 640px) {
        .cookie-banner {
            flex-direction: column;
            align-items: stretch;
            bottom: var(--space-2);
            left: var(--space-2);
            right: var(--space-2);
        }

        .cookie-icon {
            text-align: center;
        }

        .cookie-actions {
            justify-content: center;
        }
    }

    @media (prefers-reduced-motion: reduce) {
        .cookie-banner.shaking {
            animation: none;
            outline: 2px solid var(--accent);
            outline-offset: 2px;
        }
    }
</style>
