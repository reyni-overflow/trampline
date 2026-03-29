<script lang="ts">
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import PasswordInput from '$lib/components/ui/PasswordInput.svelte';
    import Modal from '$lib/components/ui/Modal.svelte';
    import { toast } from '$lib/stores/toast';
    import { onMount, onDestroy } from 'svelte';
    import { adminApi } from '$lib/api/admin';
    import { handleApiError } from '$lib/api/client';
    import { user as userStore } from '$lib/stores/auth';
    import { t, getLocaleDateString } from '$lib/i18n';

    let isSuperAdmin = $state(false);
    const unsub = userStore.subscribe((v) => {
        isSuperAdmin = v?.isSuperAdmin ?? false;
    });
    onDestroy(unsub);

    let showCreate = $state(false);
    let newName = $state('');
    let newEmail = $state('');
    let newPassword = $state('');
    let creating = $state(false);

    let emailError = $derived.by(() => {
        if (!newEmail) return '';
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(newEmail)) return $t('auth.invalidEmail');
        return '';
    });

    let apiCurators = $state<
        { id: string; name: string; email: string; isSuper: boolean; lastActive: string | null }[]
    >([]);

    function isPasswordValid(pw: string) {
        return pw.length >= 8 && /[A-ZА-ЯЁ]/.test(pw) && /[a-zа-яё]/.test(pw) && /\d/.test(pw);
    }

    async function createCurator() {
        if (!newName || !newEmail || !newPassword) {
            toast.error($t('adminCurators.fillAll'));
            return;
        }
        if (emailError) {
            toast.error(emailError);
            return;
        }
        if (!isPasswordValid(newPassword)) {
            toast.error($t('adminCurators.passwordRequirements'));
            return;
        }
        creating = true;
        try {
            await adminApi.createCurator({ name: newName, email: newEmail, password: newPassword });
            toast.success($t('adminCurators.createdMsg', { name: newName }));
            showCreate = false;
            newName = '';
            newEmail = '';
            newPassword = '';
            await loadCurators();
        } catch (err) {
            handleApiError(err);
        } finally {
            creating = false;
        }
    }

    async function removeCurator(id: string, name: string) {
        try {
            await adminApi.deleteCurator(id);
            apiCurators = apiCurators.filter((c) => c.id !== id);
            toast.warning($t('adminCurators.deletedMsg', { name }));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function loadCurators() {
        try {
            const data = await adminApi.getCurators();
            apiCurators = data.map((c) => ({
                id: c.id,
                name: c.nickname,
                email: c.email,
                isSuper: c.isSuperAdmin === true,
                lastActive: c.createdAt ?? null
            }));
        } catch (err) {
            handleApiError(err);
        }
    }

    onMount(loadCurators);
</script>

<svelte:head><title>{$t('adminCurators.pageTitle')}</title></svelte:head>

<div class="curators">
    <div class="page-header">
        <h1 class="page-heading">{$t('adminCurators.title')}</h1>
        {#if isSuperAdmin}<Button onclick={() => (showCreate = true)}
                >{$t('adminCurators.addCurator')}</Button
            >{/if}
    </div>

    <div class="curators-list">
        {#each apiCurators as curator (curator.id)}
            <div class="curator-card">
                <div class="curator-main">
                    <Avatar name={curator.name} size={40} />
                    <div class="curator-info">
                        <div class="curator-name-row">
                            <span class="curator-name">{curator.name}</span>
                            {#if curator.isSuper}<Badge variant="error" size="sm"
                                    >{$t('adminCurators.administrator')}</Badge
                                >{:else}<Badge size="sm">{$t('adminCurators.curator')}</Badge>{/if}
                        </div>
                        <span class="curator-email">{curator.email}</span>
                        <span class="curator-date"
                            >{curator.lastActive
                                ? $t('adminCurators.lastActive', {
                                      date: new Date(curator.lastActive).toLocaleDateString(
                                          getLocaleDateString(),
                                          {
                                              day: 'numeric',
                                              month: 'short'
                                          }
                                      )
                                  })
                                : ''}</span
                        >
                    </div>
                </div>
                {#if !curator.isSuper && isSuperAdmin}
                    <Button
                        size="sm"
                        variant="danger"
                        onclick={() => removeCurator(curator.id, curator.name)}
                        >{$t('adminCurators.delete')}</Button
                    >
                {/if}
            </div>
        {/each}
    </div>
</div>

<Modal bind:open={showCreate} title={$t('adminCurators.newCurator')} maxWidth="24rem">
    <div class="create-form">
        <Input
            label={$t('adminCurators.name')}
            bind:value={newName}
            placeholder={$t('adminCurators.namePlaceholder')}
        />
        <Input
            label={$t('adminCurators.email')}
            type="email"
            bind:value={newEmail}
            placeholder={$t('adminCurators.emailPlaceholder')}
            error={newEmail ? emailError : ''}
        />
        <PasswordInput
            bind:value={newPassword}
            label={$t('adminCurators.password')}
            showRules
            placeholder={$t('adminCurators.passwordHint')}
            autocomplete="new-password"
        />
        <Button onclick={createCurator} loading={creating} disabled={creating}
            >{creating ? $t('common.loading') : $t('adminCurators.create')}</Button
        >
    </div>
</Modal>

<style>
    .curators {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }
    .page-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        flex-wrap: wrap;
        gap: var(--space-4);
    }
    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .curators-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }
    .curator-card {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }
    .curator-main {
        display: flex;
        align-items: center;
        gap: var(--space-4);
    }
    .curator-info {
        display: flex;
        flex-direction: column;
        gap: 2px;
    }
    .curator-name-row {
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }
    .curator-name {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }
    .curator-email {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }
    .curator-date {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .create-form {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }
    .create-form :global(.btn) {
        width: 100%;
    }
</style>
