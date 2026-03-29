<script lang="ts">
    import type { Snippet } from 'svelte';

    interface Props {
        variant?: 'default' | 'success' | 'warning' | 'error' | 'info' | 'accent';
        size?: 'sm' | 'md';
        children: Snippet;
    }

    let { variant = 'default', size = 'sm', children }: Props = $props();
</script>

<span class="badge badge--{variant} badge--{size}">
    {@render children()}
</span>

<style>
    .badge {
        display: inline-flex;
        align-items: center;
        gap: var(--space-1);
        font-weight: var(--weight-medium);
        border-radius: var(--radius-full);
        white-space: nowrap;
    }

    .badge--sm { padding: 2px 0.5rem; font-size: var(--font-xs); }
    .badge--md { padding: 0.25rem 0.75rem; font-size: var(--font-sm); }

    .badge--default { background: var(--bg-tertiary); color: var(--text-secondary); }
    .badge--success { background: var(--color-success-subtle); color: var(--color-success); }
    .badge--warning { background: var(--color-warning-subtle); color: var(--color-warning); }
    .badge--error { background: var(--color-error-subtle); color: var(--color-error); }
    .badge--info { background: var(--color-info-subtle); color: var(--color-info); }
    .badge--accent { background: var(--accent-subtle); color: var(--accent); }
</style>
