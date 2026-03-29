<script lang="ts">
    import Modal from '$lib/components/ui/Modal.svelte';
    import AuthChoose from './AuthChoose.svelte';
    import LoginModal from './LoginModal.svelte';
    import RegisterModal from './RegisterModal.svelte';
    import { authModal, type AuthModalView } from '$lib/stores/auth-modal';
    import { onDestroy } from 'svelte';

    let state = $state({ open: false, view: 'choose' as AuthModalView });

    const unsub = authModal.subscribe((s) => (state = { open: s.open, view: s.view }));
    onDestroy(unsub);
</script>

<Modal
    open={state.open}
    onclose={() => authModal.close()}
    maxWidth="26rem"
>
    {#if state.view === 'choose'}
        <AuthChoose />
    {:else if state.view === 'login'}
        <LoginModal />
    {:else}
        <RegisterModal />
    {/if}
</Modal>
