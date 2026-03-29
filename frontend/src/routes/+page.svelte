<script lang="ts">
    import { SvelteSet } from 'svelte/reactivity';
    import { authModal } from '$lib/stores/auth-modal';
    import Button from '$lib/components/ui/Button.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import JobCard from '$lib/components/ui/JobCard.svelte';
    import MapView from '$lib/components/ui/MapView.svelte';
    import Modal from '$lib/components/ui/Modal.svelte';
    import Textarea from '$lib/components/ui/Textarea.svelte';
    import { toast } from '$lib/stores/toast';
    import { isAuthenticated } from '$lib/stores/auth';
    import {
        jobsApi,
        type JobResponse,
        type TagResponse,
        type TagStatsResponse
    } from '$lib/api/jobs';
    import { employeesApi } from '$lib/api/employees';
    import { eventsApi } from '$lib/api/events';
    import { mentorshipsApi } from '$lib/api/mentorships';
    import { workersApi } from '$lib/api/workers';
    import { reviewsApi, type ReviewResponse } from '$lib/api/reviews';
    import { handleApiError } from '$lib/api/client';
    import { formatSalary } from '$lib/utils/format';
    import { getCityCoords } from '$lib/utils/geo';
    import { onMount, onDestroy } from 'svelte';
    import { t, tGet, pluralForm } from '$lib/i18n';
    import { goto } from '$app/navigation';

    let latestJobs = $state<JobResponse[]>([]);
    let landingView = $state<'list' | 'map'>('list');
    let statsValues = $state({
        companies: '0',
        jobs: '0',
        seekers: '0',
        events: '0',
        mentorships: '0'
    });
    let directionTags = $state<TagResponse[]>([]);
    let tagStats = $state<TagStatsResponse[]>([]);
    let apiReviews = $state<ReviewResponse[]>([]);
    let activeCategory = $state('');
    let tabsContainer: HTMLDivElement;

    function handleTabClick(label: string, e: MouseEvent) {
        activeCategory = label;
        const btn = e.currentTarget as HTMLElement;
        const container = tabsContainer;
        if (!btn || !container) return;
        const pill = container.querySelector('.directions-pill') as HTMLElement;
        if (!pill) return;
        pill.style.left = `${btn.offsetLeft}px`;
        pill.style.width = `${btn.offsetWidth}px`;
    }

    function initPill() {
        if (!tabsContainer) return;
        const active = tabsContainer.querySelector('.directions-tab.active') as HTMLElement;
        const pill = tabsContainer.querySelector('.directions-pill') as HTMLElement;
        if (!active || !pill) return;
        pill.style.left = `${active.offsetLeft}px`;
        pill.style.width = `${active.offsetWidth}px`;
        pill.style.opacity = '1';
    }

    $effect(() => {
        if (directionTags.length > 0) {
            requestAnimationFrame(initPill);
        }
    });

    let mapMarkers = $derived(
        latestJobs.map((j, i) => {
            const coords = j.geoLat && j.geoLon ? [j.geoLat, j.geoLon] : getCityCoords(j.city, i);
            return {
                id: j.id,
                lat: coords[0],
                lng: coords[1],
                title: j.title,
                company: j.city,
                salary: formatSalary(j.salaryFrom, j.salaryTo),
                tags: j.tags?.map((t) => (typeof t === 'string' ? t : t.name)),
                type: j.type
            };
        })
    );

    onMount(async () => {
        try {
            const data = await jobsApi.getAll(1, 6);
            latestJobs = data.items || [];
        } catch {
            /* ignored */
        }

        try {
            tagStats = await jobsApi.getTagStats();
            directionTags = tagStats.map((s) => ({
                id: s.id,
                name: s.name,
                category: s.category,
                lvl: 0
            }));
        } catch {
            // fallback to getTags
            try {
                directionTags = await jobsApi.getTags();
            } catch {
                /* ignored */
            }
        }

        try {
            apiReviews = await reviewsApi.getApproved();
        } catch {
            /* ignored */
        }

        try {
            const companyList = await employeesApi.getAll(1, 8);
            if (companyList.items?.length > 0) {
                apiCompanies = companyList.items.map((c) => c.name);
            }
        } catch {
            /* ignored */
        }

        try {
            const [jobsData, companiesData, eventsData, seekersData, mentorshipsData] =
                await Promise.all([
                    jobsApi.getAll(1, 1).catch(() => ({ totalCount: 0 })),
                    employeesApi.getAll(1, 1).catch(() => ({ totalCount: 0 })),
                    eventsApi.getAll(1, 1).catch(() => ({ totalCount: 0 })),
                    workersApi.getCount().catch(() => ({ count: 0 })),
                    mentorshipsApi.getAll(1, 1).catch(() => ({ totalCount: 0 }))
                ]);
            statsValues = {
                companies: String((companiesData as { totalCount: number }).totalCount || 0),
                jobs: String((jobsData as { totalCount: number }).totalCount || 0),
                seekers: String((seekersData as { count: number }).count || 0),
                events: String((eventsData as { totalCount: number }).totalCount || 0),
                mentorships: String((mentorshipsData as { totalCount: number }).totalCount || 0)
            };
        } catch {
            /* ignored */
        }
    });

    const steps = $derived([
        {
            icon: '<circle cx="12" cy="7" r="4"/><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>',
            title: $t('landing.step1Title'),
            desc: $t('landing.step1Desc')
        },
        {
            icon: '<circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/>',
            title: $t('landing.step2Title'),
            desc: $t('landing.step2Desc')
        },
        {
            icon: '<path d="m22 2-7 20-4-9-9-4Z"/><path d="M22 2 11 13"/>',
            title: $t('landing.step3Title'),
            desc: $t('landing.step3Desc')
        }
    ]);

    const stats = $derived([
        { value: statsValues.companies, label: $t('landing.statsCompanies') },
        { value: statsValues.jobs, label: $t('landing.statsJobs') },
        { value: statsValues.seekers, label: $t('landing.statsSeekers') },
        { value: statsValues.events, label: $t('landing.statsEvents') },
        { value: statsValues.mentorships, label: $t('landing.statsMentorships') }
    ]);

    const categoryLabels = $derived<Record<string, string>>({
        tech: $t('category.tech'),
        design: $t('category.design'),
        marketing: $t('category.marketing'),
        management: $t('category.management'),
        finance: $t('category.finance'),
        legal: $t('category.legal'),
        education: $t('category.education'),
        event: $t('category.event'),
        soft: $t('category.soft')
    });

    const categoryOrder = [
        'tech',
        'design',
        'marketing',
        'management',
        'event',
        'soft',
        'finance',
        'education',
        'legal'
    ];

    function getTagJobCount(tagName: string): number {
        const stat = tagStats.find((s) => s.name === tagName);
        return stat?.totalCount ?? 0;
    }

    let groupedTags = $derived.by(() => {
        const groups: { label: string; tags: TagResponse[] }[] = [];
        for (const cat of categoryOrder) {
            const tags = directionTags.filter((t) => t.category === cat);
            if (tags.length > 0) {
                groups.push({ label: categoryLabels[cat] || cat, tags });
            }
        }
        return groups;
    });

    $effect(() => {
        const groups = groupedTags;
        if (groups.length > 0 && !activeCategory) {
            activeCategory = groups[0].label;
        }
    });

    const hardcodedReviews = $derived([
        {
            name: $t('landing.review0Name'),
            role: $t('landing.review0Role'),
            text: $t('landing.review0Text')
        },
        {
            name: $t('landing.review1Name'),
            role: $t('landing.review1Role'),
            text: $t('landing.review1Text')
        },
        {
            name: $t('landing.review2Name'),
            role: $t('landing.review2Role'),
            text: $t('landing.review2Text')
        },
        {
            name: $t('landing.review3Name'),
            role: $t('landing.review3Role'),
            text: $t('landing.review3Text')
        },
        {
            name: $t('landing.review4Name'),
            role: $t('landing.review4Role'),
            text: $t('landing.review4Text')
        },
        {
            name: $t('landing.review5Name'),
            role: $t('landing.review5Role'),
            text: $t('landing.review5Text')
        },
        {
            name: $t('landing.review6Name'),
            role: $t('landing.review6Role'),
            text: $t('landing.review6Text')
        },
        {
            name: $t('landing.review7Name'),
            role: $t('landing.review7Role'),
            text: $t('landing.review7Text')
        },
        {
            name: $t('landing.review8Name'),
            role: $t('landing.review8Role'),
            text: $t('landing.review8Text')
        },
        {
            name: $t('landing.review9Name'),
            role: $t('landing.review9Role'),
            text: $t('landing.review9Text')
        }
    ]);

    const reviews = $derived([
        ...apiReviews.map((r) => ({ name: r.authorName, role: r.authorRole, text: r.text })),
        ...hardcodedReviews
    ]);

    let reviewsPaused = $state(false);

    let isAuth = $state(false);
    const unsubAuth = isAuthenticated.subscribe((v) => (isAuth = v));
    onDestroy(unsubAuth);
    let reviewModalOpen = $state(false);
    let reviewText = $state('');
    let reviewRating = $state(5);
    let reviewSubmitting = $state(false);

    function openReviewModal() {
        if (!isAuth) {
            toast.info($t('landing.reviewLoginHint'));
            authModal.open();
            return;
        }
        reviewText = '';
        reviewRating = 5;
        reviewModalOpen = true;
    }

    async function submitReview() {
        if (!reviewText.trim()) return;
        reviewSubmitting = true;
        try {
            await reviewsApi.create({ text: reviewText.trim(), rating: reviewRating });
            toast.success($t('landing.reviewCreated'));
            reviewModalOpen = false;
        } catch (err) {
            handleApiError(err);
        } finally {
            reviewSubmitting = false;
        }
    }

    let apiCompanies = $state<string[]>([]);
    const mockCompanies = $derived([
        $t('landing.company0'),
        $t('landing.company1'),
        $t('landing.company2'),
        $t('landing.company3'),
        $t('landing.company4'),
        $t('landing.company5'),
        $t('landing.company6'),
        $t('landing.company7')
    ]);
    const companies = $derived(apiCompanies.length > 0 ? apiCompanies : mockCompanies);

    let visibleSections = $state(new SvelteSet<string>());

    function observeSection(node: HTMLElement) {
        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        visibleSections.add(node.dataset.section!);
                        visibleSections = new SvelteSet(visibleSections);
                        observer.unobserve(node);
                    }
                });
            },
            { threshold: 0.15 }
        );
        observer.observe(node);
        return { destroy: () => observer.disconnect() };
    }

    const jsonLd = $derived(
        '<script type="application/ld+json">' +
            JSON.stringify({
                '@context': 'https://schema.org',
                '@type': 'WebSite',
                name: 'Трамплин',
                url: 'https://trampline.localhost',
                description: $t('seo.landingDesc')
            }) +
            '</' +
            'script>'
    );
