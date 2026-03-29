<script lang="ts">
    interface Props {
        src?: string | null;
        alt?: string;
        name?: string;
        size?: number;
        clickable?: boolean;
    }

    let { src, alt = '', name = '', size = 40, clickable = false }: Props = $props();

    let initials = $derived(
        name
            .split(' ')
            .slice(0, 2)
            .map((w) => w.charAt(0).toUpperCase())
            .join('')
    );

    let fontSize = $derived(
        size <= 32 ? 'var(--font-xs)' : size <= 48 ? 'var(--font-sm)' : 'var(--font-md)'
    );
</script>

{#if clickable}
    <button
        class="avatar clickable"
        style="width: {size}px; height: {size}px; font-size: {fontSize}"
        type="button"
        aria-label={alt || name}
    >
        {#if src}
            <img {src} alt={alt || name} />
        {:else}
            <span class="initials">{initials || '?'}</span>
        {/if}
    </button>
{:else}
    <span
        class="avatar"
        style="width: {size}px; height: {size}px; font-size: {fontSize}"
        role="img"
        aria-label={alt || name}
    >
        {#if src}
            <img {src} alt={alt || name} />
        {:else}
            <span class="initials">{initials || '?'}</span>
        {/if}
    </span>
{/if}

<style>
    .avatar {
        position: relative;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        overflow: hidden;
        background: var(--accent-subtle);
        color: var(--accent);
        font-weight: var(--weight-semibold);
        flex-shrink: 0;
        border: none;
        padding: 0;
        transition: box-shadow var(--duration-normal) var(--ease-in-out);
    }

    .avatar.clickable {
        cursor: pointer;
    }

    .avatar.clickable:hover {
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }

    .avatar img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }
</style>
