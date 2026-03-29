<script lang="ts">
    interface Tab {
        id: string;
        label: string;
    }

    interface Props {
        tabs: Tab[];
        active?: string;
        onchange?: (id: string) => void;
    }

    let { tabs, active = $bindable(''), onchange }: Props = $props();

    $effect(() => {
        if (!active && tabs.length > 0) active = tabs[0].id;
    });

    function select(id: string) {
        active = id;
        onchange?.(id);
    }

    let indicatorStyle = $state('');
    let tabsEl: HTMLElement;

    $effect(() => {
        if (!tabsEl) return;
        const activeBtn = tabsEl.querySelector(`[data-tab-id="${active}"]`) as HTMLElement;
        if (activeBtn) {
            indicatorStyle = `left: ${activeBtn.offsetLeft}px; width: ${activeBtn.offsetWidth}px`;
        }
    });
</script>

<div class="tabs" bind:this={tabsEl} role="tablist">
    {#each tabs as tab (tab.id)}
        <button
            class="tab"
            class:active={tab.id === active}
            data-tab-id={tab.id}
            role="tab"
            aria-selected={tab.id === active}
            onclick={() => select(tab.id)}
            type="button"
        >
            {tab.label}
        </button>
    {/each}
    <span class="indicator" style={indicatorStyle}></span>
</div>

<style>
    .tabs {
        position: relative;
        display: flex;
        gap: var(--space-1);
        border-bottom: 1px solid var(--border-default);
    }

    .tab {
        position: relative;
        padding: var(--space-3) var(--space-4);
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
        transition: var(--transition-colors);
        cursor: pointer;
        white-space: nowrap;
    }

    .tab:hover {
        color: var(--text-primary);
    }

    .tab.active {
        color: var(--accent);
    }

    .indicator {
        position: absolute;
        bottom: -1px;
        height: 2px;
        background: var(--accent);
        border-radius: 1px;
        transition: left var(--duration-moderate) var(--ease-spring),
            width var(--duration-moderate) var(--ease-spring);
    }
</style>
