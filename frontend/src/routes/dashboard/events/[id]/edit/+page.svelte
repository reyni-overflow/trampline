<script lang="ts">
    import { page } from '$app/state';
    import Input from '$lib/components/ui/Input.svelte';
    import MarkdownEditor from '$lib/components/ui/MarkdownEditor.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import FileUpload from '$lib/components/ui/FileUpload.svelte';
    import { toast } from '$lib/stores/toast';
    import { eventsApi } from '$lib/api/events';
    import { handleApiError } from '$lib/api/client';
    import { onMount } from 'svelte';
    import { t } from '$lib/i18n';
    import { required, minLength, validate } from '$lib/utils/validation';

    let _loading = $state(true);
    let title = $state('');
    let description = $state('');
    let format = $state('Remote');
    let address = $state('');
    let city = $state('');
    let country = $state('');
    let startDate = $state('');
    let tags = $state<string[]>([]);
    let newTag = $state('');
    let isActive = $state(true);
    let photos = $state<string[]>([]);
    let videos = $state<string[]>([]);

    onMount(async () => {
        try {
            const event = await eventsApi.getById(page.params.id ?? '');
            title = event.title;
            description = event.description;
            format = event.format;
            address = event.address || '';
            city = event.city || '';
            country = event.country || '';
            tags = event.tags?.map((t) => t.name) || [];
            isActive = event.isActive;
            startDate = event.startDate ? new Date(event.startDate).toISOString().slice(0, 16) : '';
            photos = event.photos || [];
            videos = event.videos || [];
        } catch (err) {
            handleApiError(err);
        } finally {
            _loading = false;
        }
    });

    let errors = $state<Record<string, string>>({});

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

    function clearError(field: string) {
        errors[field] = '';
    }

    function validateAll(): boolean {
        validateTitle();
        validateDescription();
        validateCity();
        validateAddress();
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
    }

    async function handlePhotoUpload(files: File[]) {
        if (!files.length) return;
        try {
            const urls = await eventsApi.uploadPhotos(page.params.id ?? '', files);
            photos = [...photos, ...urls];
            toast.success($t('editEvent.photoUploaded'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function handleVideoUpload(files: File[]) {
        if (!files.length) return;
        try {
            const urls = await eventsApi.uploadVideos(page.params.id ?? '', files);
            videos = [...videos, ...urls];
            toast.success($t('editEvent.videoUploaded'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deletePhoto(path: string) {
        try {
            await eventsApi.deletePhoto(page.params.id ?? '', path);
            photos = photos.filter((p) => p !== path);
            toast.success($t('editEvent.photoDeleted'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteVideo(path: string) {
        try {
            await eventsApi.deleteVideo(page.params.id ?? '', path);
            videos = videos.filter((v) => v !== path);
            toast.success($t('editEvent.videoDeleted'));
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
            await eventsApi.update(page.params.id ?? '', {
                title,
                description,
                address,
                city,
                country,
                isPublished: isActive,
                tags: tags.map((t) => ({ name: t, category: 'event', lvl: 0 })),
                startDate: startDate || undefined
            });
            toast.success($t('editEvent.updated'));
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('editEvent.pageTitle', { title })}</title>
</svelte:head>

<div class="edit-event">
    <a href="/dashboard/events" class="back-link">
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
        {$t('editEvent.backToEvents')}
    </a>

    <h1 class="page-heading">{$t('editEvent.title')}</h1>

    <div class="form-col">
        <Input
            label={$t('editEvent.name')}
            bind:value={title}
            error={errors.title}
            onblur={validateTitle}
            oninput={() => clearError('title')}
        />
        <Select options={formatOptions} bind:value={format} label={$t('editEvent.format')} />
        <Input label={$t('createEvent.startDate')} type="datetime-local" bind:value={startDate} />
        <MarkdownEditor
            label={$t('editEvent.description')}
            bind:value={description}
            error={errors.description}
            onblur={validateDescription}
            oninput={() => clearError('description')}
        />
        <div class="form-row">
            <Input
                label={$t('editEvent.city')}
                bind:value={city}
                error={errors.city}
                onblur={validateCity}
                oninput={() => clearError('city')}
            />
            <Input label={$t('editEvent.country')} bind:value={country} />
        </div>
        {#if format !== 'Remote'}
            <Input
                label={$t('editEvent.address')}
                bind:value={address}
                error={errors.address}
                onblur={validateAddress}
                oninput={() => clearError('address')}
            />
        {/if}
        <div class="tags-section">
            <span class="field-label">{$t('editEvent.tags')}</span>
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
            <form
                class="tag-add"
                onsubmit={(e) => {
                    e.preventDefault();
                    addTag();
                }}
            >
                <Input placeholder={$t('editEvent.addTag')} bind:value={newTag} />
                <Button type="submit" variant="secondary" size="sm">+</Button>
            </form>
        </div>
        <div class="media-section">
            <span class="field-label">{$t('editEvent.media')}</span>
            <div class="media-sub">
                <span class="media-sub-label">{$t('editEvent.photos')}</span>
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
                    label={$t('editEvent.photoDrag')}
                    hint={$t('editEvent.photoFormats')}
                    onchange={(f) => handlePhotoUpload(f)}
                />
            </div>
            <div class="media-sub">
                <span class="media-sub-label">{$t('editEvent.videos')}</span>
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
                    label={$t('editEvent.videoDrag')}
                    hint={$t('editEvent.videoFormats')}
                    onchange={(f) => handleVideoUpload(f)}
                />
            </div>
        </div>
        <div class="form-actions">
            <Button size="lg" onclick={save}>{$t('editEvent.save')}</Button>
            <Button size="lg" variant="outline" href="/dashboard/events"
                >{$t('editEvent.cancel')}</Button
            >
        </div>
    </div>
</div>

<style>
    .edit-event {
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
        gap: var(--space-2);
        align-items: flex-end;
    }
    .tag-add :global(.input-group) {
        flex: 1;
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
