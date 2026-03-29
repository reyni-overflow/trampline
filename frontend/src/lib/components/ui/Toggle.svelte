<script lang="ts">
    interface Props {
        checked?: boolean;
        label?: string;
        disabled?: boolean;
        id?: string;
    }

    let { checked = $bindable(false), label, disabled = false, id }: Props = $props();
</script>

<label class="toggle" class:disabled>
    <input type="checkbox" bind:checked {disabled} {id} class="sr-only" />
    <span class="track" class:on={checked}>
        <span class="thumb"></span>
    </span>
    {#if label}
        <span class="label">{label}</span>
    {/if}
</label>

<style>
    .toggle {
        display: inline-flex;
        align-items: center;
        gap: var(--space-3);
        cursor: pointer;
        user-select: none;
    }

    .toggle.disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .track {
        position: relative;
        width: 2.75rem;
        height: 1.5rem;
        background: var(--bg-tertiary);
        border-radius: var(--radius-full);
        transition: background-color var(--duration-normal) var(--ease-in-out);
    }

    .track.on {
        background: var(--accent);
    }

    .thumb {
        position: absolute;
        top: 0.125rem;
        left: 0.125rem;
        width: 1.25rem;
        height: 1.25rem;
        background: var(--text-inverse);
        border-radius: 50%;
        box-shadow: 0 1px 0.1875rem rgba(0, 0, 0, 0.2);
        transition: transform var(--duration-normal) var(--ease-spring);
    }

    .track.on .thumb {
        transform: translateX(1.25rem);
    }

    .toggle input:focus-visible + .track {
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }

    .toggle:active .thumb {
        transform: scaleX(1.1);
    }

    .toggle:active .track.on .thumb {
        transform: translateX(1.25rem) scaleX(1.1);
    }

    .label {
        font-size: var(--font-sm);
        color: var(--text-primary);
    }
</style>
