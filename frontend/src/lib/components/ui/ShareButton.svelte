<script lang="ts">
    import Dropdown from './Dropdown.svelte';
    import { toast } from '$lib/stores/toast';
    import { t } from '$lib/i18n';

    interface Props {
        title: string;
        text?: string;
        url?: string;
    }

    let { title, text, url }: Props = $props();

    let dropdownOpen = $state(false);

    let canShare = $derived(typeof navigator !== 'undefined' && 'share' in navigator);

    function getUrl() {
        return url ?? (typeof window !== 'undefined' ? window.location.href : '');
    }

    async function handleClick() {
        if (canShare) {
            try {
                await navigator.share({ title, text: text ?? title, url: getUrl() });
            } catch (e: unknown) {
                if ((e as Error)?.name !== 'AbortError') {
                    await copyLink();
                }
            }
        }
    }

    async function copyLink() {
        try {
            await navigator.clipboard.writeText(getUrl());
            toast.success($t('share.copied'));
        } catch {
            toast.error($t('api.error'));
        }
        dropdownOpen = false;
    }

    function openTelegram() {
        const u = encodeURIComponent(getUrl());
        const t = encodeURIComponent(title);
        window.open(`https://t.me/share/url?url=${u}&text=${t}`, '_blank');
        dropdownOpen = false;
    }

    function openVK() {
        const u = encodeURIComponent(getUrl());
        const t = encodeURIComponent(title);
        window.open(`https://vk.com/share.php?url=${u}&title=${t}`, '_blank');
        dropdownOpen = false;
    }
</script>

{#if canShare}
    <button class="share-btn" type="button" onclick={handleClick} title={$t('share.button')}>
        <svg
            viewBox="0 0 24 24"
            width="18"
            height="18"
            fill="none"
            stroke="currentColor"
            stroke-width="1.75"
            stroke-linecap="round"
            stroke-linejoin="round"
        >
            <path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8" />
            <polyline points="16 6 12 2 8 6" />
            <line x1="12" y1="2" x2="12" y2="15" />
        </svg>
    </button>
{:else}
    <Dropdown bind:open={dropdownOpen} align="right">
        {#snippet trigger()}
            <button class="share-btn" type="button" title={$t('share.button')}>
                <svg
                    viewBox="0 0 24 24"
                    width="18"
                    height="18"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.75"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                >
                    <path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8" />
                    <polyline points="16 6 12 2 8 6" />
                    <line x1="12" y1="2" x2="12" y2="15" />
                </svg>
            </button>
        {/snippet}
        <button class="dropdown-item" type="button" onclick={copyLink}>
            <svg
                viewBox="0 0 24 24"
                width="16"
                height="16"
                fill="none"
                stroke="currentColor"
                stroke-width="1.75"
                stroke-linecap="round"
                stroke-linejoin="round"
            >
                <rect width="14" height="14" x="8" y="8" rx="2" />
                <path d="M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2" />
            </svg>
            {$t('share.copyLink')}
        </button>
        <button class="dropdown-item" type="button" onclick={openTelegram}>
            <svg viewBox="0 0 24 24" width="16" height="16" fill="currentColor">
                <path
                    d="M11.944 0A12 12 0 0 0 0 12a12 12 0 0 0 12 12 12 12 0 0 0 12-12A12 12 0 0 0 12 0h-.056zm4.962 7.224c.1-.002.321.023.465.14a.506.506 0 0 1 .171.325c.016.093.036.306.02.472-.18 1.898-.962 6.502-1.36 8.627-.168.9-.499 1.201-.82 1.23-.696.065-1.225-.46-1.9-.902-1.056-.693-1.653-1.124-2.678-1.8-1.185-.78-.417-1.21.258-1.91.177-.184 3.247-2.977 3.307-3.23.007-.032.014-.15-.056-.212s-.174-.041-.249-.024c-.106.024-1.793 1.14-5.061 3.345-.479.33-.913.49-1.302.48-.428-.008-1.252-.241-1.865-.44-.752-.245-1.349-.374-1.297-.789.027-.216.325-.437.893-.663 3.498-1.524 5.83-2.529 6.998-3.014 3.332-1.386 4.025-1.627 4.476-1.635z"
                />
            </svg>
            {$t('share.telegram')}
        </button>
        <button class="dropdown-item" type="button" onclick={openVK}>
            <svg viewBox="0 0 24 24" width="16" height="16" fill="currentColor">
                <path
                    d="M15.684 0H8.316C1.592 0 0 1.592 0 8.316v7.368C0 22.408 1.592 24 8.316 24h7.368C22.408 24 24 22.408 24 15.684V8.316C24 1.592 22.408 0 15.684 0zm3.692 17.123h-1.744c-.66 0-.862-.524-2.049-1.714-1.033-1.005-1.49-1.135-1.744-1.135-.356 0-.458.102-.458.593v1.563c0 .426-.136.682-1.253.682-1.846 0-3.896-1.12-5.339-3.202-2.168-3.048-2.763-5.339-2.763-5.805 0-.254.102-.491.593-.491h1.744c.44 0 .61.203.78.678.861 2.49 2.303 4.675 2.896 4.675.22 0 .322-.102.322-.66V9.721c-.068-1.186-.695-1.287-.695-1.71 0-.204.17-.407.44-.407h2.744c.373 0 .508.203.508.643v3.473c0 .372.17.508.271.508.22 0 .407-.136.813-.542 1.253-1.404 2.151-3.574 2.151-3.574.119-.254.322-.491.763-.491h1.744c.525 0 .644.27.525.643-.22 1.017-2.354 4.031-2.354 4.031-.186.305-.254.44 0 .78.186.254.796.78 1.202 1.253.746.864 1.32 1.592 1.473 2.084.17.491-.085.744-.576.744z"
                />
            </svg>
            {$t('share.vk')}
        </button>
    </Dropdown>
{/if}

<style>
    .share-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        color: var(--text-tertiary);
        border-radius: var(--radius-md);
        border: none;
        background: transparent;
        transition: var(--transition-colors);
        cursor: pointer;
    }

    .share-btn:hover {
        color: var(--accent);
    }

    .dropdown-item {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        width: 100%;
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-sm);
        color: var(--text-primary);
        border-radius: var(--radius-md);
        transition: var(--transition-colors);
        cursor: pointer;
        background: none;
        border: none;
        text-align: left;
    }

    .dropdown-item:hover {
        background: var(--bg-tertiary);
        color: var(--accent);
    }
</style>
