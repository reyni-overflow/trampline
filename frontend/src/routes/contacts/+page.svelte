<script lang="ts">
    import MapView from '$lib/components/ui/MapView.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import { toast } from '$lib/stores/toast';
    import { t } from '$lib/i18n';
    import { env } from '$env/dynamic/public';

    const PUBLIC_CONTACT_EMAIL = env.PUBLIC_CONTACT_EMAIL ?? '';

    let phoneRevealed = $state(false);

    function revealPhone() {
        phoneRevealed = true;
        toast.tip($t('contacts.phoneToast'));
    }
</script>

<svelte:head>
    <title>{$t('contacts.pageTitle')}</title>
</svelte:head>

<div class="contacts-page container--narrow">
    <h1 class="contacts-title">{$t('contacts.title')}</h1>
    <p class="contacts-subtitle">{$t('contacts.subtitle')}</p>

    <div class="contacts-map">
        <MapView
            center={[25.0, -71.0]}
            zoom={6}
            height="20rem"
            markers={[{
                id: 'hq',
                lat: 25.0,
                lng: -71.0,
                title: $t('contacts.hqTitle'),
                company: $t('contacts.hqCompany'),
                tags: [$t('contacts.hqTag')],
                type: 'Work',
                link: '/about'
            }]}
        />
    </div>

    <div class="contacts-info">
        <div class="info-card">
            <div class="info-icon">
                <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/>
                </svg>
            </div>
            <div class="info-content">
                <h3>{$t('contacts.addressTitle')}</h3>
                <p>{$t('contacts.addressText')}</p>
                <span class="info-note">{$t('contacts.addressNote')}</span>
            </div>
        </div>

        <div class="info-card">
            <div class="info-icon">
                <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                    <rect width="20" height="16" x="2" y="4" rx="2"/><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/>
                </svg>
            </div>
            <div class="info-content">
                <h3>{$t('contacts.emailTitle')}</h3>
                <a href="mailto:{PUBLIC_CONTACT_EMAIL}" class="info-link">{PUBLIC_CONTACT_EMAIL}</a>
                <span class="info-note">{$t('contacts.emailNote')}</span>
            </div>
        </div>

        <div class="info-card">
            <div class="info-icon">
                <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/>
                </svg>
            </div>
            <div class="info-content">
                <h3>{$t('contacts.phoneTitle')}</h3>
                {#if phoneRevealed}
                    <span class="phone-number">{$t('contacts.phoneRevealed')}</span>
                    <span class="info-note">{$t('contacts.phoneNote')}</span>
                {:else}
                    <button class="reveal-btn" type="button" onclick={revealPhone}>{$t('contacts.revealPhone')}</button>
                    <span class="info-note">{$t('contacts.revealNote')}</span>
                {/if}
            </div>
        </div>

        <div class="info-card">
            <div class="info-icon">
                <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                </svg>
            </div>
            <div class="info-content">
                <h3>{$t('contacts.hoursTitle')}</h3>
                <p>{$t('contacts.hoursText')}</p>
                <span class="info-note">{$t('contacts.hoursNote')}</span>
            </div>
        </div>
    </div>

    <div class="contacts-cta">
        <p>{$t('contacts.ctaText')}</p>
        <Button href="mailto:{PUBLIC_CONTACT_EMAIL}" size="lg">{$t('contacts.writeUs')}</Button>
    </div>
</div>

<style>
    .contacts-page {
        padding-top: var(--space-8);
        padding-bottom: var(--space-16);
    }

    .contacts-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-2);
    }

    .contacts-subtitle {
        font-size: var(--font-base);
        color: var(--text-secondary);
        margin-bottom: var(--space-6);
    }

    .contacts-map {
        position: relative;
        z-index: 0;
        margin-bottom: var(--space-8);
        border-radius: var(--radius-xl);
        overflow: hidden;
        border: 1px solid var(--border-default);
    }

    .contacts-info {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
        margin-bottom: var(--space-10);
    }

    .info-card {
        display: flex;
        gap: var(--space-4);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .info-icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.75rem;
        height: 2.75rem;
        background: var(--accent-subtle);
        color: var(--accent);
        border-radius: var(--radius-md);
        flex-shrink: 0;
    }

    .info-content {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .info-content h3 {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
    }

    .info-content p {
        font-size: var(--font-base);
        color: var(--text-secondary);
    }

    .info-link {
        font-size: var(--font-base);
        color: var(--accent);
        font-weight: var(--weight-medium);
    }

    .info-link:hover {
        text-decoration: underline;
    }

    .info-note {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        font-style: italic;
    }

    .phone-number {
        font-size: var(--font-base);
        font-family: var(--font-mono);
        color: var(--text-secondary);
    }

    .reveal-btn {
        font-size: var(--font-sm);
        color: var(--accent);
        font-weight: var(--weight-medium);
        cursor: pointer;
        width: fit-content;
    }

    .reveal-btn:hover {
        text-decoration: underline;
    }

    .contacts-cta {
        text-align: center;
        padding: var(--space-8);
        background: var(--bg-secondary);
        border-radius: var(--radius-xl);
    }

    .contacts-cta p {
        font-size: var(--font-base);
        color: var(--text-secondary);
        margin-bottom: var(--space-4);
    }
</style>
