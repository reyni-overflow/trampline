<script lang="ts">
    import { page } from '$app/state';
    import Input from '$lib/components/ui/Input.svelte';
    import MarkdownEditor from '$lib/components/ui/MarkdownEditor.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import FileUpload from '$lib/components/ui/FileUpload.svelte';
    import { toast } from '$lib/stores/toast';
    import { jobsApi } from '$lib/api/jobs';
    import { handleApiError } from '$lib/api/client';
    import { onMount } from 'svelte';
    import { t, tGet } from '$lib/i18n';
    import { required, minLength, validate } from '$lib/utils/validation';

    let _loading = $state(true);
    let title = $state('');
    let description = $state('');
    let type = $state('Work');
    let format = $state('Remote');
    let address = $state('');
    let city = $state('');
    let country = $state('');
    let salaryFrom = $state('');
    let salaryTo = $state('');
    let tags = $state<string[]>([]);
    let newTag = $state('');
    let isActive = $state(true);
    let photos = $state<string[]>([]);
    let videos = $state<string[]>([]);
    let allTags = $state<string[]>([]);
    let suggestions = $state<string[]>([]);

    onMount(async () => {
        try {
            const job = await jobsApi.getById(page.params.id ?? '');
            title = job.title;
            description = job.description;
            type = job.type;
            format = job.format;
            address = job.address || '';
            city = job.city || '';
            country = job.country || '';
            salaryFrom = job.salaryFrom?.toString() || '';
            salaryTo = job.salaryTo?.toString() || '';
            tags = job.tags?.map((t) => t.name) || [];
            isActive = job.isActive;
            photos = job.photos || [];
            videos = job.videos || [];
        } catch (err) {
            handleApiError(err);
        } finally {
            _loading = false;
        }
        try {
            const res = await jobsApi.getTags();
            allTags = res.map((t) => t.name);
        } catch {
            /* tags are optional, UI still works */
        }
    });

    let errors = $state<Record<string, string>>({});

    let typeOptions = $derived([
        { value: 'Work', label: $t('dashJobs.vacancy') },
        { value: 'Internship', label: $t('dashJobs.internship') },
        { value: 'Event', label: $t('events.title') }
    ]);
    let formatOptions = $derived([
        { value: 'Remote', label: $t('dashJobs.remote') },
        { value: 'Hybrid', label: $t('dashJobs.hybrid') },
        { value: 'Office', label: $t('dashJobs.office') }
    ]);

    function validateTitle() {
        errors.title = validate(title, [required]) || '';
    }

    function validateDescription() {
        errors.description = validate(description, [required, minLength(20)]) || '';
    }

    function validateCity() {
        errors.city = validate(city, [required]) || '';
    }

    function validateAddress() {
        if (format === 'Office' || format === 'Hybrid') {
            errors.address = validate(address, [required]) || '';
        } else {
            errors.address = '';
        }
    }

    function validateSalary() {
        if (salaryFrom && +salaryFrom > 1500000) {
            errors.salaryFrom = tGet('validation.salaryMax');
        } else if (salaryTo && +salaryTo > 1500000) {
            errors.salaryFrom = tGet('validation.salaryMax');
        } else if (salaryFrom && salaryTo && +salaryFrom > +salaryTo) {
            errors.salaryFrom = tGet('validation.salaryRange');
        } else {
            errors.salaryFrom = '';
        }
    }

    function clearError(field: string) {
        errors[field] = '';
    }

    function validateAll(): boolean {
        validateTitle();
        validateDescription();
        validateCity();
        validateAddress();
        validateSalary();
        return !Object.values(errors).some((e) => e);
    }

    function scrollToFirstError() {
        const el = document.querySelector('.has-error');
        if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    function addTag() {
        const t = newTag.trim();
        if (t && !tags.includes(t)) tags = [...tags, t];
        newTag = '';
        suggestions = [];
    }

    function onTagInput() {
        const q = newTag.trim().toLowerCase();
        if (!q) {
            suggestions = [];
            return;
        }
        suggestions = allTags
            .filter((t) => t.toLowerCase().includes(q) && !tags.includes(t))
            .slice(0, 5);
    }

    function selectSuggestion(tag: string) {
        if (!tags.includes(tag)) tags = [...tags, tag];
        newTag = '';
        suggestions = [];
    }

    async function handlePhotoUpload(files: File[]) {
        if (!files.length) return;
        try {
            const urls = await jobsApi.uploadPhotos(page.params.id ?? '', files);
            photos = [...photos, ...urls];
            toast.success($t('editJob.photoUploaded'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function handleVideoUpload(files: File[]) {
        if (!files.length) return;
        try {
            const urls = await jobsApi.uploadVideos(page.params.id ?? '', files);
            videos = [...videos, ...urls];
            toast.success($t('editJob.videoUploaded'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deletePhoto(path: string) {
        try {
            await jobsApi.deletePhoto(page.params.id ?? '', path);
            photos = photos.filter((p) => p !== path);
            toast.success($t('editJob.photoDeleted'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteVideo(path: string) {
        try {
            await jobsApi.deleteVideo(page.params.id ?? '', path);
            videos = videos.filter((v) => v !== path);
            toast.success($t('editJob.videoDeleted'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function save() {
        if (!validateAll()) {
            scrollToFirstError();
            return;
        }
        try {
            await jobsApi.update(page.params.id ?? '', {
                title,
                description,
                address,
                city,
                country,
                isPublished: isActive,
                salaryFrom: salaryFrom ? +salaryFrom : undefined,
                salaryTo: salaryTo ? +salaryTo : undefined,
                tags: tags.map((name) => ({ name, category: 'tech', lvl: 0 }))
            });
            toast.success($t('editJob.updated'));
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('editJob.pageTitle', { title })}</title>
</svelte:head>

<div class="edit-job">
    <a href="/dashboard/jobs" class="back-link">
        <svg
            viewBox="0 0 24 24"
            width="18"
            height="18"
            fill="none"
            stroke="currentColor"
            stroke-width="1.75"
            stroke-linecap="round"
            stroke-linejoin="round"><path d="m15 18-6-6 6-6" /></svg
        >
        {$t('editJob.backToJobs')}
    </a>

    <h1 class="page-heading">{$t('editJob.title')}</h1>

    <div class="form-col">
        <Input
            label={$t('editJob.name')}
            bind:value={title}
            error={errors.title}
            onblur={validateTitle}
            oninput={() => clearError('title')}
        />
        <div class="form-row">
            <Select options={typeOptions} bind:value={type} label={$t('editJob.type')} />
            <Select options={formatOptions} bind:value={format} label={$t('editJob.format')} />
        </div>
        <MarkdownEditor
            label={$t('editJob.description')}
            bind:value={description}
            error={errors.description}
            onblur={validateDescription}
            oninput={() => clearError('description')}
        />
        <div class="form-row">
            <Input
                label={$t('editJob.city')}
                bind:value={city}
                error={errors.city}
                onblur={validateCity}
                oninput={() => clearError('city')}
            />
            <Input label={$t('editJob.country')} bind:value={country} />
        </div>
        {#if format !== 'Remote'}
            <Input
                label={$t('editJob.address')}
                bind:value={address}
                error={errors.address}
                onblur={validateAddress}
                oninput={() => clearError('address')}
            />
        {/if}
        <div class="form-row">
            <Input
                label={$t('editJob.salaryFrom')}
                type="number"
                bind:value={salaryFrom}
                error={errors.salaryFrom}
                onblur={validateSalary}
                oninput={() => clearError('salaryFrom')}
            />
            <Input
                label={$t('editJob.salaryTo')}
                type="number"
                bind:value={salaryTo}
                onblur={validateSalary}
                oninput={() => clearError('salaryFrom')}
            />
        </div>
        <div class="tags-section">
            <span class="field-label">{$t('editJob.tags')}</span>
            {#if tags.length > 0}
                <div class="tags-list">
                    {#each tags as tag (tag)}
                        <Tag
                            removable
                            onremove={() => {
                                tags = tags.filter((t) => t !== tag);
                            }}>{tag}</Tag
                        >
                    {/each}
                </div>
            {/if}
            <form
                class="tag-add"
                onsubmit={(e) => {
                    e.preventDefault();
                    addTag();
                }}
            >
                <div class="tag-input-wrap">
                    <Input
                        placeholder={$t('editJob.addTag')}
                        bind:value={newTag}
                        oninput={onTagInput}
                    />
                    <button class="tag-add-btn" type="submit" aria-label={$t('common.add')}>
                        <svg
                            viewBox="0 0 24 24"
                            width="16"
                            height="16"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            ><line x1="12" y1="5" x2="12" y2="19" /><line
                                x1="5"
                                y1="12"
                                x2="19"
                                y2="12"
                            /></svg
                        >
                    </button>
                    {#if suggestions.length > 0}
                        <div class="tag-suggestions">
                            {#each suggestions as s (s)}
                                <button
                                    type="button"
                                    class="tag-suggestion-item"
                                    onclick={() => selectSuggestion(s)}>{s}</button
                                >
                            {/each}
                        </div>
                    {/if}
                </div>
            </form>
        </div>
        <div class="media-section">
            <span class="field-label">{$t('editJob.media')}</span>
            <div class="media-sub">
                <span class="media-sub-label">{$t('editJob.photos')}</span>
                {#if photos.length > 0}
                    <div class="media-list">
                        {#each photos as photo (photo)}
                            <div class="media-item">
                                <span>{photo.split('/').pop()}</span>
                                <button
                                    class="media-remove"
                                    type="button"
                                    onclick={() => deletePhoto(photo)}
                                    aria-label={$t('common.delete')}
                                >
                                    <svg
                                        viewBox="0 0 24 24"
                                        width="14"
                                        height="14"
                                        fill="none"
                                        stroke="currentColor"
                                        stroke-width="2"
                                        stroke-linecap="round"
                                        ><line x1="18" y1="6" x2="6" y2="18" /><line
                                            x1="6"
                                            y1="6"
                                            x2="18"
                                            y2="18"
                                        /></svg
                                    >
                                </button>
                            </div>
                        {/each}
                    </div>
                {/if}
                <FileUpload
                    accept=".jpg,.jpeg,.png,.webp"
                    multiple
                    maxSizeMb={50}
                    label={$t('editJob.photoDrag')}
                    hint={$t('editJob.photoFormats')}
                    onchange={(f) => handlePhotoUpload(f)}
                />
            </div>
            <div class="media-sub">
                <span class="media-sub-label">{$t('editJob.videos')}</span>
                {#if videos.length > 0}
                    <div class="media-list">
                        {#each videos as video (video)}
                            <div class="media-item">
                                <span>{video.split('/').pop()}</span>
                                <button
                                    class="media-remove"
                                    type="button"
                                    onclick={() => deleteVideo(video)}
                                    aria-label={$t('common.delete')}
                                >
                                    <svg
                                        viewBox="0 0 24 24"
                                        width="14"
                                        height="14"
                                        fill="none"
                                        stroke="currentColor"
                                        stroke-width="2"
                                        stroke-linecap="round"
                                        ><line x1="18" y1="6" x2="6" y2="18" /><line
                                            x1="6"
                                            y1="6"
                                            x2="18"
                                            y2="18"
                                        /></svg
                                    >
                                </button>
                            </div>
                        {/each}
                    </div>
                {/if}
                <FileUpload
                    accept=".mp4,.webp"
                    multiple
                    maxSizeMb={100}
                    label={$t('editJob.videoDrag')}
                    hint={$t('editJob.videoFormats')}
                    onchange={(f) => handleVideoUpload(f)}
                />
            </div>
        </div>
        <div class="form-actions">
            <Button size="lg" onclick={save}>{$t('editJob.save')}</Button>
            <Button size="lg" variant="outline" href="/dashboard/jobs"
                >{$t('editJob.cancel')}</Button
            >
        </div>
    </div>
</div>

<style>
    .edit-job {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
        max-width: 40rem;
    }
    .back-link {
        display: inline-flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-sm);
        color: var(--text-secondary);
        transition: var(--transition-colors);
    }
    .back-link:hover {
        color: var(--accent);
    }
    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }
    .form-col {
        display: flex;
        flex-direction: column;
        gap: var(--space-5);
    }
    .form-row {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: var(--space-4);
    }
    .field-label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }
    .tags-section {
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
    }
    .tag-input-wrap {
        position: relative;
        flex: 1;
    }
    .tag-input-wrap :global(.input-group) {
        width: 100%;
    }
    .tag-input-wrap :global(.input) {
        padding-right: 2.75rem;
    }
    .tag-add-btn {
        position: absolute;
        right: 0.375rem;
        top: 50%;
        transform: translateY(-50%);
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
        cursor: pointer;
    }
    .tag-add-btn:hover {
        color: var(--accent);
        background: var(--accent-subtle);
    }
    .tag-suggestions {
        position: absolute;
        top: 100%;
        left: 0;
        right: 0;
        z-index: 10;
        margin-top: var(--space-1);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-lg);
        overflow: hidden;
    }
    .tag-suggestion-item {
        display: block;
        width: 100%;
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-sm);
        color: var(--text-primary);
        text-align: left;
        cursor: pointer;
        transition: var(--transition-colors);
    }
    .tag-suggestion-item:hover {
        background: var(--bg-tertiary);
    }
    .media-section {
        display: flex;
        flex-direction: column;
        gap: var(--space-4);
    }
    .media-sub {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }
    .media-sub-label {
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        color: var(--text-tertiary);
    }
    .media-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }
    .media-item {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-2) var(--space-3);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }
    .media-item span {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
    .media-remove {
        display: flex;
        color: var(--text-tertiary);
        padding: var(--space-1);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
        flex-shrink: 0;
    }
    .media-remove:hover {
        color: var(--color-error);
        background: var(--color-error-subtle);
    }
    .form-actions {
        display: flex;
        gap: var(--space-3);
        padding-top: var(--space-4);
        border-top: 1px solid var(--border-default);
    }
    @media (max-width: 640px) {
        .form-row {
            grid-template-columns: 1fr;
        }
    }
</style>
