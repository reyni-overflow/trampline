<script lang="ts">
    import { page } from '$app/state';
    import Input from '$lib/components/ui/Input.svelte';
    import MarkdownEditor from '$lib/components/ui/MarkdownEditor.svelte';
    import Select from '$lib/components/ui/Select.svelte';
    import Tag from '$lib/components/ui/Tag.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import { toast } from '$lib/stores/toast';
    import { mentorshipsApi } from '$lib/api/mentorships';
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
    let endDate = $state('');
    let maxParticipants = $state('');
    let salaryFrom = $state('');
    let salaryTo = $state('');
    let tags = $state<string[]>([]);
    let newTag = $state('');
    let isActive = $state(true);

    onMount(async () => {
        try {
            const m = await mentorshipsApi.getById(page.params.id ?? '');
            title = m.title;
            description = m.description;
            format = m.format;
            address = m.address || '';
            city = m.city || '';
            country = m.country || '';
            tags = m.tags?.map((t) => t.name) || [];
            isActive = m.isActive;
            startDate = m.startDate ? new Date(m.startDate).toISOString().slice(0, 16) : '';
            endDate = m.endedAt ? new Date(m.endedAt).toISOString().slice(0, 16) : '';
            maxParticipants = m.maxParticipants ? String(m.maxParticipants) : '';
            salaryFrom = m.salaryFrom ? String(m.salaryFrom) : '';
            salaryTo = m.salaryTo ? String(m.salaryTo) : '';
        } catch (err) {
            handleApiError(err);
        } finally {
            _loading = false;
        }
    });

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
            const yWord =
                lastTwo >= 11 && lastTwo <= 19
                    ? $t('duration.yearsMany')
                    : lastDigit === 1
                      ? $t('duration.year')
                      : lastDigit >= 2 && lastDigit <= 4
                        ? $t('duration.yearsFew')
                        : $t('duration.yearsMany');
            parts.push(`${years} ${yWord}`);
        }
        if (months > 0) {
            const lastDigit = months % 10;
            const lastTwo = months % 100;
            const mWord =
                lastTwo >= 11 && lastTwo <= 19
                    ? $t('duration.monthsMany')
                    : lastDigit === 1
                      ? $t('duration.month')
                      : lastDigit >= 2 && lastDigit <= 4
                        ? $t('duration.monthsFew')
                        : $t('duration.monthsMany');
            parts.push(`${months} ${mWord}`);
        }
        if (days > 0 && years === 0) {
            const lastDigit = days % 10;
            const lastTwo = days % 100;
            const dWord =
                lastTwo >= 11 && lastTwo <= 19
                    ? $t('duration.daysMany')
                    : lastDigit === 1
                      ? $t('duration.day')
                      : lastDigit >= 2 && lastDigit <= 4
                        ? $t('duration.daysFew')
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

    async function save() {
        if (!validateAll()) {
            scrollToFirstError();
            return;
        }
        try {
            await mentorshipsApi.update(page.params.id ?? '', {
                title,
                description,
                address,
                city,
                country,
                isPublished: isActive,
                tags: tags.map((t) => ({ name: t, category: 'mentorship', lvl: 0 })),
                startDate: startDate || undefined,
                endedAt: endDate || undefined,
                duration: computedDuration || undefined,
                maxParticipants: maxParticipants ? parseInt(maxParticipants) : undefined,
                salaryFrom: salaryFrom ? parseFloat(salaryFrom) : undefined,
                salaryTo: salaryTo ? parseFloat(salaryTo) : undefined
            });
            toast.success($t('editMentorship.updated'));
        } catch (err) {
            handleApiError(err);
        }
    }
</script>

<svelte:head>
    <title>{$t('editMentorship.pageTitle', { title })}</title>
</svelte:head>

<div class="edit-mentorship">
    <a href="/dashboard/mentorships" class="back-link">
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
        {$t('editMentorship.backToMentorships')}
    </a>

    <h1 class="page-heading">{$t('editMentorship.title')}</h1>

    <div class="form-col">
        <Input
            label={$t('editMentorship.name')}
            bind:value={title}
            error={errors.title}
            onblur={validateTitle}
            oninput={() => clearError('title')}
        />
        <Select options={formatOptions} bind:value={format} label={$t('editMentorship.format')} />
        <MarkdownEditor
            label={$t('editMentorship.description')}
            bind:value={description}
            error={errors.description}
            onblur={validateDescription}
            oninput={() => clearError('description')}
        />
        <div class="form-row">
            <Input
                label={$t('createMentorship.startDate')}
                type="datetime-local"
                bind:value={startDate}
            />
            <Input
                label={$t('createMentorship.endDate')}
                type="datetime-local"
                bind:value={endDate}
            />
        </div>
        {#if computedDuration}
            <p class="computed-duration">
                {$t('createMentorship.computedDuration')}: {computedDuration}
            </p>
        {/if}
        <div class="form-row">
            <Input
                label={$t('createMentorship.maxParticipants')}
                type="number"
                bind:value={maxParticipants}
            />
            <div></div>
        </div>
        <div class="form-row">
            <Input
                label={$t('createMentorship.salaryFrom')}
                type="number"
                bind:value={salaryFrom}
            />
            <Input label={$t('createMentorship.salaryTo')} type="number" bind:value={salaryTo} />
        </div>
        <div class="form-row">
            <Input
                label={$t('editMentorship.city')}
                bind:value={city}
                error={errors.city}
                onblur={validateCity}
                oninput={() => clearError('city')}
            />
            <Input label={$t('editMentorship.country')} bind:value={country} />
        </div>
        {#if format !== 'Remote'}
            <Input
                label={$t('editMentorship.address')}
                bind:value={address}
                error={errors.address}
                onblur={validateAddress}
                oninput={() => clearError('address')}
            />
        {/if}
        <div class="tags-section">
            <span class="field-label">{$t('editMentorship.tags')}</span>
            <div class="tags-list">
                {#each tags as tag, _ki (tag + _ki)}
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
                <Input placeholder={$t('editMentorship.addTag')} bind:value={newTag} />
                <Button type="submit" variant="secondary" size="sm">+</Button>
            </form>
        </div>
        <div class="form-actions">
            <Button size="lg" onclick={save}>{$t('editMentorship.save')}</Button>
            <Button size="lg" variant="outline" href="/dashboard/mentorships"
                >{$t('editMentorship.cancel')}</Button
            >
        </div>
    </div>
</div>

<style>
    .edit-mentorship {
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
    .computed-duration {
        font-size: var(--font-sm);
        color: var(--accent);
        font-weight: var(--weight-medium);
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