</script>

<svelte:head>
    <title>{$t('seo.landingTitle')}</title>
    <meta name="description" content={$t('seo.landingDesc')} />
    <meta property="og:title" content={$t('seo.landingTitle')} />
    <meta property="og:description" content={$t('seo.landingDesc')} />
    <meta property="og:type" content="website" />
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html jsonLd}
</svelte:head>

<div class="landing">
    <section class="hero">
        <h1 class="hero-title">
            {#each $t('landing.heroTitle').split('\n') as line, i (i)}{#if i > 0}<br
                    />{/if}{line}{/each}
        </h1>
        <p class="hero-subtitle">{$t('landing.heroSubtitle')}</p>
        <div class="hero-actions">
            <Button href="/jobs" size="lg">{$t('landing.findOpportunities')}</Button>
            <Button variant="outline" size="lg" onclick={() => authModal.openRegister('Employee')}
                >{$t('landing.imEmployer')}</Button
            >
        </div>
    </section>

    <section
        class="section steps"
        data-section="steps"
        use:observeSection
        class:visible={visibleSections.has('steps')}
    >
        <div class="container">
            <h2 class="section-title">{$t('landing.howItWorks')}</h2>
            <div class="steps-grid">
                {#each steps as step, i (step.title)}
                    <div class="step-card" style="animation-delay: {i * 100}ms">
                        <span class="step-number">{i + 1}</span>
                        <span class="step-icon">
                            <svg
                                viewBox="0 0 24 24"
                                width="28"
                                height="28"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="1.75"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                            >
                                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                                {@html step.icon}
                            </svg>
                        </span>
                        <h3 class="step-title">{step.title}</h3>
                        <p class="step-desc">{step.desc}</p>
                    </div>
                {/each}
            </div>
        </div>
    </section>

    <section
        class="section stats"
        data-section="stats"
        use:observeSection
        class:visible={visibleSections.has('stats')}
    >
        <div class="container">
            <div class="stats-grid">
                {#each stats as stat, i (stat.label)}
                    <div class="stat-card" style="animation-delay: {i * 80}ms">
                        <span class="stat-value">{stat.value}</span>
                        <span class="stat-label">{stat.label}</span>
                    </div>
                {/each}
            </div>
        </div>
    </section>

    <section
        class="section opportunities"
        data-section="opps"
        use:observeSection
        class:visible={visibleSections.has('opps')}
    >
        <div class="container">
            <div class="opps-header">
                <h2 class="section-title">{$t('landing.latestOpportunities')}</h2>
                <div class="opps-toggle">
                    <button
                        class="opps-btn"
                        class:active={landingView === 'list'}
                        type="button"
                        onclick={() => (landingView = 'list')}
                        aria-label={$t('events.listView')}
                    >
                        <svg
                            viewBox="0 0 24 24"
                            width="18"
                            height="18"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="1.75"
                            stroke-linecap="round"
                            ><line x1="8" y1="6" x2="21" y2="6" /><line
                                x1="8"
                                y1="12"
                                x2="21"
                                y2="12"
                            /><line x1="8" y1="18" x2="21" y2="18" /><line
                                x1="3"
                                y1="6"
                                x2="3.01"
                                y2="6"
                            /><line x1="3" y1="12" x2="3.01" y2="12" /><line
                                x1="3"
                                y1="18"
                                x2="3.01"
                                y2="18"
                            /></svg
                        >
                    </button>
                    <button
                        class="opps-btn"
                        class:active={landingView === 'map'}
                        type="button"
                        onclick={() => (landingView = 'map')}
                        aria-label={$t('map.title')}
                    >
                        <svg
                            viewBox="0 0 24 24"
                            width="18"
                            height="18"
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
                    </button>
                </div>
            </div>
            {#if landingView === 'map'}
                <div class="opps-map">
                    <MapView
                        markers={mapMarkers}
                        center={[55.751, 37.618]}
                        zoom={4}
                        height="24rem"
                    />
                </div>
            {:else}
                <div class="opps-grid">
                    {#each latestJobs as job (job.id)}
                        <JobCard {job} mode="grid" />
                    {/each}
                </div>
            {/if}
            {#if latestJobs.length > 0}
                <div class="opps-more">
                    <Button variant="outline" href="/jobs">{$t('landing.viewAllJobs')}</Button>
                </div>
            {/if}
        </div>
    </section>

    <section
        class="section directions"
        data-section="dirs"
        use:observeSection
        class:visible={visibleSections.has('dirs')}
    >
        <div class="container">
            <h2 class="section-title">{$t('landing.popularDirections')}</h2>
            <p class="section-subtitle">{$t('landing.findInField')}</p>
            <div class="directions-tabs" bind:this={tabsContainer}>
                <div class="directions-pill"></div>
                {#each groupedTags as group (group.label)}
                    <button
                        class="directions-tab"
                        class:active={activeCategory === group.label}
                        type="button"
                        onclick={(e) => handleTabClick(group.label, e)}>{group.label}</button
                    >
                {/each}
            </div>
            <div class="directions-tags">
                {#each groupedTags as group (group.label)}
                    {#if activeCategory === group.label}
                        {#each group.tags as tag (tag.name)}
                            <Tag
                                clickable
                                onclick={() => goto(`/jobs?tags=${encodeURIComponent(tag.name)}`)}
                            >
                                {tag.name}{#if tagStats.length > 0 && getTagJobCount(tag.name) > 0}
                                    <span class="tag-count"
                                        >{pluralForm(
                                            getTagJobCount(tag.name),
                                            `${getTagJobCount(tag.name)} ${tGet('plural.opportunityOne')}`,
                                            `${getTagJobCount(tag.name)} ${tGet('plural.opportunityFew')}`,
                                            `${getTagJobCount(tag.name)} ${tGet('plural.opportunityMany')}`
                                        )}</span
                                    >
                                {/if}
                            </Tag>
                        {/each}
                    {/if}
                {/each}
            </div>
        </div>
    </section>

    <section
        class="section reviews"
        data-section="reviews"
        use:observeSection
        class:visible={visibleSections.has('reviews')}
    >
        <h2 class="section-title">{$t('landing.userReviews')}</h2>
        <div class="reviews-action">
            <Button variant="outline" size="sm" onclick={openReviewModal}
                >{$t('landing.writeReview')}</Button
            >
        </div>
        <!-- svelte-ignore a11y_no_static_element_interactions -->
        <div
            class="reviews-marquee"
            class:paused={reviewsPaused}
            onmouseenter={() => (reviewsPaused = true)}
            onmouseleave={() => (reviewsPaused = false)}
        >
            <div class="reviews-track">
                {#each [...reviews, ...reviews] as review, i (i)}
                    <div class="review-card">
                        <div class="review-header">
                            <Avatar name={review.name} size={36} />
                            <div class="review-author">
                                <span class="review-name">{review.name}</span>
                                <span class="review-role">{review.role}</span>
                            </div>
                        </div>
                        <p class="review-text">&laquo;{review.text}&raquo;</p>
                    </div>
                {/each}
            </div>
        </div>
    </section>

    <section
        class="section companies"
        data-section="companies"
        use:observeSection
        class:visible={visibleSections.has('companies')}
    >
        <div class="container">
            <h2 class="section-title">{$t('landing.companiesOnPlatform')}</h2>
            <div class="companies-row">
                {#each companies as company (company)}
                    <div class="company-logo">
                        <Avatar name={company} size={48} />
                        <span class="company-name">{company}</span>
                    </div>
                {/each}
            </div>
            {#if apiCompanies.length === 0}
                <p class="companies-disclaimer">{$t('landing.companiesDisclaimer')}</p>
            {/if}
        </div>
    </section>

    <section
        class="section cta"
        data-section="cta"
        use:observeSection
        class:visible={visibleSections.has('cta')}
    >
        <div class="container">
            <div class="cta-card">
                <h2 class="cta-title">{$t('landing.readyToStart')}</h2>
                <p class="cta-subtitle">{$t('landing.joinThousands')}</p>
                <div class="cta-actions">
                    <Button size="lg" onclick={() => authModal.openRegister()}
                        >{$t('landing.createAccount')}</Button
                    >
                    <Button size="lg" variant="ghost" href="/jobs">{$t('landing.viewJobs')}</Button>
                </div>
            </div>
        </div>
    </section>
</div>

<Modal bind:open={reviewModalOpen} title={$t('landing.reviewModalTitle')} maxWidth="480px">
    <div class="review-form">
        <div class="review-rating-group">
            <span class="review-rating-label">{$t('landing.reviewRating')}</span>
            <div class="review-stars">
                {#each [1, 2, 3, 4, 5] as star (star)}
                    <button
                        class="star-btn"
                        class:active={star <= reviewRating}
                        type="button"
                        onclick={() => (reviewRating = star)}
                        aria-label={String(star)}
                    >
                        <svg
                            viewBox="0 0 24 24"
                            width="24"
                            height="24"
                            fill={star <= reviewRating ? 'currentColor' : 'none'}
                            stroke="currentColor"
                            stroke-width="1.75"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                        >
                            <polygon
                                points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"
                            />
                        </svg>
                    </button>
                {/each}
            </div>
        </div>
        <Textarea
            label={$t('landing.reviewText')}
            bind:value={reviewText}
            placeholder={$t('landing.reviewTextPlaceholder')}
        />
        <Button onclick={submitReview} disabled={reviewSubmitting || !reviewText.trim()}
            >{$t('landing.reviewSubmit')}</Button
        >
    </div>
</Modal>

<style>
    .landing {
        display: flex;
        flex-direction: column;
    }

    .hero {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        text-align: center;
        min-height: calc(100dvh - var(--header-height) - 5rem);
        padding: var(--space-16) var(--space-8);
        max-width: 60rem;
        margin: 0 auto;
    }

    .hero-title {
        font-size: clamp(2.5rem, 4.5vw, 4.5rem);
        font-weight: var(--weight-extrabold);
        line-height: var(--leading-tight);
        letter-spacing: -0.03em;
        margin-bottom: var(--space-5);
    }

    .hero-subtitle {
        font-size: clamp(1.125rem, 1.5vw, 1.375rem);
        color: var(--text-secondary);
        margin-bottom: var(--space-10);
        max-width: 37.5rem;
        line-height: var(--leading-normal);
    }

    .hero-actions {
        display: flex;
        gap: var(--space-4);
    }

    .section {
        padding: var(--space-16) 0;
        opacity: 0;
        transform: translateY(1.25rem);
        transition:
            opacity var(--duration-slow) var(--ease-out),
            transform var(--duration-slow) var(--ease-out);
    }

    .section.visible {
        opacity: 1;
        transform: translateY(0);
    }

    .section-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        text-align: center;
        margin-bottom: var(--space-3);
    }

    .section-subtitle {
        font-size: var(--font-base);
        color: var(--text-secondary);
        text-align: center;
        margin-bottom: var(--space-8);
    }

    .steps {
        background: var(--bg-secondary);
    }

    .steps-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: var(--space-6);
        margin-top: var(--space-8);
    }

    .step-card {
        position: relative;
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        padding: var(--space-8) var(--space-6);
        background: var(--bg-primary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        transition: var(--transition-transform), var(--transition-shadow);
    }

    .section.visible .step-card {
        animation: slide-up var(--duration-slow) var(--ease-out) both;
    }

    .step-card:hover {
        transform: translateY(-0.25rem);
        box-shadow: var(--shadow-lg);
    }

    .step-number {
        position: absolute;
        top: var(--space-4);
        left: var(--space-4);
        font-size: var(--font-xs);
        font-weight: var(--weight-bold);
        color: var(--text-tertiary);
    }

    .step-icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 3.5rem;
        height: 3.5rem;
        background: var(--accent-subtle);
        color: var(--accent);
        border-radius: var(--radius-lg);
        margin-bottom: var(--space-4);
    }

    .step-title {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
        margin-bottom: var(--space-2);
    }

    .step-desc {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
    }

    .stats {
        border-top: 1px solid var(--border-default);
        border-bottom: 1px solid var(--border-default);
    }

    .stats-grid {
        display: grid;
        grid-template-columns: repeat(5, 1fr);
        gap: var(--space-6);
    }

    .stat-card {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        padding: var(--space-6);
    }

    .section.visible .stat-card {
        animation: slide-up var(--duration-slow) var(--ease-out) both;
    }

    .stat-value {
        font-size: var(--font-3xl);
        font-weight: var(--weight-extrabold);
        color: var(--accent);
        line-height: 1;
        margin-bottom: var(--space-2);
    }

    .stat-label {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .opps-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--space-6);
    }

    .opps-toggle {
        display: flex;
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        overflow: hidden;
    }

    .opps-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.25rem;
        height: 2.25rem;
        color: var(--text-tertiary);
        transition: var(--transition-colors);
    }

    .opps-btn.active {
        background: var(--accent-subtle);
        color: var(--accent);
    }

    .opps-btn:hover:not(.active) {
        background: var(--bg-tertiary);
        color: var(--text-primary);
    }

    .opps-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(18rem, 1fr));
        gap: var(--space-4);
    }

    .opps-map {
        border-radius: var(--radius-lg);
        overflow: hidden;
    }

    .opps-more {
        display: flex;
        justify-content: center;
        margin-top: var(--space-6);
    }

    .directions-tabs {
        position: relative;
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
        background: var(--bg-secondary);
        border-radius: var(--radius-full);
        padding: 0.25rem;
        margin-bottom: var(--space-6);
        width: fit-content;
        margin-inline: auto;
    }

    .directions-pill {
        position: absolute;
        top: 0.25rem;
        height: calc(100% - 0.5rem);
        border-radius: var(--radius-full);
        background: var(--accent);
        transition:
            left 0.3s cubic-bezier(0.4, 0, 0.2, 1),
            width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        opacity: 0;
    }

    .directions-tab {
        position: relative;
        z-index: 1;
        padding: var(--space-2) var(--space-4);
        border-radius: var(--radius-full);
        border: none;
        background: transparent;
        color: var(--text-secondary);
        font-size: var(--font-sm);
        font-family: var(--font-family);
        cursor: pointer;
        transition: color 0.2s ease;
    }

    .directions-tab:hover {
        color: var(--text-primary);
    }

    .directions-tab.active {
        color: var(--text-inverse);
    }

    .directions-tags {
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
        gap: var(--space-2);
    }

    .directions-tags :global(.tag) {
        font-size: var(--font-sm);
        padding: var(--space-2) var(--space-4);
    }

    .tag-count {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        margin-left: var(--space-1);
    }

    .reviews {
        background: var(--bg-secondary);
        overflow: hidden;
    }

    .reviews .section-title {
        padding-inline: var(--space-8);
    }

    .reviews-marquee {
        overflow: hidden;
        margin-top: var(--space-8);
        mask-image: linear-gradient(to right, transparent, black 5%, black 95%, transparent);
        -webkit-mask-image: linear-gradient(
            to right,
            transparent,
            black 5%,
            black 95%,
            transparent
        );
    }

    .reviews-track {
        display: flex;
        gap: var(--space-4);
        width: max-content;
        animation: marquee 60s linear infinite;
    }

    .reviews-marquee.paused .reviews-track {
        animation-play-state: paused;
    }

    @keyframes marquee {
        0% {
            transform: translateX(0);
        }
        100% {
            transform: translateX(-50%);
        }
    }

    .review-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
        padding: var(--space-5);
        background: var(--bg-primary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        min-width: 18rem;
        max-width: 22rem;
        flex-shrink: 0;
        transition: var(--transition-shadow);
    }

    .review-card:hover {
        box-shadow: var(--shadow-md);
    }

    .review-header {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .review-author {
        display: flex;
        flex-direction: column;
    }

    .review-name {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }

    .review-role {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .review-text {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        font-style: italic;
    }

    .companies-row {
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
        gap: var(--space-8);
        margin-top: var(--space-8);
    }

    .company-logo {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-2);
        opacity: 0.6;
        transition: opacity var(--duration-normal) var(--ease-in-out);
    }

    .company-logo:hover {
        opacity: 1;
    }

    .company-name {
        font-size: var(--font-xs);
        color: var(--text-secondary);
        font-weight: var(--weight-medium);
    }

    .companies-disclaimer {
        margin-top: var(--space-6);
        text-align: center;
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        font-style: italic;
    }

    .cta {
        padding-bottom: var(--space-16);
    }

    .cta-card {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        padding: var(--space-16) var(--space-8);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
    }

    .cta-title {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-3);
    }

    .cta-subtitle {
        font-size: var(--font-base);
        color: var(--text-secondary);
        margin-bottom: var(--space-8);
        max-width: 30rem;
    }

    .cta-actions {
        display: flex;
        gap: var(--space-4);
    }

    @media (max-width: 768px) {
        .steps-grid {
            grid-template-columns: 1fr;
        }

        .stats-grid {
            grid-template-columns: repeat(2, 1fr);
        }

        .hero-actions,
        .cta-actions {
            flex-direction: column;
            width: 100%;
        }

        .hero-actions :global(.btn),
        .cta-actions :global(.btn) {
            width: 100%;
        }
    }

    .reviews-action {
        display: flex;
        justify-content: center;
        margin-top: var(--space-2);
        padding-inline: var(--space-8);
    }

    .review-form {
        display: flex;
        flex-direction: column;
        gap: var(--space-5);
    }

    .review-rating-group {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .review-rating-label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }

    .review-stars {
        display: flex;
        gap: var(--space-1);
    }

    .star-btn {
        color: var(--text-tertiary);
        transition: var(--transition-colors);
        padding: var(--space-1);
    }

    .star-btn.active {
        color: var(--color-warning);
    }

    .star-btn:hover {
        color: var(--color-warning);
    }

    @media (prefers-reduced-motion: reduce) {
        .section {
            opacity: 1;
            transform: none;
        }
    }
</style>
