<script lang="ts">
    import { page } from '$app/state';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import { isAuthenticated } from '$lib/stores/auth';
    import { authModal } from '$lib/stores/auth-modal';
    import { toast } from '$lib/stores/toast';
    import { handleApiError } from '$lib/api/client';
    import { workersApi } from '$lib/api/workers';
    import { contactsApi } from '$lib/api/contacts';
    import { onMount, onDestroy } from 'svelte';
    import { t, tGet } from '$lib/i18n';

    let isAuth = $state(false);
    const unsubAuth = isAuthenticated.subscribe((v) => (isAuth = v));
    onDestroy(unsubAuth);

    let _loading = $state(true);
    let isPrivate = $state(false);

    let profile = $state({
        id: page.params.id,
        name: '',
        lastName: '',
        patronymic: '',
        avatar: null as string | null,
        university: '',
        course: '',
        about: '',
        skills: [] as string[],
        repos: [] as { name: string; url: string }[],
        isContact: false
    });

    function applyProfileData(data: {
        name?: string;
        lastName?: string;
        patronymic?: string;
        photo?: string | null;
        info?: { university?: string; course?: number } | null;
        about?: string | null;
        skills?: string[];
        repos?: string[];
    }) {
        profile = {
            id: page.params.id,
            name: data.name ?? '',
            lastName: data.lastName ?? '',
            patronymic: data.patronymic ?? '',
            avatar: data.photo ?? null,
            university: data.info?.university ?? '',
            course: data.info?.course ? `${data.info.course} ${tGet('profile.courseLabel')}` : '',
            about: data.about ?? '',
            skills: data.skills ?? [],
            repos: (data.repos ?? []).map((r: string) => ({
                name: r.replace(/^https?:\/\//, ''),
                url: r.startsWith('http') ? r : `https://${r}`
            })),
            isContact: false
        };
    }

    onMount(async () => {
        try {
            const profileId = page.params.id ?? '';
            const data = await workersApi.getByUser(profileId);
            applyProfileData(data);
        } catch (err: unknown) {
            if ((err as Record<string, unknown>)?.status === 404) {
                try {
                    const data = await workersApi.getById(page.params.id ?? '');
                    applyProfileData(data);
                } catch {
                    isPrivate = true;
                }
            } else {
                handleApiError(err);
            }
        } finally {
            _loading = false;
        }
    });

    let isContact = $state(false);

    async function handleContact() {
        if (!isAuth) {
            authModal.open();
            return;
        }
        const prev = isContact;
        isContact = !isContact;
        toast.success(
            isContact ? $t('profile.contactRequestSent') : $t('profile.contactRequestCancelled')
        );

        try {
            await contactsApi.sendRequest(page.params.id ?? '');
        } catch {
            isContact = prev;
            toast.error($t('common.error'));
        }
    }

    function handleRecommend() {
        if (!isAuth) {
            authModal.open();
            return;
        }
        toast.info($t('profile.recommendUnavailable'));
    }
</script>

<svelte:head>
    <title>{$t('profile.pageTitle', { name: `${profile.lastName} ${profile.name}` })}</title>
</svelte:head>

<div class="profile-page container">
    {#if isPrivate}
        <div class="private-notice">
            <svg
                viewBox="0 0 24 24"
                width="48"
                height="48"
                fill="none"
                stroke="currentColor"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
            >
                <rect width="18" height="11" x="3" y="11" rx="2" /><path
                    d="M7 11V7a5 5 0 0 1 10 0v4"
                />
            </svg>
            <h2>{$t('profile.hidden')}</h2>
            <p>{$t('profile.hiddenText')}</p>
            <Button href="/jobs" variant="outline">{$t('profile.toJobs')}</Button>
        </div>
    {:else}
        <div class="profile-layout">
            <aside class="profile-sidebar">
                <div class="sidebar-top">
                    <Avatar
                        name="{profile.name} {profile.lastName}"
                        src={profile.avatar}
                        size={96}
                    />
                    <h1 class="profile-name">
                        {profile.lastName}
                        {profile.name}
                        {profile.patronymic}
                    </h1>
                    <div class="profile-meta">
                        <Badge variant="info">{profile.university}</Badge>
                        <Badge>{profile.course}</Badge>
                    </div>
                </div>

                <div class="sidebar-actions">
                    <Button
                        size="md"
                        variant={isContact ? 'secondary' : 'primary'}
                        onclick={handleContact}
                    >
                        {#if isContact}
                            <svg
                                viewBox="0 0 24 24"
                                width="16"
                                height="16"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2"
                                stroke-linecap="round"
                                stroke-linejoin="round"><polyline points="20 6 9 17 4 12" /></svg
                            >
                            {$t('profile.inContacts')}
                        {:else}
                            <svg
                                viewBox="0 0 24 24"
                                width="16"
                                height="16"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                ><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" /><circle
                                    cx="9"
                                    cy="7"
                                    r="4"
                                /><line x1="19" y1="8" x2="19" y2="14" /><line
                                    x1="22"
                                    y1="11"
                                    x2="16"
                                    y2="11"
                                /></svg
                            >
                            {$t('profile.addToContacts')}
                        {/if}
                    </Button>
                    <Button size="md" variant="outline" onclick={handleRecommend}>
                        <svg
                            viewBox="0 0 24 24"
                            width="16"
                            height="16"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            ><path d="m22 2-7 20-4-9-9-4Z" /><path d="M22 2 11 13" /></svg
                        >
                        {$t('profile.recommend')}
                    </Button>
                </div>

                {#if profile.repos.length > 0}
                    <div class="sidebar-section">
                        <h3 class="sidebar-heading">{$t('profile.repositories')}</h3>
                        <div class="repo-list">
                            {#each profile.repos as repo (repo.url)}
                                <a href={repo.url} class="repo-link" target="_blank" rel="noopener">
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
                                            d="M15 22v-4a4.8 4.8 0 0 0-1-3.5c3 0 6-2 6-5.5.08-1.25-.27-2.48-1-3.5.28-1.15.28-2.35 0-3.5 0 0-1 0-3 1.5-2.64-.5-5.36-.5-8 0C6 2 5 2 5 2c-.3 1.15-.3 2.35 0 3.5A5.4 5.4 0 0 0 4 9c0 3.5 3 5.5 6 5.5-.39.49-.68 1.05-.85 1.65S8.93 17.38 9 18v4"
                                        /><path d="M9 18c-4.51 2-5-2-7-2" /></svg
                                    >
                                    {repo.name}
                                </a>
                            {/each}
                        </div>
                    </div>
                {/if}
            </aside>

            <main class="profile-main">
                {#if profile.about}
                    <section class="profile-section">
                        <h2 class="section-heading">{$t('profile.about')}</h2>
                        <p class="about-text">{profile.about}</p>
                    </section>
                {/if}

                {#if profile.skills.length > 0}
                    <section class="profile-section">
                        <h2 class="section-heading">{$t('profile.skills')}</h2>
                        <div class="skills-list">
                            {#each profile.skills as skill, _ki (skill + _ki)}
                                <Tag>{skill}</Tag>
                            {/each}
                        </div>
                    </section>
                {/if}

                {#if !profile.about && profile.skills.length === 0}
                    <div class="empty-profile">
                        <p>{$t('profile.noInfo')}</p>
                    </div>
                {/if}
            </main>
        </div>
    {/if}
</div>

<style>
    .profile-page {
        padding-top: var(--space-8);
        padding-bottom: var(--space-16);
        min-height: calc(100dvh - var(--header-height));
    }

    .private-notice {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-16);
        text-align: center;
        color: var(--text-tertiary);
    }

    .private-notice h2 {
        font-size: var(--font-xl);
        font-weight: var(--weight-bold);
        color: var(--text-primary);
    }

    .private-notice p {
        color: var(--text-secondary);
    }

    .profile-layout {
        display: grid;
        grid-template-columns: 18rem 1fr;
        gap: var(--space-8);
        align-items: start;
    }

    .profile-sidebar {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
        padding: var(--space-6);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        position: sticky;
        top: calc(var(--header-height) + var(--space-4));
    }

    .sidebar-top {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        gap: var(--space-3);
    }

    .profile-name {
        font-size: var(--font-lg);
        font-weight: var(--weight-bold);
        line-height: var(--leading-tight);
    }

    .profile-meta {
        display: flex;
        gap: var(--space-2);
        flex-wrap: wrap;
        justify-content: center;
    }

    .sidebar-actions {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .sidebar-actions :global(.btn) {
        width: 100%;
        justify-content: center;
        gap: var(--space-2);
    }

    .sidebar-section {
        border-top: 1px solid var(--border-default);
        padding-top: var(--space-4);
    }

    .sidebar-heading {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.05em;
        margin-bottom: var(--space-3);
    }

    .repo-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .repo-link {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-xs);
        color: var(--text-secondary);
        transition: var(--transition-colors);
        word-break: break-all;
    }

    .repo-link:hover {
        color: var(--accent);
    }

    .repo-link svg {
        flex-shrink: 0;
    }

    .profile-main {
        display: flex;
        flex-direction: column;
        gap: var(--space-8);
    }

    .section-heading {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
        margin-bottom: var(--space-4);
    }

    .about-text {
        font-size: var(--font-base);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
        max-width: 45rem;
    }

    .skills-list {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-2);
    }

    .empty-profile {
        color: var(--text-tertiary);
        font-size: var(--font-sm);
        padding: var(--space-8);
    }

    @media (max-width: 768px) {
        .profile-layout {
            grid-template-columns: 1fr;
        }

        .profile-sidebar {
            position: static;
        }
    }
</style>
