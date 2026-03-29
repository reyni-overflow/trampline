<script lang="ts">
    import Input from '$lib/components/ui/Input.svelte';
    import Textarea from '$lib/components/ui/Textarea.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import FileUpload from '$lib/components/ui/FileUpload.svelte';
    import Modal from '$lib/components/ui/Modal.svelte';
    import { toast } from '$lib/stores/toast';
    import { user as userStore } from '$lib/stores/auth';
    import { workersApi } from '$lib/api/workers';
    import { employeesApi } from '$lib/api/employees';
    import { authApi } from '$lib/api/auth';
    import { handleApiError } from '$lib/api/client';
    import { t } from '$lib/i18n';
    import { required, email as emailRule, inn as innRule, url as urlRule, validate } from '$lib/utils/validation';
    import { onMount, onDestroy } from 'svelte';

    let role = $state('Worker');
    let saving = $state(false);
    let _loaded = $state(false);

    let errors = $state<Record<string, string>>({});

    let companyName = $state('');
    let companyDesc = $state('');
    let companyActivity = $state('');
    let companyLink = $state('');
    let companySocials = $state<string[]>([]);
    let newSocial = $state('');
    let companyInn = $state('');
    let companyAddress = $state('');
    let companyEmail = $state('');
    let companyVideos = $state<string[]>([]);
    let companyPhotos = $state<string[]>([]);
    let isVerified = $state(false);
    let verificationLevel = $state(0);
    let verifiedName = $state<string | null>(null);
    let originalName = $state('');

    let name = $state('');
    let lastName = $state('');
    let patronymic = $state('');
    let university = $state('');
    let course = $state('');
    let about = $state('');
    let skills = $state<string[]>([]);
    let newSkill = $state('');
    let repos = $state<string[]>([]);
    let newRepo = $state('');
    let avatar: string | null = $state(null);
    let resumeFile: string | null = $state(null);

    let showVerificationWarning = $state(false);

    let unsubUser: (() => void) | undefined;
    onMount(() => {
        unsubUser = userStore.subscribe((v) => {
            if (!v) return;
            role = v.role;
            avatar = v.avatar || null;
            if (v.workerProfile) {
                const wp = v.workerProfile;
                name = wp.name || '';
                lastName = wp.lastName || '';
                patronymic = wp.patronymic || '';
                about = wp.about || '';
                avatar = wp.photo || null;
                resumeFile = wp.resume || null;
                skills = wp.skills || [];
                repos = wp.repos || [];
                if (wp.info) {
                    university = wp.info.university || '';
                    course = wp.info.course ? String(wp.info.course) : '';
                }
                }
            if (!name && !lastName && !patronymic && v.nickname) {
                const parts = v.nickname.trim().split(/\s+/);
                if (parts.length === 1) {
                    name = parts[0];
                } else if (parts.length === 2) {
                    name = parts[0];
                    lastName = parts[1];
                } else {
                    lastName = parts[0];
                    name = parts[1];
                    patronymic = parts.slice(2).join(' ');
                }
            }
            if (v.employeeProfile) {
                const ep = v.employeeProfile;
                companyName = ep.name || '';
                companyDesc = ep.description || '';
                companyActivity = ep.activity || '';
                companyLink = ep.link || '';
                companySocials = ep.socials || [];
                isVerified = ep.isVerified || false;
                verificationLevel = ep.verificationLevel ?? 0;
                verifiedName = ep.verifiedName || null;
                originalName = ep.name || '';
                if (ep.info) {
                    companyInn = ep.info.inn || '';
                    companyAddress = ep.info.address || '';
                    companyEmail = ep.info.email || '';
                }
                companyVideos = ep.videos || [];
                companyPhotos = ep.photos || [];
            }
            _loaded = true;
        });
    });
    onDestroy(() => unsubUser?.());

    function addSocial() {
        const s = newSocial.trim();
        if (s && !companySocials.includes(s)) companySocials = [...companySocials, s];
        newSocial = '';
    }

    function removeSocial(url: string) {
        companySocials = companySocials.filter((s) => s !== url);
    }

    async function verify() {
        toast.info($t('dashProfile.verifying'));
        try {
            const res = await employeesApi.verify();
            isVerified = true;
            verificationLevel = 1;
            if (res.value) {
                companyName = res.value;
                verifiedName = res.value;
                originalName = res.value;
            }
            await userStore.fetchUser();
            toast.success($t('dashProfile.companyVerified'));
        } catch (err) {
            handleApiError(err);
        }
    }

    function validateName() {
        errors.name = validate(name, [required]) || '';
    }

    function validateLastName() {
        errors.lastName = validate(lastName, [required]) || '';
    }

    function validateCompanyName() {
        errors.companyName = validate(companyName, [required]) || '';
    }

    function validateCompanyEmail() {
        if (companyEmail) {
            errors.companyEmail = validate(companyEmail, [emailRule]) || '';
        } else {
            errors.companyEmail = '';
        }
    }

    function validateCompanyInn() {
        if (companyInn) {
            errors.companyInn = validate(companyInn, [innRule]) || '';
        } else {
            errors.companyInn = '';
        }
    }

    function validateCompanyLink() {
        if (companyLink) {
            errors.companyLink = validate(companyLink, [urlRule]) || '';
        } else {
            errors.companyLink = '';
        }
    }

    function clearError(field: string) {
        errors[field] = '';
    }

    function scrollToFirstError() {
        const el = document.querySelector('.has-error');
        if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    function validateWorkerAll(): boolean {
        validateName();
        validateLastName();
        return !Object.values(errors).some((e) => e);
    }

    function validateEmployerAll(): boolean {
        validateCompanyName();
        validateCompanyEmail();
        validateCompanyInn();
        validateCompanyLink();
        return !Object.values(errors).some((e) => e);
    }

    async function handlePhotoUpload(files: File[]) {
        if (!files.length) return;
        try {
            const urls = await employeesApi.uploadPhotos(files);
            companyPhotos = [...companyPhotos, ...urls];
            await userStore.fetchUser();
            toast.success($t('dashProfile.photoUpdated'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deletePhoto(path: string) {
        try {
            await employeesApi.deletePhoto(path);
            companyPhotos = companyPhotos.filter(p => p !== path);
            if (avatar === path) avatar = companyPhotos[0] || null;
            await userStore.fetchUser();
            toast.success($t('dashProfile.photoDeleted'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteVideo(path: string) {
        try {
            await employeesApi.deleteVideo(path);
            companyVideos = companyVideos.filter(v => v !== path);
            await userStore.fetchUser();
            toast.success($t('dashProfile.videoDeleted'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function handleVideoUpload(files: File[]) {
        if (!files.length) return;
        try {
            const urls = await employeesApi.uploadVideos(files);
            companyVideos = [...companyVideos, ...urls];
            await userStore.fetchUser();
            toast.success($t('dashProfile.videoUploaded'));
        } catch (err) {
            handleApiError(err);
        }
    }

    function isNameChangeSignificant(): boolean {
        if (!verifiedName || verificationLevel < 1) return false;
        const newTrimmed = companyName.trim();
        if (newTrimmed.toLowerCase() === originalName.toLowerCase()) return false;
        if (verifiedName.toLowerCase().includes(newTrimmed.toLowerCase())) return false;
        if (newTrimmed.toLowerCase().includes(verifiedName.toLowerCase())) return false;
        return true;
    }

    function saveCompany() {
        if (!validateEmployerAll()) {
            scrollToFirstError();
            return;
        }
        if (isNameChangeSignificant()) {
            showVerificationWarning = true;
            return;
        }
        doSaveCompany();
    }

    async function doSaveCompany() {
        showVerificationWarning = false;
        saving = true;
        try {
            await employeesApi.updateProfile({
                name: companyName,
                description: companyDesc,
                activity: companyActivity,
                link: companyLink || undefined,
                info: { address: companyAddress, inn: companyInn, email: companyEmail }
            });
            if (showVerificationWarning) {
                isVerified = false;
                verificationLevel = 0;
                verifiedName = null;
            }
            await userStore.fetchUser();
            toast.success($t('dashProfile.companySaved'));
        } catch (err) {
            handleApiError(err);
        } finally {
            saving = false;
        }
    }

    function addSkill() {
        const s = newSkill.trim();
        if (s && !skills.includes(s)) { skills = [...skills, s]; }
        newSkill = '';
    }

    function removeSkill(skill: string) {
        skills = skills.filter((s) => s !== skill);
    }

    function addRepo() {
        const r = newRepo.trim();
        if (!r) return;
        if (!/^https?:\/\//i.test(r)) {
            toast.error($t('dashProfile.invalidRepoUrl'));
            return;
        }
        if (!repos.includes(r)) { repos = [...repos, r]; }
        newRepo = '';
    }

    function removeRepo(repo: string) {
        repos = repos.filter((r) => r !== repo);
    }

    async function handleAvatarUpload(e: Event) {
        const input = e.target as HTMLInputElement;
        if (!input.files?.[0]) return;
        const file = input.files[0];
        try {
            if (role === 'Worker') {
                const url = await workersApi.uploadAvatar(file);
                avatar = url;
            } else {
                const url = await authApi.uploadAvatar(file);
                avatar = typeof url === 'string' ? url : '';
            }
            await userStore.fetchUser();
            toast.success($t('dashProfile.photoUpdated'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function handleResumeUpload(files: File[]) {
        if (!files.length) return;
        try {
            const url = await workersApi.uploadResume(files[0]);
            resumeFile = url || files[0].name;
            await userStore.fetchUser();
            toast.success($t('dashProfile.resumeUploaded'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function save() {
        if (!validateWorkerAll()) {
            scrollToFirstError();
            return;
        }
        saving = true;
        try {
            await workersApi.updateProfile({
                name,
                lastName,
                patronymic,
                about: about || undefined,
                skills: skills.length > 0 ? skills : undefined,
                repos: repos.length > 0 ? repos : undefined,
                info: university ? {
                    university,
                    course: course ? parseInt(course) : 0
                } : undefined
            });
            await userStore.fetchUser();
            toast.success($t('dashProfile.profileSaved'));
        } catch (err) {
            handleApiError(err);
        } finally {
            saving = false;
        }
    }
</script>

<svelte:head>
    <title>{$t('dashProfile.pageTitle')}</title>
</svelte:head>

<div class="profile-edit">
{#if role === 'Worker'}
    <h1 class="page-heading">{$t('dashProfile.title')}</h1>

    {@const completionSteps = [
        { done: !!avatar, label: $t('profileCompletion.addAvatar') },
        { done: !!about.trim(), label: $t('profileCompletion.addAbout') },
        { done: skills.length > 0, label: $t('profileCompletion.addSkills') },
        { done: !!resumeFile, label: $t('profileCompletion.addResume') },
        { done: repos.length > 0, label: $t('profileCompletion.addRepos') }
    ]}
    {@const completionPercent = Math.round(completionSteps.filter(s => s.done).length / completionSteps.length * 100)}

    <div class="completion-card">
        <div class="completion-header">
            <span class="completion-title">{$t('profileCompletion.title')}</span>
            <span class="completion-percent" class:complete={completionPercent === 100}>{completionPercent}%</span>
        </div>
        <div class="completion-bar">
            <div class="completion-fill" style="width: {completionPercent}%"></div>
        </div>
        {#if completionPercent < 100}
            <ul class="completion-hints">
                {#each completionSteps.filter(s => !s.done) as step (step.label)}
                    <li>{step.label}</li>
                {/each}
            </ul>
        {:else}
            <p class="completion-done">{$t('profileCompletion.complete')}</p>
        {/if}
    </div>

    <div class="qr-card">
        <div class="qr-info">
            <h3 class="qr-title">{$t('dashProfile.qrTitle')}</h3>
            <p class="qr-desc">{$t('dashProfile.qrDesc')}</p>
            <Button size="sm" variant="outline" onclick={() => { if (typeof navigator.clipboard !== 'undefined') { navigator.clipboard.writeText(`${typeof window !== 'undefined' ? window.location.origin : ''}/profile/me`); toast.success($t('share.copied')); } }}>
                {$t('share.copyLink')}
            </Button>
        </div>
        <div class="qr-wrapper">
            <img class="qr-image" src="https://api.qrserver.com/v1/create-qr-code/?size=120x120&data={encodeURIComponent(`${typeof window !== 'undefined' ? window.location.origin : ''}/profile/me`)}" alt="QR" loading="lazy" />
        </div>
    </div>

    <section class="form-section">
        <h2>{$t('dashProfile.photo')}</h2>
        <div class="avatar-upload">
            <Avatar name="{name} {lastName}" src={avatar} size={96} />
            <div class="avatar-actions">
                <label class="upload-btn">
                    {$t('dashProfile.uploadPhoto')}
                    <input type="file" accept="image/*" onchange={handleAvatarUpload} hidden />
                </label>
                {#if avatar}
                    <button class="remove-link" type="button" onclick={() => { avatar = null; }}>{$t('dashProfile.removePhoto')}</button>
                {/if}
            </div>
        </div>
    </section>

    <section class="form-section">
        <h2>{$t('dashProfile.mainInfo')}</h2>
        <div class="form-grid">
            <Input label={$t('dashProfile.lastName')} bind:value={lastName} error={errors.lastName} onblur={validateLastName} oninput={() => clearError('lastName')} />
            <Input label={$t('dashProfile.firstName')} bind:value={name} error={errors.name} onblur={validateName} oninput={() => clearError('name')} />
            <Input label={$t('dashProfile.middleName')} bind:value={patronymic} />
            <Input label={$t('dashProfile.university')} bind:value={university} />
            <Input label={$t('dashProfile.courseYear')} bind:value={course} />
        </div>
    </section>

    <section class="form-section">
        <h2>{$t('dashProfile.aboutMe')}</h2>
        <Textarea bind:value={about} placeholder={$t('dashProfile.aboutPlaceholder')} />
    </section>

    <section class="form-section">
        <h2>{$t('dashProfile.skills')}</h2>
        <div class="tags-edit">
            <div class="tags-list">
                {#each skills as skill (skill)}
                    <Tag removable onremove={() => removeSkill(skill)}>{skill}</Tag>
                {/each}
            </div>
            <form class="tag-add" onsubmit={(e) => { e.preventDefault(); addSkill(); }}>
                <Input placeholder={$t('dashProfile.addSkillPlaceholder')} bind:value={newSkill} />
                <Button type="submit" variant="secondary" size="sm">{$t('dashProfile.addSkill')}</Button>
            </form>
        </div>
    </section>

    <section class="form-section">
        <h2>{$t('dashProfile.resume')}</h2>
        <div class="resume-upload">
            <FileUpload
                accept=".pdf,.docx,.doc"
                label={$t('dashProfile.resumeDrag')}
                hint={$t('dashProfile.resumeFormats')}
                onchange={(f) => handleResumeUpload(f)}
            />
        </div>
    </section>

    <section class="form-section">
        <h2>{$t('dashProfile.repositories')}</h2>
        <div class="repos-list">
            {#each repos as repo (repo)}
                <div class="repo-item">
                    <span>{repo}</span>
                    <button class="remove-link" type="button" onclick={() => removeRepo(repo)}>{$t('common.delete')}</button>
                </div>
            {/each}
        </div>
        <form class="tag-add" onsubmit={(e) => { e.preventDefault(); addRepo(); }}>
            <Input placeholder={$t('dashProfile.repoPlaceholder')} bind:value={newRepo} />
            <Button type="submit" variant="secondary" size="sm">{$t('common.add')}</Button>
        </form>
    </section>

    <div class="form-actions">
        <Button size="lg" onclick={save} disabled={saving}>{saving ? $t('common.saving') : $t('dashProfile.save')}</Button>
    </div>
{:else}
    <h1 class="page-heading">{$t('dashProfile.companyTitle')}</h1>

    <section class="form-section">
        <h2>{$t('dashProfile.logo')}</h2>
        <div class="avatar-upload">
            <Avatar name={companyName} src={avatar} size={96} />
            <div class="avatar-actions">
                <label class="upload-btn">
                    {$t('dashProfile.uploadLogo')}
                    <input type="file" accept="image/*" onchange={handleAvatarUpload} hidden />
                </label>
            </div>
        </div>
    </section>

    <section class="form-section">
        <h2>{$t('dashProfile.mainInfo')}</h2>
        <div class="form-grid">
            <Input label={$t('dashProfile.companyName')} bind:value={companyName} error={errors.companyName} onblur={validateCompanyName} oninput={() => clearError('companyName')} />
            <Input label={$t('dashProfile.fieldOfActivity')} bind:value={companyActivity} />
        </div>
        <Textarea label={$t('dashProfile.description')} bind:value={companyDesc} placeholder={$t('dashProfile.descPlaceholder')} />
        <Input label={$t('dashProfile.website')} bind:value={companyLink} placeholder={$t('dashProfile.websitePlaceholder')} error={errors.companyLink} onblur={validateCompanyLink} oninput={() => clearError('companyLink')} />
    </section>

    <section class="form-section" class:compact-section={companySocials.length === 0}>
        <h2>{$t('dashProfile.socials')}</h2>
        {#if companySocials.length > 0}
            <div class="repos-list">
                {#each companySocials as social (social)}
                    <div class="repo-item">
                        <span>{social}</span>
                        <button class="remove-link" type="button" onclick={() => removeSocial(social)}>{$t('common.delete')}</button>
                    </div>
                {/each}
            </div>
        {/if}
        <form class="tag-add" onsubmit={(e) => { e.preventDefault(); addSocial(); }}>
            <Input placeholder={$t('dashProfile.socialsPlaceholder')} bind:value={newSocial} />
            <Button type="submit" variant="secondary" size="sm">{$t('common.add')}</Button>
        </form>
    </section>

    <section class="form-section">
        <h2>{$t('dashProfile.photos')}</h2>
        {#if companyPhotos.length > 0}
            <div class="repos-list">
                {#each companyPhotos as photo (photo)}
                    <div class="repo-item">
                        <span>{photo.split('/').pop()}</span>
                        <button class="remove-link" type="button" onclick={() => deletePhoto(photo)}>{$t('dashProfile.deletePhoto')}</button>
                    </div>
                {/each}
            </div>
        {/if}
        <FileUpload
            accept=".jpg,.jpeg,.png,.webp"
            multiple
            maxSizeMb={50}
            label={$t('dashProfile.photoDrag')}
            hint={$t('dashProfile.photoFormats')}
            onchange={(f) => handlePhotoUpload(f)}
        />
    </section>

    <section class="form-section">
        <h2>{$t('dashProfile.videos')}</h2>
        {#if companyVideos.length > 0}
            <div class="repos-list">
                {#each companyVideos as video (video)}
                    <div class="repo-item">
                        <span>{video.split('/').pop()}</span>
                        <button class="remove-link" type="button" onclick={() => deleteVideo(video)}>{$t('dashProfile.deleteVideo')}</button>
                    </div>
                {/each}
            </div>
        {/if}
        <FileUpload
            accept=".mp4,.webp"
            multiple
            maxSizeMb={100}
            label={$t('dashProfile.videoDrag')}
            hint={$t('dashProfile.videoFormats')}
            onchange={(f) => handleVideoUpload(f)}
        />
    </section>

    <section class="form-section">
        <div class="section-header-row">
            <h2>{$t('dashProfile.verification')}</h2>
            {#if verificationLevel >= 2}
                <Badge variant="info">{$t('dashProfile.trustedVerified')}</Badge>
            {:else if isVerified}
                <Badge variant="success">{$t('dashProfile.verified')}</Badge>
            {:else}
                <Badge variant="warning">{$t('dashProfile.notVerified')}</Badge>
            {/if}
        </div>
        <div class="form-grid">
            <Input label={$t('dashProfile.inn')} bind:value={companyInn} error={errors.companyInn} onblur={validateCompanyInn} oninput={() => clearError('companyInn')} />
            <Input label={$t('dashProfile.corpEmail')} bind:value={companyEmail} type="email" error={errors.companyEmail} onblur={validateCompanyEmail} oninput={() => clearError('companyEmail')} />
        </div>
        <Input label={$t('dashProfile.legalAddress')} bind:value={companyAddress} />
        {#if !isVerified}
            <Button variant="secondary" onclick={verify}>{$t('dashProfile.verifyBtn')}</Button>
        {/if}
    </section>

    <div class="form-actions">
        <Button size="lg" onclick={saveCompany} disabled={saving}>{saving ? $t('common.saving') : $t('dashProfile.save')}</Button>
    </div>
{/if}
</div>

<Modal bind:open={showVerificationWarning} title={$t('dashProfile.verificationWarningTitle')} maxWidth="480px">
    <p class="verification-warning-text">{$t('dashProfile.verificationWarningText')}</p>
    <div class="verification-warning-actions">
        <Button onclick={doSaveCompany} disabled={saving}>{$t('dashProfile.confirmChange')}</Button>
        <Button variant="outline" onclick={() => { showVerificationWarning = false; }}>{$t('common.cancel')}</Button>
    </div>
</Modal>

<style>
    .profile-edit {
        display: flex;
        flex-direction: column;
        gap: var(--space-8);
        max-width: 40rem;
    }

    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .form-section {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }

    .form-section.compact-section {
        gap: var(--space-2);
    }

    .form-section h2 {
        font-size: var(--font-md);
        font-weight: var(--weight-semibold);
    }

    .form-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: var(--space-4);
    }

    .avatar-upload {
        display: flex;
        align-items: center;
        gap: var(--space-5);
    }

    .avatar-actions {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .upload-btn {
        display: inline-flex;
        align-items: center;
        padding: var(--space-2) var(--space-4);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--accent);
        background: var(--accent-subtle);
        border-radius: var(--radius-md);
        cursor: pointer;
        transition: var(--transition-colors);
        width: fit-content;
    }

    .upload-btn:hover {
        background: var(--accent);
        color: var(--accent-contrast);
    }

    .remove-link {
        font-size: var(--font-xs);
        color: var(--color-error);
        cursor: pointer;
    }

    .remove-link:hover { text-decoration: underline; }

    .tags-edit {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .tags-list {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-2);
    }

    .tag-add {
        display: flex;
        gap: var(--space-2);
        align-items: flex-end;
    }

    .tag-add :global(.btn) {
        height: 2.75rem;
        flex-shrink: 0;
    }

    .tag-add :global(.input-group) { flex: 1; }

    .resume-upload {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .repos-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .repo-item {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-2) var(--space-4);
        background: var(--bg-secondary);
        border-radius: var(--radius-md);
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .form-actions {
        padding-top: var(--space-4);
        border-top: 1px solid var(--border-default);
    }

    .section-header-row {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .section-header-row h2 {
        margin-bottom: 0;
    }

    .qr-card {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-4);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        margin-bottom: var(--space-6);
        max-width: 100%;
        overflow: hidden;
    }

    .qr-info {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .qr-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }

    .qr-desc {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        max-width: 20rem;
    }

    .qr-wrapper {
        width: 7.125rem;
        height: 7.125rem;
        background: var(--text-inverse);
        border-radius: var(--radius-sm);
        padding: 0.25rem;
        flex-shrink: 0;
    }

    .qr-image {
        width: 100%;
        height: 100%;
        border-radius: 2px;
        object-fit: contain;
    }

    .completion-card {
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        margin-bottom: var(--space-6);
    }

    .completion-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: var(--space-3);
    }

    .completion-title {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }

    .completion-percent {
        font-size: var(--font-sm);
        font-weight: var(--weight-bold);
        color: var(--accent);
    }

    .completion-percent.complete { color: var(--color-success); }

    .completion-bar {
        height: 0.375rem;
        background: var(--bg-tertiary);
        border-radius: var(--radius-full);
        overflow: hidden;
    }

    .completion-fill {
        height: 100%;
        background: var(--accent);
        border-radius: var(--radius-full);
        transition: width var(--duration-moderate) var(--ease-out);
    }

    .completion-hints {
        margin-top: var(--space-3);
        padding-left: var(--space-5);
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .completion-hints li::marker { color: var(--accent); }

    .completion-done {
        margin-top: var(--space-3);
        font-size: var(--font-xs);
        color: var(--color-success);
        font-weight: var(--weight-medium);
    }

    .verification-warning-text {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-relaxed);
        margin-bottom: var(--space-5);
    }

    .verification-warning-actions {
        display: flex;
        gap: var(--space-3);
    }

    @media (max-width: 640px) {
        .form-grid { grid-template-columns: 1fr; }
        .avatar-upload { flex-direction: column; align-items: flex-start; }
    }
</style>
