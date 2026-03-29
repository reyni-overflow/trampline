<script lang="ts">
    import Input from '$lib/components/ui/Input.svelte';
    import MarkdownEditor from '$lib/components/ui/MarkdownEditor.svelte';
    import MarkdownRenderer from '$lib/components/ui/MarkdownRenderer.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import { toast } from '$lib/stores/toast';
    import { formatSalary, jobTypeLabel, workFormatLabel } from '$lib/utils/format';
    import { jobsApi } from '$lib/api/jobs';
    import { handleApiError } from '$lib/api/client';
    import { user as userStore } from '$lib/stores/auth';
    import { t, tGet } from '$lib/i18n';
    import { required, minLength, validate } from '$lib/utils/validation';
    import { onMount, onDestroy } from 'svelte';

    let isTrusted = $state(false);
    const unsub = userStore.subscribe((v) => { isTrusted = (v?.employeeProfile?.verificationLevel ?? 0) >= 2; });
    onDestroy(unsub);

    let title = $state('');
    let description = $state('');
    let type = $state('Work');
    let format = $state('Remote');
    let address = $state('');
    let city = $state('');
    let country = $state(tGet('geo.defaultCountry'));
    let salaryFrom = $state('');
    let salaryTo = $state('');
    let tags = $state<string[]>([]);
    let newTag = $state('');
    let allTags = $state<string[]>([]);
    let suggestions = $state<string[]>([]);

    onMount(async () => {
        try {
            const res = await jobsApi.getTags();
            allTags = res.map(t => t.name);
        } catch { /* tags are optional, UI still works */ }
    });

    function onTagInput() {
        const q = newTag.trim().toLowerCase();
        if (!q) {
            suggestions = [];
            return;
        }
        suggestions = allTags
            .filter(t => t.toLowerCase().includes(q) && !tags.includes(t))
            .slice(0, 5);
    }

    function selectSuggestion(tag: string) {
        if (!tags.includes(tag)) tags = [...tags, tag];
        newTag = '';
        suggestions = [];
    }

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
    }

    function removeTag(tag: string) {
        tags = tags.filter((t) => t !== tag);
    }

    let submitting = $state(false);

    async function publish() {
        if (!validateAll()) {
            scrollToFirstError();
            return;
        }
        submitting = true;
        try {
            await jobsApi.create({
                title,
                description,
                type: type as 'Work' | 'Internship' | 'Mentorship' | 'Event',
                format: format as 'Remote' | 'Hybrid' | 'Office',
                address: [address, city, country].filter(Boolean).join(', '),
                salaryFrom: salaryFrom ? +salaryFrom : undefined,
                salaryTo: salaryTo ? +salaryTo : undefined,
                tags: tags.map((t) => ({ name: t, category: 'tech', lvl: 0 }))
            });
            toast.success(isTrusted ? $t('createJob.published') : $t('createJob.sentToModeration'));
            window.location.href = '/dashboard/jobs';
        } catch (err) {
            handleApiError(err);
        } finally {
            submitting = false;
        }
    }

    async function saveDraft() {
        if (!title) {
            toast.error($t('createJob.titleRequired'));
            return;
        }
        submitting = true;
        try {
            await jobsApi.create({
                title,
                description: description || title,
                type: type as 'Work' | 'Internship' | 'Mentorship' | 'Event',
                format: format as 'Remote' | 'Hybrid' | 'Office',
                address: [address, city, country].filter(Boolean).join(', ') || tGet('geo.notSpecified'),
                salaryFrom: salaryFrom ? +salaryFrom : undefined,
                salaryTo: salaryTo ? +salaryTo : undefined,
                tags: tags.map((t) => ({ name: t, category: 'tech', lvl: 0 }))
            });
            toast.success($t('createJob.draftSaved'));
            window.location.href = '/dashboard/jobs';
        } catch (err) {
            handleApiError(err);
        } finally {
            submitting = false;
        }
    }
</script>

<svelte:head>
    <title>{$t('createJob.pageTitle')}</title>
</svelte:head>

