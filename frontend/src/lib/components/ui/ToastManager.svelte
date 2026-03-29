<script lang="ts">
    import { toast, type ToastItem } from '$lib/stores/toast';
    import Toast from './Toast.svelte';
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';

    let toasts = $state<ToastItem[]>([]);

    const unsub = toast.subscribe((v) => (toasts = v));
    onDestroy(unsub);
</script>

<div class="toast-container" aria-live="polite" aria-label={$t('ui.notifications')}>
    {#each toasts as t (t.id)}
        <Toast id={t.id} type={t.type} message={t.message} onclose={(id) => toast.remove(id)} />
    {/each}
</div>

<style>
    .toast-container {
        position: fixed;
        top: calc(var(--header-height) + var(--space-4));
        right: var(--space-4);
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        z-index: var(--z-toast);
        pointer-events: none;
    }

    .toast-container > :global(*) {
        pointer-events: auto;
    }

    @media (max-width: 640px) {
        .toast-container {
            left: var(--space-4);
            right: var(--space-4);
        }
    }
</style>
