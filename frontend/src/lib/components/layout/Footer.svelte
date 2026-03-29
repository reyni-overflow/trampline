<script lang="ts">
    import { env } from '$env/dynamic/public';
    import { toast } from '$lib/stores/toast';
    import { authModal } from '$lib/stores/auth-modal';
    import { t, locale, LOCALES } from '$lib/i18n';
    import QRCode from 'qrcode';

    const links = {
        telegram: env.PUBLIC_LINK_TELEGRAM || '#',
        vk: env.PUBLIC_LINK_VK || '#',
        dzen: env.PUBLIC_LINK_DZEN || '#',
        max: env.PUBLIC_LINK_MAX || '#'
    };

    const mobileAppLink = env.PUBLIC_MOBILE_APP_LINK || '';

    const year = new Date().getFullYear();

    let localeOpen = $state(false);
    let qrDataUrl = $state('');

    function notifyBotUnavailable() {
        toast.info($t('footer.botUnavailable'));
    }

    function handleLocaleClickOutside(e: MouseEvent) {
        if (!(e.target as HTMLElement).closest('.locale-picker')) localeOpen = false;
    }

    $effect(() => {
        if (typeof window !== 'undefined') {
            QRCode.toDataURL(`${window.location.origin}/mobile-app`, {
                width: 200,
                margin: 1,
                color: { dark: '#000000', light: '#ffffff' }
            }).then((url: string) => {
                qrDataUrl = url;
            });
        }
    });
</script>

<svelte:window onclick={handleLocaleClickOutside} />

