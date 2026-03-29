<script lang="ts">
    import {
        detectContactType,
        formatPhone,
        validatePhone,
        validateEmail,
        type ContactType
    } from '$lib/utils/contact-input';
    import { t } from '$lib/i18n';

    interface Props {
        value?: string;
        error?: string;
        label?: string;
        id?: string;
    }

    let { value = $bindable(''), error = $bindable(''), label, id }: Props = $props();

    let contactType = $state<ContactType>('unknown');
    let rawPhone = $state('');
    let focused = $state(false);

    function handleKeydown(e: KeyboardEvent) {
        if (e.key !== 'Backspace' || contactType !== 'phone') return;

        const input = e.target as HTMLInputElement;
        const pos = input.selectionStart ?? 0;

        if (pos === 0 || !rawPhone) return;

        const charBefore = input.value[pos - 1];
        if (charBefore && /\D/.test(charBefore)) {
            e.preventDefault();
            rawPhone = rawPhone.slice(0, -1);

            if (!rawPhone) {
                value = '';
                contactType = 'unknown';
                error = '';
                input.value = '';
                return;
            }

            const formatted = formatPhone(rawPhone);
            value = formatted;
            input.value = formatted;
            input.setSelectionRange(formatted.length, formatted.length);
            error = validatePhone(rawPhone) || '';
        }
    }

    function handleInput(e: Event) {
        const input = e.target as HTMLInputElement;
        let raw = input.value;

        if (!raw.trim()) {
            value = '';
            rawPhone = '';
            contactType = 'unknown';
            error = '';
            return;
        }

        contactType = detectContactType(raw);

        if (contactType === 'phone') {
            const digits = raw.replace(/\D/g, '');
            rawPhone = digits;

            const formatted = formatPhone(digits);
            value = formatted;

            requestAnimationFrame(() => {
                input.value = formatted;
                input.setSelectionRange(formatted.length, formatted.length);
            });

            error = validatePhone(digits) || '';
        } else {
            rawPhone = '';
            value = raw;
            if (contactType === 'email') {
                error = validateEmail(raw) || '';
            } else {
                error = '';
            }
        }
    }

    let typeLabel = $derived(
        contactType === 'phone'
            ? $t('ui.contactPhone')
            : contactType === 'email'
              ? $t('ui.contactEmail')
              : ''
    );
</script>

<div class="contact-input" class:has-error={!!error && !focused}>
    {#if label || $t('ui.contactLabel')}
        <label class="label" for={id}>
            {label || $t('ui.contactLabel')}
            {#if typeLabel}
                <span class="type-badge">{typeLabel}</span>
            {/if}
        </label>
    {/if}
    <div class="input-wrapper">
        <span class="input-icon">
            {#if contactType === 'phone'}
                <svg
                    viewBox="0 0 24 24"
                    width="18"
                    height="18"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.75"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path
                        d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"
                    />
                </svg>
            {:else}
                <svg
                    viewBox="0 0 24 24"
                    width="18"
                    height="18"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.75"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <rect width="20" height="16" x="2" y="4" rx="2" /><path
                        d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"
                    />
                </svg>
            {/if}
        </span>
        <input
            class="input"
            type="text"
            {id}
            placeholder={contactType === 'phone'
                ? $t('ui.contactPhonePlaceholder')
                : $t('ui.contactEmailPlaceholder')}
            {value}
            oninput={handleInput}
            onkeydown={handleKeydown}
            onfocus={() => (focused = true)}
            onblur={() => (focused = false)}
            autocomplete="off"
        />
    </div>
    {#if error && !focused}
        <p class="message error-message">{error}</p>
    {/if}
</div>

<style>
    .contact-input {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .label {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }

    .type-badge {
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        color: var(--accent);
        background: var(--accent-subtle);
        padding: 1px 0.5rem;
        border-radius: var(--radius-full);
    }

    .input-wrapper {
        position: relative;
        display: flex;
        align-items: center;
    }

    .input-icon {
        position: absolute;
        left: 0.875rem;
        display: flex;
        color: var(--text-tertiary);
        pointer-events: none;
        transition: color var(--duration-normal) var(--ease-in-out);
    }

    .input {
        width: 100%;
        height: 2.75rem;
        padding: 0 0.875rem 0 2.75rem;
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

    .has-error .input-icon {
        color: var(--color-error);
    }

    .message {
        font-size: var(--font-xs);
        animation: slide-down var(--duration-fast) var(--ease-out);
    }

    .error-message {
        color: var(--color-error);
    }
</style>
