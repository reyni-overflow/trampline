<script lang="ts">
    import Skeleton from './Skeleton.svelte';

    interface Props {
        mode?: 'grid' | 'list';
    }

    let { mode = 'grid' }: Props = $props();
</script>

{#if mode === 'grid'}
    <div class="skeleton-card grid">
        <div class="sk-top">
            <Skeleton circle height="2.5rem" />
            <div class="sk-badges">
                <Skeleton width="4rem" height="1.25rem" radius="var(--radius-full)" />
                <Skeleton width="3.5rem" height="1.25rem" radius="var(--radius-full)" />
            </div>
        </div>
        <Skeleton width="75%" height="1rem" />
        <Skeleton width="50%" height="0.875rem" />
        <Skeleton width="40%" height="0.875rem" />
        <div class="sk-tags">
            <Skeleton width="3rem" height="1.25rem" radius="var(--radius-full)" />
            <Skeleton width="4rem" height="1.25rem" radius="var(--radius-full)" />
            <Skeleton width="3.5rem" height="1.25rem" radius="var(--radius-full)" />
        </div>
        <Skeleton width="5rem" height="0.75rem" />
    </div>
{:else}
    <div class="skeleton-card list">
        <Skeleton circle height="2.75rem" />
        <div class="sk-main">
            <Skeleton width="60%" height="1rem" />
            <Skeleton width="40%" height="0.75rem" />
        </div>
        <div class="sk-right">
            <Skeleton width="5rem" height="0.875rem" />
            <Skeleton width="4rem" height="0.75rem" />
        </div>
    </div>
{/if}

<style>
    .skeleton-card {
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .skeleton-card.grid {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
        padding: var(--space-5);
    }

    .skeleton-card.list {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
    }

    .sk-top {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
    }

    .sk-badges {
        display: flex;
        gap: var(--space-1);
    }

    .sk-tags {
        display: flex;
        gap: var(--space-1);
    }

    .sk-main {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .sk-right {
        display: flex;
        flex-direction: column;
        align-items: flex-end;
        gap: var(--space-1);
    }
</style>
