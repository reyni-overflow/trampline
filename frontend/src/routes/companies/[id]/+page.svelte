<script lang="ts">
    import { onMount } from 'svelte';
    import { page } from '$app/state';
    import MapView from '$lib/components/ui/MapView.svelte';
    import Modal from '$lib/components/ui/Modal.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import JobCard from '$lib/components/ui/JobCard.svelte';
    import { employeesApi } from '$lib/api/employees';
    import { jobsApi, type JobResponse } from '$lib/api/jobs';
    import { eventsApi, type EventResponse } from '$lib/api/events';
    import { mentorshipsApi, type MentorshipResponse } from '$lib/api/mentorships';
    import type { EmployeeProfileResponse } from '$lib/api/auth';
    import { handleApiError } from '$lib/api/client';
    import { getCityCoords } from '$lib/utils/geo';
    import { t } from '$lib/i18n';

    const companyId = page.params.id ?? '';
    let companyData = $state<EmployeeProfileResponse | null>(null);
    let companyJobs = $state<JobResponse[]>([]);
    let companyEvents = $state<EventResponse[]>([]);
    let companyMentorships = $state<MentorshipResponse[]>([]);
    let _loading = $state(true);

    const emptyCompany: EmployeeProfileResponse = {
        id: companyId,
        userId: '',
        name: '...',
        description: '',
        activity: '',
        link: null,
        socials: [],
        photos: [],
        videos: [],
        isVerified: false,
        verificationLevel: 0,
        verifiedName: null,
        info: { address: '', inn: '', email: '' }
    };

    let company = $derived(companyData ?? emptyCompany);
    let jobs = $derived(companyJobs.filter((j) => j.isActive));
    let events = $derived(companyEvents.filter((e) => e.isActive));
    let mentorships = $derived(companyMentorships.filter((m) => m.isActive));
    let companyCoords = $derived.by(() => {
        const allItems = [...companyJobs, ...companyEvents, ...companyMentorships];
        const first = allItems.find((i) => i.geoLat && i.geoLon);
        if (first) return [first.geoLat, first.geoLon] as [number, number];
        return getCityCoords(allItems[0]?.city ?? '');
    });
    let lightboxOpen = $state(false);
    let lightboxSrc = $state('');

    onMount(async () => {
        try {
            companyData = await employeesApi.getById(companyId);
            if (companyData?.userId) {
                const [jobsData, eventsData, mentorshipsData] = await Promise.all([
                    jobsApi.getByUser(companyData.userId, 1, 50).catch(() => []),
                    eventsApi.getByUser(companyData.userId, 1, 50).catch(() => []),
                    mentorshipsApi.getByUser(companyData.userId, 1, 50).catch(() => [])
                ]);
                companyJobs = jobsData as JobResponse[];
                companyEvents = eventsData as EventResponse[];
                companyMentorships = mentorshipsData as MentorshipResponse[];
            }
        } catch (error) {
            handleApiError(error);
        } finally {
            _loading = false;
        }
    });
</script>

<svelte:head>
    <title>{$t('company.pageTitle', { name: company.name })}</title>
</svelte:head>

