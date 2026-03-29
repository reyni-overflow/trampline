<script lang="ts">
    import { t } from '$lib/i18n';
    import { env } from '$env/dynamic/public';

    const PUBLIC_CONTACT_EMAIL = env.PUBLIC_CONTACT_EMAIL ?? '';
</script>

<svelte:head>
    <title>{$t('privacy.pageTitle')}</title>
</svelte:head>

<div class="legal-page container--narrow">
    <h1 class="legal-title">{$t('privacy.title')}</h1>
    <p class="legal-date">{$t('privacy.lastUpdateLabel', { date: $t('privacy.lastUpdate') })}</p>

    <div class="legal-content">
        <section>
            <h2>{$t('privacy.s1Title')}</h2>
            <p>{$t('privacy.s1p1')}</p>
            <p>{$t('privacy.s1p2')}</p>
        </section>

        <section>
            <h2>{$t('privacy.s2Title')}</h2>
            <p>{$t('privacy.s2p1')}</p>
            <ul>
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                <li>{@html $t('privacy.s2li1')}</li>
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                <li>{@html $t('privacy.s2li2')}</li>
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                <li>{@html $t('privacy.s2li3')}</li>
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                <li>{@html $t('privacy.s2li4')}</li>
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                <li>{@html $t('privacy.s2li5')}</li>
            </ul>
        </section>

        <section>
            <h2>{$t('privacy.s3Title')}</h2>
            <p>{$t('privacy.s3p1')}</p>
            <ul>
                <li>{$t('privacy.s3li1')}</li>
                <li>{$t('privacy.s3li2')}</li>
                <li>{$t('privacy.s3li3')}</li>
                <li>{$t('privacy.s3li4')}</li>
                <li>{$t('privacy.s3li5')}</li>
            </ul>
        </section>

        <section>
            <h2>{$t('privacy.s4Title')}</h2>
            <p>{$t('privacy.s4p1')}</p>
            <ul>
                <li>{$t('privacy.s4li1')}</li>
                <li>{$t('privacy.s4li2')}</li>
                <li>{$t('privacy.s4li3')}</li>
                <li>{$t('privacy.s4li4')}</li>
            </ul>
            <p>{$t('privacy.s4p2')}</p>
        </section>

        <section>
            <h2>{$t('privacy.s5Title')}</h2>
            <p>{$t('privacy.s5p1')}</p>
            <ul>
                <li>{$t('privacy.s5li1')}</li>
                <li>{$t('privacy.s5li2')}</li>
                <li>{$t('privacy.s5li3')}</li>
                <li>{$t('privacy.s5li4')}</li>
                <li>{$t('privacy.s5li5')}</li>
            </ul>
        </section>

        <section>
            <h2>{$t('privacy.s6Title')}</h2>
            <p>{$t('privacy.s6p1')}</p>
        </section>

        <section>
            <h2>{$t('privacy.s7Title')}</h2>
            <p>{$t('privacy.s7p1')}</p>
        </section>

        <section>
            <h2>{$t('privacy.s8Title')}</h2>
            <p>{$t('privacy.s8p1')}</p>
            <p>Email: <a href="mailto:{PUBLIC_CONTACT_EMAIL}">{PUBLIC_CONTACT_EMAIL}</a></p>
        </section>
    </div>
</div>

<style>
    .legal-page {
        padding-top: var(--space-8);
        padding-bottom: var(--space-16);
    }

    .legal-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-2);
    }

    .legal-date {
        font-size: var(--font-sm);
        color: var(--text-tertiary);
        margin-bottom: var(--space-8);
    }

    .legal-content {
        display: flex;
        flex-direction: column;
        gap: var(--space-8);
    }

    .legal-content h2 {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
        margin-bottom: var(--space-3);
    }

    .legal-content p {
        font-size: var(--font-base);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        margin-bottom: var(--space-3);
    }

    .legal-content p:last-child {
        margin-bottom: 0;
    }

    .legal-content ul {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        padding-left: var(--space-5);
        list-style: none;
    }

    .legal-content li {
        position: relative;
        font-size: var(--font-base);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        padding-left: var(--space-3);
    }

    .legal-content li::before {
        content: '';
        position: absolute;
        left: 0;
        top: 0.6em;
        width: 0.375rem;
        height: 0.375rem;
        background: var(--accent);
        border-radius: 50%;
    }

    .legal-content a {
        color: var(--accent);
    }

    .legal-content a:hover {
        text-decoration: underline;
    }
</style>
