<script lang="ts">
    import { t } from '$lib/i18n';

    interface Option {
        value: string;
        label: string;
    }

    interface Props {
        options: Option[];
        value?: string;
        placeholder?: string;
        label?: string;
        error?: string;
        searchable?: boolean;
        disabled?: boolean;
        id?: string;
        onchange?: (value: string) => void;
    }

    let {
        options,
        value = $bindable(''),
        placeholder,
        label: labelText,
        error,
        searchable = false,
        disabled = false,
        id,
        onchange
    }: Props = $props();

    let open = $state(false);
    let search = $state('');
    let searchInput = $state<HTMLInputElement>(null!);

    let filtered = $derived(
        searchable && search
            ? options.filter((o) => o.label.toLowerCase().includes(search.toLowerCase()))
            : options
    );

    let selectedLabel = $derived(options.find((o) => o.value === value)?.label || '');

    function toggle() {
        if (disabled) return;
        open = !open;
        if (open && searchable) {
            search = '';
            requestAnimationFrame(() => searchInput?.focus());
        }
    }

    function select(opt: Option) {
        value = opt.value;
        open = false;
        search = '';
        onchange?.(opt.value);
    }

    function handleClickOutside(e: MouseEvent) {
        if (!(e.target as HTMLElement).closest('.select')) {
            open = false;
            search = '';
        }
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === 'Escape') {
            open = false;
            search = '';
        }
    }
</script>

<svelte:window onclick={handleClickOutside} onkeydown={handleKeydown} />

<div class="select-group" class:has-error={!!error}>
    {#if labelText}
        <label class="label" for={id}>{labelText}</label>
    {/if}
    <div class="select" class:disabled>
        <button
            class="select-trigger"
            class:open
            class:has-value={!!value}
            {disabled}
            type="button"
            {id}
            onclick={toggle}
            aria-expanded={open}
            aria-haspopup="listbox"
        >
            <span class="select-text"
                >{selectedLabel || placeholder || $t('ui.selectPlaceholder')}</span
            >
            <svg
                class="chevron"
                class:rotated={open}
                viewBox="0 0 24 24"
                width="18"
                height="18"
                stroke="currentColor"
                stroke-width="2"
                fill="none"
                stroke-linecap="round"
                stroke-linejoin="round"
            >
                <polyline points="6 9 12 15 18 9" />
            </svg>
        </button>

        {#if open}
            <div class="select-dropdown" role="listbox">
                {#if searchable}
                    <div class="search-wrapper">
                        <input
                            bind:this={searchInput}
                            bind:value={search}
                            class="search-input"
                            placeholder={$t('ui.selectSearch')}
                            type="text"
                        />
                    </div>
                {/if}
                <div class="options scrollbar-thin">
                    {#each filtered as opt (opt.value)}
                        <button
                            class="option"
                            class:selected={opt.value === value}
                            role="option"
                            aria-selected={opt.value === value}
                            type="button"
                            onclick={() => select(opt)}
                        >
                            <span>{opt.label}</span>
                            {#if opt.value === value}
                                <svg
                                    viewBox="0 0 24 24"
                                    width="16"
                                    height="16"
                                    stroke="currentColor"
                                    stroke-width="2.5"
                                    fill="none"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                >
                                    <polyline points="20 6 9 17 4 12" />
                                </svg>
                            {/if}
                        </button>
                    {:else}
                        <div class="no-results">{$t('ui.selectNotFound')}</div>
                    {/each}
                </div>
            </div>
        {/if}
    </div>
    {#if error}
        <p class="message error-message">{error}</p>
    {/if}
</div>

<style>
    .select-group {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }

    .select {
        position: relative;
    }

    .select.disabled {
        opacity: 0.5;
        pointer-events: none;
    }

    .select-trigger {
        display: flex;
        align-items: center;
        justify-content: space-between;
        width: 100%;
        height: 2.75rem;
        padding: 0 0.875rem;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        color: var(--text-tertiary);
        font-size: var(--font-base);
        text-align: left;
        transition:
            var(--transition-colors),
            border-color var(--duration-normal) var(--ease-in-out),
            box-shadow var(--duration-normal) var(--ease-in-out);
    }

    .select-trigger.has-value {
        color: var(--text-primary);
    }

    .select-trigger:hover {
        border-color: var(--border-hover);
    }

    .select-trigger:focus-visible {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }

    .select-trigger.open {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }

    .has-error .select-trigger {
        border-color: var(--color-error);
    }

    .chevron {
        flex-shrink: 0;
        transition: transform var(--duration-fast) var(--ease-out);
        color: var(--text-tertiary);
    }

    .chevron.rotated {
        transform: rotate(180deg);
    }

    .select-dropdown {
        position: absolute;
        top: calc(100% + 0.375rem);
        left: 0;
        right: 0;
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        box-shadow: var(--shadow-lg);
        z-index: var(--z-dropdown);
        overflow: hidden;
        animation: scale-in var(--duration-fast) var(--ease-out);
    }

    .search-wrapper {
        padding: var(--space-2);
        border-bottom: 1px solid var(--border-default);
    }

    .search-input {
        width: 100%;
        height: 2.25rem;
        padding: 0 0.625rem;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        font-size: var(--font-sm);
        color: var(--text-primary);
    }

    .search-input::placeholder {
        color: var(--text-tertiary);
    }

    .search-input:focus {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }

    .options {
        max-height: 15rem;
        overflow-y: auto;
        padding: var(--space-1);
    }

    .option {
        display: flex;
        align-items: center;
        justify-content: space-between;
        width: 100%;
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-sm);
        border-radius: var(--radius-md);
        text-align: left;
        transition: var(--transition-colors);
        cursor: pointer;
        color: var(--text-primary);
    }

    .option:hover {
        background: var(--bg-tertiary);
    }

    .option.selected {
        background: var(--accent-subtle);
        color: var(--accent);
    }

    .no-results {
        padding: var(--space-4);
        text-align: center;
        font-size: var(--font-sm);
        color: var(--text-tertiary);
    }

    .message {
        font-size: var(--font-xs);
        animation: slide-down var(--duration-fast) var(--ease-out);
    }
    .error-message {
        color: var(--color-error);
    }
</style>
