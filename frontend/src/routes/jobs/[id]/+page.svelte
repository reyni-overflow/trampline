<script lang="ts">
    import { page } from '$app/state';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import ShareButton from '$lib/components/ui/ShareButton.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Skeleton from '$lib/components/ui/Skeleton.svelte';
    import {
        formatSalary,
        formatDate,
        jobTypeLabel,
        workFormatLabel,
        timeAgo,
        formatViews
    } from '$lib/utils/format';
    import { isAuthenticated } from '$lib/stores/auth';
    import { authModal } from '$lib/stores/auth-modal';
    import { toast } from '$lib/stores/toast';
    import { jobsApi, type JobResponse } from '$lib/api/jobs';
    import { employeesApi } from '$lib/api/employees';
    import { favoritesApi } from '$lib/api/favorites';
    import { handleApiError } from '$lib/api/client';
    import type { EmployeeProfileResponse } from '$lib/api/auth';
    import { favorites } from '$lib/stores/favorites';
    import MapView from '$lib/components/ui/MapView.svelte';
    import MarkdownRenderer from '$lib/components/ui/MarkdownRenderer.svelte';
    import Modal from '$lib/components/ui/Modal.svelte';
    import Textarea from '$lib/components/ui/Textarea.svelte';
    import { onMount, onDestroy } from 'svelte';
    import { t } from '$lib/i18n';

    let isAuth = $state(false);
    const unsubAuth = isAuthenticated.subscribe((v) => (isAuth = v));
    onDestroy(unsubAuth);

    let loading = $state(true);
    let notFound = $state(false);
    let isFavorite = $state(false);
    let applyModalOpen = $state(false);
    let coverLetter = $state('');
    let applyError = $state('');
    let applying = $state(false);
    let job = $state<JobResponse | null>(null);
    let company = $state<EmployeeProfileResponse | null>(null);
    let similarJobs = $state<JobResponse[]>([]);

    let data = $derived(
        job ?? {
            id: page.params.id ?? '',
            employeeId: '',
            userId: '',
            type: 'Work' as const,
            title: '...',
            description: '',
            address: '',
            city: '',
            country: '',
            street: '',
            geoLat: 0,
            geoLon: 0,
            salaryFrom: null,
            salaryTo: null,
            tags: [],
            format: 'Remote' as const,
            createdAt: '',
            updatedAt: '',
            deletedAt: null,
            endedAt: '',
            isActive: true,
            views: 0
        }
    );

    onMount(async () => {
        try {
            const id = page.params.id;
            if (id) {
                isFavorite = favorites.isJobFavorite(id);
                job = await jobsApi.getById(id);
                if (job?.employeeId) {
                    try {
                        company = await employeesApi.getById(job.employeeId);
                    } catch {
                        /* ignored */
                    }
                }
                try {
                    const allData = await jobsApi.getAll(1, 20);
                    const currentTags = new Set(job?.tags?.map((t) => t.name) || []);
                    similarJobs = allData.items
                        .filter((j) => j.id !== id)
                        .map((j) => ({
                            job: j,
                            score: j.tags?.filter((t) => currentTags.has(t.name)).length || 0
                        }))
                        .sort((a, b) => b.score - a.score)
                        .slice(0, 3)
                        .map((r) => r.job);
                } catch {
                    /* ignored */
                }
            }
        } catch (err) {
            const apiErr = err as { status?: number };
            if (apiErr.status === 404 || apiErr.status === 500) {
                notFound = true;
            } else {
                handleApiError(err);
            }
        } finally {
            loading = false;
        }
    });

    let typeBadge = $derived(
        data.type === 'Internship'
            ? ('info' as const)
            : data.type === 'Mentorship'
              ? ('warning' as const)
              : data.type === 'Event'
                ? ('accent' as const)
                : ('default' as const)
    );

    async function toggleFavorite() {
        const id = page.params.id ?? '';
        const prev = isFavorite;
        isFavorite = !isFavorite;
        favorites.toggleJob(id);
        toast.success(isFavorite ? $t('job.addToFavorites') : $t('job.removeFromFavorites'));

        try {
            if (isAuth) {
                await favoritesApi.toggle(id, 'Job');
            }
        } catch {
            isFavorite = prev;
            favorites.toggleJob(id);
            toast.error($t('common.error'));
        }
    }

    function handleApply() {
        if (!isAuth) {
            authModal.open();
            return;
        }
        coverLetter = '';
        applyError = '';
        applyModalOpen = true;
    }

    async function submitApplication() {
        const trimmed = coverLetter.trim();
        if (trimmed.length < 50) {
            applyError = $t('job.coverLetterMin');
            return;
        }
        if (trimmed.length > 5000) {
            applyError = $t('job.coverLetterMax');
            return;
        }
        applying = true;
        try {
            await jobsApi.apply(page.params.id ?? '', trimmed);
            applyModalOpen = false;
            toast.success($t('job.applicationSent'));
        } catch (err) {
            handleApiError(err);
        } finally {
            applying = false;
        }
    }

    const jobJsonLd = $derived(
        '<script type="application/ld+json">' +
            JSON.stringify({
                '@context': 'https://schema.org',
                '@type': 'JobPosting',
                title: data.title,
                description: data.description,
                datePosted: data.createdAt,
                jobLocation: {
                    '@type': 'Place',
                    address: {
                        '@type': 'PostalAddress',
                        addressLocality: data.city,
                        addressCountry: data.country
                    }
                }
            }) +
            '</' +
            'script>'
    );
