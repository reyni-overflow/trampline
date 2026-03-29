<script lang="ts">
    interface Props {
        checked?: boolean;
        label?: string;
        disabled?: boolean;
        id?: string;
    }

    let { checked = $bindable(false), label, disabled = false, id }: Props = $props();
</script>

<label class="checkbox" class:disabled>
    <input type="checkbox" bind:checked {disabled} {id} class="sr-only" />
    <span class="box" class:checked>
        <svg
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="3"
            stroke-linecap="round"
            stroke-linejoin="round"
        >
            <polyline points="20 6 9 17 4 12" />
        </svg>
    </span>
    {#if label}
        <span class="label">{label}</span>
    {/if}
</label>

<style>
    .checkbox {
        display: inline-flex;
        align-items: center;
        gap: var(--space-2);
        cursor: pointer;
        user-select: none;
    }

    .checkbox.disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .box {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 1.25rem;
        height: 1.25rem;
        border: 2px solid var(--border-hover);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors), var(--transition-transform);
    }

    .box svg {
        width: 0.875rem;
        height: 0.875rem;
        opacity: 0;
        stroke-dasharray: 20;
        stroke-dashoffset: 20;
        transition: opacity var(--duration-normal) var(--ease-out);
    }

    .box.checked {
        background: var(--accent);
        border-color: var(--accent);
    }

    .box.checked svg {
        opacity: 1;
        color: var(--accent-contrast);
        animation: check-stroke var(--duration-moderate) var(--ease-out) forwards;
    }

    .checkbox:hover .box:not(.checked) {
        border-color: var(--accent);
    }

    .checkbox input:focus-visible + .box {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }

    .checkbox:active .box {
        transform: scale(0.9);
    }

    .label {
        font-size: var(--font-sm);
        color: var(--text-primary);
    }
</style>
