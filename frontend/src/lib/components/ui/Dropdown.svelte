<script lang="ts">
    import type { Snippet } from 'svelte';

    interface Props {
        open?: boolean;
        align?: 'left' | 'right';
        trigger: Snippet;
        children: Snippet;
    }

    let { open = $bindable(false), align = 'right', trigger, children }: Props = $props();

    function handleClickOutside(e: MouseEvent) {
        const target = e.target as HTMLElement;
        if (!target.closest('.dropdown')) open = false;
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === 'Escape') open = false;
    }
</script>

<svelte:window onclick={handleClickOutside} onkeydown={handleKeydown} />

<div class="dropdown">
    <div
        class="dropdown-trigger"
        role="button"
        tabindex={0}
        onclick={() => (open = !open)}
        onkeydown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); open = !open; } }}
    >
        {@render trigger()}
    </div>
    {#if open}
        <div class="dropdown-menu dropdown--{align}" role="menu">
            {@render children()}
        </div>
    {/if}
</div>

<style>
    .dropdown {
        position: relative;
        display: inline-flex;
    }

    .dropdown-trigger {
        display: inline-flex;
    }

    .dropdown-menu {
        position: absolute;
        top: calc(100% + 0.5rem);
        min-width: 11.25rem;
        padding: var(--space-2);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        box-shadow: var(--shadow-lg);
        z-index: var(--z-dropdown);
        animation: scale-in var(--duration-fast) var(--ease-out);
    }

    .dropdown--left { left: 0; }
    .dropdown--right { right: 0; }
</style>
