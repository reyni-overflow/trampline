<script lang="ts">
    import type { Snippet } from 'svelte';
    import { t } from '$lib/i18n';

    interface Props {
        removable?: boolean;
        selected?: boolean;
        clickable?: boolean;
        onremove?: () => void;
        onclick?: () => void;
        children: Snippet;
    }

    let {
        removable = false,
        selected = false,
        clickable = false,
        onremove,
        onclick,
        children
    }: Props = $props();
</script>

{#if clickable}
    <button class="tag clickable" class:selected type="button" {onclick}>
        {@render children()}
        {#if removable}
            <span
                class="remove"
                role="button"
                tabindex={-1}
                onclick={(e) => {
                    e.stopPropagation();
                    onremove?.();
                }}
                onkeydown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        e.stopPropagation();
                        onremove?.();
                    }
                }}
                aria-label={$t('common.remove')}
            >
                <svg
                    viewBox="0 0 24 24"
                    width="14"
                    height="14"
                    stroke="currentColor"
                    stroke-width="2"
                    fill="none"
                    stroke-linecap="round"
                    ><line x1="18" y1="6" x2="6" y2="18" /><line
                        x1="6"
                        y1="6"
                        x2="18"
                        y2="18"
                    /></svg
                >
            </span>
        {/if}
    </button>
{:else}
    <span class="tag" class:selected>
        {@render children()}
        {#if removable}
            <button
                class="remove"
                onclick={(e) => {
                    e.stopPropagation();
                    onremove?.();
                }}
                aria-label={$t('common.remove')}
                type="button"
            >
                <svg
                    viewBox="0 0 24 24"
                    width="14"
                    height="14"
                    stroke="currentColor"
                    stroke-width="2"
                    fill="none"
                    stroke-linecap="round"
                    ><line x1="18" y1="6" x2="6" y2="18" /><line
                        x1="6"
                        y1="6"
                        x2="18"
                        y2="18"
                    /></svg
                >
            </button>
        {/if}
    </span>
{/if}

<style>
    .tag {
        display: inline-flex;
        align-items: center;
        gap: var(--space-1);
        padding: 0.25rem 0.75rem;
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        font-family: inherit;
        background: var(--bg-tertiary);
        color: var(--text-secondary);
        border: none;
        border-radius: var(--radius-full);
        transition: var(--transition-colors);
        white-space: nowrap;
    }

    .tag.clickable {
        cursor: pointer;
    }

    .tag.clickable:hover {
        background: var(--border-hover);
        color: var(--text-primary);
    }

    .tag.selected {
        background: var(--accent-subtle);
        color: var(--accent);
        box-shadow: inset 0 0 0 1px var(--accent);
    }

    .remove {
        display: flex;
        align-items: center;
        margin-left: 2px;
        margin-right: -0.25rem;
        padding: 2px;
        color: var(--text-tertiary);
        border-radius: 50%;
        transition: var(--transition-colors);
    }

    .remove:hover {
        color: var(--color-error);
        background: var(--color-error-subtle);
    }
</style>
