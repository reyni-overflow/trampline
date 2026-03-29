<script lang="ts">
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';

    interface Props {
        value?: string;
        placeholder?: string;
        debounce?: number;
        onsearch?: (value: string) => void;
    }

    let {
        value = $bindable(''),
        placeholder,
        debounce = 300,
        onsearch
    }: Props = $props();

    let timer: ReturnType<typeof setTimeout>;
    onDestroy(() => clearTimeout(timer));

    function handleInput(e: Event) {
        const v = (e.target as HTMLInputElement).value;
        value = v;
        clearTimeout(timer);
        timer = setTimeout(() => onsearch?.(v), debounce);
    }

    function clear() {
        value = '';
        onsearch?.('');
    }
</script>

<div class="search-input-wrapper">
    <svg class="search-icon" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
    </svg>
    <input
        class="search-field"
        type="text"
        placeholder={placeholder || $t('common.search')}
        {value}
        oninput={handleInput}
    />
    {#if value}
        <button class="clear-btn" type="button" onclick={clear} aria-label={$t('common.close')}>
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
        </button>
    {/if}
</div>

<style>
    .search-input-wrapper {
        position: relative;
        display: flex;
        align-items: center;
    }

    .search-icon {
        position: absolute;
        left: 0.75rem;
        color: var(--text-tertiary);
        pointer-events: none;
    }

    .search-field {
        width: 100%;
        height: 2.5rem;
        padding: 0 2.25rem 0 2.5rem;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        color: var(--text-primary);
        font-size: var(--font-sm);
        transition: var(--transition-colors), border-color var(--duration-normal) var(--ease-in-out),
            box-shadow var(--duration-normal) var(--ease-in-out);
    }

    .search-field::placeholder {
        color: var(--text-tertiary);
    }

    .search-field:hover:not(:focus) {
        border-color: var(--border-hover);
    }

    .search-field:focus {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
        outline: none;
    }

    .clear-btn {
        position: absolute;
        right: 0.5rem;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 1.5rem;
        height: 1.5rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }

    .clear-btn:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }
</style>
