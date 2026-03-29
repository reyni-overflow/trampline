<script lang="ts">
    import { comparison, comparisonCount, type ComparisonJob } from '$lib/stores/comparison';
    import Badge from './Badge.svelte';
    import Button from './Button.svelte';
    import { t } from '$lib/i18n';
    import { onDestroy } from 'svelte';

    let items = $state<ComparisonJob[]>([]);
    let count = $state(0);
    let expanded = $state(false);

    const unsubItems = comparison.subscribe(v => items = v);
    const unsubCount = comparisonCount.subscribe(v => count = v);

    onDestroy(() => { unsubItems(); unsubCount(); });
</script>

{#if count > 0}
    <div class="comparison-bar" class:expanded>
        <div class="bar-header" >
            <button class="bar-toggle" type="button" onclick={() => expanded = !expanded}>
                <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/>
                </svg>
                {$t('comparison.title')}
                <Badge variant="accent" size="sm">{count}</Badge>
            </button>
            <div class="bar-actions">
                <Button size="sm" variant="ghost" onclick={() => comparison.clear()}>{$t('comparison.clear')}</Button>
            </div>
        </div>

        {#if expanded}
            <div class="comparison-table" style="--comp-cols: {items.length}">
                <div class="comp-row comp-header">
                    <span class="comp-label"></span>
                    {#each items as job (job.id)}
                        <div class="comp-cell comp-job-header">
                            <strong>{job.title}</strong>
                            <button class="comp-remove" type="button" onclick={() => comparison.remove(job.id)} aria-label={$t('comparison.remove')}>
                                <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                            </button>
                        </div>
                    {/each}
                </div>
                {#each [
                    ['comparison.company', 'company'],
                    ['comparison.salary', 'salary'],
                    ['comparison.format', 'format'],
                    ['comparison.type', 'type'],
                    ['comparison.location', 'city']
                ] as [labelKey, field] (field)}
                    <div class="comp-row">
                        <span class="comp-label">{$t(labelKey)}</span>
                        {#each items as job (job.id)}
                            <span class="comp-cell">{(job as unknown as Record<string, unknown>)[field] || '—'}</span>
                        {/each}
                    </div>
                {/each}
                <div class="comp-row">
                    <span class="comp-label">{$t('comparison.tags')}</span>
                    {#each items as job (job.id)}
                        <span class="comp-cell comp-tags">{job.tags?.join(', ') || '—'}</span>
                    {/each}
                </div>
            </div>
        {/if}
    </div>
{/if}

<style>
    .comparison-bar {
        position: fixed;
        bottom: 0;
        left: 0;
        right: 0;
        z-index: var(--z-overlay);
        background: var(--bg-elevated);
        border-top: 1px solid var(--border-default);
        box-shadow: 0 -4px 20px rgba(0,0,0,0.15);
        animation: slide-up var(--duration-fast) var(--ease-out);
    }

    .bar-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--space-3) var(--space-5);
    }

    .bar-toggle {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
        color: var(--text-primary);
        cursor: pointer;
    }

    .bar-actions { display: flex; gap: var(--space-2); }

    .comparison-table {
        padding: 0 var(--space-5) var(--space-5);
        overflow-x: auto;
    }

    .comp-row {
        display: grid;
        grid-template-columns: 7rem repeat(var(--comp-cols, 3), 1fr);
        gap: var(--space-3);
        padding: var(--space-2) 0;
        border-bottom: 1px solid var(--border-default);
        align-items: center;
    }

    .comp-row.comp-header { border-bottom: 2px solid var(--border-default); }

    .comp-label {
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        color: var(--text-tertiary);
    }

    .comp-cell {
        font-size: var(--font-sm);
        color: var(--text-primary);
    }

    .comp-job-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-2);
    }

    .comp-job-header strong { font-size: var(--font-sm); }

    .comp-remove {
        display: flex;
        color: var(--text-tertiary);
        padding: 2px;
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
    }
    .comp-remove:hover { color: var(--color-error); }

    .comp-tags {
        font-size: var(--font-xs);
        color: var(--text-secondary);
        line-height: var(--leading-relaxed);
    }

    @media (max-width: 640px) {
        .comp-row { grid-template-columns: 5rem repeat(var(--comp-cols, 3), 1fr); gap: var(--space-2); }
    }
</style>
