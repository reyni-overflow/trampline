<script lang="ts">
    import Skeleton from './Skeleton.svelte';
</script>

<div class="skeleton-card">
    <Skeleton circle height="3rem" />
    <Skeleton width="70%" height="1rem" />
    <Skeleton width="50%" height="0.875rem" />
    <div class="sk-footer">
        <Skeleton width="4rem" height="0.75rem" />
        <Skeleton width="6rem" height="0.75rem" />
    </div>
</div>

<style>
    .skeleton-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .sk-footer {
        display: flex;
        justify-content: space-between;
        padding-top: var(--space-2);
        border-top: 1px solid var(--border-default);
    }
</style>
