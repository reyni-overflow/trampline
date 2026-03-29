<script lang="ts">
    import Modal from '$lib/components/ui/Modal.svelte';
    import Input from '$lib/components/ui/Input.svelte';
    import PasswordInput from '$lib/components/ui/PasswordInput.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import { forgotPasswordModal } from '$lib/stores/forgot-password-modal';
    import { authModal } from '$lib/stores/auth-modal';
    import { validate, required, email as emailRule, minLength } from '$lib/utils/validation';
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';
    import { authApi } from '$lib/api/auth';
    import { toast } from '$lib/stores/toast';

    let isOpen = $state(false);
    const unsubModal = forgotPasswordModal.subscribe((s) => (isOpen = s.open));

    onDestroy(() => {
        unsubModal();

        if (cooldownTimer) {
            clearInterval(cooldownTimer);
            cooldownTimer = null;
        }
    });

    let step = $state<'email' | 'sent' | 'reset'>('email');
    let emailValue = $state('');
    let emailError = $state('');
    let loading = $state(false);
    let cooldown = $state(0);
    let cooldownTimer: ReturnType<typeof setInterval> | null = null;

    let codeValue = $state('');
    let codeError = $state('');
    let newPassword = $state('');
    let newPasswordError = $state('');
    let confirmPassword = $state('');

    function resetState() {
        step = 'email';
        emailValue = '';
        emailError = '';
        loading = false;
        cooldown = 0;
        codeValue = '';
        codeError = '';
        newPassword = '';
        newPasswordError = '';
        confirmPassword = '';

        if (cooldownTimer) {
            clearInterval(cooldownTimer);
            cooldownTimer = null;
        }
    }

    function handleClose() {
        forgotPasswordModal.close();
        resetState();
    }

    function backToLogin() {
        handleClose();
        authModal.openLogin();
    }

    function startCooldown() {
        cooldown = 60;
        if (cooldownTimer) clearInterval(cooldownTimer);

        cooldownTimer = setInterval(() => {
            cooldown -= 1;

            if (cooldown <= 0) {
                cooldown = 0;

                if (cooldownTimer) {
                    clearInterval(cooldownTimer);
                    cooldownTimer = null;
                }
            }
        }, 1000);
    }

    function validateEmail(): boolean {
        const error = validate(emailValue, [required, emailRule]);
        emailError = error || '';

        return !error;
    }

    function validateResetForm(): boolean {
        let valid = true;

        const codeErr =
            validate(codeValue, [required]) ||
            (!/^\d{6}$/.test(codeValue) ? $t('forgotPassword.codeFormat') : null);
        codeError = codeErr || '';
        if (codeErr) valid = false;

        const passErr = validate(newPassword, [required, minLength(8)]);
        newPasswordError = passErr || '';
        if (passErr) valid = false;

        if (newPassword !== confirmPassword) {
            newPasswordError = $t('auth.passwordsMismatch');
            valid = false;
        }

        return valid;
    }

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        if (!validateEmail()) return;

        loading = true;

        try {
            const res = await authApi.forgotPassword(emailValue);
            if (res.debugCode) {
                toast.info(`[DEV] ${$t('forgotPassword.code')}: ${res.debugCode}`, {
                    duration: 30000
                });
            }
            step = 'sent';
            startCooldown();
        } catch {
            step = 'sent';
            startCooldown();
        } finally {
            loading = false;
        }
    }

    async function handleResend() {
        if (cooldown > 0) return;
        loading = true;

        try {
            const res = await authApi.forgotPassword(emailValue);
            if (res.debugCode) {
                toast.info(`[DEV] ${$t('forgotPassword.code')}: ${res.debugCode}`, {
                    duration: 30000
                });
            }
        } catch {
            /* resend is best-effort */
        }

        loading = false;
        startCooldown();
    }

    function goToReset() {
        step = 'reset';
    }

    async function handleReset(e: SubmitEvent) {
        e.preventDefault();
        if (!validateResetForm()) return;

        loading = true;

        try {
            await authApi.resetPassword(emailValue, codeValue, newPassword);
            toast.success($t('forgotPassword.resetSuccess'));

            handleClose();
            authModal.openLogin();
        } catch (err: unknown) {
            const e = err as Record<string, unknown>;
            const message = (e?.detail as string) || (e?.title as string) || 'Error';
            toast.error(message);
        } finally {
            loading = false;
        }
    }
</script>

