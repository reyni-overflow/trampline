<script lang="ts">
    import { t } from '$lib/i18n';

    interface Props {
        mode?: 'grid' | 'list';
        onchange?: (mode: 'grid' | 'list') => void;
    }

    let { mode = $bindable('grid'), onchange }: Props = $props();

    function toggle(m: 'grid' | 'list') {
        mode = m;
        onchange?.(m);
    }
</script>

<div class="view-toggle" role="radiogroup" aria-label={$t('ui.viewLabel')}>
    <button
        class="toggle-btn"
        class:active={mode === 'list'}
        onclick={() => toggle('list')}
        aria-label={$t('ui.viewList')}
        title={$t('ui.viewList')}
        type="button"
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
        >
            <line x1="8" y1="6" x2="21" y2="6" /><line x1="8" y1="12" x2="21" y2="12" /><line
                x1="8"
                y1="18"
                x2="21"
                y2="18"
            /><line x1="3" y1="6" x2="3.01" y2="6" /><line x1="3" y1="12" x2="3.01" y2="12" /><line
                x1="3"
                y1="18"
                x2="3.01"
                y2="18"
            />
        </svg>
    </button>
    <button
        class="toggle-btn"
        class:active={mode === 'grid'}
        onclick={() => toggle('grid')}
        aria-label={$t('ui.viewGrid')}
        title={$t('ui.viewGrid')}
        type="button"
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
        >
            <rect width="7" height="7" x="3" y="3" rx="1" /><rect
                width="7"
                height="7"
                x="14"
                y="3"
                rx="1"
            /><rect width="7" height="7" x="3" y="14" rx="1" /><rect
                width="7"
                height="7"
                x="14"
                y="14"
                rx="1"
            />
        </svg>
    </button>
</div>

<style>
    .view-toggle {
        display: flex;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        padding: 0.125rem;
    }

    .toggle-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.25rem;
        height: 2.25rem;
        border-radius: var(--radius-sm);
        color: var(--text-tertiary);
        transition: var(--transition-colors);
    }

    .toggle-btn:hover {
        color: var(--text-primary);
    }

    .toggle-btn.active {
        background: var(--bg-tertiary);
        color: var(--accent);
    }
</style>
