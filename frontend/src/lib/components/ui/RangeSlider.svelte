<script lang="ts">
    interface Props {
        min?: number;
        max?: number;
        step?: number;
        valueMin?: number;
        valueMax?: number;
        label?: string;
        formatValue?: (v: number) => string;
    }

    let {
        min = 0,
        max = 100,
        step = 1,
        valueMin = $bindable(0),
        valueMax = $bindable(100),
        label,
        formatValue = (v: number) => String(v)
    }: Props = $props();

    function handleMinInput(e: Event) {
        const v = +(e.target as HTMLInputElement).value;
        valueMin = Math.min(v, valueMax - step);
    }

    function handleMaxInput(e: Event) {
        const v = +(e.target as HTMLInputElement).value;
        valueMax = Math.max(v, valueMin + step);
    }

    let leftPercent = $derived(((valueMin - min) / (max - min)) * 100);
    let rightPercent = $derived(((valueMax - min) / (max - min)) * 100);
</script>

<div class="range-slider">
    {#if label}
        <div class="range-header">
            <span class="range-label">{label}</span>
            <span class="range-values">{formatValue(valueMin)} — {formatValue(valueMax)}</span>
        </div>
    {/if}
    <div class="range-track-wrapper">
        <div class="range-track">
            <div
                class="range-fill"
                style="left: {leftPercent}%; right: {100 - rightPercent}%"
            ></div>
        </div>
        <input
            type="range"
            class="range-input"
            {min}
            {max}
            {step}
            value={valueMin}
            oninput={handleMinInput}
        />
        <input
            type="range"
            class="range-input"
            {min}
            {max}
            {step}
            value={valueMax}
            oninput={handleMaxInput}
        />
    </div>
</div>

<style>
    .range-slider {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .range-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .range-label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }

    .range-values {
        font-size: var(--font-xs);
        font-weight: var(--weight-medium);
        color: var(--accent);
    }

    .range-track-wrapper {
        position: relative;
        height: 2rem;
    }

    .range-track {
        position: absolute;
        top: 50%;
        left: 0;
        right: 0;
        height: 0.25rem;
        background: var(--bg-tertiary);
        border-radius: 2px;
        transform: translateY(-50%);
    }

    .range-fill {
        position: absolute;
        top: 0;
        bottom: 0;
        background: var(--accent);
        border-radius: 2px;
    }

    .range-input {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        -webkit-appearance: none;
        appearance: none;
        background: transparent;
        pointer-events: none;
        margin: 0;
    }

    .range-input::-webkit-slider-thumb {
        -webkit-appearance: none;
        width: 1.125rem;
        height: 1.125rem;
        border-radius: 50%;
        background: var(--accent);
        border: 2px solid var(--bg-primary);
        box-shadow: 0 1px 4px rgba(0, 0, 0, 0.3);
        cursor: pointer;
        pointer-events: auto;
        transition: transform 0.1s ease;
    }

    .range-input::-webkit-slider-thumb:hover {
        transform: scale(1.2);
    }

    .range-input::-moz-range-thumb {
        width: 1.125rem;
        height: 1.125rem;
        border-radius: 50%;
        background: var(--accent);
        border: 2px solid var(--bg-primary);
        box-shadow: 0 1px 4px rgba(0, 0, 0, 0.3);
        cursor: pointer;
        pointer-events: auto;
    }
</style>
