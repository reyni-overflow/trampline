<script lang="ts">
    import { t } from '$lib/i18n';
    import { env } from '$env/dynamic/public';

    const PUBLIC_CONTACT_EMAIL = env.PUBLIC_CONTACT_EMAIL ?? '';
</script>

<svelte:head>
    <title>{$t('terms.pageTitle')}</title>
</svelte:head>

<div class="legal-page container--narrow">
    <h1 class="legal-title">{$t('terms.title')}</h1>
    <p class="legal-date">{$t('terms.lastUpdateLabel', { date: $t('terms.lastUpdate') })}</p>

    <div class="legal-content">
        <section>
            <h2>{$t('terms.s1Title')}</h2>
            <p>{$t('terms.s1p1')}</p>
            <p>{$t('terms.s1p2')}</p>
            <p>{$t('terms.s1p3')}</p>
        </section>

        <section>
            <h2>{$t('terms.s2Title')}</h2>
            <p>{$t('terms.s2p1')}</p>
            <ul>
                <li>{$t('terms.s2li1')}</li>
                <li>{$t('terms.s2li2')}</li>
                <li>{$t('terms.s2li3')}</li>
            </ul>
        </section>

        <section>
            <h2>{$t('terms.s3Title')}</h2>
            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
            <p>{@html $t('terms.s3p1')}</p>
            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
            <p>{@html $t('terms.s3p2')}</p>
            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
            <p>{@html $t('terms.s3p3')}</p>
        </section>

        <section>
            <h2>{$t('terms.s4Title')}</h2>
            <p>{$t('terms.s4p1')}</p>
            <ul>
                <li>{$t('terms.s4li1')}</li>
                <li>{$t('terms.s4li2')}</li>
                <li>{$t('terms.s4li3')}</li>
                <li>{$t('terms.s4li4')}</li>
                <li>{$t('terms.s4li5')}</li>
            </ul>
            <p>{$t('terms.s4p2')}</p>
        </section>

        <section>
            <h2>{$t('terms.s5Title')}</h2>
            <p>{$t('terms.s5p1')}</p>
        </section>

        <section>
            <h2>{$t('terms.s6Title')}</h2>
            <p>{$t('terms.s6p1')}</p>
            <p>{$t('terms.s6p2')}</p>
        </section>

        <section>
            <h2>{$t('terms.s7Title')}</h2>
            <p>{$t('terms.s7p1')}</p>
            <ul>
                <li>{$t('terms.s7li1')}</li>
                <li>{$t('terms.s7li2')}</li>
                <li>{$t('terms.s7li3')}</li>
                <li>{$t('terms.s7li4')}</li>
            </ul>
        </section>

        <section>
            <h2>{$t('terms.s8Title')}</h2>
            <p>{$t('terms.s8p1')}</p>
        </section>

        <section>
            <h2>{$t('terms.s9Title')}</h2>
            <p>{$t('terms.s9p1')}</p>
        </section>

        <section>
            <h2>{$t('terms.s10Title')}</h2>
            <p>{$t('terms.s10p1')}</p>
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
