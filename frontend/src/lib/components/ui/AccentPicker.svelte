<script lang="ts">
    import { accent, ACCENT_COLORS, type AccentColor } from '$lib/stores/theme';
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';

    let open = $state(false);
    let currentAccent = $state<AccentColor>('neutral');

    const unsub = accent.subscribe((v) => (currentAccent = v));
    onDestroy(unsub);

    let previewColor = $state<AccentColor | null>(null);

    function select(color: AccentColor) {
        previewColor = null;
        accent.set(color);
        open = false;
    }

    function previewEnter(color: AccentColor) {
        previewColor = color;
        document.documentElement.setAttribute('data-accent', color);
    }

    function previewLeave() {
        previewColor = null;
        document.documentElement.setAttribute('data-accent', currentAccent);
    }

    function toggleOpen() {
        open = !open;
    }

    function closeDropdown() {
        open = false;
        if (previewColor) {
            document.documentElement.setAttribute('data-accent', currentAccent);
            previewColor = null;
        }
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === 'Escape') closeDropdown();
    }

    function handleClickOutside(e: MouseEvent) {
        const target = e.target as HTMLElement;
        if (!target.closest('.accent-picker')) closeDropdown();
    }
</script>

<svelte:window onkeydown={handleKeydown} onclick={handleClickOutside} />

<div class="accent-picker">
    <button
        class="trigger"
        onclick={toggleOpen}
        aria-label={$t('ui.accentLabel')}
        aria-expanded={open}
        title={$t('ui.accentTitle')}
    >
        <svg
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.75"
            stroke-linecap="round"
            stroke-linejoin="round"
            width="20"
            height="20"
        >
            <circle cx="13.5" cy="6.5" r=".5" fill="currentColor" /><circle
                cx="17.5"
                cy="10.5"
                r=".5"
                fill="currentColor"
            /><circle cx="8.5" cy="7.5" r=".5" fill="currentColor" /><circle
                cx="6.5"
                cy="12.5"
                r=".5"
                fill="currentColor"
            /><path
                d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.926 0 1.648-.746 1.648-1.688 0-.437-.18-.835-.437-1.125-.29-.289-.438-.652-.438-1.125a1.64 1.64 0 0 1 1.668-1.668h1.996c3.051 0 5.555-2.503 5.555-5.554C21.965 6.012 17.461 2 12 2z"
            />
        </svg>
    </button>

    {#if open}
        <div class="dropdown" role="listbox" aria-label={$t('ui.accentListLabel')}>
            {#each ACCENT_COLORS as { id, labelKey, color } (id)}
                <button
                    class="color-option"
                    class:selected={currentAccent === id}
                    role="option"
                    aria-selected={currentAccent === id}
                    onclick={() => select(id)}
                    onmouseenter={() => previewEnter(id)}
                    onmouseleave={previewLeave}
                    title={$t(labelKey)}
                >
                    <span
                        class="swatch"
                        class:neutral={id === 'neutral'}
                        style={id !== 'neutral' ? `background-color: ${color}` : ''}
                    ></span>
                    <span class="label">{$t(labelKey)}</span>
                    {#if currentAccent === id}
                        <svg
                            class="check"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2.5"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            width="16"
                            height="16"
                        >
                            <polyline points="20 6 9 17 4 12" />
                        </svg>
                    {/if}
                </button>
            {/each}
        </div>
    {/if}
</div>

<style>
    .accent-picker {
        position: relative;
    }

    .trigger {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
    }

    .trigger:hover {
        background: var(--bg-tertiary);
    }

    .trigger:active {
        transform: scale(0.93);
    }

    .dropdown {
        position: absolute;
        top: calc(100% + 0.5rem);
        right: 0;
        min-width: 12.5rem;
        padding: var(--space-2);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        box-shadow: var(--shadow-lg);
        z-index: var(--z-dropdown);
        animation: scale-in var(--duration-fast) var(--ease-out);
    }

    .color-option {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        width: 100%;
        padding: var(--space-2) var(--space-3);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        cursor: pointer;
        text-align: left;
    }

    .color-option:hover {
        background: var(--bg-tertiary);
    }

    .color-option.selected {
        background: var(--accent-subtle);
    }

    .swatch {
        width: 1.125rem;
        height: 1.125rem;
        border-radius: var(--radius-full);
        flex-shrink: 0;
    }

    .swatch.neutral {
        background: linear-gradient(135deg, #ffffff 50%, #000000 50%);
        border: 1px solid var(--border-default);
    }

    .label {
        flex: 1;
        font-size: var(--font-sm);
        color: var(--text-primary);
    }

    .check {
        color: var(--accent);
        flex-shrink: 0;
    }
</style>
