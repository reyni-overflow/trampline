<script lang="ts">
    import Input from '$lib/components/ui/Input.svelte';
    import PasswordInput from '$lib/components/ui/PasswordInput.svelte';
    import Toggle from '$lib/components/ui/Toggle.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import ThemeToggle from '$lib/components/ui/ThemeToggle.svelte';
    import AccentPicker from '$lib/components/ui/AccentPicker.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import DOMPurify from 'dompurify';
    import { browser } from '$app/environment';
    import QRCode from 'qrcode';
    import { goto } from '$app/navigation';
    import { toast } from '$lib/stores/toast';
    import { authApi, type SessionResponse } from '$lib/api/auth';
    import { handleApiError } from '$lib/api/client';
    import { user as userStore, isAuthenticated, authReady } from '$lib/stores/auth';
    import { authModal } from '$lib/stores/auth-modal';
    import { onMount, onDestroy } from 'svelte';
    import { t, getLocaleDateString } from '$lib/i18n';

    let isAuth = $state(false);
    const unsubAuth = isAuthenticated.subscribe((v) => {
        isAuth = v;
    });
    const unsubReady = authReady.subscribe((v) => {
        if (v && !isAuth && typeof window !== 'undefined') {
            goto('/');
            authModal.openLogin();
        }
    });

    let email = $state('');
    let currentPassword = $state('');
    let newPassword = $state('');
    let passwordError = $state('');
    let newPasswordError = $state('');

    let hideApplications = $state(false);
    let hideResume = $state(false);
    let publicProfile = $state(true);

    let sessions = $state<
        {
            id: string;
            userAgent: string;
            osIcon: string;
            osName: string;
            browser: string;
            ip: string;
            lastUsed: string;
            current: boolean;
        }[]
    >([]);
    let sessionsLoading = $state(true);
    let totpEnabled = $state(false);
    let totpSetupSecret = $state('');
    let totpQrDataUrl = $state('');
    let totpSetupCode = $state('');
    let totpDisableCode = $state('');
    let totpSetupLoading = $state(false);
    let totpSetupActive = $state(false);

    let unsubUser: (() => void) | undefined;
    onMount(() => {
        unsubUser = userStore.subscribe((v) => {
            if (v) {
                email = v.email;
                totpEnabled = v.isTotpEnabled ?? false;
                publicProfile = !(v.isPrivate ?? false);
                hideApplications = v.hideApplications ?? false;
                hideResume = v.hideResume ?? false;
            }
        });
        loadSessions();
    });
    onDestroy(() => {
        unsubUser?.();
        unsubAuth();
        unsubReady();
    });

    const svgIcon = (d: string) =>
        `<svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">${d}</svg>`;
    const osIcons = {
        monitor: svgIcon(
            '<rect x="2" y="3" width="20" height="14" rx="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/>'
        ),
        laptop: svgIcon(
            '<path d="M20 16V7a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v9m16 0H4m16 0 1.28 2.55a1 1 0 0 1-.9 1.45H3.62a1 1 0 0 1-.9-1.45L4 16"/>'
        ),
        smartphone: svgIcon(
            '<rect x="5" y="2" width="14" height="20" rx="2" ry="2"/><line x1="12" y1="18" x2="12.01" y2="18"/>'
        ),
        terminal: svgIcon(
            '<polyline points="4 17 10 11 4 5"/><line x1="12" y1="19" x2="20" y2="19"/>'
        ),
        zap: svgIcon('<polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/>'),
        globe: svgIcon(
            '<circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/>'
        )
    };

    function parseOS(ua: string): { os: string; icon: string; browser: string } {
        const s = ua.toLowerCase();
        let os = $t('settings.unknownOS');
        let icon = osIcons.globe;
        let browser = $t('settings.webBrowser');

        if (s.includes('windows')) {
            os = 'Windows';
            icon = osIcons.monitor;
        } else if (s.includes('mac os') || s.includes('macintosh')) {
            os = 'macOS';
            icon = osIcons.laptop;
        } else if (s.includes('iphone') || s.includes('ipad')) {
            os = 'iOS';
            icon = osIcons.smartphone;
        } else if (s.includes('android')) {
            os = 'Android';
            icon = osIcons.smartphone;
        } else if (s.includes('linux')) {
            os = 'Linux';
            icon = osIcons.terminal;
        } else if (s.startsWith('curl/')) {
            os = 'API';
            icon = osIcons.zap;
            browser = 'curl';
        } else if (s.includes('postman')) {
            os = 'API';
            icon = osIcons.zap;
            browser = 'Postman';
        } else if (!s || s === 'unknown') {
            os = $t('settings.unknownOS');
            icon = osIcons.globe;
        }

        if (browser === $t('settings.webBrowser')) {
            if (s.includes('firefox')) browser = 'Firefox';
            else if (s.includes('edg/')) browser = 'Edge';
            else if (s.includes('chrome')) browser = 'Chrome';
            else if (s.includes('safari')) browser = 'Safari';
            else if (s.includes('opera') || s.includes('opr/')) browser = 'Opera';
            else if (s.includes('yabrowser')) browser = 'Yandex';
        }

        return { os, icon, browser };
    }

    async function loadSessions() {
        sessionsLoading = true;
        try {
            const data = await authApi.sessions();
            const sorted = [...data].sort((a, b) => {
                const ta = a.lastUsedAt ? new Date(a.lastUsedAt).getTime() : 0;
                const tb = b.lastUsedAt ? new Date(b.lastUsedAt).getTime() : 0;
                return tb - ta;
            });
            const browserSessions = sorted.filter(
                (s) =>
                    s.isActive &&
                    s.userAgent?.agent &&
                    !s.userAgent.agent.toLowerCase().startsWith('curl/') &&
                    !s.userAgent.agent.toLowerCase().includes('postman')
            );
            const mostRecentId = (browserSessions[0] ?? sorted.find((s) => s.isActive))?.id;
            sessions = sorted.map((s: SessionResponse) => {
                const rawAgent = s.userAgent?.agent || '';
                const parsed = parseOS(rawAgent);
                return {
                    id: s.id,
                    userAgent: rawAgent,
                    osIcon: parsed.icon,
                    osName: parsed.os,
                    browser: parsed.browser,
                    ip:
                        s.userAgent?.ip && s.userAgent.ip !== 'Unknown device'
                            ? s.userAgent.ip
                            : '',
                    lastUsed: s.lastUsedAt
                        ? new Date(s.lastUsedAt).toLocaleString(getLocaleDateString())
                        : s.createdAt
                          ? new Date(s.createdAt).toLocaleString(getLocaleDateString())
                          : '—',
                    current: s.id === mostRecentId
                };
            });
        } catch {
            sessions = [];
        } finally {
            sessionsLoading = false;
        }
    }

    function validateCurrentPassword() {
        if (!currentPassword) {
            passwordError = $t('settings.currentPasswordRequired');
        } else {
            passwordError = '';
        }
    }

    function validateNewPassword() {
        if (!newPassword) {
            newPasswordError = '';
            return;
        }
        if (newPassword.length < 8) {
            newPasswordError = $t('settings.passwordTooShort');
            return;
        }
        if (
            !/[A-ZА-ЯЁ]/.test(newPassword) ||
            !/[a-zа-яё]/.test(newPassword) ||
            !/\d/.test(newPassword)
        ) {
            newPasswordError = $t('settings.passwordRequirements');
            return;
        }
        if (newPassword === currentPassword) {
            newPasswordError = $t('settings.passwordSameAsCurrent');
            return;
        }
        newPasswordError = '';
    }

    async function saveAccount() {
        validateCurrentPassword();
        validateNewPassword();
        if (passwordError || newPasswordError) return;
        if (!currentPassword || !newPassword) {
            if (!currentPassword) passwordError = $t('settings.currentPasswordRequired');
            if (!newPassword) newPasswordError = $t('settings.newPasswordRequired');
            return;
        }
        try {
            await authApi.changePassword(currentPassword, newPassword);
            toast.success($t('settings.accountSaved'));
            currentPassword = '';
            newPassword = '';
            passwordError = '';
            newPasswordError = '';
        } catch (err) {
            handleApiError(err);
        }
    }

    async function savePrivacy() {
        try {
            await authApi.updatePrivacy({
                isPrivate: !publicProfile,
                hideApplications,
                hideResume
            });
            const me = await authApi.me();
            userStore.setUser(me);
            toast.success($t('settings.privacySaved'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function terminateSession(id: string) {
        try {
            await authApi.closeSession(id);
            sessions = sessions.filter((s) => s.id !== id);
            toast.success($t('settings.sessionTerminated'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function terminateAllSessions() {
        try {
            await authApi.closeAllSessions();
            await loadSessions();
            toast.success($t('settings.allSessionsTerminated'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteAccount() {
        const password = prompt($t('settings.confirmDeletePassword'));
        if (!password) return;
        try {
            await authApi.deleteAccount(password);
            toast.danger($t('settings.deleteAccountMsg'));
            window.location.href = '/';
        } catch (err) {
            handleApiError(err);
        }
    }

    async function startTotpSetup() {
        totpSetupLoading = true;
        try {
            const data = await authApi.totpSetup();
            totpSetupSecret = data.secret;
            totpQrDataUrl = await QRCode.toDataURL(data.uri, {
                width: 200,
                margin: 2,
                color: { dark: '#000000', light: '#ffffff' }
            });
            totpSetupActive = true;
        } catch (err) {
            handleApiError(err);
        } finally {
            totpSetupLoading = false;
        }
    }

    async function confirmTotpEnable() {
        if (totpSetupCode.length !== 6) return;
        try {
            await authApi.totpEnable(totpSetupCode);
            totpEnabled = true;
            totpSetupActive = false;
            totpSetupCode = '';
            const me = await authApi.me();
            userStore.setUser(me);
            toast.success($t('settings.totpEnabled'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function disableTotp() {
        if (totpDisableCode.length !== 6) return;
        try {
            await authApi.totpDisable(totpDisableCode);
            totpEnabled = false;
            totpDisableCode = '';
            const me = await authApi.me();
            userStore.setUser(me);
            toast.success($t('settings.totpDisabled'));
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('settings.pageTitle')}</title>
</svelte:head>

<div class="settings-container">
    <h1 class="page-heading">{$t('settings.title')}</h1>

    <div class="settings-grid">
        <section class="settings-section">
            <h2>{$t('settings.account')}</h2>
            <div class="settings-card">
                <div class="field-row">
                    <Input label="Email" bind:value={email} type="email" disabled />
                </div>
                <div class="field-row">
                    <PasswordInput
                        bind:value={currentPassword}
                        label={$t('settings.currentPassword')}
                        placeholder={$t('settings.currentPasswordPlaceholder')}
                        error={passwordError}
                        autocomplete="current-password"
                    />
                </div>
                <div class="field-row">
                    <PasswordInput
                        bind:value={newPassword}
                        label={$t('settings.newPassword')}
                        placeholder={$t('settings.newPasswordPlaceholder')}
                        error={newPasswordError}
                        showRules
                        autocomplete="new-password"
                    />
                </div>
                <Button onclick={saveAccount}>{$t('common.save')}</Button>
            </div>
        </section>

        <section class="settings-section">
            <h2>{$t('settings.privacy')}</h2>
            <div class="settings-card">
                <Toggle label={$t('settings.hideApplications')} bind:checked={hideApplications} />
                <Toggle label={$t('settings.hideResume')} bind:checked={hideResume} />
                <Toggle label={$t('settings.publicProfile')} bind:checked={publicProfile} />
                <Button variant="secondary" onclick={savePrivacy}
                    >{$t('settings.saveSettings')}</Button
                >
            </div>
        </section>

        <section class="settings-section">
            <h2>{$t('settings.twoFactor')}</h2>
            <div class="settings-card">
                {#if totpEnabled}
                    <div class="totp-status">
                        <Badge variant="success">{$t('settings.totpActive')}</Badge>
                        <p class="totp-desc">{$t('settings.totpActiveDesc')}</p>
                    </div>
                    <div class="field-row">
                        <Input
                            label={$t('settings.totpCodeLabel')}
                            bind:value={totpDisableCode}
                            placeholder="000000"
                            maxlength={6}
                        />
                    </div>
                    <Button
                        variant="danger"
                        onclick={disableTotp}
                        disabled={totpDisableCode.length !== 6}
                        >{$t('settings.totpDisableBtn')}</Button
                    >
                {:else if totpSetupActive}
                    <div class="totp-setup">
                        <p class="totp-desc">{$t('settings.totpScanQR')}</p>
                        {#if totpQrDataUrl}
                            <img
                                class="totp-qr"
                                src={totpQrDataUrl}
                                alt="TOTP QR Code"
                                width="200"
                                height="200"
                            />
                        {/if}
                        <div class="totp-secret">
                            <span class="totp-secret-label">{$t('settings.totpManualKey')}</span>
                            <code class="totp-secret-value">{totpSetupSecret}</code>
                        </div>
                        <div class="field-row">
                            <Input
                                label={$t('settings.totpVerifyCode')}
                                bind:value={totpSetupCode}
                                placeholder="000000"
                                maxlength={6}
                            />
                        </div>
                        <Button onclick={confirmTotpEnable} disabled={totpSetupCode.length !== 6}
                            >{$t('settings.totpConfirmEnable')}</Button
                        >
                    </div>
                {:else}
                    <p class="totp-desc">{$t('settings.totpDesc')}</p>
                    <Button onclick={startTotpSetup} loading={totpSetupLoading}
                        >{$t('settings.totpEnableBtn')}</Button
                    >
                {/if}
            </div>
        </section>

        <section class="settings-section">
            <h2>{$t('settings.appearance')}</h2>
            <div class="settings-card">
                <div class="appearance-row">
                    <span class="appearance-label">{$t('settings.theme')}</span>
                    <ThemeToggle />
                </div>
                <div class="appearance-row">
                    <span class="appearance-label">{$t('settings.accentColor')}</span>
                    <AccentPicker />
                </div>
            </div>
        </section>

        <section class="settings-section">
            <h2>{$t('settings.activeSessions')}</h2>
            {#if sessionsLoading}
                <p style="color: var(--text-tertiary); font-size: var(--font-sm);">
                    {$t('settings.loadingSessions')}
                </p>
            {:else if sessions.length === 0}
                <p style="color: var(--text-tertiary); font-size: var(--font-sm);">
                    {$t('settings.noSessions')}
                </p>
            {:else}
                <div class="sessions-list">
                    {#each sessions as session (session.id)}
                        <div class="session-entry">
                            <div class="session-row">
                                <div class="session-info">
                                    <span class="session-device">
                                        <span class="session-os-icon">
                                            <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                                            {@html browser
                                                ? DOMPurify.sanitize(session.osIcon)
                                                : session.osIcon}
                                        </span>
                                        {session.osName} &middot; {session.browser}
                                        {#if session.current}<Badge variant="success" size="sm"
                                                >{$t('settings.currentSession')}</Badge
                                            >{/if}
                                    </span>
                                    <span class="session-meta"
                                        >{session.ip
                                            ? `IP: ${session.ip} · `
                                            : ''}{session.lastUsed}</span
                                    >
                                </div>
                                <div class="session-actions">
                                    {#if !session.current}
                                        <Button
                                            size="sm"
                                            variant="ghost"
                                            onclick={() => terminateSession(session.id)}
                                            >{$t('settings.terminate')}</Button
                                        >
                                    {/if}
                                </div>
                            </div>
                        </div>
                    {/each}
                </div>
                {#if sessions.length > 1}
                    <button class="terminate-all-btn" type="button" onclick={terminateAllSessions}>
                        <svg
                            viewBox="0 0 24 24"
                            width="16"
                            height="16"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            ><path
                                d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"
                            /><line x1="12" y1="9" x2="12" y2="13" /><line
                                x1="12"
                                y1="17"
                                x2="12.01"
                                y2="17"
                            /></svg
                        >
                        {$t('settings.terminateAll')}
                    </button>
                {/if}
            {/if}
        </section>
    </div>

    <section class="settings-section danger-zone">
        <h2>{$t('settings.dangerZone')}</h2>
        <div class="settings-card">
            <p class="danger-text">{$t('settings.dangerText')}</p>
            <Button variant="danger" onclick={deleteAccount}>{$t('settings.deleteAccount')}</Button>
        </div>
    </section>
</div>

<style>
    .settings-container {
        max-width: 56rem;
        margin: 0 auto;
        padding: var(--space-6) var(--space-6) var(--space-12);
    }

    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
        margin-bottom: var(--space-8);
    }

    .settings-grid {
        columns: 2;
        column-gap: var(--space-8);
    }

    .settings-grid > .settings-section {
        break-inside: avoid;
        margin-bottom: var(--space-8);
    }

    .settings-section h2 {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
        margin-bottom: var(--space-4);
    }

    .settings-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
    }

    .field-row {
        width: 100%;
    }

    .appearance-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .appearance-label {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .sessions-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        max-height: calc(5 * 4.5rem);
        overflow-y: auto;
        scrollbar-width: thin;
        scrollbar-color: var(--scrollbar-thumb) transparent;
    }

    .sessions-list::-webkit-scrollbar {
        width: 4px;
    }

    .sessions-list::-webkit-scrollbar-track {
        background: transparent;
    }

    .sessions-list::-webkit-scrollbar-thumb {
        background: var(--scrollbar-thumb);
        border-radius: 9999px;
    }

    .sessions-list::-webkit-scrollbar-thumb:hover {
        background: var(--scrollbar-thumb-hover);
    }

    .terminate-all-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: var(--space-2);
        width: 100%;
        margin-top: var(--space-3);
        padding: var(--space-2) var(--space-4);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--color-error);
        background: var(--color-error-subtle);
        border: 1px solid var(--color-error-subtle);
        border-radius: var(--radius-md);
        cursor: pointer;
        transition: var(--transition-colors);
    }

    .terminate-all-btn:hover {
        background: var(--color-error);
        color: white;
    }

    .session-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-3) var(--space-4);
    }

    .session-info {
        display: flex;
        flex-direction: column;
        gap: 2px;
    }

    .session-device {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        display: flex;
        align-items: center;
        gap: var(--space-2);
    }

    .session-os-icon {
        display: inline-flex;
        align-items: center;
        color: var(--text-tertiary);
    }

    .session-meta {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .session-entry {
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        overflow: hidden;
    }

    .session-actions {
        display: flex;
        gap: var(--space-1);
        flex-shrink: 0;
    }

    .danger-zone h2 {
        color: var(--color-error);
    }

    .danger-zone .settings-card {
        border-color: var(--color-error-subtle);
    }

    .danger-text {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .danger-zone {
        margin-top: var(--space-2);
    }

    .totp-status {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .totp-desc {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .totp-setup {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-4);
        width: 100%;
    }

    .totp-qr {
        border-radius: var(--radius-md);
        border: 1px solid var(--border-default);
    }

    .totp-secret {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-1);
    }

    .totp-secret-label {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .totp-secret-value {
        font-size: var(--font-sm);
        padding: var(--space-2) var(--space-3);
        background: var(--bg-tertiary);
        border-radius: var(--radius-sm);
        word-break: break-all;
        user-select: all;
    }

    .totp-setup .field-row {
        width: 100%;
    }

    @media (max-width: 640px) {
        .settings-grid {
            columns: 1;
        }
    }
</style>
