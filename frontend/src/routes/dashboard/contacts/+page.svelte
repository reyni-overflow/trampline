<script lang="ts">
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Tabs from '$lib/components/ui/Tabs.svelte';
    import { toast } from '$lib/stores/toast';
    import { contactsApi, type RecommendationResponse } from '$lib/api/contacts';
    import { handleApiError } from '$lib/api/client';
    import { onMount } from 'svelte';
    import { t } from '$lib/i18n';

    let search = $state('');
    let activeTab = $state('contacts');
    let _loading = $state(true);
    let tabs = $derived([
        { id: 'contacts', label: $t('dashContacts.myContacts') },
        { id: 'incoming', label: $t('dashContacts.incoming') },
        { id: 'recommendations', label: $t('dashContacts.recommendations') }
    ]);

    interface ContactItem {
        id: string;
        contactId: string;
        name: string;
        university: string;
        skills: string[];
        avatar: string | null;
    }

    let contacts = $state<ContactItem[]>([]);
    let incoming = $state<ContactItem[]>([]);
    let recommendations = $state<RecommendationResponse[]>([]);

    onMount(async () => {
        try {
            const [contactsData, incomingData, recsData] = await Promise.all([
                contactsApi.getContacts(),
                contactsApi.getIncoming(),
                contactsApi.getRecommendations().catch(() => [] as RecommendationResponse[])
            ]);
            contacts = contactsData.map((c) => ({
                id: c.contactUserId,
                contactId: c.id,
                name:
                    [c.lastName, c.name, c.patronymic].filter(Boolean).join(' ') || c.contactUserId,
                university: '',
                skills: c.skills || [],
                avatar: c.photo
            }));
            incoming = incomingData.map((c) => ({
                id: c.contactUserId,
                contactId: c.id,
                name:
                    [c.lastName, c.name, c.patronymic].filter(Boolean).join(' ') || c.contactUserId,
                university: '',
                skills: c.skills || [],
                avatar: c.photo
            }));
            recommendations = recsData;
        } catch {
            contacts = [];
            incoming = [];
            recommendations = [];
        } finally {
            _loading = false;
        }
    });

    let filteredContacts = $derived(
        search
            ? contacts.filter((c) => c.name.toLowerCase().includes(search.toLowerCase()))
            : contacts
    );

    async function removeContact(contactId: string, name: string) {
        if (!confirm($t('dashContacts.confirmRemove', { name }))) return;
        try {
            await contactsApi.remove(contactId);
            contacts = contacts.filter((c) => c.contactId !== contactId);
            toast.success($t('dashContacts.removedMsg', { name }));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function recommend(contactUserId: string, name: string) {
        const jobId = prompt($t('dashContacts.recommendPrompt'));
        if (!jobId) return;
        try {
            await contactsApi.recommend(contactUserId, jobId);
            toast.success($t('dashContacts.recommendMsg', { name }));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function acceptRequest(contactId: string, name: string) {
        try {
            await contactsApi.accept(contactId);
            incoming = incoming.filter((c) => c.contactId !== contactId);
            toast.success($t('dashContacts.addedMsg', { name }));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function declineRequest(contactId: string, name: string) {
        try {
            await contactsApi.decline(contactId);
            incoming = incoming.filter((c) => c.contactId !== contactId);
            toast.info($t('dashContacts.rejectedMsg', { name }));
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('dashContacts.pageTitle')}</title>
</svelte:head>

<div class="contacts">
    <h1 class="page-heading">{$t('dashContacts.title')}</h1>

    <Tabs {tabs} bind:active={activeTab} />

    <div class="tab-content">
        {#if activeTab === 'contacts'}
            <div class="contacts-search">
                <SearchInput
                    placeholder={$t('dashContacts.searchPlaceholder')}
                    bind:value={search}
                />
            </div>

            {#if filteredContacts.length === 0}
                <div class="empty">
                    <svg
                        width="48"
                        height="48"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="1.5"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        ><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" /><circle
                            cx="9"
                            cy="7"
                            r="4"
                        /><path d="M22 21v-2a4 4 0 0 0-3-3.87" /><path
                            d="M16 3.13a4 4 0 0 1 0 7.75"
                        /></svg
                    >
                    <p>{$t('dashContacts.notFound')}</p>
                    <p>{$t('dashContacts.notFoundHint')}</p>
                </div>
            {:else}
                <div class="contacts-list">
                    {#each filteredContacts as contact (contact.id)}
                        <div class="contact-card">
                            <a href="/profile/{contact.id}" class="contact-main">
                                <Avatar name={contact.name} src={contact.avatar} size={44} />
                                <div class="contact-info">
                                    <span class="contact-name">{contact.name}</span>
                                    <Badge size="sm">{contact.university}</Badge>
                                </div>
                            </a>
                            <div class="contact-skills">
                                {#each contact.skills as skill (skill)}
                                    <Tag>{skill}</Tag>
                                {/each}
                            </div>
                            <div class="contact-actions">
                                <Button
                                    size="sm"
                                    variant="ghost"
                                    onclick={() => recommend(contact.id, contact.name)}
                                    >{$t('dashContacts.recommend')}</Button
                                >
                                <Button
                                    size="sm"
                                    variant="ghost"
                                    onclick={() => removeContact(contact.contactId, contact.name)}
                                    >{$t('dashContacts.removeContact')}</Button
                                >
                            </div>
                        </div>
                    {/each}
                </div>
            {/if}
        {:else if activeTab === 'incoming'}
            {#if incoming.length === 0}
                <div class="empty">
                    <svg
                        width="48"
                        height="48"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="1.5"
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
                    <p>{$t('dashContacts.noIncoming')}</p>
                    <p>{$t('dashContacts.noIncomingHint')}</p>
                </div>
            {:else}
                <div class="contacts-list">
                    {#each incoming as req (req.id)}
                        <div class="contact-card">
                            <a href="/profile/{req.id}" class="contact-main">
                                <Avatar name={req.name} src={req.avatar} size={44} />
                                <div class="contact-info">
                                    <span class="contact-name">{req.name}</span>
                                    <Badge size="sm">{req.university}</Badge>
                                </div>
                            </a>
                            <div class="contact-skills">
                                {#each req.skills as skill (skill)}
                                    <Tag>{skill}</Tag>
                                {/each}
                            </div>
                            <div class="request-actions">
                                <Button
                                    size="sm"
                                    onclick={() => acceptRequest(req.contactId, req.name)}
                                    >{$t('dashContacts.accept')}</Button
                                >
                                <Button
                                    size="sm"
                                    variant="ghost"
                                    onclick={() => declineRequest(req.contactId, req.name)}
                                    >{$t('dashContacts.reject')}</Button
                                >
                            </div>
                        </div>
                    {/each}
                </div>
            {/if}
        {:else if activeTab === 'recommendations'}
            {#if recommendations.length === 0}
                <div class="empty">
                    <svg
                        width="48"
                        height="48"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="1.5"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        ><path d="m22 2-7 20-4-9-9-4Z" /><path d="M22 2 11 13" /></svg
                    >
                    <p>{$t('dashContacts.noRecommendations')}</p>
                    <p>{$t('dashContacts.noRecommendationsHint')}</p>
                </div>
            {:else}
                <div class="contacts-list">
                    {#each recommendations as rec (rec.id)}
                        <div class="contact-card rec-card">
                            <div class="rec-info">
                                <div class="rec-job-title">{rec.jobTitle}</div>
                                <div class="rec-company">{rec.companyName}</div>
                                {#if rec.message}
                                    <p class="rec-message">&laquo;{rec.message}&raquo;</p>
                                {/if}
                                <div class="rec-meta">
                                    <span class="rec-from"
                                        >{$t('dashContacts.recommendedBy')} {rec.fromUserName}</span
                                    >
                                    <span class="rec-date"
                                        >{new Date(rec.createdAt).toLocaleDateString()}</span
                                    >
                                </div>
                            </div>
                            <Button size="sm" href="/jobs/{rec.jobId}"
                                >{$t('dashContacts.viewJob')}</Button
                            >
                        </div>
                    {/each}
                </div>
            {/if}
        {/if}
    </div>
</div>

<style>
    .contacts {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }

    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .tab-content {
        margin-top: var(--space-4);
    }

    .contacts-search {
        margin-bottom: var(--space-4);
        max-width: 20rem;
    }

    .contacts-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .contact-card {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors);
    }

    .contact-card:hover {
        border-color: var(--border-hover);
    }

    .contact-main {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        text-decoration: none;
        color: inherit;
        min-width: 12rem;
    }

    .contact-main:hover .contact-name {
        color: var(--accent);
    }

    .contact-info {
        display: flex;
        flex-direction: column;
        gap: 2px;
    }

    .contact-name {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        transition: var(--transition-colors);
    }

    .contact-skills {
        display: flex;
        gap: var(--space-1);
        flex-wrap: wrap;
        flex: 1;
    }

    .contact-actions {
        display: flex;
        gap: var(--space-1);
        flex-shrink: 0;
    }

    .request-actions {
        display: flex;
        gap: var(--space-2);
        flex-shrink: 0;
    }

    .empty {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-16) var(--space-4);
        text-align: center;
        color: var(--text-tertiary);
    }
    .empty svg {
        opacity: 0.5;
    }
    .empty p {
        max-width: 20rem;
    }

    .rec-card {
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
    }

    .rec-info {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
        min-width: 0;
        flex: 1;
    }

    .rec-job-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }

    .rec-company {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .rec-message {
        font-size: var(--font-xs);
        color: var(--text-secondary);
        font-style: italic;
        margin-top: var(--space-1);
    }

    .rec-meta {
        display: flex;
        gap: var(--space-3);
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        margin-top: var(--space-1);
    }

    @media (max-width: 640px) {
        .contacts-search {
            max-width: 100%;
        }
    }

    @media (max-width: 768px) {
        .contact-card {
            flex-direction: column;
            align-items: stretch;
        }
        .contact-main {
            min-width: 0;
        }
        .rec-card {
            flex-direction: column;
            align-items: stretch;
        }
    }
</style>
