<script lang="ts">
    import type { ToastType } from '$lib/stores/toast';
    import { t } from '$lib/i18n';

    interface Props {
        id: string;
        type: ToastType;
        message: string;
        onclose: (id: string) => void;
    }

    let { id, type, message, onclose }: Props = $props();

    let el: HTMLElement;
    let startX = 0;
    let currentX = 0;
    let swiping = $state(false);
    let dismissed = $state(false);

    const icons: Record<ToastType, string> = {
        info: '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>',
        success: '<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>',
        warning: '<path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>',
        error: '<circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>',
        danger: '<polygon points="7.86 2 16.14 2 22 7.86 22 16.14 16.14 22 7.86 22 2 16.14 2 7.86 7.86 2"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>',
        tip: '<path d="M9 18h6"/><path d="M10 22h4"/><path d="M12 2a7 7 0 0 0-4 12.7V17h8v-2.3A7 7 0 0 0 12 2z"/>'
    };

    function dismiss() {
        if (dismissed) return;
        dismissed = true;
        setTimeout(() => onclose(id), 250);
    }

    function handlePointerDown(e: PointerEvent) {
        startX = e.clientX;
        swiping = true;
        el.setPointerCapture(e.pointerId);
    }

    function handlePointerMove(e: PointerEvent) {
        if (!swiping) return;
        currentX = Math.max(0, e.clientX - startX);
        el.style.transform = `translateX(${currentX}px)`;
        el.style.opacity = String(1 - currentX / 300);
    }

    function handlePointerUp() {
        if (!swiping) return;
        swiping = false;
        if (currentX > 100) {
            dismiss();
        } else {
            el.style.transform = '';
            el.style.opacity = '';
            currentX = 0;
        }
    }
</script>

<div
    bind:this={el}
    class="toast toast--{type}"
    class:dismissed
    role="alert"
    aria-live="polite"
    onpointerdown={handlePointerDown}
    onpointermove={handlePointerMove}
    onpointerup={handlePointerUp}
>
    <span class="toast-stripe"></span>
    <svg class="toast-icon" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <!-- eslint-disable-next-line svelte/no-at-html-tags -->
        {@html icons[type]}
    </svg>
    <p class="toast-message">{message}</p>
    <button class="toast-close" onpointerdown={(e) => e.stopPropagation()} onclick={(e) => { e.stopPropagation(); dismiss(); }} aria-label={$t('common.close')} type="button">
        <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round">
            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
        </svg>
    </button>
</div>

<style>
    .toast {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        position: relative;
        padding: var(--space-3) var(--space-4);
        padding-left: var(--space-5);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-lg);
        overflow: hidden;
        animation: slide-in-right var(--duration-moderate) var(--ease-spring);
        touch-action: pan-y;
        cursor: grab;
        min-width: 18.75rem;
        max-width: 26.25rem;
    }

    .toast.dismissed {
        animation: slide-out-right var(--duration-normal) var(--ease-out) forwards;
    }

    .toast-stripe {
        position: absolute;
        left: 0;
        top: 0;
        bottom: 0;
        width: 0.25rem;
    }

    .toast--info .toast-stripe { background: var(--color-info); }
    .toast--info .toast-icon { color: var(--color-info); }
    .toast--success .toast-stripe { background: var(--color-success); }
    .toast--success .toast-icon { color: var(--color-success); }
    .toast--warning .toast-stripe { background: var(--color-warning); }
    .toast--warning .toast-icon { color: var(--color-warning); }
    .toast--error .toast-stripe { background: var(--color-error); }
    .toast--error .toast-icon { color: var(--color-error); }
    .toast--danger .toast-stripe { background: var(--color-danger); }
    .toast--danger .toast-icon { color: var(--color-danger); }
    .toast--tip .toast-stripe { background: var(--color-tip); }
    .toast--tip .toast-icon { color: var(--color-tip); }

    .toast-icon {
        flex-shrink: 0;
    }

    .toast-message {
        flex: 1;
        font-size: var(--font-sm);
        color: var(--text-primary);
        line-height: var(--leading-snug);
        user-select: none;
    }

    .toast-close {
        flex-shrink: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 1.75rem;
        height: 1.75rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }

    .toast-close:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }
</style>
