<script lang="ts">
    import type { HTMLInputAttributes } from 'svelte/elements';
    import { t } from '$lib/i18n';

    interface Props extends HTMLInputAttributes {
        value?: string;
        label?: string;
        error?: string;
        showRules?: boolean;
        confirmValue?: string;
    }

    let {
        value = $bindable(''),
        label,
        error,
        showRules = false,
        confirmValue = $bindable(undefined),
        ...rest
    }: Props = $props();

    let showPassword = $state(false);
    let focused = $state(false);

    let rules = $derived.by(() => {
        if (!showRules) return [];
        return [
            { label: $t('ui.passwordMin8'), ok: value.length >= 8 },
            { label: $t('ui.passwordUppercase'), ok: /[A-ZА-ЯЁ]/.test(value) },
            { label: $t('ui.passwordLowercase'), ok: /[a-zа-яё]/.test(value) },
            { label: $t('ui.passwordDigit'), ok: /\d/.test(value) }
        ];
    });

    let strength = $derived.by(() => {
        if (!value) return 0;
        const passed = rules.filter((r) => r.ok).length;
        return passed;
    });

    let strengthLabel = $derived(
        strength === 0
            ? ''
            : strength <= 1
              ? $t('ui.passwordWeak')
              : strength <= 2
                ? $t('ui.passwordMedium')
                : strength <= 3
                  ? $t('ui.passwordGood')
                  : $t('ui.passwordStrong')
    );

    let strengthColor = $derived(
        strength <= 1
            ? 'var(--color-error)'
            : strength <= 2
              ? 'var(--color-warning)'
              : strength <= 3
                ? 'var(--color-info)'
                : 'var(--color-success)'
    );

    let confirmError = $derived.by(() => {
        if (confirmValue === undefined) return '';
        if (!confirmValue) return '';
        if (confirmValue !== value) return $t('ui.passwordsMismatch');
        return '';
    });

    let hasExternalError = $derived(!!error);
</script>

<div class="password-group" class:has-error={hasExternalError}>
    {#if label || $t('ui.passwordLabel')}
        <label class="label" for={rest.id}>{label || $t('ui.passwordLabel')}</label>
    {/if}
    <div class="input-wrapper">
        <input
            class="input"
            type={showPassword ? 'text' : 'password'}
            bind:value
            onfocus={() => (focused = true)}
            onblur={() => (focused = false)}
            {...rest}
        />
        <button
            class="eye-btn"
            onclick={() => (showPassword = !showPassword)}
            tabindex={-1}
            type="button"
            aria-label={showPassword ? $t('ui.hidePassword') : $t('ui.showPassword')}
        >
            {#if showPassword}
                <svg
                    viewBox="0 0 24 24"
                    width="18"
                    height="18"
                    stroke="currentColor"
                    stroke-width="1.75"
                    fill="none"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path
                        d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"
                    />
                    <line x1="1" y1="1" x2="23" y2="23" />
                </svg>
            {:else}
                <svg
                    viewBox="0 0 24 24"
                    width="18"
                    height="18"
                    stroke="currentColor"
                    stroke-width="1.75"
                    fill="none"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                    <circle cx="12" cy="12" r="3" />
                </svg>
            {/if}
        </button>
    </div>

    {#if showRules && value && (focused || hasExternalError)}
        <div class="rules-panel">
            <div class="strength-bar">
                {#each [0, 1, 2, 3] as i, _ki (i + _ki)}
                    <span
                        class="strength-segment"
                        style="background: {i < strength ? strengthColor : 'var(--bg-tertiary)'}"
                    ></span>
                {/each}
                {#if strengthLabel}
                    <span class="strength-label" style="color: {strengthColor}"
                        >{strengthLabel}</span
                    >
                {/if}
            </div>
            <ul class="rules-list">
                {#each rules as rule (rule.label)}
                    <li class="rule" class:passed={rule.ok}>
                        <svg
                            viewBox="0 0 24 24"
                            width="14"
                            height="14"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2.5"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                        >
                            {#if rule.ok}
                                <polyline points="20 6 9 17 4 12" />
                            {:else}
                                <circle cx="12" cy="12" r="10" /><line
                                    x1="15"
                                    y1="9"
                                    x2="9"
                                    y2="15"
                                /><line x1="9" y1="9" x2="15" y2="15" />
                            {/if}
                        </svg>
                        <span>{rule.label}</span>
                    </li>
                {/each}
            </ul>
        </div>
    {/if}

    {#if error}
        <p class="message error-message">{error}</p>
    {/if}
</div>

{#if confirmValue !== undefined}
    <div class="password-group" class:has-error={!!confirmError && confirmValue.length > 0}>
        <label class="label" for="confirm-password">{$t('ui.confirmPassword')}</label>
        <div class="input-wrapper">
            <input
                class="input"
                id="confirm-password"
                type={showPassword ? 'text' : 'password'}
                value={confirmValue}
                oninput={(e) => {
                    const target = e.target as HTMLInputElement;
                    confirmValue = target.value;
                }}
                placeholder={$t('ui.confirmPasswordPlaceholder')}
                autocomplete="new-password"
            />
            {#if confirmValue && !confirmError}
                <span class="match-icon">
                    <svg
                        viewBox="0 0 24 24"
                        width="18"
                        height="18"
                        fill="none"
                        stroke="var(--color-success)"
                        stroke-width="2"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                    >
                        <polyline points="20 6 9 17 4 12" />
                    </svg>
                </span>
            {/if}
        </div>
        {#if confirmError && confirmValue.length > 0}
            <p class="message error-message">{confirmError}</p>
        {/if}
    </div>
{/if}

<style>
    .password-group {
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
        padding: 0 2.75rem 0 0.875rem;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        color: var(--text-primary);
        font-size: var(--font-base);
        transition:
            var(--transition-colors),
            border-color var(--duration-normal) var(--ease-in-out),
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

    .match-icon {
        position: absolute;
        right: 0.75rem;
        display: flex;
    }

    .rules-panel {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        padding: var(--space-3);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        animation: slide-down var(--duration-fast) var(--ease-out);
    }

    .strength-bar {
        display: flex;
        align-items: center;
        gap: var(--space-1);
    }

    .strength-segment {
        flex: 1;
        height: 0.1875rem;
        border-radius: 2px;
        transition: background-color var(--duration-normal) var(--ease-in-out);
    }

    .strength-label {
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        margin-left: var(--space-2);
        white-space: nowrap;
    }

    .rules-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .rule {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        transition: color var(--duration-normal) var(--ease-in-out);
    }

    .rule.passed {
        color: var(--color-success);
    }

    .message {
        font-size: var(--font-xs);
        animation: slide-down var(--duration-fast) var(--ease-out);
    }
    .error-message {
        color: var(--color-error);
    }
</style>
