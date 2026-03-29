<script lang="ts">
    import Avatar from './Avatar.svelte';
    import Badge from './Badge.svelte';
    import { t, pluralForm } from '$lib/i18n';

    interface Props {
        id: string;
        name: string;
        activity: string;
        isVerified?: boolean;
        verificationLevel?: number;
        jobCount?: number;
        link?: string | null;
    }

    let {
        id,
        name,
        activity,
        isVerified = false,
        verificationLevel = 0,
        jobCount = 0,
        link
    }: Props = $props();
</script>

<a href="/companies/{id}" class="company-card">
    <div class="card-top">
        <Avatar {name} size={48} />
        {#if verificationLevel >= 2}
            <Badge variant="info" size="sm">
                <svg
                    viewBox="0 0 24 24"
                    width="12"
                    height="12"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2.5"
                    stroke-linecap="round"
                    stroke-linejoin="round"><polyline points="20 6 9 17 4 12" /></svg
                >
            </Badge>
        {:else if isVerified}
            <Badge variant="success" size="sm">
                <svg
                    viewBox="0 0 24 24"
                    width="12"
                    height="12"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2.5"
                    stroke-linecap="round"
                    stroke-linejoin="round"><polyline points="20 6 9 17 4 12" /></svg
                >
            </Badge>
        {/if}
    </div>
    <h3 class="card-name">{name}</h3>
    <p class="card-activity">{activity}</p>
    <div class="card-footer">
        <span class="card-jobs"
            >{jobCount}
            {pluralForm(
                jobCount,
                $t('plural.vacancyOne'),
                $t('plural.vacancyFew'),
                $t('plural.vacancyMany')
            )}</span
        >
        {#if link}
            <span class="card-link">{link.replace(/^https?:\/\//, '')}</span>
        {/if}
    </div>
</a>

<style>
    .company-card {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        text-decoration: none;
        color: inherit;
        transition: var(--transition-colors), var(--transition-transform), var(--transition-shadow);
    }

    .company-card:hover {
        border-color: var(--border-hover);
        box-shadow: var(--shadow-md);
        transform: translateY(-0.125rem);
        color: inherit;
    }

    .card-top {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
    }

    .card-name {
        font-size: var(--font-base);
        font-weight: var(--weight-semibold);
    }

    .card-activity {
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .card-footer {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-top: auto;
        padding-top: var(--space-2);
        border-top: 1px solid var(--border-default);
    }

    .card-jobs {
        font-size: var(--font-xs);
        color: var(--accent);
        font-weight: var(--weight-medium);
    }

    .card-link {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        max-width: 10rem;
    }
</style>
