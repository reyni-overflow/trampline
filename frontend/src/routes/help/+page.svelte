<script lang="ts">
    import { t } from '$lib/i18n';
    import { env } from '$env/dynamic/public';

    const PUBLIC_CONTACT_EMAIL = env.PUBLIC_CONTACT_EMAIL ?? '';

    let openId = $state<string | null>(null);

    function toggle(id: string) {
        openId = openId === id ? null : id;
    }

    const faqGroups = $derived([
        {
            title: $t('help.group0'),
            items: [
                { id: 'q1', q: $t('help.q0'), a: $t('help.a0') },
                { id: 'q2', q: $t('help.q1'), a: $t('help.a1') },
                { id: 'q3', q: $t('help.q2'), a: $t('help.a2') }
            ]
        },
        {
            title: $t('help.group1'),
            items: [
                { id: 'q4', q: $t('help.q3'), a: $t('help.a3') },
                { id: 'q5', q: $t('help.q4'), a: $t('help.a4') },
                { id: 'q6', q: $t('help.q5'), a: $t('help.a5') },
                { id: 'q7', q: $t('help.q6'), a: $t('help.a6') }
            ]
        },
        {
            title: $t('help.group2'),
            items: [
                { id: 'q8', q: $t('help.q7'), a: $t('help.a7') },
                { id: 'q9', q: $t('help.q8'), a: $t('help.a8') },
                { id: 'q10', q: $t('help.q9'), a: $t('help.a9') }
            ]
        },
        {
            title: $t('help.group3'),
            items: [
                { id: 'q11', q: $t('help.q10'), a: $t('help.a10') },
                { id: 'q12', q: $t('help.q11'), a: $t('help.a11') },
                { id: 'q13', q: $t('help.q12'), a: $t('help.a12') }
            ]
        }
    ]);
</script>

<svelte:head>
    <title>{$t('help.pageTitle')}</title>
</svelte:head>

<div class="help-page container--narrow">
    <div class="help-header">
        <h1 class="help-title">{$t('help.title')}</h1>
        <p class="help-subtitle">{$t('help.subtitle')}</p>
    </div>

    <div class="faq-list">
        {#each faqGroups as group (group.title)}
            <div class="faq-group">
                <h2 class="faq-group-title">{group.title}</h2>
                {#each group.items as item (item.id)}
                    <div class="faq-item" class:open={openId === item.id}>
                        <button class="faq-question" type="button" onclick={() => toggle(item.id)} aria-expanded={openId === item.id}>
                            <span>{item.q}</span>
                            <svg class="faq-chevron" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <polyline points="6 9 12 15 18 9"/>
                            </svg>
                        </button>
                        {#if openId === item.id}
                            <div class="faq-answer">
                                <p>{item.a}</p>
                            </div>
                        {/if}
                    </div>
                {/each}
            </div>
        {/each}
    </div>

    <div class="help-contact">
        <p>{$t('help.noAnswer')}</p>
        <a href="mailto:{PUBLIC_CONTACT_EMAIL}" class="contact-link">{PUBLIC_CONTACT_EMAIL}</a>
    </div>
</div>

<style>
    .help-page {
        padding-top: var(--space-8);
        padding-bottom: var(--space-16);
    }

    .help-header {
        margin-bottom: var(--space-8);
    }

    .help-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-2);
    }

    .help-subtitle {
        font-size: var(--font-base);
        color: var(--text-secondary);
    }

    .faq-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-8);
    }

    .faq-group-title {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
        margin-bottom: var(--space-4);
        color: var(--accent);
    }

    .faq-group {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .faq-item {
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        overflow: hidden;
        transition: border-color var(--duration-normal) var(--ease-in-out);
    }

    .faq-item.open {
        border-color: var(--accent);
    }

    .faq-question {
        display: flex;
        align-items: center;
        justify-content: space-between;
        width: 100%;
        padding: var(--space-4) var(--space-5);
        text-align: left;
        font-size: var(--font-base);
        font-weight: var(--weight-medium);
        color: var(--text-primary);
        cursor: pointer;
        transition: var(--transition-colors);
        gap: var(--space-4);
    }

    .faq-question:hover {
        background: var(--bg-secondary);
    }

    .faq-chevron {
        flex-shrink: 0;
        color: var(--text-tertiary);
        transition: transform var(--duration-normal) var(--ease-out);
    }

    .faq-item.open .faq-chevron {
        transform: rotate(180deg);
        color: var(--accent);
    }

    .faq-answer {
        padding: 0 var(--space-5) var(--space-5);
        animation: slide-down var(--duration-fast) var(--ease-out);
    }

    .faq-answer p {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
    }

    .help-contact {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-2);
        margin-top: var(--space-12);
        padding: var(--space-8);
        background: var(--bg-secondary);
        border-radius: var(--radius-xl);
        text-align: center;
    }

    .help-contact p {
        font-size: var(--font-base);
        color: var(--text-secondary);
    }

    .contact-link {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
        color: var(--accent);
    }

    .contact-link:hover {
        text-decoration: underline;
    }
</style>
