<script lang="ts">
    import type { Snippet } from 'svelte';
    import { t } from '$lib/i18n';

    interface Props {
        open: boolean;
        onclose?: () => void;
        maxWidth?: string;
        title?: string;
        children: Snippet;
    }

    const FOCUSABLE = 'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])';

    let { open = $bindable(false), onclose, maxWidth = '640px', title, children }: Props = $props();

    let modalEl = $state<HTMLDivElement>();
    let previousFocus: HTMLElement | null = null;

    $effect(() => {
        if (open) {
            previousFocus = document.activeElement as HTMLElement;
            requestAnimationFrame(() => {
                const first = modalEl?.querySelector<HTMLElement>(FOCUSABLE);
                first?.focus();
            });
        } else if (previousFocus) {
            previousFocus.focus();
            previousFocus = null;
        }
    });

    function close() {
        open = false;
        onclose?.();
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === 'Escape' && open) close();
    }

    function handleOverlayKeydown(e: KeyboardEvent) {
        if (e.key !== 'Tab' || !modalEl) return;

        const focusable = [...modalEl.querySelectorAll<HTMLElement>(FOCUSABLE)];
        if (!focusable.length) return;

        const first = focusable[0];
        const last = focusable[focusable.length - 1];

        if (e.shiftKey && document.activeElement === first) {
            e.preventDefault();
            last.focus();
        } else if (!e.shiftKey && document.activeElement === last) {
            e.preventDefault();
            first.focus();
        }
    }

    function handleOverlayClick(e: MouseEvent) {
        if ((e.target as HTMLElement).classList.contains('modal-overlay')) close();
    }
</script>

<svelte:window onkeydown={handleKeydown} />

{#if open}
    <!-- svelte-ignore a11y_no_static_element_interactions -->
    <div class="modal-overlay" onclick={handleOverlayClick} onkeydown={handleOverlayKeydown}>
        <div
            bind:this={modalEl}
            class="modal"
            style="max-width: {maxWidth}"
            role="dialog"
            aria-modal="true"
            aria-label={title}
        >
            {#if title}
                <div class="modal-header">
                    <h2 class="modal-title">{title}</h2>
                    <button class="modal-close" onclick={close} aria-label={$t('common.close')} type="button">
                        <svg viewBox="0 0 24 24" width="20" height="20" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round">
                            <line x1="18" y1="6" x2="6" y2="18"/>
                            <line x1="6" y1="6" x2="18" y2="18"/>
                        </svg>
                    </button>
                </div>
            {:else}
                <button class="modal-close floating" onclick={close} aria-label={$t('common.close')} type="button">
                    <svg viewBox="0 0 24 24" width="20" height="20" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round">
                        <line x1="18" y1="6" x2="6" y2="18"/>
                        <line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                </button>
            {/if}
            <div class="modal-body">
                {@render children()}
            </div>
        </div>
    </div>
{/if}

<style>
    .modal-overlay {
        position: fixed;
        inset: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: var(--space-4);
        background: var(--bg-overlay);
        backdrop-filter: blur(0.25rem);
        z-index: var(--z-modal);
        animation: fade-in var(--duration-fast) var(--ease-out);
    }

    .modal {
        position: relative;
        width: 100%;
        max-height: 90vh;
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xl);
        box-shadow: var(--shadow-xl);
        overflow: hidden;
        display: flex;
        flex-direction: column;
        animation: scale-in var(--duration-moderate) var(--ease-spring);
    }

    .modal-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-5) var(--space-6);
        border-bottom: 1px solid var(--border-default);
    }

    .modal-title {
        font-size: var(--font-lg);
        font-weight: var(--weight-semibold);
    }

    .modal-close {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
        flex-shrink: 0;
    }

    .modal-close:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .modal-close.floating {
        position: absolute;
        top: var(--space-3);
        right: var(--space-3);
        z-index: 1;
    }

    .modal-body {
        padding: var(--space-6);
        overflow-y: auto;
    }

    @media (max-width: 640px) {
        .modal {
            max-height: 95vh;
            border-radius: var(--radius-lg);
        }
    }
</style>