<Modal open={isOpen} onclose={handleClose} maxWidth="26rem">
    {#if step === 'email'}
        <div class="forgot-form">
            <div class="forgot-header">
                <h2 class="forgot-title">{$t('forgotPassword.title')}</h2>
                <p class="forgot-subtitle">{$t('forgotPassword.enterEmail')}</p>
            </div>

            <form onsubmit={handleSubmit}>
                <div class="fields">
                    <Input
                        type="email"
                        bind:value={emailValue}
                        error={emailError}
                        placeholder={$t('auth.enterEmail')}
                        autocomplete="email"
                    />
                </div>

                <Button type="submit" size="lg" {loading} disabled={loading}>
                    {loading ? $t('common.loading') : $t('forgotPassword.send')}
                </Button>
            </form>

            <button class="back-link" type="button" onclick={backToLogin}>
                <svg
                    viewBox="0 0 24 24"
                    width="16"
                    height="16"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.75"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path d="m15 18-6-6 6-6" />
                </svg>
                {$t('forgotPassword.backToLogin')}
            </button>
        </div>
    {:else if step === 'sent'}
        <div class="forgot-form sent-step">
            <div class="check-icon">
                <svg
                    viewBox="0 0 24 24"
                    width="48"
                    height="48"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
                    <polyline points="22 4 12 14.01 9 11.01" />
                </svg>
            </div>

            <div class="forgot-header">
                <h2 class="forgot-title">{$t('forgotPassword.sent')}</h2>
                <p class="forgot-subtitle">
                    {$t('forgotPassword.sentMessage', { email: emailValue })}
                </p>
            </div>

            <div class="resend-row">
                {#if cooldown > 0}
                    <span class="resend-disabled"
                        >{$t('forgotPassword.resendIn', { seconds: String(cooldown) })}</span
                    >
                {:else}
                    <button
                        class="resend-link"
                        type="button"
                        onclick={handleResend}
                        disabled={loading}
                    >
                        {$t('forgotPassword.resend')}
                    </button>
                {/if}
            </div>

            <Button size="lg" onclick={goToReset}>
                {$t('forgotPassword.enterCode')}
            </Button>
        </div>
    {:else}
        <div class="forgot-form">
            <div class="forgot-header">
                <h2 class="forgot-title">{$t('forgotPassword.resetTitle')}</h2>
                <p class="forgot-subtitle">{$t('forgotPassword.resetSubtitle')}</p>
            </div>

            <form onsubmit={handleReset}>
                <div class="fields">
                    <Input
                        bind:value={codeValue}
                        error={codeError}
                        placeholder={$t('forgotPassword.code')}
                        maxlength={6}
                        autocomplete="one-time-code"
                    />
                    <PasswordInput
                        bind:value={newPassword}
                        bind:confirmValue={confirmPassword}
                        error={newPasswordError}
                        showRules
                        placeholder={$t('forgotPassword.newPassword')}
                        autocomplete="new-password"
                    />
                </div>

                <Button type="submit" size="lg" {loading} disabled={loading}>
                    {loading ? $t('common.loading') : $t('forgotPassword.resetButton')}
                </Button>
            </form>

            <button class="back-link" type="button" onclick={backToLogin}>
                <svg
                    viewBox="0 0 24 24"
                    width="16"
                    height="16"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.75"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path d="m15 18-6-6 6-6" />
                </svg>
                {$t('forgotPassword.backToLogin')}
            </button>
        </div>
    {/if}
</Modal>

<style>
    .forgot-form {
        display: flex;
        flex-direction: column;
        gap: var(--space-5);
    }

    .forgot-header {
        text-align: center;
    }

    .forgot-title {
        font-size: var(--font-xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-2);
    }

    .forgot-subtitle {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: 1.5;
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

    .back-link {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: var(--space-1);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        cursor: pointer;
        transition: var(--transition-colors);
    }

    .back-link:hover {
        color: var(--text-primary);
    }

    .sent-step {
        align-items: center;
        text-align: center;
    }

    .check-icon {
        color: var(--accent);
    }

    .resend-row {
        font-size: var(--font-sm);
    }

    .resend-disabled {
        color: var(--text-tertiary);
    }

    .resend-link {
        color: var(--accent);
        font-weight: var(--weight-medium);
        cursor: pointer;
    }

    .resend-link:hover {
        text-decoration: underline;
    }

    .resend-link:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .sent-step :global(.btn) {
        width: 100%;
    }
</style>
