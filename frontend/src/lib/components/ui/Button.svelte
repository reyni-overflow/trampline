<script lang="ts">
    import type { Snippet } from 'svelte';

    interface Props {
        variant?: 'primary' | 'secondary' | 'ghost' | 'outline' | 'danger';
        size?: 'sm' | 'md' | 'lg';
        loading?: boolean;
        href?: string;
        children: Snippet;
        disabled?: boolean;
        [key: string]: unknown;
    }

    let {
        variant = 'primary',
        size = 'md',
        loading = false,
        href,
        children,
        disabled,
        ...rest
    }: Props = $props();
</script>

{#if href && !disabled}
    <a
        {href}
        class="btn btn--{variant} btn--{size}"
        class:loading
        {...rest}
    >
        {#if loading}
            <span class="spinner"></span>
        {/if}
        <span class="content" class:invisible={loading}>{@render children()}</span>
    </a>
{:else}
    <button
        class="btn btn--{variant} btn--{size}"
        class:loading
        disabled={disabled || loading}
        {...rest}
    >
        {#if loading}
            <span class="spinner"></span>
        {/if}
        <span class="content" class:invisible={loading}>{@render children()}</span>
    </button>
{/if}

<style>
    .btn {
        position: relative;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: var(--space-2);
        font-weight: var(--weight-medium);
        border-radius: var(--radius-md);
        transition: var(--transition-colors), var(--transition-transform), var(--transition-shadow);
        cursor: pointer;
        user-select: none;
        white-space: nowrap;
        text-decoration: none;
    }

    .btn:active:not(:disabled) {
        transform: scale(0.97);
    }

    .btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .btn--sm { height: 2rem; padding: 0 0.75rem; font-size: var(--font-sm); }
    .btn--md { height: 2.5rem; padding: 0 1rem; font-size: var(--font-sm); }
    .btn--lg { height: 3rem; padding: 0 1.5rem; font-size: var(--font-base); }

    .btn--primary {
        background: var(--accent);
        color: var(--accent-contrast);
    }
    .btn--primary:hover:not(:disabled) {
        background: var(--accent-hover);
        box-shadow: var(--shadow-md);
    }

    .btn--secondary {
        background: var(--bg-tertiary);
        color: var(--text-primary);
    }
    .btn--secondary:hover:not(:disabled) {
        background: var(--border-hover);
    }

    .btn--ghost {
        background: transparent;
        color: var(--accent);
    }
    .btn--ghost:hover:not(:disabled) {
        background: var(--accent-subtle);
    }

    .btn--outline {
        background: transparent;
        color: var(--text-primary);
        box-shadow: inset 0 0 0 1px var(--border-default);
    }
    .btn--outline:hover:not(:disabled) {
        box-shadow: inset 0 0 0 1px var(--border-hover);
        background: var(--bg-secondary);
    }

    .btn--danger {
        background: var(--color-danger);
        color: var(--text-inverse);
    }
    .btn--danger:hover:not(:disabled) {
        background: var(--color-danger-dark, #B91C1C);
    }

    .spinner {
        position: absolute;
        width: 1.125rem;
        height: 1.125rem;
        border: 2px solid transparent;
        border-top-color: currentColor;
        border-radius: 50%;
        animation: spin 0.6s linear infinite;
    }

    .content.invisible {
        visibility: hidden;
    }
</style>
