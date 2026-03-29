<script lang="ts">
    import ContactInput from '$lib/components/ui/ContactInput.svelte';
    import PasswordInput from '$lib/components/ui/PasswordInput.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import SocialLogin from './SocialLogin.svelte';
    import { goto } from '$app/navigation';
    import { authModal } from '$lib/stores/auth-modal';
    import { forgotPasswordModal } from '$lib/stores/forgot-password-modal';
    import { user } from '$lib/stores/auth';
    import { authApi } from '$lib/api/auth';
    import { startConnection } from '$lib/api/signalr';
    import { toast } from '$lib/stores/toast';
    import { handleApiError } from '$lib/api/client';
    import { syncWithServer } from '$lib/stores/favorites';
    import { t } from '$lib/i18n';

    let contact = $state('');
    let contactError = $state('');
    let password = $state('');
    let passwordError = $state('');
    let loading = $state(false);
    let totpRequired = $state(false);
    let totpChallengeId = $state('');
    let totpCode = $state('');
    let totpError = $state('');

    function validate(): boolean {
        let valid = true;
        if (!contact.trim()) {
            contactError = $t('auth.enterContact');
            valid = false;
        }
        if (!password) {
            passwordError = $t('auth.enterPassword');
            valid = false;
        }
        return valid;
    }

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        if (!validate()) return;

        loading = true;
        try {
            const res = await authApi.login({ contact: contact.trim(), password });
            if (res.requiresTotp) {
                totpRequired = true;
                totpChallengeId = res.challengeId!;
                return;
            }
            const me = await authApi.me();
            user.setUser(me);
            syncWithServer();
            startConnection();
            authModal.close();
            if (me.mustChangePassword) {
                toast.warning($t('auth.mustChangePassword'));
                goto('/settings');
            } else {
                toast.success($t('auth.welcomeBack', { name: me.nickname }));
                goto(me.role === 'Admin' ? '/admin' : '/dashboard');
            }
        } catch (err) {
            handleApiError(err);
        } finally {
            loading = false;
        }
    }

    async function handleTotpSubmit(e: SubmitEvent) {
        e.preventDefault();
        if (totpCode.length !== 6) {
            totpError = $t('auth.totpInvalid');
            return;
        }
        loading = true;
        try {
            await authApi.totpVerify(totpChallengeId, totpCode);
            const me = await authApi.me();
            user.setUser(me);
            syncWithServer();
            startConnection();
            authModal.close();
            if (me.mustChangePassword) {
                toast.warning($t('auth.mustChangePassword'));
                goto('/settings');
            } else {
                toast.success($t('auth.welcomeBack', { name: me.nickname }));
                goto(me.role === 'Admin' ? '/admin' : '/dashboard');
            }
        } catch {
            totpError = $t('auth.totpInvalid');
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
        <h2 class="auth-title">{$t('auth.loginTitle')}</h2>
        <p class="auth-subtitle">{$t('auth.loginSubtitle')}</p>
    </div>

    {#if !totpRequired}
        <form onsubmit={handleSubmit}>
            <div class="fields">
                <ContactInput bind:value={contact} bind:error={contactError} />
                <PasswordInput
                    bind:value={password}
                    error={passwordError}
                    placeholder={$t('auth.yourPassword')}
                    autocomplete="current-password"
                />
            </div>

            <div class="password-extras">
                <button
                    class="forgot-link"
                    type="button"
                    onclick={() => {
                        authModal.close();
                        forgotPasswordModal.open();
                    }}
                >
                    {$t('forgotPassword.link')}
                </button>
            </div>

            <Button type="submit" size="lg" {loading} disabled={loading}>
                {loading ? $t('auth.loggingIn') : $t('auth.login')}
            </Button>
        </form>

        <SocialLogin />

        <div class="auth-footer">
            <span class="auth-footer-text">{$t('auth.noAccount')}</span>
            <button class="auth-link" type="button" onclick={() => authModal.goToRegister()}>
                {$t('auth.register')}
            </button>
        </div>
    {:else}
        <form onsubmit={handleTotpSubmit}>
            <div class="fields">
                <p class="totp-hint">{$t('auth.totpHint')}</p>
                <Input
                    label={$t('auth.totpCode')}
                    bind:value={totpCode}
                    error={totpError}
                    placeholder="000000"
                    maxlength={6}
                    autocomplete="one-time-code"
                    oninput={() => {
                        totpError = '';
                    }}
                />
            </div>
            <Button type="submit" size="lg" {loading} disabled={loading || totpCode.length !== 6}>
                {loading ? $t('auth.verifying') : $t('auth.verify')}
            </Button>
        </form>
    {/if}
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

    .password-extras {
        display: flex;
        justify-content: flex-end;
        margin-top: calc(-1 * var(--space-3));
    }

    .forgot-link {
        font-size: var(--font-sm);
        color: var(--accent);
        cursor: pointer;
        font-weight: var(--weight-medium);
    }

    .forgot-link:hover {
        text-decoration: underline;
    }

    .totp-hint {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        text-align: center;
    }
</style>
