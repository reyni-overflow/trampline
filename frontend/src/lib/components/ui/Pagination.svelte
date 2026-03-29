<script lang="ts">
    import { t } from '$lib/i18n';

    interface Props {
        page: number;
        totalPages: number;
        onchange?: (page: number) => void;
    }

    let { page = $bindable(1), totalPages, onchange }: Props = $props();

    let pages = $derived.by(() => {
        const items: (number | '...')[] = [];
        if (totalPages <= 7) {
            for (let i = 1; i <= totalPages; i++) items.push(i);
        } else {
            items.push(1);
            if (page > 3) items.push('...');
            const start = Math.max(2, page - 1);
            const end = Math.min(totalPages - 1, page + 1);
            for (let i = start; i <= end; i++) items.push(i);
            if (page < totalPages - 2) items.push('...');
            items.push(totalPages);
        }
        return items;
    });

    function go(p: number) {
        if (p < 1 || p > totalPages || p === page) return;
        page = p;
        onchange?.(p);
    }
</script>

{#if totalPages > 1}
    <nav class="pagination" aria-label={$t('ui.paginationLabel')}>
        <button
            class="page-btn nav-btn"
            disabled={page === 1}
            onclick={() => go(page - 1)}
            type="button"
            aria-label={$t('ui.paginationPrev')}
        >
            <svg
                viewBox="0 0 24 24"
                width="18"
                height="18"
                stroke="currentColor"
                stroke-width="2"
                fill="none"
                stroke-linecap="round"
                stroke-linejoin="round"><polyline points="15 18 9 12 15 6" /></svg
            >
        </button>

        <div class="pages">
            {#each pages as p, i (i)}
                {#if p === '...'}
                    <span class="ellipsis">...</span>
                {:else}
                    <button
                        class="page-btn"
                        class:active={p === page}
                        onclick={() => go(p)}
                        type="button"
                        aria-label="{$t('ui.paginationPage')} {p}"
                        aria-current={p === page ? 'page' : undefined}>{p}</button
                    >
                {/if}
            {/each}
        </div>

        <button
            class="page-btn nav-btn"
            disabled={page === totalPages}
            onclick={() => go(page + 1)}
            type="button"
            aria-label={$t('ui.paginationNext')}
        >
            <svg
                viewBox="0 0 24 24"
                width="18"
                height="18"
                stroke="currentColor"
                stroke-width="2"
                fill="none"
                stroke-linecap="round"
                stroke-linejoin="round"><polyline points="9 18 15 12 9 6" /></svg
            >
        </button>
    </nav>
{/if}

<style>
    .pagination {
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }

    .pages {
        display: flex;
        align-items: center;
        gap: var(--space-1);
    }

    .page-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        min-width: 2.25rem;
        height: 2.25rem;
        padding: 0 var(--space-2);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
    }

    .page-btn:hover:not(:disabled):not(.active) {
        background: var(--bg-tertiary);
        color: var(--text-primary);
    }

    .page-btn.active {
        background: var(--accent);
        color: var(--accent-contrast);
    }

    .page-btn:disabled {
        opacity: 0.3;
        cursor: not-allowed;
    }

    .ellipsis {
        color: var(--text-tertiary);
        padding: 0 var(--space-1);
    }

    @media (max-width: 640px) {
        .pages {
            display: none;
        }
    }
</style>
