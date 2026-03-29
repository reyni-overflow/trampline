<script lang="ts">
    import Input from '$lib/components/ui/Input.svelte';
    import MarkdownEditor from '$lib/components/ui/MarkdownEditor.svelte';
    import MarkdownRenderer from '$lib/components/ui/MarkdownRenderer.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import { toast } from '$lib/stores/toast';
    import { workFormatLabel } from '$lib/utils/format';
    import { mentorshipsApi } from '$lib/api/mentorships';
    import { handleApiError } from '$lib/api/client';
    import { t, tGet } from '$lib/i18n';
    import { required, minLength, validate } from '$lib/utils/validation';

    let title = $state('');
    let description = $state('');
    let format = $state('Remote');
    let address = $state('');
    let city = $state('');
    let country = $state(tGet('geo.defaultCountry'));
    let startDate = $state('');
    let endDate = $state('');
    let maxParticipants = $state('');
    let salaryFrom = $state('');
    let salaryTo = $state('');
    let tags = $state<string[]>([]);
    let newTag = $state('');

    function computeDuration(start: string, end: string): string {
        if (!start || !end) return '';
        const s = new Date(start);
        const e = new Date(end);
        if (isNaN(s.getTime()) || isNaN(e.getTime()) || e <= s) return '';
        let years = e.getFullYear() - s.getFullYear();
        let months = e.getMonth() - s.getMonth();
        let days = e.getDate() - s.getDate();
        if (days < 0) {
            months--;
            const prev = new Date(e.getFullYear(), e.getMonth(), 0);
            days += prev.getDate();
        }
        if (months < 0) {
            years--;
            months += 12;
        }
        const parts: string[] = [];
        if (years > 0) {
            const lastDigit = years % 10;
            const lastTwo = years % 100;
            const yWord = (lastTwo >= 11 && lastTwo <= 19) ? $t('duration.yearsMany')
                : lastDigit === 1 ? $t('duration.year')
                : (lastDigit >= 2 && lastDigit <= 4) ? $t('duration.yearsFew')
                : $t('duration.yearsMany');
            parts.push(`${years} ${yWord}`);
        }
        if (months > 0) {
            const lastDigit = months % 10;
            const lastTwo = months % 100;
            const mWord = (lastTwo >= 11 && lastTwo <= 19) ? $t('duration.monthsMany')
                : lastDigit === 1 ? $t('duration.month')
                : (lastDigit >= 2 && lastDigit <= 4) ? $t('duration.monthsFew')
                : $t('duration.monthsMany');
            parts.push(`${months} ${mWord}`);
        }
        if (days > 0 && years === 0) {
            const lastDigit = days % 10;
            const lastTwo = days % 100;
            const dWord = (lastTwo >= 11 && lastTwo <= 19) ? $t('duration.daysMany')
                : lastDigit === 1 ? $t('duration.day')
                : (lastDigit >= 2 && lastDigit <= 4) ? $t('duration.daysFew')
                : $t('duration.daysMany');
            parts.push(`${days} ${dWord}`);
        }
        return parts.join(' ') || '';
    }

    let computedDuration = $derived(computeDuration(startDate, endDate));

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
            await mentorshipsApi.create({
                title,
                description,
                format: format as 'Remote' | 'Hybrid' | 'Office',
                address: [address, city, country].filter(Boolean).join(', '),
                tags: tags.map((t) => ({ name: t, category: 'mentorship', lvl: 0 })),
                startDate: startDate || undefined,
                endedAt: endDate || undefined,
                duration: computedDuration || undefined,
                maxParticipants: maxParticipants ? parseInt(maxParticipants) : undefined,
                salaryFrom: salaryFrom ? parseFloat(salaryFrom) : undefined,
                salaryTo: salaryTo ? parseFloat(salaryTo) : undefined
            });
            toast.success($t('createMentorship.published'));
            window.location.href = '/dashboard/mentorships';
        } catch (err) {
            handleApiError(err);
        } finally {
            submitting = false;
        }
    }

    async function saveDraft() {
        if (!title) {
            toast.error($t('createMentorship.titleRequired'));
            return;
        }
        submitting = true;
        try {
            await mentorshipsApi.create({
                title,
                description: description || title,
                format: format as 'Remote' | 'Hybrid' | 'Office',
                address: [address, city, country].filter(Boolean).join(', ') || tGet('geo.notSpecified'),
                tags: tags.map((t) => ({ name: t, category: 'mentorship', lvl: 0 })),
                startDate: startDate || undefined,
                endedAt: endDate || undefined,
                duration: computedDuration || undefined,
                maxParticipants: maxParticipants ? parseInt(maxParticipants) : undefined
            });
            toast.success($t('createMentorship.draftSaved'));
            window.location.href = '/dashboard/mentorships';
        } catch (err) {
            handleApiError(err);
        } finally {
            submitting = false;
        }
    }
</script>

<svelte:head>
    <title>{$t('createMentorship.pageTitle')}</title>
</svelte:head>