<div class="create-job">
    <a href="/dashboard/jobs" class="back-link">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
        {$t('createJob.backToJobs')}
    </a>

    <h1 class="page-heading">{$t('createJob.title')}</h1>

    <div class="create-layout">
        <div class="form-col">
            <section class="form-section">
                <Input label={$t('createJob.nameLabel')} bind:value={title} placeholder={$t('createJob.namePlaceholder')} error={errors.title} onblur={validateTitle} oninput={() => clearError('title')} />
            </section>

            <section class="form-section">
                <div class="form-row">
                    <Select options={typeOptions} bind:value={type} label={$t('createJob.type')} />
                    <Select options={formatOptions} bind:value={format} label={$t('createJob.format')} />
                </div>
            </section>

            <section class="form-section">
                <MarkdownEditor label={$t('createJob.descLabel')} bind:value={description} placeholder={$t('createJob.descPlaceholder')} error={errors.description} onblur={validateDescription} oninput={() => clearError('description')} />
            </section>

            <section class="form-section">
                <div class="form-row">
                    <Input label={$t('createJob.city')} bind:value={city} placeholder={$t('createJob.cityPlaceholder')} error={errors.city} onblur={validateCity} oninput={() => clearError('city')} />
                    <Input label={$t('createJob.country')} bind:value={country} placeholder={$t('createJob.countryPlaceholder')} />
                </div>
                {#if format !== 'Remote'}
                    <Input label={$t('createJob.address')} bind:value={address} placeholder={$t('createJob.addressPlaceholder')} error={errors.address} onblur={validateAddress} oninput={() => clearError('address')} />
                {/if}
            </section>

            <section class="form-section">
                <div class="form-row">
                    <Input label={$t('createJob.salaryFrom')} type="number" bind:value={salaryFrom} placeholder={$t('createJob.salaryFromPlaceholder')} error={errors.salaryFrom} onblur={validateSalary} oninput={() => clearError('salaryFrom')} />
                    <Input label={$t('createJob.salaryTo')} type="number" bind:value={salaryTo} placeholder={$t('createJob.salaryToPlaceholder')} onblur={validateSalary} oninput={() => clearError('salaryFrom')} />
                </div>
            </section>

            <section class="form-section">
                <span class="field-label">{$t('createJob.tags')}</span>
                <div class="tags-edit">
                    {#if tags.length > 0}
                        <div class="tags-list">
                            {#each tags as tag (tag)}
                                <Tag removable onremove={() => removeTag(tag)}>{tag}</Tag>
                            {/each}
                        </div>
                    {/if}
                    <form class="tag-add" onsubmit={(e) => { e.preventDefault(); addTag(); suggestions = []; }}>
                        <div class="tag-input-wrap">
                            <Input placeholder={$t('createJob.tagsPlaceholder')} bind:value={newTag} oninput={onTagInput} />
                            <button class="tag-add-btn" type="submit" aria-label={$t('common.add')}>
                                <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                            </button>
                            {#if suggestions.length > 0}
                                <div class="tag-suggestions">
                                    {#each suggestions as s (s)}
                                        <button type="button" class="tag-suggestion-item" onclick={() => selectSuggestion(s)}>{s}</button>
                                    {/each}
                                </div>
                            {/if}
                        </div>
                    </form>
                </div>
            </section>

            <div class="form-actions">
                <Button size="lg" onclick={publish} disabled={submitting}>{submitting ? $t('common.loading') : $t('createJob.publish')}</Button>
                <Button size="lg" variant="outline" onclick={saveDraft} disabled={submitting}>{$t('createJob.saveDraft')}</Button>
            </div>
        </div>

        <aside class="preview-col">
            <h3 class="preview-heading">{$t('createJob.preview')}</h3>
            <div class="preview-card">
                <h4 class="preview-title">{title || $t('createJob.positionName')}</h4>
                <div class="preview-meta">
                    <Badge>{jobTypeLabel(type)}</Badge>
                    <Badge variant={format === 'Remote' ? 'success' : format === 'Office' ? 'warning' : 'default'}>{workFormatLabel(format)}</Badge>
                </div>
                <p class="preview-location">{city || $t('createJob.cityPlaceholder')}{address ? `, ${address}` : ''}</p>
                <p class="preview-salary">{formatSalary(salaryFrom ? +salaryFrom : null, salaryTo ? +salaryTo : null)}</p>
                {#if tags.length > 0}
                    <div class="preview-tags">
                        {#each tags as tag (tag)}
                            <Tag>{tag}</Tag>
                        {/each}
                    </div>
                {/if}
                {#if description}
                    <div class="preview-desc">
                        <MarkdownRenderer source={description.slice(0, 300)} />
                    </div>
                {/if}
            </div>
        </aside>
    </div>
</div>

<style>
    .create-job { display: flex; flex-direction: column; gap: var(--space-6); }
    .back-link { display: inline-flex; align-items: center; gap: var(--space-2); font-size: var(--font-sm); color: var(--text-secondary); transition: var(--transition-colors); }
    .back-link:hover { color: var(--accent); }
    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }

    .create-layout { display: grid; grid-template-columns: 1fr 20rem; gap: var(--space-8); align-items: start; }
    .form-col { display: flex; flex-direction: column; gap: var(--space-6); }
    .form-section { display: flex; flex-direction: column; gap: var(--space-3); }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-4); }
    .field-label { font-size: var(--font-sm); font-weight: var(--weight-medium); color: var(--text-secondary); }

    .tags-edit { display: flex; flex-direction: column; gap: var(--space-3); }
    .tags-list { display: flex; flex-wrap: wrap; gap: var(--space-2); }
    .tag-add { display: flex; }
    .tag-input-wrap { position: relative; flex: 1; }
    .tag-input-wrap :global(.input-group) { width: 100%; }
    .tag-input-wrap :global(.input) { padding-right: 2.75rem; }
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
    .tag-add-btn:hover { color: var(--accent); background: var(--accent-subtle); }
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
    .tag-suggestion-item:hover { background: var(--bg-tertiary); }

    .form-actions { display: flex; gap: var(--space-3); padding-top: var(--space-4); border-top: 1px solid var(--border-default); }

    .preview-col { position: sticky; top: calc(var(--header-height) + var(--space-4)); }
    .preview-heading { font-size: var(--font-sm); font-weight: var(--weight-semibold); color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: var(--space-3); }

    .preview-card {
        display: flex; flex-direction: column; gap: var(--space-3);
        padding: var(--space-5);
        background: var(--bg-secondary); border: 1px solid var(--border-default); border-radius: var(--radius-lg);
    }
    .preview-title { font-size: var(--font-base); font-weight: var(--weight-semibold); }
    .preview-meta { display: flex; gap: var(--space-1); }
    .preview-location { font-size: var(--font-sm); color: var(--text-secondary); }
    .preview-salary { font-size: var(--font-sm); font-weight: var(--weight-semibold); color: var(--accent); }
    .preview-tags { display: flex; flex-wrap: wrap; gap: var(--space-1); }
    .preview-desc { font-size: var(--font-xs); color: var(--text-tertiary); line-height: var(--leading-normal); }

    @media (max-width: 1024px) { .create-layout { grid-template-columns: 1fr; } .preview-col { position: static; } }
    @media (max-width: 640px) { .form-row { grid-template-columns: 1fr; } }
</style>
