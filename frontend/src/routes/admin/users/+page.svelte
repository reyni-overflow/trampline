<script lang="ts">
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import SearchInput from '$lib/components/ui/SearchInput.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Pagination from '$lib/components/ui/Pagination.svelte';
    import { toast } from '$lib/stores/toast';
    import { onMount } from 'svelte';
    import { adminApi } from '$lib/api/admin';
    import { handleApiError } from '$lib/api/client';
    import { t, getLocaleDateString } from '$lib/i18n';

    import Modal from '$lib/components/ui/Modal.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import Textarea from '$lib/components/ui/Textarea.svelte';
    import Checkbox from '$lib/components/ui/Checkbox.svelte';

    let search = $state('');
    let roleFilter = $state('');
    let showBlocked = $state(false);
    let page = $state(1);
    let _loading = $state(false);
    let roleChangeTarget = $state<string | null>(null);

    let editTarget = $state<{ id: string; role: string; name: string } | null>(null);
    let editForm = $state<Record<string, string>>({});
    let editSaving = $state(false);

    let roleOptions = $derived([
        { value: '', label: $t('adminUsers.allRoles') },
        { value: 'Worker', label: $t('adminUsers.worker') },
        { value: 'Employee', label: $t('adminUsers.employee') },
        { value: 'Admin', label: $t('adminUsers.curator') }
    ]);

    let changeRoleOptions = $derived([
        { value: 'Worker', label: $t('adminUsers.worker') },
        { value: 'Employee', label: $t('adminUsers.employee') }
    ]);

    let apiUsers = $state<{ id: string; name: string; email: string; role: string; date: string; blocked: boolean }[]>([]);
    let users = $derived(apiUsers);

    let filtered = $derived.by(() => {
        let list = users;
        if (roleFilter) list = list.filter((u) => u.role === roleFilter);
        if (!showBlocked) list = list.filter((u) => !u.blocked);
        if (search) { const q = search.toLowerCase(); list = list.filter((u) => u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)); }
        return list;
    });

    const roleVariant = (r: string) => r === 'Admin' ? 'error' as const : r === 'Employee' ? 'info' as const : 'default' as const;
    const roleLabel = (r: string) => r === 'Admin' ? $t('adminUsers.curator') : r === 'Employee' ? $t('adminUsers.employee') : $t('adminUsers.worker');

    async function loadUsers() {
        _loading = true;
        try {
            const data = await adminApi.getUsers(page, 20);
            apiUsers = data.map((u) => ({
                id: u.id,
                name: u.nickname,
                email: u.email,
                role: u.role,
                date: u.createdAt,
                blocked: u.isBlocked
            }));
        } catch (err) {
            handleApiError(err);
        } finally {
            _loading = false;
        }
    }

    async function blockUser(id: string, name: string) {
        try {
            await adminApi.blockUser(id);
            toast.warning($t('adminUsers.blockedMsg', { name }));
            apiUsers = apiUsers.map((u) => u.id === id ? { ...u, blocked: true } : u);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function unblockUser(id: string, name: string) {
        try {
            await adminApi.unblockUser(id);
            toast.success($t('adminUsers.unblockedMsg', { name }));
            apiUsers = apiUsers.map((u) => u.id === id ? { ...u, blocked: false } : u);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function changeRole(id: string, name: string, newRole: string) {
        try {
            await adminApi.changeRole(id, newRole);
            toast.success($t('adminUsers.roleChangedMsg', { name, role: roleLabel(newRole) }));
            apiUsers = apiUsers.map((u) => u.id === id ? { ...u, role: newRole } : u);
        } catch (err) {
            handleApiError(err);
        } finally {
            roleChangeTarget = null;
        }
    }

    function openEditProfile(u: { id: string; role: string; name: string }) {
        editTarget = u;
        editForm = u.role === 'Worker'
            ? { name: '', lastName: '', patronymic: '', about: '' }
            : { name: '', description: '', activity: '' };
    }

    async function saveProfile() {
        if (!editTarget) return;
        editSaving = true;
        try {
            if (editTarget.role === 'Worker') {
                await adminApi.updateWorkerProfile(editTarget.id, {
                    name: editForm.name,
                    lastName: editForm.lastName,
                    patronymic: editForm.patronymic,
                    about: editForm.about
                });
            } else if (editTarget.role === 'Employee') {
                await adminApi.updateEmployeeProfile(editTarget.id, {
                    name: editForm.name,
                    description: editForm.description,
                    activity: editForm.activity
                });
            }
            toast.success($t('adminUsers.profileUpdated', { name: editTarget!.name }));
            editTarget = null;
        } catch (err) {
            handleApiError(err);
        } finally {
            editSaving = false;
        }
    }

    onMount(loadUsers);
</script>

<svelte:head><title>{$t('adminUsers.pageTitle')}</title></svelte:head>

<div class="users-page">
    <h1 class="page-heading">{$t('adminUsers.title')}</h1>
    <div class="controls">
        <SearchInput placeholder={$t('adminUsers.searchPlaceholder')} bind:value={search} />
        <Select options={roleOptions} bind:value={roleFilter} placeholder={$t('adminUsers.role')} />
        <label class="blocked-toggle">
            <Checkbox bind:checked={showBlocked} />
            <span>{$t('adminUsers.showBlocked')}</span>
        </label>
    </div>

    <div class="users-table">
        <div class="table-header">
            <span class="col-user">{$t('adminUsers.user')}</span>
            <span class="col-role">{$t('adminUsers.role')}</span>
            <span class="col-date">{$t('adminUsers.registration')}</span>
            <span class="col-actions">{$t('adminUsers.actions')}</span>
        </div>
        {#each filtered as u (u.id)}
            <div class="table-row" class:blocked={u.blocked}>
                <div class="col-user">
                    <Avatar name={u.name} size={32} />
                    <div class="user-info">
                        <span class="user-name">{u.name}</span>
                        <span class="user-email">{u.email}</span>
                    </div>
                </div>
                <span class="col-role"><Badge variant={roleVariant(u.role)} size="sm">{roleLabel(u.role)}</Badge></span>
                <span class="col-date">{new Date(u.date).toLocaleDateString(getLocaleDateString(), { day: 'numeric', month: 'short', year: 'numeric' })}</span>
                <div class="col-actions">
                    {#if u.role !== 'Admin'}
                        <Button size="sm" variant="ghost" onclick={() => openEditProfile(u)} title={$t('adminUsers.editProfile')}>
                            <svg viewBox="0 0 24 24" width="14" height="14" stroke="currentColor" stroke-width="2" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                        </Button>
                        {#if roleChangeTarget === u.id}
                            <div class="role-change-dropdown">
                                {#each changeRoleOptions as opt (opt.value)}
                                    {#if opt.value !== u.role}
                                        <button class="role-option" type="button" onclick={() => changeRole(u.id, u.name, opt.value)}>
                                            {opt.label}
                                        </button>
                                    {/if}
                                {/each}
                                <button class="role-option cancel" type="button" onclick={() => (roleChangeTarget = null)}>✕</button>
                            </div>
                        {:else}
                            <Button size="sm" variant="ghost" onclick={() => (roleChangeTarget = u.id)} title={$t('adminUsers.changeRole')}>
                                <svg viewBox="0 0 24 24" width="14" height="14" stroke="currentColor" stroke-width="2" fill="none"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                            </Button>
                        {/if}
                        {#if u.blocked}
                            <Button size="sm" variant="ghost" onclick={() => unblockUser(u.id, u.name)}>{$t('common.unblock')}</Button>
                        {:else}
                            <Button size="sm" variant="ghost" onclick={() => blockUser(u.id, u.name)}>{$t('common.block')}</Button>
                        {/if}
                    {/if}
                </div>
            </div>
        {/each}
    </div>
    <div class="pagination-row"><Pagination bind:page totalPages={1} /></div>
</div>

{#if editTarget}
    <Modal open={true} onclose={() => (editTarget = null)} title={$t('adminUsers.editProfile')}>
        <div class="edit-profile-form">
            {#if editTarget.role === 'Worker'}
                <Input label={$t('adminUsers.firstName')} bind:value={editForm.name} />
                <Input label={$t('adminUsers.lastName')} bind:value={editForm.lastName} />
                <Input label={$t('adminUsers.patronymic')} bind:value={editForm.patronymic} />
                <Textarea label={$t('adminUsers.about')} bind:value={editForm.about} />
            {:else}
                <Input label={$t('adminUsers.companyName')} bind:value={editForm.name} />
                <Textarea label={$t('adminUsers.description')} bind:value={editForm.description} />
                <Input label={$t('adminUsers.activity')} bind:value={editForm.activity} />
            {/if}
            <div class="edit-profile-actions">
                <Button onclick={saveProfile} disabled={editSaving}>{editSaving ? $t('common.loading') : $t('common.save')}</Button>
                <Button variant="outline" onclick={() => (editTarget = null)}>{$t('common.cancel')}</Button>
            </div>
        </div>
    </Modal>
{/if}

<style>
    .users-page { display: flex; flex-direction: column; gap: var(--space-6); }
    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }
    .controls { display: flex; gap: var(--space-3); flex-wrap: wrap; align-items: center; }
    .controls :global(.select-group) { min-width: 10rem; }
    .controls :global(.select-trigger) { height: 2.5rem; font-size: var(--font-sm); padding: 0 0.75rem; }
    .controls :global(.option) { font-size: var(--font-sm); }
    .blocked-toggle { display: flex; align-items: center; gap: var(--space-2); font-size: var(--font-sm); color: var(--text-secondary); cursor: pointer; white-space: nowrap; height: 2.25rem; }

    .users-table { border: 1px solid var(--border-default); border-radius: var(--radius-lg); overflow: hidden; }
    .table-header, .table-row { display: grid; grid-template-columns: 2fr 7rem 8rem 13rem; align-items: center; padding: var(--space-3) var(--space-4); gap: var(--space-3); }
    .table-header { font-size: var(--font-xs); font-weight: var(--weight-semibold); color: var(--text-tertiary); text-transform: uppercase; letter-spacing: 0.05em; background: var(--bg-secondary); border-bottom: 1px solid var(--border-default); }
    .table-row { font-size: var(--font-sm); border-bottom: 1px solid var(--border-default); transition: var(--transition-colors); }
    .table-row:last-child { border-bottom: none; }
    .table-row:hover { background: var(--bg-secondary); }
    .table-row.blocked { opacity: 0.5; }

    .col-user { display: flex; align-items: center; gap: var(--space-3); }
    .user-info { display: flex; flex-direction: column; gap: 1px; min-width: 0; }
    .user-name { font-weight: var(--weight-medium); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .user-email { font-size: var(--font-xs); color: var(--text-tertiary); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .col-date { font-size: var(--font-xs); color: var(--text-tertiary); }
    .col-actions { display: flex; justify-content: flex-end; align-items: center; gap: var(--space-1); }

    .role-change-dropdown { display: flex; align-items: center; gap: 2px; background: var(--bg-tertiary); border-radius: var(--radius-md); padding: 2px; }
    .role-option { padding: var(--space-1) var(--space-2); font-size: var(--font-xs); font-weight: var(--weight-medium); border-radius: var(--radius-sm); color: var(--text-primary); cursor: pointer; transition: var(--transition-colors); white-space: nowrap; }
    .role-option:hover { background: var(--accent); color: var(--accent-contrast); }
    .role-option.cancel { color: var(--text-tertiary); }
    .role-option.cancel:hover { background: var(--color-error-subtle); color: var(--color-error); }

    .pagination-row { display: flex; justify-content: center; }

    .edit-profile-form { display: flex; flex-direction: column; gap: var(--space-4); padding: var(--space-2) 0; }
    .edit-profile-actions { display: flex; gap: var(--space-3); justify-content: flex-end; padding-top: var(--space-3); border-top: 1px solid var(--border-default); }

    @media (max-width: 768px) { .table-header { display: none; } .table-row { grid-template-columns: 1fr auto; } .col-date { display: none; } }
</style>