<footer class="footer">
    <div class="footer-top container">
        <div class="footer-col">
            <h3 class="footer-heading">{$t('footer.platform')}</h3>
            <a href="/about" class="footer-link">{$t('footer.about')}</a>
            <a href="/help" class="footer-link">{$t('footer.help')}</a>
            <a href="/contacts" class="footer-link">{$t('footer.contacts')}</a>
            <a href="/terms" class="footer-link">{$t('footer.terms')}</a>
            <a href="/privacy" class="footer-link">{$t('footer.privacy')}</a>
        </div>

        <div class="footer-col">
            <h3 class="footer-heading">{$t('footer.forSeekers')}</h3>
            <a href="/jobs" class="footer-link">{$t('footer.searchJobs')}</a>
            <a href="/jobs?type=internship" class="footer-link">{$t('footer.internships')}</a>
            <a href="/events" class="footer-link">{$t('footer.careerEvents')}</a>
            <a href="/companies" class="footer-link">{$t('footer.companyCatalog')}</a>
        </div>

        <div class="footer-col">
            <h3 class="footer-heading">{$t('footer.forEmployers')}</h3>
            <button
                class="footer-link"
                type="button"
                onclick={() => authModal.openRegister('Employee')}>{$t('footer.postJobs')}</button
            >
            <a href="/jobs" class="footer-link">{$t('footer.searchSeekers')}</a>
            <a href="/help" class="footer-link">{$t('footer.companyVerification')}</a>
        </div>

        <div class="footer-col footer-col--side">
            <h3 class="footer-heading">{$t('footer.social')}</h3>
            <div class="social-row">
                <a
                    href={links.telegram}
                    class="social-link"
                    target="_blank"
                    rel="noopener"
                    aria-label="Telegram"
                    title="Telegram"
                >
                    <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"
                        ><path
                            d="M11.944 0A12 12 0 0 0 0 12a12 12 0 0 0 12 12 12 12 0 0 0 12-12A12 12 0 0 0 12 0a12 12 0 0 0-.056 0zm4.962 7.224c.1-.002.321.023.465.14a.506.506 0 0 1 .171.325c.016.093.036.306.02.472-.18 1.898-.962 6.502-1.36 8.627-.168.9-.499 1.201-.82 1.23-.696.065-1.225-.46-1.9-.902-1.056-.693-1.653-1.124-2.678-1.8-1.185-.78-.417-1.21.258-1.91.177-.184 3.247-2.977 3.307-3.23.007-.032.014-.15-.056-.212s-.174-.041-.249-.024c-.106.024-1.793 1.14-5.061 3.345-.48.33-.913.49-1.302.48-.428-.008-1.252-.241-1.865-.44-.752-.245-1.349-.374-1.297-.789.027-.216.325-.437.893-.663 3.498-1.524 5.83-2.529 6.998-3.014 3.332-1.386 4.025-1.627 4.476-1.635z"
                        /></svg
                    >
                </a>
                <a
                    href={links.vk}
                    class="social-link"
                    target="_blank"
                    rel="noopener"
                    aria-label="VK"
                    title="ВКонтакте"
                >
                    <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"
                        ><path
                            d="M15.684 0H8.316C1.592 0 0 1.592 0 8.316v7.368C0 22.408 1.592 24 8.316 24h7.368C22.408 24 24 22.408 24 15.684V8.316C24 1.592 22.391 0 15.684 0zm3.692 17.123h-1.744c-.66 0-.864-.525-2.05-1.727-1.033-1-1.49-1.135-1.744-1.135-.356 0-.458.102-.458.593v1.575c0 .424-.135.678-1.253.678-1.846 0-3.896-1.118-5.335-3.202C4.624 10.857 4.03 8.57 4.03 8.096c0-.254.102-.491.593-.491h1.744c.44 0 .61.203.78.678.863 2.49 2.303 4.675 2.896 4.675.22 0 .322-.102.322-.66V9.721c-.068-1.186-.695-1.287-.695-1.71 0-.204.17-.407.44-.407h2.744c.373 0 .508.203.508.644v3.473c0 .372.17.508.271.508.22 0 .407-.136.813-.542 1.253-1.406 2.149-3.574 2.149-3.574.119-.254.322-.491.763-.491h1.744c.525 0 .644.27.525.644-.22 1.017-2.354 4.031-2.354 4.031-.186.305-.254.44 0 .78.186.254.796.779 1.203 1.253.745.847 1.32 1.558 1.473 2.05.17.49-.085.744-.576.744z"
                        /></svg
                    >
                </a>
                <a
                    href={links.dzen}
                    class="social-link"
                    target="_blank"
                    rel="noopener"
                    aria-label="Дзен"
                    title="Дзен"
                >
                    <svg
                        viewBox="0 0 28 28"
                        width="20"
                        height="20"
                        fill="currentColor"
                        fill-rule="evenodd"
                        ><path
                            d="M16.7 16.7c-2.2 2.27-2.36 5.1-2.55 11.3 5.78 0 9.77-.02 11.83-2.02 2-2.06 2.02-6.24 2.02-11.83-6.2.2-9.03.35-11.3 2.55M0 14.15c0 5.59.02 9.77 2.02 11.83 2.06 2 6.05 2.02 11.83 2.02-.2-6.2-.35-9.03-2.55-11.3-2.27-2.2-5.1-2.36-11.3-2.55M13.85 0C8.08 0 4.08.02 2.02 2.02.02 4.08 0 8.26 0 13.85c6.2-.2 9.03-.35 11.3-2.55 2.2-2.27 2.36-5.1 2.55-11.3m2.85 11.3C14.5 9.03 14.34 6.2 14.15 0c5.78 0 9.77.02 11.83 2.02 2 2.06 2.02 6.24 2.02 11.83-6.2-.2-9.03-.35-11.3-2.55"
                        /></svg
                    >
                </a>
                <a
                    href={links.max}
                    class="social-link"
                    target="_blank"
                    rel="noopener"
                    aria-label="Max"
                    title="Max"
                >
                    <svg viewBox="100 100 800 800" width="20" height="20" fill="currentColor"
                        ><path
                            fill-rule="evenodd"
                            d="M508.211 878.328c-75.007 0-109.864-10.95-170.453-54.75-38.325 49.275-159.686 87.783-164.979 21.9 0-49.456-10.95-91.248-23.36-136.873-14.782-56.21-31.572-118.807-31.572-209.508 0-216.626 177.754-379.597 388.357-379.597 210.785 0 375.947 171.001 375.947 381.604.707 207.346-166.595 376.118-373.94 377.224m3.103-571.585c-102.564-5.292-182.499 65.7-200.201 177.024-14.6 92.162 11.315 204.398 33.397 210.238 10.585 2.555 37.23-18.98 53.837-35.587a189.8 189.8 0 0 0 92.71 33.032c106.273 5.112 197.08-75.794 204.215-181.95 4.154-106.382-77.67-196.486-183.958-202.574Z"
                            clip-rule="evenodd"
                        /></svg
                    >
                </a>
            </div>

            <h3 class="footer-heading" style="margin-top: var(--space-4)">
                {$t('footer.notifBots')}
            </h3>
            <div class="bot-row">
                <button
                    class="bot-link"
                    onclick={notifyBotUnavailable}
                    type="button"
                    title="Telegram-бот"
                >
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="currentColor"
                        ><path
                            d="M11.944 0A12 12 0 0 0 0 12a12 12 0 0 0 12 12 12 12 0 0 0 12-12A12 12 0 0 0 12 0a12 12 0 0 0-.056 0zm4.962 7.224c.1-.002.321.023.465.14a.506.506 0 0 1 .171.325c.016.093.036.306.02.472-.18 1.898-.962 6.502-1.36 8.627-.168.9-.499 1.201-.82 1.23-.696.065-1.225-.46-1.9-.902-1.056-.693-1.653-1.124-2.678-1.8-1.185-.78-.417-1.21.258-1.91.177-.184 3.247-2.977 3.307-3.23.007-.032.014-.15-.056-.212s-.174-.041-.249-.024c-.106.024-1.793 1.14-5.061 3.345-.48.33-.913.49-1.302.48-.428-.008-1.252-.241-1.865-.44-.752-.245-1.349-.374-1.297-.789.027-.216.325-.437.893-.663 3.498-1.524 5.83-2.529 6.998-3.014 3.332-1.386 4.025-1.627 4.476-1.635z"
                        /></svg
                    >
                    TG
                </button>
                <button
                    class="bot-link"
                    onclick={notifyBotUnavailable}
                    type="button"
                    title="VK-бот"
                >
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="currentColor"
                        ><path
                            d="M15.684 0H8.316C1.592 0 0 1.592 0 8.316v7.368C0 22.408 1.592 24 8.316 24h7.368C22.408 24 24 22.408 24 15.684V8.316C24 1.592 22.391 0 15.684 0zm3.692 17.123h-1.744c-.66 0-.864-.525-2.05-1.727-1.033-1-1.49-1.135-1.744-1.135-.356 0-.458.102-.458.593v1.575c0 .424-.135.678-1.253.678-1.846 0-3.896-1.118-5.335-3.202C4.624 10.857 4.03 8.57 4.03 8.096c0-.254.102-.491.593-.491h1.744c.44 0 .61.203.78.678.863 2.49 2.303 4.675 2.896 4.675.22 0 .322-.102.322-.66V9.721c-.068-1.186-.695-1.287-.695-1.71 0-.204.17-.407.44-.407h2.744c.373 0 .508.203.508.644v3.473c0 .372.17.508.271.508.22 0 .407-.136.813-.542 1.253-1.406 2.149-3.574 2.149-3.574.119-.254.322-.491.763-.491h1.744c.525 0 .644.27.525.644-.22 1.017-2.354 4.031-2.354 4.031-.186.305-.254.44 0 .78.186.254.796.779 1.203 1.253.745.847 1.32 1.558 1.473 2.05.17.49-.085.744-.576.744z"
                        /></svg
                    >
                    VK
                </button>
                <button
                    class="bot-link"
                    onclick={notifyBotUnavailable}
                    type="button"
                    title="Max-бот"
                >
                    <svg viewBox="100 100 800 800" width="16" height="16" fill="currentColor"
                        ><path
                            fill-rule="evenodd"
                            d="M508.211 878.328c-75.007 0-109.864-10.95-170.453-54.75-38.325 49.275-159.686 87.783-164.979 21.9 0-49.456-10.95-91.248-23.36-136.873-14.782-56.21-31.572-118.807-31.572-209.508 0-216.626 177.754-379.597 388.357-379.597 210.785 0 375.947 171.001 375.947 381.604.707 207.346-166.595 376.118-373.94 377.224m3.103-571.585c-102.564-5.292-182.499 65.7-200.201 177.024-14.6 92.162 11.315 204.398 33.397 210.238 10.585 2.555 37.23-18.98 53.837-35.587a189.8 189.8 0 0 0 92.71 33.032c106.273 5.112 197.08-75.794 204.215-181.95 4.154-106.382-77.67-196.486-183.958-202.574Z"
                            clip-rule="evenodd"
                        /></svg
                    >
                    Max
                </button>
            </div>

            {#if mobileAppLink}
                <div class="qr-section">
                    <p class="qr-label">{$t('footer.mobileApp')}</p>
                    <div class="qr-placeholder">
                        <a href="/mobile-app">
                            {#if qrDataUrl}
                                <img
                                    src={qrDataUrl}
                                    alt={$t('footer.qrAlt')}
                                    width="100"
                                    height="100"
                                />
                            {/if}
                        </a>
                    </div>
                </div>
            {/if}
        </div>
    </div>

    <div class="footer-bottom">
        <div class="footer-bottom-inner container">
            <div class="locale-picker">
                <button
                    class="locale-btn"
                    onclick={() => (localeOpen = !localeOpen)}
                    type="button"
                    aria-expanded={localeOpen}
                >
                    <svg
                        viewBox="0 0 24 24"
                        width="16"
                        height="16"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="1.75"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        ><circle cx="12" cy="12" r="10" /><path
                            d="M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20"
                        /><path d="M2 12h20" /></svg
                    >
                    <span>{LOCALES.find((l) => l.id === $locale)?.label}</span>
                    <svg
                        viewBox="0 0 24 24"
                        width="14"
                        height="14"
                        stroke="currentColor"
                        stroke-width="2"
                        fill="none"><polyline points="6 9 12 15 18 9" /></svg
                    >
                </button>
                {#if localeOpen}
                    <div class="locale-dropdown">
                        {#each LOCALES as loc (loc.id)}
                            <button
                                class="locale-option"
                                class:active={$locale === loc.id}
                                type="button"
                                onclick={() => {
                                    locale.set(loc.id);
                                    localeOpen = false;
                                }}
                            >
                                <span>{loc.flag}</span>
                                {loc.label}
                            </button>
                        {/each}
                    </div>
                {/if}
            </div>

            <p class="copyright">{$t('footer.copyright', { year })}</p>

            <a href="/" class="footer-logo" aria-label={$t('brand.name')}>
                <svg viewBox="0 0 200 200" width="20" height="20">
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
                <span>{$t('brand.name')}</span>
            </a>
        </div>
    </div>
</footer>

<style>
    .footer {
        margin-top: auto;
        border-top: 1px solid var(--border-default);
        background: var(--bg-secondary);
    }

    .footer-top {
        display: grid;
        grid-template-columns: repeat(3, 1fr) auto;
        gap: var(--space-8);
        padding-top: var(--space-10);
        padding-bottom: var(--space-10);
    }

    .footer-col {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .footer-heading {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
        margin-bottom: var(--space-1);
    }

    .footer-link {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        text-decoration: none;
        transition: var(--transition-colors);
        width: fit-content;
    }

    .footer-link:hover {
        color: var(--accent);
    }

    .social-row {
        display: flex;
        gap: var(--space-2);
    }

    .social-link {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.25rem;
        height: 2.25rem;
        color: var(--text-secondary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
    }

    .social-link:hover {
        color: var(--accent);
        background: var(--accent-subtle);
    }

    .bot-row {
        display: flex;
        gap: var(--space-2);
    }

    .bot-link {
        display: flex;
        align-items: center;
        gap: var(--space-1);
        padding: var(--space-1) var(--space-3);
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        background: var(--bg-tertiary);
        border-radius: var(--radius-full);
        transition: var(--transition-colors);
    }

    .bot-link:hover {
        color: var(--text-primary);
        background: var(--border-hover);
    }

    .qr-section {
        margin-top: var(--space-4);
    }

    .qr-label {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        margin-bottom: var(--space-2);
    }

    .qr-placeholder {
        width: 6.25rem;
        height: 6.25rem;
        background: var(--text-inverse);
        border-radius: var(--radius-sm);
        padding: 0.25rem;
    }

    .qr-placeholder img {
        width: 100%;
        height: 100%;
        border-radius: 2px;
        object-fit: contain;
    }

    .footer-bottom {
        border-top: 1px solid var(--border-default);
        background: var(--bg-primary);
    }

    .footer-bottom-inner {
        display: flex;
        align-items: center;
        justify-content: space-between;
        height: 3.25rem;
    }

    .copyright {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .locale-picker {
        position: relative;
    }

    .locale-btn {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        padding: var(--space-1) var(--space-3);
        font-size: var(--font-xs);
        color: var(--text-secondary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
    }

    .locale-btn:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .locale-dropdown {
        position: absolute;
        bottom: calc(100% + 0.375rem);
        left: 0;
        min-width: 8.75rem;
        padding: var(--space-1);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-lg);
        z-index: var(--z-dropdown);
        animation: scale-in var(--duration-fast) var(--ease-out);
    }

    .locale-option {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        width: 100%;
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-sm);
        color: var(--text-primary);
        border-radius: var(--radius-sm);
        text-align: left;
        transition: var(--transition-colors);
    }

    .locale-option:hover {
        background: var(--bg-tertiary);
    }

    .locale-option.active {
        color: var(--accent);
    }

    .footer-logo {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        color: var(--text-tertiary);
        font-size: var(--font-xs);
        font-weight: var(--weight-semibold);
        text-decoration: none;
        transition: var(--transition-colors);
    }

    .footer-logo:hover {
        color: var(--accent);
    }

    @media (max-width: 768px) {
        .footer-top {
            grid-template-columns: 1fr 1fr;
            gap: var(--space-6);
        }

        .footer-col--side {
            grid-column: 1 / -1;
        }

        .footer-bottom-inner {
            flex-wrap: wrap;
            height: auto;
            padding-top: var(--space-3);
            padding-bottom: var(--space-3);
            gap: var(--space-2);
        }
    }

    @media (max-width: 480px) {
        .footer-top {
            grid-template-columns: 1fr;
        }
    }
</style>
