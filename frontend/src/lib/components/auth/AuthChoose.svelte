<script lang="ts">
    import Button from '$lib/components/ui/Button.svelte';
    import { authModal, type AuthRole } from '$lib/stores/auth-modal';
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';

    let role = $state<AuthRole>('Worker');

    const unsub = authModal.subscribe((s) => (role = s.role));
    onDestroy(unsub);

    function selectRole(r: AuthRole) {
        role = r;
        authModal.setRole(r);
    }
</script>

<div class="choose">
    <div class="choose-header">
        <div class="logo-row">
            <svg viewBox="0 0 200 200" width="32" height="32" class="logo-icon">
                <path d="M 90 160 L 90 80" fill="none" stroke="currentColor" stroke-width="20" stroke-linecap="round" />
                <path d="M 30 100 L 135 65" fill="none" stroke="currentColor" stroke-width="20" stroke-linecap="round" />
                <circle cx="165" cy="55" r="14" fill="currentColor" />
            </svg>
        </div>
        <h2 class="choose-title">{$t('auth.welcome')}</h2>
        <p class="choose-subtitle">{$t('auth.chooseRole')}</p>
    </div>

    <div class="role-picker">
        <button
            class="role-option"
            class:active={role === 'Worker'}
            type="button"
            onclick={() => selectRole('Worker')}
        >
            <span class="role-icon">
                <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>
                </svg>
            </span>
            <div class="role-info">
                <span class="role-title">{$t('auth.workerRole')}</span>
                <span class="role-desc">{$t('auth.workerRoleDesc')}</span>
            </div>
        </button>
        <button
            class="role-option"
            class:active={role === 'Employee'}
            type="button"
            onclick={() => selectRole('Employee')}
        >
            <span class="role-icon">
                <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M6 22V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v18Z"/><path d="M6 12H4a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h2"/><path d="M18 9h2a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-2"/><path d="M10 6h4"/><path d="M10 10h4"/><path d="M10 14h4"/><path d="M10 18h4"/>
                </svg>
            </span>
            <div class="role-info">
                <span class="role-title">{$t('auth.employeeRole')}</span>
                <span class="role-desc">{$t('auth.employeeRoleDesc')}</span>
            </div>
        </button>
    </div>

    <div class="choose-actions">
        <Button size="lg" onclick={() => authModal.goToLogin()}>{$t('auth.login')}</Button>
        <Button size="lg" variant="outline" onclick={() => authModal.goToRegister()}>{$t('auth.register')}</Button>
    </div>
</div>

<style>
    .choose {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }

    .choose-header {
        text-align: center;
    }

    .logo-row {
        display: flex;
        justify-content: center;
        margin-bottom: var(--space-4);
        color: var(--accent);
    }

    .choose-title {
        font-size: var(--font-xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-2);
    }

    .choose-subtitle {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .role-picker {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .role-option {
        display: flex;
        align-items: center;
        gap: var(--space-4);
        padding: var(--space-4) var(--space-5);
        background: var(--bg-secondary);
        border: 2px solid var(--border-default);
        border-radius: var(--radius-lg);
        cursor: pointer;
        transition: var(--transition-colors), border-color var(--duration-normal) var(--ease-in-out);
        text-align: left;
    }

    .role-option:hover {
        border-color: var(--border-hover);
    }

    .role-option.active {
        border-color: var(--accent);
        background: var(--accent-subtle);
    }

    .role-icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.75rem;
        height: 2.75rem;
        background: var(--bg-tertiary);
        border-radius: var(--radius-md);
        color: var(--text-secondary);
        flex-shrink: 0;
        transition: var(--transition-colors);
    }

    .role-option.active .role-icon {
        background: var(--accent);
        color: var(--accent-contrast);
    }

    .role-info {
        display: flex;
        flex-direction: column;
        gap: 2px;
    }

    .role-title {
        font-size: var(--font-base);
        font-weight: var(--weight-medium);
        color: var(--text-primary);
    }

    .role-desc {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .choose-actions {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .choose-actions :global(.btn) {
        width: 100%;
    }
</style>
