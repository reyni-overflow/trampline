<script lang="ts">
    import Tag from '$lib/components/ui/Tag.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import { toast } from '$lib/stores/toast';
    import { onMount } from 'svelte';
    import { adminApi } from '$lib/api/admin';
    import { jobsApi, type TagStatsResponse } from '$lib/api/jobs';
    import { handleApiError } from '$lib/api/client';
    import { t } from '$lib/i18n';

    let search = $state('');
    let newTag = $state('');

    let apiTags = $state<{ name: string; count: number }[]>([]);
    let tagStats = $state<TagStatsResponse[]>([]);
    let allTags = $derived(apiTags);

    let filtered = $derived(
        search
            ? allTags.filter((t) => t.name.toLowerCase().includes(search.toLowerCase()))
            : allTags
    );

    async function addTag() {
        const name = newTag.trim();
        if (!name) return;
        if (allTags.some((t) => t.name.toLowerCase() === name.toLowerCase())) {
            toast.warning($t('adminTags.tagExists'));
            return;
        }
        try {
            await adminApi.createTag(name);
            apiTags = [...apiTags, { name, count: 0 }];
            newTag = '';
            toast.success($t('adminTags.addedMsg', { name }));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteTag(name: string) {
        try {
            await adminApi.deleteTag(name);
            apiTags = apiTags.filter((t) => t.name !== name);
            toast.success($t('adminTags.deletedMsg', { name }));
        } catch (err) {
            handleApiError(err);
        }
    }

    function getStats(name: string): TagStatsResponse | undefined {
        return tagStats.find((s) => s.name === name);
    }

    onMount(async () => {
        try {
            const [tags, stats] = await Promise.all([
                adminApi.getTags(),
                jobsApi.getTagStats().catch(() => [] as TagStatsResponse[])
            ]);
            apiTags = tags;
            tagStats = stats;
        } catch (err) {
            handleApiError(err);
        }
    });
</script>

<svelte:head><title>{$t('adminTags.pageTitle')}</title></svelte:head>

<div class="tags-page">
    <h1 class="page-heading">{$t('adminTags.title')}</h1>

    <div class="controls">
        <div class="search-box">
            <Input placeholder={$t('adminTags.searchPlaceholder')} bind:value={search} />
        </div>
        <form
            class="add-form"
            onsubmit={(e) => {
                e.preventDefault();
                addTag();
            }}
        >
            <div class="add-input-wrapper">
                <input
                    class="add-input"
                    type="text"
                    placeholder={$t('adminTags.newTagPlaceholder')}
                    bind:value={newTag}
                />
                <button class="add-btn" type="submit" title={$t('common.add')}>
                    <svg
                        viewBox="0 0 24 24"
                        width="16"
                        height="16"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2.5"
                        stroke-linecap="round"
                        ><line x1="12" y1="5" x2="12" y2="19" /><line
                            x1="5"
                            y1="12"
                            x2="19"
                            y2="12"
                        /></svg
                    >
                </button>
            </div>
        </form>
    </div>

    <div class="tags-grid">
        {#each filtered.sort((a, b) => b.count - a.count) as tag (tag.name)}
            <div class="tag-card">
                <div class="tag-main">
                    <Tag>{tag.name}</Tag>
                    <Badge size="sm">{tag.count} {$t('adminTags.jobs')}</Badge>
                    {#if getStats(tag.name)}
                        <Badge variant="info" size="sm"
                            >{getStats(tag.name)?.eventCount ?? 0} {$t('adminTags.events')}</Badge
                        >
                    {/if}
                </div>
                <button
                    class="delete-btn"
                    type="button"
                    onclick={() => deleteTag(tag.name)}
                    title={$t('common.delete')}
                >
                    <svg
                        viewBox="0 0 24 24"
                        width="14"
                        height="14"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2"
                        stroke-linecap="round"
                        ><line x1="18" y1="6" x2="6" y2="18" /><line
                            x1="6"
                            y1="6"
                            x2="18"
                            y2="18"
                        /></svg
                    >
                </button>
            </div>
        {/each}
    </div>
</div>

<style>
    .tags-page {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }
    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }
    .controls {
        display: flex;
        gap: var(--space-4);
        flex-wrap: wrap;
    }
    .search-box {
        width: 14rem;
    }
    .add-form {
        display: flex;
    }
    .add-input-wrapper {
        display: flex;
        align-items: center;
        width: 14rem;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        transition:
            border-color var(--duration-normal) var(--ease-in-out),
            box-shadow var(--duration-normal) var(--ease-in-out);
    }
    .add-input-wrapper:focus-within {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }
    .add-input-wrapper:hover:not(:focus-within) {
        border-color: var(--border-hover);
    }
    .add-input {
        flex: 1;
        height: 2.5rem;
        min-width: 0;
        padding: 0 0.75rem;
        background: transparent;
        border: none;
        outline: none;
        color: var(--text-primary);
        font-size: var(--font-sm);
    }
    .add-input::placeholder {
        color: var(--text-tertiary);
    }
    .add-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        flex-shrink: 0;
        margin-right: 0.25rem;
        color: var(--accent);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }
    .add-btn:hover {
        background: var(--accent-subtle);
    }

    .tags-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(14rem, 1fr));
        gap: var(--space-2);
    }
    .tag-card {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-3) var(--space-4);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
    }
    .tag-main {
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }
    .delete-btn {
        display: flex;
        color: var(--text-tertiary);
        transition: var(--transition-colors);
        padding: var(--space-1);
        border-radius: var(--radius-sm);
    }
    .delete-btn:hover {
        color: var(--color-error);
        background: var(--color-error-subtle);
    }
</style>
