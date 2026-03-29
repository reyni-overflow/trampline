<script lang="ts">
    import Input from '$lib/components/ui/Input.svelte';
    import PasswordInput from '$lib/components/ui/PasswordInput.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import SocialLogin from './SocialLogin.svelte';
    import { authModal } from '$lib/stores/auth-modal';
    import { user } from '$lib/stores/auth';
    import { authApi } from '$lib/api/auth';
    import { startConnection } from '$lib/api/signalr';
    import { toast } from '$lib/stores/toast';
    import { handleApiError } from '$lib/api/client';
    import { syncWithServer } from '$lib/stores/favorites';
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';

    let name = $state('');
    let email = $state('');
    let password = $state('');
    let confirmPassword = $state('');
    let errors = $state<Record<string, string>>({});
    let loading = $state(false);

    let role = $state<'Worker' | 'Employee'>('Worker');
    const unsubRole = authModal.subscribe((s) => (role = s.role));
    onDestroy(unsubRole);

    function validate(): boolean {
        errors = {};
        if (!name.trim()) errors.name = $t('auth.enterName');
        if (!email.trim()) errors.contact = $t('auth.enterEmail');
        else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
            errors.contact = $t('auth.invalidEmail');
        if (!password) errors.password = $t('auth.enterPassword');
        else if (password.length < 8) errors.password = $t('auth.minChars');
        else if (!/[A-ZА-ЯЁ]/.test(password) || !/[a-zа-яё]/.test(password) || !/\d/.test(password))
            errors.password = $t('auth.passwordRequirements');
        if (password !== confirmPassword) errors.confirmPassword = $t('auth.passwordsMismatch');
        return Object.keys(errors).length === 0;
    }

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        if (!validate()) return;

        loading = true;
        try {
            await authApi.register({ name, email: email.trim(), password, role });
            const me = await authApi.me();
            user.setUser(me);
            syncWithServer();
            startConnection();
            toast.success($t('auth.accountCreated'));
            authModal.close();
        } catch (err) {
            handleApiError(err);
        } finally {
            loading = false;
        }
    }
</script>

<div class="auth-form">
    <button
        class="back-btn"
        type="button"
        onclick={() => authModal.goBack()}
        aria-label={$t('common.back')}
    >
        <svg
            viewBox="0 0 24 24"
            width="20"
            height="20"
            fill="none"
            stroke="currentColor"
            stroke-width="1.75"
            stroke-linecap="round"
            stroke-linejoin="round"
        >
            <path d="m15 18-6-6 6-6" />
        </svg>
    </button>

    <div class="auth-header">
        <h2 class="auth-title">{$t('auth.registerTitle')}</h2>
        <p class="auth-subtitle">
            {role === 'Worker' ? $t('auth.workerSubtitle') : $t('auth.employeeSubtitle')}
        </p>
    </div>

    <form onsubmit={handleSubmit}>
        <div class="fields">
            <Input
                label={role === 'Worker' ? $t('auth.nameLabel') : $t('auth.companyNameLabel')}
                placeholder={role === 'Worker'
                    ? $t('auth.namePlaceholder')
                    : $t('auth.companyNamePlaceholder')}
                bind:value={name}
                error={errors.name}
                autocomplete="name"
            />
            <Input
                type="email"
                label="Email"
                placeholder="you@example.com"
                bind:value={email}
                error={errors.contact}
                autocomplete="email"
            />
            <PasswordInput
                bind:value={password}
                bind:confirmValue={confirmPassword}
                error={errors.password}
                showRules
                placeholder={$t('auth.passwordPlaceholder')}
                autocomplete="new-password"
            />
        </div>

        <Button type="submit" size="lg" {loading} disabled={loading}>
            {loading ? $t('auth.creating') : $t('auth.createAccount')}
        </Button>
    </form>

    <SocialLogin />

    <div class="auth-footer">
        <span class="auth-footer-text">{$t('auth.haveAccount')}</span>
        <button class="auth-link" type="button" onclick={() => authModal.goToLogin()}>
            {$t('auth.login')}
        </button>
    </div>
</div>

<style>
    .auth-form {
        display: flex;
        flex-direction: column;
        gap: var(--space-5);
        position: relative;
    }

    .back-btn {
        position: absolute;
        top: 0;
        left: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        color: var(--text-secondary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }

    .back-btn:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .auth-header {
        text-align: center;
        padding-top: var(--space-2);
    }

    .auth-title {
        font-size: var(--font-xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-2);
    }

    .auth-subtitle {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    form {
        display: flex;
        flex-direction: column;
        gap: var(--space-5);
    }

    .fields {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    form :global(.btn) {
        width: 100%;
    }

    .auth-footer {
        display: flex;
        justify-content: center;
        gap: var(--space-2);
        font-size: var(--font-sm);
    }

    .auth-footer-text {
        color: var(--text-secondary);
    }

    .auth-link {
        color: var(--accent);
        font-weight: var(--weight-medium);
        cursor: pointer;
    }

    .auth-link:hover {
        text-decoration: underline;
    }
</style>
