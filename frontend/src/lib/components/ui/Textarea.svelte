<script lang="ts">
    import type { HTMLTextareaAttributes } from 'svelte/elements';

    interface Props extends HTMLTextareaAttributes {
        label?: string;
        error?: string;
        hint?: string;
        autoresize?: boolean;
    }

    let {
        label,
        error,
        hint,
        autoresize = true,
        value = $bindable(''),
        oninput: externalOninput,
        ...rest
    }: Props = $props();

    function handleInput(e: Event) {
        const textarea = e.target as HTMLTextAreaElement;
        value = textarea.value;
        if (autoresize) {
            textarea.style.height = 'auto';
            textarea.style.height = textarea.scrollHeight + 'px';
        }
        if (externalOninput) (externalOninput as (e: Event) => void)(e);
    }
</script>

<div class="textarea-group" class:has-error={!!error}>
    {#if label}
        <label class="label" for={rest.id}>{label}</label>
    {/if}
    <textarea class="textarea" {value} oninput={handleInput} {...rest}></textarea>
    {#if error}
        <p class="message error-message">{error}</p>
    {:else if hint}
        <p class="message hint-message">{hint}</p>
    {/if}
</div>

<style>
    .textarea-group {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }

    .textarea {
        width: 100%;
        min-height: 6.25rem;
        max-height: 25rem;
        padding: 0.75rem 0.875rem;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        color: var(--text-primary);
        font-size: var(--font-base);
        line-height: var(--leading-normal);
        resize: vertical;
        transition:
            var(--transition-colors),
            border-color var(--duration-normal) var(--ease-in-out),
            box-shadow var(--duration-normal) var(--ease-in-out);
    }

    .textarea::placeholder {
        color: var(--text-tertiary);
    }
    .textarea:hover:not(:focus) {
        border-color: var(--border-hover);
    }
    .textarea:focus {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }

    .has-error .textarea {
        border-color: var(--color-error);
    }
    .has-error .textarea:focus {
        box-shadow: 0 0 0 0.1875rem var(--color-error-subtle);
    }

    .message {
        font-size: var(--font-xs);
        animation: slide-down var(--duration-fast) var(--ease-out);
    }
    .error-message {
        color: var(--color-error);
    }
    .hint-message {
        color: var(--text-tertiary);
    }
</style>
