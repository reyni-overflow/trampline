<script lang="ts">
    import { t } from '$lib/i18n';

    interface Props {
        accept?: string;
        multiple?: boolean;
        label?: string;
        hint?: string;
        maxSizeMb?: number;
        files?: File[];
        onchange?: (files: File[]) => void;
    }

    let {
        accept = '*',
        multiple = false,
        label,
        hint,
        maxSizeMb = 50,
        files = $bindable([]),
        onchange
    }: Props = $props();

    let dragOver = $state(false);
    let error = $state('');
    let inputEl: HTMLInputElement;

    function isAcceptedType(file: File): boolean {
        if (accept === '*') return true;
        const allowed = accept.split(',').map((s) => s.trim().toLowerCase());
        const ext = '.' + file.name.split('.').pop()?.toLowerCase();
        const mime = file.type.toLowerCase();
        return allowed.some((a) => {
            if (a.startsWith('.')) return ext === a;
            if (a.endsWith('/*')) return mime.startsWith(a.replace('/*', '/'));
            return mime === a;
        });
    }

    function handleFiles(fileList: FileList | null) {
        if (!fileList) return;
        error = '';

        const maxBytes = maxSizeMb * 1024 * 1024;
        const newFiles: File[] = [];

        for (const file of Array.from(fileList)) {
            if (file.size > maxBytes) {
                error = $t('ui.fileTooLarge', { max: `${maxSizeMb} ${$t('ui.mb')}` });
                continue;
            }
            if (!isAcceptedType(file)) {
                error = $t('ui.fileTypeNotAllowed');
                continue;
            }
            newFiles.push(file);
        }

        if (newFiles.length === 0) return;
        files = multiple ? [...files, ...newFiles] : newFiles;
        onchange?.(files);
    }

    function handleDrop(e: DragEvent) {
        e.preventDefault();
        dragOver = false;

        handleFiles(e.dataTransfer?.files ?? null);
    }

    function handleInput(e: Event) {
        handleFiles((e.target as HTMLInputElement).files);
    }

    function removeFile(index: number) {
        files = files.filter((_, i) => i !== index);
        onchange?.(files);
    }

    function formatSize(bytes: number): string {
        if (bytes < 1024) return `${bytes} ${$t('ui.bytes')}`;
        if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} ${$t('ui.kb')}`;
        return `${(bytes / 1048576).toFixed(1)} ${$t('ui.mb')}`;
    }
</script>

<div class="file-upload">
    <div
        class="drop-zone"
        class:drag-over={dragOver}
        role="button"
        tabindex={0}
        onclick={() => inputEl.click()}
        onkeydown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                inputEl.click();
            }
        }}
        ondragover={(e) => {
            e.preventDefault();
            dragOver = true;
        }}
        ondragleave={() => (dragOver = false)}
        ondrop={handleDrop}
    >
        <svg
            viewBox="0 0 24 24"
            width="24"
            height="24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.75"
            stroke-linecap="round"
            stroke-linejoin="round"
        >
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" /><polyline
                points="17 8 12 3 7 8"
            /><line x1="12" y1="3" x2="12" y2="15" />
        </svg>
        <span class="drop-label">{label || $t('ui.uploadFile')}</span>
        {#if hint}
            <span class="drop-hint">{hint}</span>
        {/if}
        {#if error}
            <span class="drop-error">{error}</span>
        {/if}
        <input bind:this={inputEl} type="file" {accept} {multiple} oninput={handleInput} hidden />
    </div>

    {#if files.length > 0}
        <div class="file-list">
            {#each files as file, i (file.name)}
                <div class="file-item">
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
                        <path
                            d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z"
                        /><polyline points="14 2 14 8 20 8" />
                    </svg>
                    <span class="file-name">{file.name}</span>
                    <span class="file-size">{formatSize(file.size)}</span>
                    <button
                        class="file-remove"
                        type="button"
                        onclick={() => removeFile(i)}
                        aria-label={$t('common.remove')}
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
</div>

<style>
    .file-upload {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .drop-zone {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-2);
        padding: var(--space-8) var(--space-6);
        border: 2px dashed var(--border-default);
        border-radius: var(--radius-lg);
        color: var(--text-tertiary);
        cursor: pointer;
        transition:
            var(--transition-colors),
            border-color var(--duration-normal) var(--ease-in-out);
    }

    .drop-zone:hover,
    .drop-zone.drag-over {
        border-color: var(--accent);
        color: var(--accent);
        background: var(--accent-subtle);
    }

    .drop-label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
    }

    .drop-hint {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .drop-error {
        font-size: var(--font-xs);
        color: var(--color-error);
        font-weight: var(--weight-medium);
    }

    .file-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .file-item {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        padding: var(--space-2) var(--space-3);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        font-size: var(--font-sm);
        color: var(--text-secondary);
    }

    .file-name {
        flex: 1;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .file-size {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
        flex-shrink: 0;
    }

    .file-remove {
        display: flex;
        color: var(--text-tertiary);
        padding: var(--space-1);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
        flex-shrink: 0;
    }

    .file-remove:hover {
        color: var(--color-error);
        background: var(--color-error-subtle);
    }
</style>
