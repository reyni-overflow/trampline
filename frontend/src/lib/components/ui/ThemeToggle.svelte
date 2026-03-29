<script lang="ts">
    import { theme } from '$lib/stores/theme';
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';

    function toggle() {
        theme.toggle();
    }

    let current = $state('dark');
    const unsub = theme.subscribe((v) => (current = v));
    onDestroy(unsub);

    let resolved = $derived(current === 'system' ? 'dark' : current);
</script>

<button
    class="theme-toggle"
    onclick={toggle}
    aria-label={resolved === 'dark' ? $t('ui.lightTheme') : $t('ui.darkTheme')}
    title={resolved === 'dark' ? $t('ui.lightThemeTitle') : $t('ui.darkThemeTitle')}
>
    <svg
        class="icon sun"
        class:active={resolved === 'light'}
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="1.75"
        stroke-linecap="round"
        stroke-linejoin="round"
    >
        <circle cx="12" cy="12" r="4" /><path d="M12 2v2" /><path d="M12 20v2" /><path
            d="m4.93 4.93 1.41 1.41"
        /><path d="m17.66 17.66 1.41 1.41" /><path d="M2 12h2" /><path d="M20 12h2" /><path
            d="m6.34 17.66-1.41 1.41"
        /><path d="m19.07 4.93-1.41 1.41" />
    </svg>
    <svg
        class="icon moon"
        class:active={resolved === 'dark'}
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="1.75"
        stroke-linecap="round"
        stroke-linejoin="round"
    >
        <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z" />
    </svg>
</button>

<style>
    .theme-toggle {
        position: relative;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        border-radius: var(--radius-md);
        background: transparent;
        transition: var(--transition-colors);
        cursor: pointer;
    }

    .theme-toggle:hover {
        background: var(--bg-tertiary);
    }

    .theme-toggle:active {
        transform: scale(0.93);
    }

    .icon {
        position: absolute;
        width: 1.25rem;
        height: 1.25rem;
        transition:
            opacity var(--duration-moderate) var(--ease-out),
            transform var(--duration-moderate) var(--ease-spring);
        opacity: 0;
        transform: scale(0.6) rotate(-30deg);
    }

    .icon.active {
        opacity: 1;
        transform: scale(1) rotate(0deg);
    }
</style>