<div class="create-mentorship">
    <a href="/dashboard/mentorships" class="back-link">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
        {$t('createMentorship.backToMentorships')}
    </a>

    <h1 class="page-heading">{$t('createMentorship.title')}</h1>

    <div class="create-layout">
        <div class="form-col">
            <section class="form-section">
                <Input label={$t('createMentorship.nameLabel')} bind:value={title} placeholder={$t('createMentorship.namePlaceholder')} error={errors.title} onblur={validateTitle} oninput={() => clearError('title')} />
            </section>

            <section class="form-section">
                <Select options={formatOptions} bind:value={format} label={$t('createMentorship.format')} />
            </section>

            <section class="form-section">
                <MarkdownEditor label={$t('createMentorship.descLabel')} bind:value={description} placeholder={$t('createMentorship.descPlaceholder')} error={errors.description} onblur={validateDescription} oninput={() => clearError('description')} />
            </section>

            <section class="form-section">
                <div class="form-row">
                    <Input label={$t('createMentorship.startDate')} type="datetime-local" bind:value={startDate} />
                    <Input label={$t('createMentorship.endDate')} type="datetime-local" bind:value={endDate} />
                </div>
                {#if computedDuration}
                    <p class="computed-duration">{$t('createMentorship.computedDuration')}: {computedDuration}</p>
                {/if}
                <Input label={$t('createMentorship.maxParticipants')} type="number" bind:value={maxParticipants} placeholder={$t('createMentorship.maxParticipantsPlaceholder')} />
            </section>

            <section class="form-section">
                <div class="form-row">
                    <Input label={$t('createMentorship.salaryFrom')} type="number" bind:value={salaryFrom} placeholder="0" />
                    <Input label={$t('createMentorship.salaryTo')} type="number" bind:value={salaryTo} placeholder="0" />
                </div>
            </section>

            <section class="form-section">
                <div class="form-row">
                    <Input label={$t('createMentorship.city')} bind:value={city} placeholder={$t('createMentorship.cityPlaceholder')} error={errors.city} onblur={validateCity} oninput={() => clearError('city')} />
                    <Input label={$t('createMentorship.country')} bind:value={country} placeholder={$t('createMentorship.countryPlaceholder')} />
                </div>
                {#if format !== 'Remote'}
                    <Input label={$t('createMentorship.address')} bind:value={address} placeholder={$t('createMentorship.addressPlaceholder')} error={errors.address} onblur={validateAddress} oninput={() => clearError('address')} />
                {/if}
            </section>

            <section class="form-section">
                <span class="field-label">{$t('createMentorship.tags')}</span>
                <div class="tags-edit">
                    <div class="tags-list">
                        {#each tags as tag (tag)}
                            <Tag removable onremove={() => removeTag(tag)}>{tag}</Tag>
                        {/each}
                    </div>
                    <form class="tag-add" onsubmit={(e) => { e.preventDefault(); addTag(); }}>
                        <div class="tag-input-wrap">
                            <Input placeholder={$t('createMentorship.tagsPlaceholder')} bind:value={newTag} />
                            <button class="tag-add-btn" type="submit" aria-label={$t('common.add')}>
                                <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                            </button>
                        </div>
                    </form>
                </div>
            </section>

            <div class="form-actions">
                <Button size="lg" onclick={publish} disabled={submitting}>{submitting ? $t('common.loading') : $t('createMentorship.publish')}</Button>
                <Button size="lg" variant="outline" onclick={saveDraft} disabled={submitting}>{$t('createMentorship.saveDraft')}</Button>
            </div>
        </div>

        <aside class="preview-col">
            <h3 class="preview-heading">{$t('createMentorship.preview')}</h3>
            <div class="preview-card">
                <h4 class="preview-title">{title || $t('createMentorship.mentorshipName')}</h4>
                <div class="preview-meta">
                    <Badge variant={format === 'Remote' ? 'success' : format === 'Office' ? 'warning' : 'default'}>{workFormatLabel(format)}</Badge>
                    <Badge variant="info">{$t('createMentorship.typeBadge')}</Badge>
                </div>
                {#if computedDuration}
                    <p class="preview-duration">{$t('createMentorship.durationLabel')}: {computedDuration}</p>
                {/if}
                {#if maxParticipants}
                    <p class="preview-participants">{$t('createMentorship.participantsLabel')}: {maxParticipants}</p>
                {/if}
                {#if startDate}
                    <p class="preview-date">{new Date(startDate).toLocaleString()}</p>
                {/if}
                <p class="preview-location">{city || $t('createMentorship.cityPlaceholder')}{address ? `, ${address}` : ''}</p>
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
    .create-mentorship { display: flex; flex-direction: column; gap: var(--space-6); }
    .back-link { display: inline-flex; align-items: center; gap: var(--space-2); font-size: var(--font-sm); color: var(--text-secondary); transition: var(--transition-colors); }
    .back-link:hover { color: var(--accent); }
    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }

    .create-layout { display: grid; grid-template-columns: 1fr 20rem; gap: var(--space-8); align-items: start; }
    .form-col { display: flex; flex-direction: column; gap: var(--space-6); }
    .form-section { display: flex; flex-direction: column; gap: var(--space-3); }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-4); }
    .field-label { font-size: var(--font-sm); font-weight: var(--weight-medium); color: var(--text-secondary); }
    .computed-duration { font-size: var(--font-sm); color: var(--accent); font-weight: var(--weight-medium); }

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
    .preview-date { font-size: var(--font-xs); color: var(--accent); font-weight: var(--weight-medium); }
    .preview-duration { font-size: var(--font-sm); color: var(--text-secondary); }
    .preview-participants { font-size: var(--font-sm); color: var(--text-secondary); }
    .preview-tags { display: flex; flex-wrap: wrap; gap: var(--space-1); }
    .preview-desc { font-size: var(--font-xs); color: var(--text-tertiary); line-height: var(--leading-normal); }

    @media (max-width: 1024px) { .create-layout { grid-template-columns: 1fr; } .preview-col { position: static; } }
    @media (max-width: 640px) { .form-row { grid-template-columns: 1fr; } }
</style>
