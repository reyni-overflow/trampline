<script lang="ts">
    import { t } from '$lib/i18n';

    interface Props {
        value?: string;
        label?: string;
        placeholder?: string;
        error?: string;
        onblur?: () => void;
        oninput?: () => void;
    }

    let { value = $bindable(''), label, placeholder, error, onblur, oninput }: Props = $props();

    let textareaEl = $state<HTMLTextAreaElement>();
    let backdropEl = $state<HTMLDivElement>();

    interface ToolbarAction {
        icon: string;
        title: string;
        prefix: string;
        suffix: string;
        block?: boolean;
    }

    const svg = (d: string) =>
        `<svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${d}</svg>`;

    let tools = $derived<ToolbarAction[]>([
        { icon: 'B', title: $t('md.bold'), prefix: '**', suffix: '**' },
        { icon: 'I', title: $t('md.italic'), prefix: '*', suffix: '*' },
        { icon: 'U̲', title: $t('md.underline'), prefix: '__', suffix: '__' },
        { icon: 'S', title: $t('md.strikethrough'), prefix: '~~', suffix: '~~' },
        { icon: '||', title: $t('md.spoiler'), prefix: '||', suffix: '||' },
        { icon: '`', title: $t('md.code'), prefix: '`', suffix: '`' },
        {
            icon: svg('<polyline points="4 17 10 11 4 5"/><line x1="12" y1="19" x2="20" y2="19"/>'),
            title: $t('md.codeblock'),
            prefix: '```\n',
            suffix: '\n```',
            block: true
        },
        {
            icon: svg(
                '<path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/>'
            ),
            title: $t('md.link'),
            prefix: '[',
            suffix: '](url)'
        },
        {
            icon: svg(
                '<line x1="3" y1="10" x2="21" y2="10"/><line x1="3" y1="6" x2="11" y2="6"/><line x1="3" y1="14" x2="15" y2="14"/><line x1="3" y1="18" x2="9" y2="18"/>'
            ),
            title: $t('md.blockquote'),
            prefix: '> ',
            suffix: '',
            block: true
        },
        {
            icon: svg(
                '<line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/>'
            ),
            title: $t('md.list'),
            prefix: '- ',
            suffix: '',
            block: true
        }
    ]);

    const admIcons: Record<string, string> = {
        note: svg(
            '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>'
        ),
        tip: svg(
            '<path d="M9 18h6"/><path d="M10 22h4"/><path d="M12 2a7 7 0 0 0-4 12.7V17h8v-2.3A7 7 0 0 0 12 2z"/>'
        ),
        warning: svg(
            '<path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>'
        ),
        danger: svg(
            '<circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>'
        ),
        info: svg(
            '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>'
        )
    };

    let admonitions = $derived([
        { type: 'note', label: $t('md.admNote'), icon: admIcons.note },
        { type: 'tip', label: $t('md.admTip'), icon: admIcons.tip },
        { type: 'warning', label: $t('md.admWarning'), icon: admIcons.warning },
        { type: 'danger', label: $t('md.admDanger'), icon: admIcons.danger },
        { type: 'info', label: $t('md.admInfo'), icon: admIcons.info }
    ]);

    let showAdmMenu = $state(false);
    let showTableMenu = $state(false);
    let tableCols = $state(3);
    let tableRows = $state(3);
    let tableAligns = $state<('left' | 'center' | 'right')[]>(['left', 'left', 'left']);

    function updateTableAligns() {
        while (tableAligns.length < tableCols) tableAligns.push('left');
        tableAligns = tableAligns.slice(0, tableCols);
    }

    function cycleAlign(idx: number) {
        const order: ('left' | 'center' | 'right')[] = ['left', 'center', 'right'];
        const cur = order.indexOf(tableAligns[idx]);
        tableAligns[idx] = order[(cur + 1) % 3];
        tableAligns = [...tableAligns];
    }

    function insertTable() {
        if (!textareaEl) return;
        const start = textareaEl.selectionStart;
        const before = value.slice(0, start);
        const after = value.slice(textareaEl.selectionEnd);
        const nl = before.length > 0 && !before.endsWith('\n') ? '\n' : '';
        const alignMap = { left: ':---', center: ':---:', right: '---:' };
        const headerCells = Array.from(
            { length: tableCols },
            (_, i) => ` ${$t('md.header')} ${i + 1} `
        );
        const alignCells = tableAligns.slice(0, tableCols).map((a) => ` ${alignMap[a]} `);
        const bodyRows = Array.from(
            { length: tableRows },
            () => '|' + Array.from({ length: tableCols }, () => '  ').join('|') + '|'
        );
        const table = `${nl}|${headerCells.join('|')}|\n|${alignCells.join('|')}|\n${bodyRows.join('\n')}\n`;
        value = before + table + after;
        showTableMenu = false;
        oninput?.();
        requestAnimationFrame(() => {
            textareaEl?.focus();
            const pos = start + nl.length + 2;
            textareaEl?.setSelectionRange(pos, pos + `${$t('md.header')} 1`.length);
        });
    }

    function insertFormat(tool: ToolbarAction) {
        if (!textareaEl) return;
        const start = textareaEl.selectionStart;
        const end = textareaEl.selectionEnd;
        const selected = value.slice(start, end) || (tool.block ? '' : $t('md.placeholder'));
        const before = value.slice(0, start);
        const after = value.slice(end);
        const needNewline = tool.block && before.length > 0 && !before.endsWith('\n') ? '\n' : '';
        value = before + needNewline + tool.prefix + selected + tool.suffix + after;
        oninput?.();
        const cursorPos = start + needNewline.length + tool.prefix.length + selected.length;
        requestAnimationFrame(() => {
            textareaEl?.focus();
            textareaEl?.setSelectionRange(cursorPos, cursorPos);
        });
    }

    function insertAdmonition(type: string) {
        if (!textareaEl) return;
        const start = textareaEl.selectionStart;
        const before = value.slice(0, start);
        const after = value.slice(textareaEl.selectionEnd);
        const needNewline = before.length > 0 && !before.endsWith('\n') ? '\n' : '';
        const block = `${needNewline}> [!${type}]\n> `;
        value = before + block + after;
        showAdmMenu = false;
        oninput?.();
        const cursorPos = start + block.length;
        requestAnimationFrame(() => {
            textareaEl?.focus();
            textareaEl?.setSelectionRange(cursorPos, cursorPos);
        });
    }

    function wrapSelection(prefix: string, suffix: string) {
        if (!textareaEl) return;
        const start = textareaEl.selectionStart;
        const end = textareaEl.selectionEnd;
        const selected = value.slice(start, end) || $t('md.placeholder');
        const before = value.slice(0, start);
        const after = value.slice(end);
        value = before + prefix + selected + suffix + after;
        oninput?.();
        const cursorPos = start + prefix.length + selected.length;
        requestAnimationFrame(() => {
            textareaEl?.focus();
            textareaEl?.setSelectionRange(cursorPos, cursorPos);
        });
    }

    function handleKeydown(e: KeyboardEvent) {
        if (!e.ctrlKey && !e.metaKey) return;
        switch (e.code) {
            case 'KeyB':
                e.preventDefault();
                wrapSelection('**', '**');
                break;
            case 'KeyI':
                e.preventDefault();
                wrapSelection('*', '*');
                break;
            case 'KeyU':
                e.preventDefault();
                wrapSelection('__', '__');
                break;
            case 'KeyK':
                e.preventDefault();
                wrapSelection('[', '](url)');
                break;
        }
    }

    function escapeHtml(text: string): string {
        return text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function findClosing(raw: string, start: number, marker: string): number {
        const idx = raw.indexOf(marker, start);
        return idx;
    }

    function findSingleMark(text: string, start: number, ch: string): number {
        for (let i = start; i < text.length; i++) {
            if (text[i] === ch && text[i + 1] !== ch && (i === start || text[i - 1] !== ch))
                return i;
        }
        return -1;
    }

    function parseInlinePreview(raw: string): string {
        let result = '';
        let i = 0;
        const len = raw.length;

        while (i < len) {
            if (raw[i] === '`' && raw[i + 1] !== '`') {
                const end = raw.indexOf('`', i + 1);
                if (end !== -1) {
                    result += `<span class="lp-dim">\`</span><span class="lp-code">${escapeHtml(raw.slice(i + 1, end))}</span><span class="lp-dim">\`</span>`;
                    i = end + 1;
                    continue;
                }
            }

            if (raw[i] === '[') {
                const textEnd = raw.indexOf(']', i + 1);
                if (textEnd !== -1 && raw[textEnd + 1] === '(') {
                    const hrefEnd = raw.indexOf(')', textEnd + 2);
                    if (hrefEnd !== -1) {
                        result += `<span class="lp-dim">[</span><span class="lp-link">${parseInlinePreview(raw.slice(i + 1, textEnd))}</span><span class="lp-dim">](${escapeHtml(raw.slice(textEnd + 2, hrefEnd))})</span>`;
                        i = hrefEnd + 1;
                        continue;
                    }
                }
            }

            if (raw[i] === '*' && raw[i + 1] === '*') {
                const end = findClosing(raw, i + 2, '**');
                if (end !== -1) {
                    result += `<span class="lp-dim">**</span><span class="lp-bold">${parseInlinePreview(raw.slice(i + 2, end))}</span><span class="lp-dim">**</span>`;
                    i = end + 2;
                    continue;
                }
            }

            if (raw[i] === '*' && raw[i + 1] !== '*') {
                const end = findSingleMark(raw, i + 1, '*');
                if (end !== -1) {
                    result += `<span class="lp-dim">*</span><span class="lp-italic">${parseInlinePreview(raw.slice(i + 1, end))}</span><span class="lp-dim">*</span>`;
                    i = end + 1;
                    continue;
                }
            }

            if (raw[i] === '_' && raw[i + 1] === '_') {
                const end = findClosing(raw, i + 2, '__');
                if (end !== -1) {
                    result += `<span class="lp-dim">__</span><span class="lp-underline">${parseInlinePreview(raw.slice(i + 2, end))}</span><span class="lp-dim">__</span>`;
                    i = end + 2;
                    continue;
                }
            }

            if (raw[i] === '~' && raw[i + 1] === '~') {
                const end = findClosing(raw, i + 2, '~~');
                if (end !== -1) {
                    result += `<span class="lp-dim">~~</span><span class="lp-strike">${parseInlinePreview(raw.slice(i + 2, end))}</span><span class="lp-dim">~~</span>`;
                    i = end + 2;
                    continue;
                }
            }

            if (raw[i] === '|' && raw[i + 1] === '|') {
                const end = findClosing(raw, i + 2, '||');
                if (end !== -1) {
                    result += `<span class="lp-dim">||</span><span class="lp-spoiler">${escapeHtml(raw.slice(i + 2, end))}</span><span class="lp-dim">||</span>`;
                    i = end + 2;
                    continue;
                }
            }

            if (raw[i] === '=' && raw[i + 1] === '=') {
                const end = findClosing(raw, i + 2, '==');
                if (end !== -1) {
                    result += `<span class="lp-dim">==</span><span class="lp-highlight">${parseInlinePreview(raw.slice(i + 2, end))}</span><span class="lp-dim">==</span>`;
                    i = end + 2;
                    continue;
                }
            }

            result += escapeHtml(raw[i]);
            i++;
        }

        return result;
    }

    function renderLivePreview(text: string): string {
        if (!text)
            return '<span class="lp-placeholder">' + escapeHtml(placeholder || '') + '</span>';

        const lines = text.split('\n');
        const out: string[] = [];
        let i = 0;

        while (i < lines.length) {
            const line = lines[i];

            if (line.trimStart().startsWith('```')) {
                out.push('<span class="lp-dim">' + escapeHtml(line) + '</span>');
                i++;
                while (i < lines.length && !lines[i].trimStart().startsWith('```')) {
                    out.push('<span class="lp-code-block">' + escapeHtml(lines[i]) + '</span>');
                    i++;
                }
                if (i < lines.length) {
                    out.push('<span class="lp-dim">' + escapeHtml(lines[i]) + '</span>');
                    i++;
                }
                continue;
            }

            const headingMatch = line.match(/^(#{1,3})\s+(.*)/);
            if (headingMatch) {
                out.push(
                    '<span class="lp-dim">' +
                        escapeHtml(headingMatch[1]) +
                        ' </span><span class="lp-heading">' +
                        parseInlinePreview(headingMatch[2]) +
                        '</span>'
                );
                i++;
                continue;
            }

            if (line.startsWith('> ') || line === '>') {
                const prefix = line.startsWith('> ') ? '> ' : '>';
                const rest = line.startsWith('> ') ? line.slice(2) : '';
                out.push(
                    '<span class="lp-dim">' +
                        escapeHtml(prefix) +
                        '</span><span class="lp-quote">' +
                        parseInlinePreview(rest) +
                        '</span>'
                );
                i++;
                continue;
            }

            const listMatch = line.match(/^(\s*[-*])\s(.*)/);
            if (listMatch) {
                out.push(
                    '<span class="lp-dim">' +
                        escapeHtml(listMatch[1]) +
                        ' </span>' +
                        parseInlinePreview(listMatch[2])
                );
                i++;
                continue;
            }

            const cbMatch = line.match(/^(\s*\[[ x]\])\s(.*)/);
            if (cbMatch) {
                out.push(
                    '<span class="lp-dim">' +
                        escapeHtml(cbMatch[1]) +
                        ' </span>' +
                        parseInlinePreview(cbMatch[2])
                );
                i++;
                continue;
            }

            out.push(parseInlinePreview(line));
            i++;
        }

        return out.join('\n');
    }

    let rendered = $derived(renderLivePreview(value));

    function syncScroll() {
        if (textareaEl && backdropEl) {
            backdropEl.scrollTop = textareaEl.scrollTop;
            backdropEl.scrollLeft = textareaEl.scrollLeft;
        }
    }

    function autoResize() {
        if (!textareaEl) return;
        textareaEl.style.height = 'auto';
        textareaEl.style.height = textareaEl.scrollHeight + 'px';
    }

    function handleInput() {
        autoResize();
        syncScroll();
        oninput?.();
    }
</script>

<div class="md-editor" class:has-error={!!error}>
    {#if label}
        <span class="md-label">{label}</span>
    {/if}

    <div class="editor-wrapper">
        <div class="md-toolbar">
            {#each tools as tool, idx (idx)}
                <button
                    class="tb-btn"
                    type="button"
                    title={tool.title}
                    onclick={() => insertFormat(tool)}
                    ><!-- eslint-disable svelte/no-at-html-tags -->{#if tool.icon.startsWith('<svg')}{@html tool.icon}{:else}{tool.icon}{/if}<!-- eslint-enable svelte/no-at-html-tags --></button
                >
            {/each}

            <div class="tb-sep"></div>

            <div class="tb-adm-wrap">
                <button
                    class="tb-btn"
                    type="button"
                    title={$t('md.admonition')}
                    onclick={() => (showAdmMenu = !showAdmMenu)}
                >
                    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                    {@html svg(
                        '<path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>'
                    )}
                </button>
                {#if showAdmMenu}
                    <div class="tb-adm-menu">
                        {#each admonitions as adm (adm.type)}
                            <button
                                class="tb-adm-item"
                                type="button"
                                onclick={() => insertAdmonition(adm.type)}
                            >
                                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                                <span class="adm-icon adm-icon--{adm.type}">{@html adm.icon}</span>
                                {adm.label}
                            </button>
                        {/each}
                    </div>
                {/if}
            </div>

            <div class="tb-adm-wrap">
                <button
                    class="tb-btn"
                    type="button"
                    title={$t('md.table')}
                    onclick={() => {
                        showTableMenu = !showTableMenu;
                        updateTableAligns();
                    }}
                >
                    <svg
                        viewBox="0 0 24 24"
                        width="14"
                        height="14"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        ><rect x="3" y="3" width="18" height="18" rx="2" /><line
                            x1="3"
                            y1="9"
                            x2="21"
                            y2="9"
                        /><line x1="3" y1="15" x2="21" y2="15" /><line
                            x1="9"
                            y1="3"
                            x2="9"
                            y2="21"
                        /><line x1="15" y1="3" x2="15" y2="21" /></svg
                    >
                </button>
                {#if showTableMenu}
                    <div class="tb-adm-menu tb-table-menu">
                        <div class="tb-table-row">
                            <span class="tb-table-label">{$t('md.cols')}</span>
                            <input
                                type="number"
                                class="tb-table-input"
                                min="1"
                                max="10"
                                bind:value={tableCols}
                                oninput={updateTableAligns}
                            />
                        </div>
                        <div class="tb-table-row">
                            <span class="tb-table-label">{$t('md.rows')}</span>
                            <input
                                type="number"
                                class="tb-table-input"
                                min="1"
                                max="20"
                                bind:value={tableRows}
                            />
                        </div>
                        <div class="tb-table-row">
                            <span class="tb-table-label">{$t('md.align')}</span>
                            <div class="tb-align-btns">
                                {#each Array(tableCols) as _, idx (idx)}
                                    <button
                                        class="tb-align-btn"
                                        type="button"
                                        onclick={() => cycleAlign(idx)}
                                        title="Column {idx + 1}: {tableAligns[idx]}"
                                    >
                                        {tableAligns[idx] === 'left'
                                            ? '←'
                                            : tableAligns[idx] === 'center'
                                              ? '↔'
                                              : '→'}
                                    </button>
                                {/each}
                            </div>
                        </div>
                        <button
                            class="tb-adm-item tb-table-insert"
                            type="button"
                            onclick={insertTable}>{$t('md.insertTable')}</button
                        >
                    </div>
                {/if}
            </div>
        </div>

        <div class="editor-body">
            <div class="editor-backdrop" bind:this={backdropEl} aria-hidden="true">
                <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                {@html rendered}
            </div>
            <textarea
                bind:this={textareaEl}
                bind:value
                class="editor-input"
                {placeholder}
                rows={8}
                oninput={handleInput}
                {onblur}
                onkeydown={handleKeydown}
                onscroll={syncScroll}
            ></textarea>
        </div>
    </div>

    {#if error}
        <span class="md-error">{error}</span>
    {/if}
</div>

<style>
    .md-editor {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
    }

    .md-label {
        font-size: var(--font-sm);
        font-weight: var(--weight-medium);
        color: var(--text-secondary);
    }

    .editor-wrapper {
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        overflow: hidden;
        transition:
            border-color var(--duration-normal) var(--ease-in-out),
            box-shadow var(--duration-normal) var(--ease-in-out);
    }

    .editor-wrapper:focus-within {
        border-color: var(--accent);
        box-shadow: 0 0 0 0.1875rem var(--accent-subtle);
    }

    .has-error .editor-wrapper {
        border-color: var(--color-error);
    }

    .has-error .editor-wrapper:focus-within {
        box-shadow: 0 0 0 0.1875rem var(--color-error-subtle);
    }

    .md-toolbar {
        display: flex;
        align-items: center;
        gap: 2px;
        padding: var(--space-1) var(--space-2);
        background: var(--bg-secondary);
        border-bottom: 1px solid var(--border-default);
        flex-wrap: wrap;
    }

    .tb-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        min-width: 1.75rem;
        height: 1.75rem;
        padding: 0 var(--space-1);
        font-size: 0.75rem;
        font-weight: var(--weight-semibold);
        color: var(--text-tertiary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
        font-family: inherit;
        cursor: pointer;
    }

    .tb-btn:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
    }

    .tb-sep {
        width: 1px;
        height: 1.25rem;
        background: var(--border-default);
        margin: 0 var(--space-1);
    }

    .tb-adm-wrap {
        position: relative;
    }

    .tb-adm-menu {
        position: absolute;
        top: calc(100% + var(--space-1));
        left: 0;
        z-index: var(--z-dropdown);
        display: flex;
        flex-direction: column;
        padding: var(--space-1);
        background: var(--bg-elevated);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-lg);
        min-width: 10rem;
        animation: scale-in var(--duration-fast) var(--ease-out);
    }

    .tb-adm-item {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        padding: var(--space-2) var(--space-3);
        font-size: var(--font-xs);
        color: var(--text-primary);
        border-radius: var(--radius-sm);
        transition: var(--transition-colors);
        white-space: nowrap;
        cursor: pointer;
        text-align: left;
    }

    .tb-adm-item:hover {
        background: var(--bg-tertiary);
    }

    .tb-adm-item :global(.adm-icon) {
        display: inline-flex;
        flex-shrink: 0;
    }
    .tb-adm-item :global(.adm-icon--note) {
        color: #3b82f6;
    }
    .tb-adm-item :global(.adm-icon--tip) {
        color: #10b981;
    }
    .tb-adm-item :global(.adm-icon--warning) {
        color: #f59e0b;
    }
    .tb-adm-item :global(.adm-icon--danger) {
        color: #ef4444;
    }
    .tb-adm-item :global(.adm-icon--info) {
        color: #06b6d4;
    }
    .tb-table-menu {
        min-width: 12rem;
    }

    .tb-table-row {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        padding: var(--space-1) var(--space-3);
    }

    .tb-table-label {
        font-size: var(--font-xs);
        color: var(--text-secondary);
        width: 2.5rem;
        flex-shrink: 0;
    }

    .tb-table-input {
        width: 3.5rem;
        padding: var(--space-1);
        font-size: var(--font-xs);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        color: var(--text-primary);
        text-align: center;
    }

    .tb-align-btns {
        display: flex;
        gap: 2px;
        flex-wrap: wrap;
    }

    .tb-align-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 1.5rem;
        height: 1.5rem;
        font-size: 0.75rem;
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        color: var(--text-secondary);
        cursor: pointer;
        transition: var(--transition-colors);
    }

    .tb-align-btn:hover {
        border-color: var(--accent);
        color: var(--accent);
    }

    .tb-table-insert {
        margin-top: var(--space-1);
        font-weight: var(--weight-medium);
        color: var(--accent);
    }

    .editor-body {
        position: relative;
        min-height: 12rem;
        background: var(--bg-secondary);
    }

    .editor-backdrop,
    .editor-input {
        font-family: var(
            --font-body,
            -apple-system,
            BlinkMacSystemFont,
            'Segoe UI',
            system-ui,
            sans-serif
        );
        font-size: var(--font-sm);
        line-height: var(--leading-relaxed);
        letter-spacing: normal;
        word-spacing: normal;
        padding: var(--space-3) var(--space-4);
        margin: 0;
        border: none;
        box-sizing: border-box;
        white-space: pre-wrap;
        word-break: break-word;
        overflow-wrap: break-word;
        tab-size: 4;
        overflow-y: auto;
    }

    .editor-backdrop {
        position: absolute;
        inset: 0;
        pointer-events: none;
        color: var(--text-primary);
    }

    .editor-input {
        position: relative;
        z-index: 1;
        width: 100%;
        min-height: 12rem;
        background: transparent;
        color: transparent;
        caret-color: var(--text-primary);
        resize: vertical;
    }

    .editor-input:focus {
        outline: none;
    }

    .editor-input::placeholder {
        color: transparent;
    }

    .editor-input::selection {
        background: var(--accent-subtle);
        color: transparent;
    }

    :global(.lp-dim) {
        color: var(--text-tertiary);
        opacity: 0.4;
    }

    :global(.lp-placeholder) {
        color: var(--text-tertiary);
    }

    :global(.lp-bold) {
        color: var(--accent);
    }

    :global(.lp-italic) {
        color: #a78bfa;
    }

    :global(.lp-underline) {
        text-decoration: underline;
        text-decoration-color: var(--accent);
    }

    :global(.lp-strike) {
        text-decoration: line-through;
        color: var(--text-tertiary);
    }

    :global(.lp-highlight) {
        background: rgba(250, 204, 21, 0.2);
    }

    :global(.lp-heading) {
        color: var(--accent);
    }

    :global(.lp-spoiler) {
        background: var(--text-tertiary);
        color: var(--text-tertiary);
        border-radius: 3px;
    }

    :global(.lp-code) {
        background: rgba(127, 127, 127, 0.15);
        border-radius: 3px;
    }

    :global(.lp-code-block) {
        background: rgba(127, 127, 127, 0.1);
    }

    :global(.lp-quote) {
        color: var(--text-secondary);
    }

    :global(.lp-link) {
        color: var(--accent);
        text-decoration: underline;
    }

    .md-error {
        font-size: var(--font-xs);
        color: var(--color-error);
        animation: slide-down var(--duration-fast) var(--ease-out);
    }
</style>