<div class="company-page container">
    <a href="/companies" class="back-link">
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
        {$t('company.backToCompanies')}
    </a>

    <header class="company-header">
        <div class="header-main">
            <Avatar name={company.name} size={72} />
            <div class="header-info">
                <div class="header-name-row">
                    <h1 class="company-name">{company.name}</h1>
                    {#if company.verificationLevel >= 2}
                        <Badge variant="info">
                            <svg
                                viewBox="0 0 24 24"
                                width="12"
                                height="12"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2.5"
                                stroke-linecap="round"
                                stroke-linejoin="round"><polyline points="20 6 9 17 4 12" /></svg
                            >
                            {$t('companies.trustedVerified')}
                        </Badge>
                    {:else if company.isVerified}
                        <Badge variant="success">
                            <svg
                                viewBox="0 0 24 24"
                                width="12"
                                height="12"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2.5"
                                stroke-linecap="round"
                                stroke-linejoin="round"><polyline points="20 6 9 17 4 12" /></svg
                            >
                            {$t('companies.verified')}
                        </Badge>
                    {/if}
                </div>
                <p class="company-activity">{company.activity}</p>
                <div class="company-contacts">
                    {#if company.link}
                        <a href={company.link} class="contact-link" target="_blank" rel="noopener">
                            <svg
                                viewBox="0 0 24 24"
                                width="14"
                                height="14"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="1.75"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                ><path
                                    d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"
                                /><path
                                    d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"
                                /></svg
                            >
                            {company.link.replace(/^https?:\/\//, '')}
                        </a>
                    {/if}
                    <span class="contact-item">
                        <svg
                            viewBox="0 0 24 24"
                            width="14"
                            height="14"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="1.75"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            ><rect width="20" height="16" x="2" y="4" rx="2" /><path
                                d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"
                            /></svg
                        >
                        {company.info.email}
                    </span>
                    <span class="contact-item">
                        <svg
                            viewBox="0 0 24 24"
                            width="14"
                            height="14"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="1.75"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            ><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z" /><circle
                                cx="12"
                                cy="10"
                                r="3"
                            /></svg
                        >
                        {company.info.address}
                    </span>
                </div>
            </div>
        </div>
        {#if company.socials.length > 0}
            <div class="social-links">
                {#each company.socials as social (social)}
                    <a href={social} class="social-chip" target="_blank" rel="noopener"
                        >{social.replace(/^https?:\/\//, '')}</a
                    >
                {/each}
            </div>
        {/if}
    </header>

    <section class="company-section">
        <h2 class="section-heading">{$t('company.about')}</h2>
        <p class="company-description">{company.description}</p>
    </section>

    {#if company.photos.length > 0}
        <section class="company-section">
            <h2 class="section-heading">{$t('company.gallery')}</h2>
            <div class="gallery-grid">
                {#each company.photos as photo, i (photo)}
                    <button
                        class="gallery-item"
                        type="button"
                        onclick={() => {
                            lightboxSrc = photo;
                            lightboxOpen = true;
                        }}
                    >
                        <img src={photo} alt="{company.name} — фото {i + 1}" loading="lazy" />
                    </button>
                {/each}
            </div>
        </section>
    {/if}

    <Modal bind:open={lightboxOpen}>
        {#if lightboxSrc}
            <img src={lightboxSrc} alt="" class="lightbox-img" />
        {/if}
    </Modal>

    <section class="company-section">
        <div class="section-heading-row">
            <h2 class="section-heading">{$t('company.vacancies')}</h2>
            <Badge variant="accent">{jobs.length}</Badge>
        </div>
        {#if jobs.length === 0}
            <p class="empty-text">{$t('company.noVacancies')}</p>
        {:else}
            <div class="company-jobs">
                {#each jobs as job (job.id)}
                    <JobCard {job} mode="list" />
                {/each}
            </div>
        {/if}
    </section>

    <section class="company-section">
        <div class="section-heading-row">
            <h2 class="section-heading">{$t('company.mentorships')}</h2>
            <Badge variant="accent">{mentorships.length}</Badge>
        </div>
        {#if mentorships.length === 0}
            <p class="empty-text">{$t('company.noMentorships')}</p>
        {:else}
            <div class="company-jobs">
                {#each mentorships as m (m.id)}
                    <a href="/mentorships/{m.id}" class="opportunity-card">
                        <div class="opp-header">
                            <span class="opp-title">{m.title}</span>
                            <Badge variant="info">{$t('company.mentorship')}</Badge>
                        </div>
                        <p class="opp-meta">
                            {m.city}{m.duration ? ` · ${m.duration}` : ''}{m.maxParticipants
                                ? ` · ${$t('company.upToParticipants', { count: m.maxParticipants })}`
                                : ''}
                        </p>
                    </a>
                {/each}
            </div>
        {/if}
    </section>

    <section class="company-section">
        <div class="section-heading-row">
            <h2 class="section-heading">{$t('company.events')}</h2>
            <Badge variant="accent">{events.length}</Badge>
        </div>
        {#if events.length === 0}
            <p class="empty-text">{$t('company.noEvents')}</p>
        {:else}
            <div class="company-jobs">
                {#each events as ev (ev.id)}
                    <a href="/events/{ev.id}" class="opportunity-card">
                        <div class="opp-header">
                            <span class="opp-title">{ev.title}</span>
                            <Badge variant="warning">{$t('company.event')}</Badge>
                        </div>
                        <p class="opp-meta">
                            {ev.city}{ev.startDate
                                ? ` · ${new Date(ev.startDate).toLocaleDateString()}`
                                : ''}
                        </p>
                    </a>
                {/each}
            </div>
        {/if}
    </section>

    {#if company.info?.address}
        <section class="company-section">
            <h2 class="section-heading">{$t('job.location')}</h2>
            <div class="company-map">
                <MapView
                    markers={[
                        {
                            id: company.id,
                            lat: companyCoords[0],
                            lng: companyCoords[1],
                            title: company.name,
                            company: company.name,
                            type: 'Work',
                            link: `/companies/${company.id}`
                        }
                    ]}
                    center={companyCoords}
                    zoom={14}
                    height="16rem"
                />
            </div>
            <p class="company-map-address">{company.info.address}</p>
        </section>
    {/if}
</div>

<style>
    .company-page {
        padding-top: var(--space-6);
        padding-bottom: var(--space-16);
        min-height: calc(100dvh - var(--header-height));
    }

    .back-link {
        display: inline-flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        margin-bottom: var(--space-6);
        transition: var(--transition-colors);
    }

    .back-link:hover {
        color: var(--accent);
    }

    .company-header {
        display: flex;
        flex-direction: column;
        gap: var(--space-5);
        padding: var(--space-6);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        margin-bottom: var(--space-6);
    }

    .header-main {
        display: flex;
        align-items: flex-start;
        gap: var(--space-5);
    }

    .header-info {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        flex: 1;
    }

    .header-name-row {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        flex-wrap: wrap;
    }

    .company-name {
        font-size: var(--font-xl);
        font-weight: var(--weight-bold);
    }

    .company-activity {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .company-contacts {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-4);
        margin-top: var(--space-1);
    }

    .contact-link,
    .contact-item {
        display: flex;
        align-items: center;
        gap: var(--space-1);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }

    .contact-link:hover {
        color: var(--accent);
    }

    .social-links {
        display: flex;
        gap: var(--space-2);
        flex-wrap: wrap;
    }

    .social-chip {
        padding: var(--space-1) var(--space-3);
        font-size: var(--font-xs);
        color: var(--text-secondary);
        background: var(--bg-tertiary);
        border-radius: var(--radius-full);
        transition: var(--transition-colors);
    }

    .social-chip:hover {
        color: var(--accent);
        background: var(--accent-subtle);
    }

    .company-section {
        margin-bottom: var(--space-8);
    }

    .section-heading {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
        margin-bottom: var(--space-4);
    }

    .section-heading-row {
        display: flex;
        align-items: baseline;
        gap: var(--space-3);
        margin-bottom: var(--space-4);
    }

    .section-heading-row .section-heading {
        margin-bottom: 0;
    }

    .company-description {
        font-size: var(--font-base);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        max-width: 50rem;
    }

    .company-jobs {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .empty-text {
        color: var(--text-tertiary);
        font-size: var(--font-sm);
    }

    .opportunity-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        padding: var(--space-4);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        text-decoration: none;
        color: inherit;
        transition: var(--transition-colors);
    }

    .opportunity-card:hover {
        border-color: var(--accent);
    }

    .opp-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-2);
    }

    .opp-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }

    .opp-meta {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .gallery-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(10rem, 1fr));
        gap: var(--space-3);
    }

    .gallery-item {
        aspect-ratio: 3 / 2;
        border-radius: var(--radius-md);
        overflow: hidden;
        cursor: pointer;
        border: none;
        padding: 0;
        transition: transform var(--duration-normal) var(--ease-out);
    }

    .gallery-item:hover {
        transform: scale(1.03);
    }

    .gallery-item img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    :global(.lightbox-img) {
        width: 100%;
        border-radius: var(--radius-md);
    }

    .company-map {
        border-radius: var(--radius-lg);
        overflow: hidden;
    }

    .company-map-address {
        margin-top: var(--space-2);
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    @media (max-width: 640px) {
        .header-main {
            flex-direction: column;
            align-items: center;
            text-align: center;
        }

        .company-contacts {
            justify-content: center;
        }
    }
</style>
