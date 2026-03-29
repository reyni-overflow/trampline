<script lang="ts">
    import type { HTMLInputAttributes } from 'svelte/elements';
    import { t } from '$lib/i18n';

    interface Props extends HTMLInputAttributes {
        label?: string;
        error?: string;
        hint?: string;
    }

    let {
        label,
        error,
        hint,
        type = 'text',
        value = $bindable(''),
        ...rest
    }: Props = $props();

    let showPassword = $state(false);
    let isNumber = $derived(type === 'number');
    let inputType = $derived(type === 'password' && showPassword ? 'text' : isNumber ? 'text' : type);
    let displayValue = $state('');
    let focused = $state(false);

    function formatWithSpaces(num: string | number): string {
        const str = String(num).replace(/[^\d]/g, '');
        if (!str) return '';
        return str.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
    }

    function rawNumber(val: string): string {
        return String(val).replace(/[^\d]/g, '');
    }

    function handleNumberInput(e: Event) {
        const input = e.target as HTMLInputElement;
        const raw = rawNumber(input.value);
        value = raw;
        displayValue = formatWithSpaces(raw);
        const cursorPos = input.selectionStart ?? displayValue.length;
        requestAnimationFrame(() => {
            input.setSelectionRange(cursorPos, cursorPos);
        });
    }

    function handleNumberFocus() {
        focused = true;
        displayValue = rawNumber(value as string);
    }

    function handleNumberBlur() {
        focused = false;
        displayValue = formatWithSpaces(value as string);
    }

    $effect(() => {
        if (isNumber && !focused) {
            displayValue = formatWithSpaces(value as string);
        }
    });

    function increment() {
        if (isNumber) {
            const num = parseFloat(rawNumber(value as string)) || 0;
            const step = parseFloat((rest.step as string) || '1');
            const max = rest.max !== undefined ? parseFloat(rest.max as string) : Infinity;
            value = String(Math.min(num + step, max));
        }
    }

    function decrement() {
        if (isNumber) {
            const num = parseFloat(rawNumber(value as string)) || 0;
            const step = parseFloat((rest.step as string) || '1');
            const min = rest.min !== undefined ? parseFloat(rest.min as string) : -Infinity;
            value = String(Math.max(num - step, min));
        }
    }
</script>

<div class="input-group" class:has-error={!!error}>
    {#if label}
        <label class="label" for={rest.id}>{label}</label>
    {/if}
    <div class="input-wrapper">
        {#if isNumber}
            <button class="stepper stepper--minus" onclick={decrement} tabindex={-1} type="button" aria-label={$t('ui.decrease')}>
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2" fill="none"><line x1="5" y1="12" x2="19" y2="12"/></svg>
            </button>
        {/if}
        {#if isNumber}
            <input
                class="input has-stepper"
                type="text"
                inputmode="numeric"
                value={displayValue}
                oninput={handleNumberInput}
                onfocus={handleNumberFocus}
                onblur={handleNumberBlur}
                {...rest}
            />
        {:else}
            <input
                class="input"
                class:has-eye={type === 'password'}
                type={inputType}
                bind:value
                {...rest}
            />
        {/if}
        {#if type === 'password'}
            <button class="eye-btn" onclick={() => showPassword = !showPassword} tabindex={-1} type="button" aria-label={showPassword ? $t('ui.hidePassword') : $t('ui.showPassword')}>
                {#if showPassword}
                    <svg viewBox="0 0 24 24" width="18" height="18" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                        <line x1="1" y1="1" x2="23" y2="23"/>
                    </svg>
                {:else}
                    <svg viewBox="0 0 24 24" width="18" height="18" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                        <circle cx="12" cy="12" r="3"/>
                    </svg>
                {/if}
            </button>
        {/if}
        {#if isNumber}
            <button class="stepper stepper--plus" onclick={increment} tabindex={-1} type="button" aria-label={$t('ui.increase')}>
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2" fill="none"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            </button>
        {/if}
    </div>
    {#if error}
        <p class="message error-message">{error}</p>
    {:else if hint}
        <p class="message hint-message">{hint}</p>
    {/if}
</div>

<style>
    .input-group {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }

    .input-wrapper {
        position: relative;
        display: flex;
        align-items: center;
    }

    .input {
        width: 100%;
        height: 2.75rem;
        padding: 0 0.875rem;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        color: var(--text-primary);
        font-size: var(--font-base);
        transition: var(--transition-colors), border-color var(--duration-normal) var(--ease-in-out),
            box-shadow var(--duration-normal) var(--ease-in-out);
    }

    .input::placeholder {
        color: var(--text-tertiary);
    }

    .input:hover:not(:focus) {
        border-color: var(--border-hover);
    }

    .input:focus {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
        outline: none;
    }

    .has-error .input {
        border-color: var(--color-error);
    }

    .has-error .input:focus {
        box-shadow: 0 0 0 0.1875rem var(--color-error-subtle);
    }

    .input.has-stepper {
        padding-left: 2.5rem;
        padding-right: 2.5rem;
        text-align: center;
    }

    .stepper {
        position: absolute;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.25rem;
        height: 2.25rem;
        color: var(--text-secondary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
        z-index: 1;
    }

    .stepper:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .stepper--minus { left: 0.25rem; }
    .stepper--plus { right: 0.25rem; }

    .input.has-eye {
        padding-right: 2.75rem;
    }

    .eye-btn {
        position: absolute;
        right: 0.5rem;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }

    .eye-btn:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .message {
        font-size: var(--font-xs);
        animation: slide-down var(--duration-fast) var(--ease-out);
    }

    .error-message { color: var(--color-error); }
    .hint-message { color: var(--text-tertiary); }
</style>
