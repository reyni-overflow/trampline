<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import { t } from '$lib/i18n';
</script>

<svelte:head>
    <title>{$t('about.pageTitle')}</title>
</svelte:head>

<div class="about-page container--narrow">
    <div class="about-header">
        <div class="logo-row">
            <svg viewBox="0 0 200 200" width="48" height="48" class="logo-icon">
                <path d="M 90 160 L 90 80" fill="none" stroke="currentColor" stroke-width="20" stroke-linecap="round" />
                <path d="M 30 100 L 135 65" fill="none" stroke="currentColor" stroke-width="20" stroke-linecap="round" />
                <circle cx="165" cy="55" r="14" fill="currentColor" />
            </svg>
        </div>
        <h1 class="about-title">{$t('about.title')}</h1>
        <p class="about-subtitle">{$t('about.subtitle')}</p>
    </div>

    <div class="about-content">
        <section>
            <h2>{$t('about.whyTitle')}</h2>
            <p>{$t('about.whyText1')}</p>
            <p>{$t('about.whyText2')}</p>
        </section>

        <section>
            <h2>{$t('about.howTitle')}</h2>
            <div class="features-grid">
                <div class="feature-card">
                    <span class="feature-icon">
                        <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="7" r="4"/><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/></svg>
                    </span>
                    <h3>{$t('about.seekersTitle')}</h3>
                    <p>{$t('about.seekersText')}</p>
                </div>
                <div class="feature-card">
                    <span class="feature-icon">
                        <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="M6 22V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v18Z"/><path d="M6 12H4a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h2"/><path d="M18 9h2a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-2"/></svg>
                    </span>
                    <h3>{$t('about.employersTitle')}</h3>
                    <p>{$t('about.employersText')}</p>
                </div>
                <div class="feature-card">
                    <span class="feature-icon">
                        <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/></svg>
                    </span>
                    <h3>{$t('about.mapTitle')}</h3>
                    <p>{$t('about.mapText')}</p>
                </div>
            </div>
        </section>

        <section>
            <h2>{$t('about.stackTitle')}</h2>
            <p>{$t('about.stackSubtitle')}</p>
            <div class="stack-grid">
                <div class="stack-item">
                    <Badge variant="accent">Frontend</Badge>
                    <span>SvelteKit, TypeScript, CSS</span>
                </div>
                <div class="stack-item">
                    <Badge variant="info">Backend</Badge>
                    <span>ASP.NET Core 10, C#</span>
                </div>
                <div class="stack-item">
                    <Badge variant="success">Database</Badge>
                    <span>PostgreSQL, Redis</span>
                </div>
                <div class="stack-item">
                    <Badge>{$t('about.stackMap')}</Badge>
                    <span>Leaflet, OpenStreetMap</span>
                </div>
                <div class="stack-item">
                    <Badge variant="warning">Auth</Badge>
                    <span>JWT, PBKDF2-SHA256</span>
                </div>
                <div class="stack-item">
                    <Badge variant="error">API</Badge>
                    <span>{$t('about.stackVerification')}</span>
                </div>
            </div>
        </section>

        <section>
            <h2>{$t('about.teamTitle')}</h2>
            <p>{$t('about.teamText')}</p>
        </section>

        <section class="license-section">
            <h2>{$t('about.licenseTitle')}</h2>
            <div class="license-card">
                <div class="license-badge-row">
                    <Badge variant="accent">AGPL-3.0</Badge>
                    <Badge variant="success">Open Source</Badge>
                </div>
                <p>{$t('about.licenseText1')}</p>
                <p>{$t('about.licenseText2')}</p>
                <div class="license-details">
                    <div class="license-detail">
                        <span class="license-label">{$t('about.licenseName')}</span>
                        <span class="license-value">GNU Affero General Public License v3.0</span>
                    </div>
                    <div class="license-detail">
                        <span class="license-label">{$t('about.licenseCopyright')}</span>
                        <span class="license-value">&copy; 2025–2026 reyni-overflow, programistro</span>
                    </div>
                    <div class="license-detail">
                        <span class="license-label">{$t('about.licenseRepo')}</span>
                        <a href="https://github.com/reyni-overflow/trampline" target="_blank" rel="noopener noreferrer" class="license-link">
                            GitHub
                            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>
                        </a>
                    </div>
                </div>
                {#if $t('about.licenseNote')}
                    <p class="license-note">{$t('about.licenseNote')}</p>
                {/if}
            </div>
        </section>

        <section class="about-cta">
            <p>{$t('about.wantToTry')}</p>
            <div class="cta-buttons">
                <Button href="/jobs" size="lg">{$t('about.viewJobs')}</Button>
                <Button href="/contacts" variant="outline" size="lg">{$t('about.contactUs')}</Button>
            </div>
        </section>
    </div>
</div>

<style>
    .about-page {
        padding-top: var(--space-8);
        padding-bottom: var(--space-16);
    }

    .about-header {
        text-align: center;
        margin-bottom: var(--space-10);
    }

    .logo-row {
        display: flex;
        justify-content: center;
        margin-bottom: var(--space-4);
        color: var(--accent);
    }

    .about-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-2);
    }

    .about-subtitle {
        font-size: var(--font-base);
        color: var(--text-secondary);
    }

    .about-content {
        display: flex;
        flex-direction: column;
        gap: var(--space-10);
    }

    .about-content h2 {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
        margin-bottom: var(--space-4);
    }

    .about-content p {
        font-size: var(--font-base);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        margin-bottom: var(--space-3);
    }

    .features-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: var(--space-4);
    }

    .feature-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .feature-card h3 {
        font-size: var(--font-base);
        font-weight: var(--weight-semibold);
    }

    .feature-card p {
        font-size: var(--font-sm);
        margin-bottom: 0;
    }

    .feature-icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        background: var(--accent-subtle);
        color: var(--accent);
        border-radius: var(--radius-md);
    }

    .stack-grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: var(--space-3);
    }

    .stack-item {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-3) var(--space-4);
        background: var(--bg-secondary);
        border-radius: var(--radius-md);
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .about-cta {
        text-align: center;
        padding: var(--space-8);
        background: var(--bg-secondary);
        border-radius: var(--radius-xl);
    }

    .about-cta p {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
        margin-bottom: var(--space-5);
    }

    .cta-buttons {
        display: flex;
        justify-content: center;
        gap: var(--space-3);
    }

    .license-section {
        margin-top: var(--space-2);
    }

    .license-card {
        padding: var(--space-6);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .license-badge-row {
        display: flex;
        gap: var(--space-2);
    }

    .license-card p {
        margin-bottom: 0;
    }

    .license-details {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        padding: var(--space-4);
        background: var(--bg-primary);
        border-radius: var(--radius-md);
        font-size: var(--font-sm);
    }

    .license-detail {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .license-label {
        color: var(--text-tertiary);
        min-width: 8rem;
        flex-shrink: 0;
    }

    .license-value {
        color: var(--text-primary);
        font-weight: var(--weight-medium);
    }

    .license-link {
        display: inline-flex;
        align-items: center;
        gap: var(--space-1);
        color: var(--accent);
        font-weight: var(--weight-medium);
        text-decoration: none;
    }

    .license-link:hover {
        text-decoration: underline;
    }

    .license-note {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        font-style: italic;
    }

    @media (max-width: 768px) {
        .stack-grid { grid-template-columns: 1fr; }
        .license-detail { flex-direction: column; gap: var(--space-1); }
        .license-label { min-width: unset; }
    }

    @media (max-width: 640px) {
        .features-grid { grid-template-columns: 1fr; }
        .cta-buttons { flex-direction: column; }
    }
</style>