</script>

<svelte:head>
    <title>{data.title} · {$t('brand.name')}</title>
    <meta name="description" content={data.description?.slice(0, 160)} />
    <meta property="og:title" content="{data.title} · {$t('brand.name')}" />
    <meta property="og:description" content={data.description?.slice(0, 160)} />
    <meta property="og:type" content="website" />
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html jobJsonLd}
</svelte:head>

{#if notFound}
    <div class="not-found container">
        <div class="not-found-card">
            <svg
                viewBox="0 0 24 24"
                width="48"
                height="48"
                fill="none"
                stroke="var(--text-tertiary)"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
                ><circle cx="12" cy="12" r="10" /><path d="m15 9-6 6" /><path d="m9 9 6 6" /></svg
            >
            <h1 class="not-found-title">{$t('job.notFoundTitle')}</h1>
            <p class="not-found-text">{$t('job.notFoundText')}</p>
            <div class="not-found-actions">
                <Button href="/jobs">{$t('job.browseJobs')}</Button>
                <Button variant="outline" href="/">{$t('job.backToHome')}</Button>
            </div>
        </div>
    </div>
{:else}
    <div class="job-page container">
        {#if loading}
            <div class="job-skeleton">
                <div class="sk-main">
                    <Skeleton width="8rem" height="0.875rem" />
                    <div class="sk-title-row">
                        <Skeleton circle height="3.5rem" />
                        <div class="sk-title-info">
                            <Skeleton width="60%" height="1.5rem" />
                            <Skeleton width="40%" height="0.875rem" />
                        </div>
                    </div>
                    <div class="sk-badges-row">
                        <Skeleton width="5rem" height="1.25rem" radius="var(--radius-full)" />
                        <Skeleton width="4rem" height="1.25rem" radius="var(--radius-full)" />
                        <Skeleton width="6rem" height="0.875rem" />
                    </div>
                    <Skeleton width="100%" height="3.5rem" radius="var(--radius-lg)" />
                    <div class="sk-tags-row">
                        {#each Array(5) as _, i (i)}
                            <Skeleton width="4rem" height="1.5rem" radius="var(--radius-full)" />
                        {/each}
                    </div>
                    <Skeleton width="100%" height="1rem" />
                    <Skeleton width="100%" height="1rem" />
                    <Skeleton width="80%" height="1rem" />
                    <Skeleton width="100%" height="1rem" />
                    <Skeleton width="60%" height="1rem" />
                </div>
                <div class="sk-sidebar">
                    <Skeleton width="100%" height="10rem" radius="var(--radius-lg)" />
                    <Skeleton width="100%" height="8rem" radius="var(--radius-lg)" />
                </div>
            </div>
        {:else}
            <div class="job-layout content-fade-in">
                <main class="job-main">
                    <div class="job-header">
                        <div class="job-header-top">
                            <a href="/jobs" class="back-link">
                                <svg
                                    viewBox="0 0 24 24"
                                    width="18"
                                    height="18"
                                    fill="none"
                                    stroke="currentColor"
                                    stroke-width="1.75"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                >
                                    <path d="m15 18-6-6 6-6" />
                                </svg>
                                {$t('job.backToJobs')}
                            </a>
                            <div class="header-actions">
                                <ShareButton title={data.title} />
                                <button
                                    class="fav-btn"
                                    class:active={isFavorite}
                                    onclick={toggleFavorite}
                                    title={isFavorite
                                        ? $t('job.removeFromFavorites')
                                        : $t('job.addToFavorites')}
                                    type="button"
                                >
                                    <svg
                                        viewBox="0 0 24 24"
                                        width="22"
                                        height="22"
                                        fill={isFavorite ? 'currentColor' : 'none'}
                                        stroke="currentColor"
                                        stroke-width="1.75"
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                    >
                                        <path
                                            d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"
                                        />
                                    </svg>
                                </button>
                            </div>
                        </div>

                        <div class="job-title-row">
                            <Avatar name={data.title} size={56} />
                            <div class="job-title-info">
                                <h1 class="job-title">{data.title}</h1>
                                <p class="job-location">
                                    {data.city}, {data.country}
                                    {#if data.address}&middot; {data.address}{/if}
                                </p>
                            </div>
                        </div>

                        <div class="job-meta">
                            <Badge variant={typeBadge}>{jobTypeLabel(data.type)}</Badge>
                            <Badge
                                variant={data.format === 'Remote'
                                    ? 'success'
                                    : data.format === 'Office'
                                      ? 'warning'
                                      : 'default'}>{workFormatLabel(data.format)}</Badge
                            >
                            <span class="meta-dot">&middot;</span>
                            <span class="meta-text">{timeAgo(data.createdAt)}</span>
                            <span class="meta-dot">&middot;</span>
                            <span class="meta-text">{formatViews(data.views)}</span>
                        </div>
                    </div>

                    <div class="job-salary-bar">
                        <span class="salary-label">{$t('job.salary')}</span>
                        <span class="salary-value"
                            >{formatSalary(data.salaryFrom, data.salaryTo)}</span
                        >
                    </div>

                    {#if data.tags?.length}
                        <div class="job-tags">
                            {#each data.tags as tag (tag.name)}
                                <Tag clickable onclick={() => {}}>{tag.name}</Tag>
                            {/each}
                        </div>
                    {/if}

                    <div class="job-description">
                        <MarkdownRenderer source={data.description} />
                    </div>

                    <div class="job-actions">
                        <Button size="lg" onclick={handleApply}>
                            {isAuth ? $t('job.respond') : $t('job.loginToRespond')}
                        </Button>
                        <Button size="lg" variant="outline" onclick={toggleFavorite}>
                            {isFavorite ? $t('job.removeFromFavorites') : $t('job.addToFavorites')}
                        </Button>
                    </div>

                    <div class="job-footer-info">
                        <span class="footer-item"
                            >{$t('job.published')} {formatDate(data.createdAt)}</span
                        >
                        {#if data.endedAt}
                            <span class="footer-item"
                                >&middot; {$t('job.deadline')} {formatDate(data.endedAt)}</span
                            >
                        {/if}
                    </div>
                </main>

                <aside class="job-sidebar">
                    {#if data.city}
                        <div class="sidebar-card minimap-card">
                            <h3 class="sidebar-title">{$t('job.location')}</h3>
                            <div class="minimap-wrap">
                                <MapView
                                    markers={[
                                        {
                                            id: data.id,
                                            lat: data.geoLat || 55.757,
                                            lng: data.geoLon || 37.617,
                                            title: data.title,
                                            company: company?.name ?? '',
                                            type: data.type
                                        }
                                    ]}
                                    center={[data.geoLat || 55.757, data.geoLon || 37.617]}
                                    zoom={14}
                                    height="12rem"
                                />
                            </div>
                            <p class="minimap-address">
                                {data.address ? `${data.address}, ` : ''}{data.city}, {data.country}
                            </p>
                        </div>
                    {/if}

                    <div class="sidebar-card">
                        <h3 class="sidebar-title">{$t('job.company')}</h3>
                        <div class="sidebar-company">
                            <Avatar name={company?.name ?? $t('job.company')} size={48} />
                            <div>
                                <a href="/companies/{data.employeeId}" class="company-link"
                                    >{company?.name ?? $t('job.company')}</a
                                >
                                <p class="company-activity">{company?.activity ?? ''}</p>
                            </div>
                        </div>
                        {#if (company?.verificationLevel ?? 0) >= 2}
                            <Badge variant="info">{$t('companies.trustedVerified')}</Badge>
                        {:else if company?.isVerified}
                            <Badge variant="success">{$t('companies.verified')}</Badge>
                        {/if}
                        {#if company?.info?.email}
                            <div class="company-contacts">
                                <a href="mailto:{company.info.email}" class="contact-link">
                                    <svg
                                        viewBox="0 0 24 24"
                                        width="14"
                                        height="14"
                                        fill="none"
                                        stroke="currentColor"
                                        stroke-width="1.75"
                                        ><rect width="20" height="16" x="2" y="4" rx="2" /><path
                                            d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"
                                        /></svg
                                    >
                                    {company.info.email}
                                </a>
                            </div>
                        {/if}
                        {#if company?.link}
                            <a
                                href={company.link}
                                class="contact-link"
                                target="_blank"
                                rel="noopener"
                            >
                                <svg
                                    viewBox="0 0 24 24"
                                    width="14"
                                    height="14"
                                    fill="none"
                                    stroke="currentColor"
                                    stroke-width="1.75"
                                    ><path
                                        d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"
                                    /><path
                                        d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"
                                    /></svg
                                >
                                {company.link.replace(/^https?:\/\//, '')}
                            </a>
                        {/if}
                    </div>

                    {#if similarJobs.length > 0}
                        <div class="sidebar-card">
                            <h3 class="sidebar-title">{$t('job.similarJobs')}</h3>
                            <div class="similar-list">
                                {#each similarJobs as sj (sj.id)}
                                    <a href="/jobs/{sj.id}" class="similar-item">
                                        <span class="similar-title">{sj.title}</span>
                                        <span class="similar-salary"
                                            >{formatSalary(sj.salaryFrom, sj.salaryTo)}</span
                                        >
                                        {#if sj.tags?.length}
                                            <span class="similar-tags"
                                                >{sj.tags
                                                    .slice(0, 3)
                                                    .map((t) => t.name)
                                                    .join(', ')}</span
                                            >
                                        {/if}
                                    </a>
                                {/each}
                            </div>
                        </div>
                    {/if}
                </aside>
            </div>
        {/if}
    </div>
{/if}

<Modal open={applyModalOpen} title={$t('job.applyTitle')} onclose={() => (applyModalOpen = false)}>
    <div class="apply-modal-body">
        <p class="apply-hint">{$t('job.coverLetterHint')}</p>
        <Textarea
            bind:value={coverLetter}
            placeholder={$t('job.coverLetterPlaceholder')}
            rows={5}
        />
        {#if applyError}
            <p class="apply-error">{applyError}</p>
        {/if}
        <span
            class="apply-counter"
            class:warn={coverLetter.trim().length < 50 || coverLetter.trim().length > 5000}
            >{coverLetter.trim().length} / 5000</span
        >
        <Button onclick={submitApplication} disabled={applying}>
            {applying ? $t('common.sending') : $t('job.sendApplication')}
        </Button>
    </div>
</Modal>

<style>
    .not-found {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 60vh;
        padding: var(--space-8);
    }

    .not-found-card {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-4);
        text-align: center;
        max-width: 24rem;
    }

    .not-found-title {
        font-size: var(--font-xl);
        font-weight: var(--weight-bold);
    }

    .not-found-text {
        font-size: var(--font-sm);
        color: var(--text-tertiary);
        line-height: var(--leading-relaxed);
    }

    .not-found-actions {
        display: flex;
        gap: var(--space-3);
        margin-top: var(--space-2);
    }

    .job-skeleton {
        display: grid;
        grid-template-columns: 1fr 20rem;
        gap: var(--space-8);
        align-items: start;
    }

    .sk-main {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .sk-title-row {
        display: flex;
        align-items: center;
        gap: var(--space-4);
    }

    .sk-title-info {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .sk-badges-row,
    .sk-tags-row {
        display: flex;
        gap: var(--space-2);
        flex-wrap: wrap;
    }

    .sk-sidebar {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    @media (max-width: 1024px) {
        .job-skeleton {
            grid-template-columns: 1fr;
        }
    }
    .job-page {
        padding-top: var(--space-6);
        padding-bottom: var(--space-16);
        min-height: calc(100dvh - var(--header-height));
    }

    .job-layout {
        display: grid;
        grid-template-columns: 1fr 20rem;
        gap: var(--space-8);
        align-items: start;
    }

    .job-main {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }

    .job-header {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .job-header-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .back-link {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }

    .back-link:hover {
        color: var(--accent);
    }

    .header-actions {
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }

    .fav-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
    }

    .fav-btn:hover,
    .fav-btn.active {
        color: var(--color-error);
    }

    .fav-btn.active {
        background: var(--color-error-subtle);
    }

    .job-title-row {
        display: flex;
        align-items: center;
        gap: var(--space-4);
    }

    .job-title-info {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .job-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        line-height: var(--leading-tight);
    }

    .job-location {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .job-meta {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        flex-wrap: wrap;
    }

    .meta-dot {
        color: var(--text-tertiary);
    }
    .meta-text {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .job-salary-bar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .salary-label {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .salary-value {
        font-size: var(--font-lg);
        font-weight: var(--weight-bold);
        color: var(--accent);
    }

    .job-tags {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-2);
    }

    .job-description {
        line-height: var(--leading-normal);
    }

    .job-actions {
        display: flex;
        gap: var(--space-3);
        padding-top: var(--space-4);
        border-top: 1px solid var(--border-default);
    }

    .job-footer-info {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .job-sidebar {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
        position: sticky;
        top: calc(var(--header-height) + var(--space-4));
    }

    .sidebar-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .sidebar-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.05em;
    }

    .sidebar-company {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .company-link {
        font-size: var(--font-base);
        font-weight: var(--weight-semibold);
        transition: var(--transition-colors);
    }

    .company-link:hover {
        color: var(--accent);
    }

    .company-activity {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .company-contacts {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
        margin-top: var(--space-2);
    }

    .contact-link {
        display: inline-flex;
        align-items: center;
        gap: var(--space-1);
        font-size: var(--font-xs);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }

    .contact-link:hover {
        color: var(--accent);
    }

    .similar-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .similar-item {
        display: flex;
        flex-direction: column;
        gap: 2px;
        padding: var(--space-3);
        background: var(--bg-tertiary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        text-decoration: none;
    }

    .similar-item:hover {
        background: var(--border-hover);
        color: inherit;
    }

    .similar-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-primary);
    }

    .similar-salary {
        font-size: var(--font-xs);
        color: var(--accent);
    }

    .similar-tags {
        font-size: 0.6875rem;
        color: var(--text-tertiary);
    }

    .minimap-card {
        padding: 0;
        overflow: hidden;
    }

    .minimap-card .sidebar-title {
        padding: var(--space-4) var(--space-4) var(--space-2);
    }

    .minimap-wrap :global(.map-wrapper) {
        border-radius: 0;
    }

    .minimap-address {
        padding: var(--space-2) var(--space-4) var(--space-4);
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    @media (max-width: 1024px) {
        .job-layout {
            grid-template-columns: 1fr;
        }

        .job-sidebar {
            position: static;
        }
    }

    @media (max-width: 640px) {
        .job-actions {
            flex-direction: column;
        }
    }

    .apply-modal-body {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .apply-hint {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .apply-error {
        font-size: var(--font-xs);
        color: var(--color-error);
    }

    .apply-counter {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        text-align: right;
    }

    .apply-counter.warn {
        color: var(--color-warning);
    }
</style>
