<script lang="ts">
    import DOMPurify from 'dompurify';
    import { browser } from '$app/environment';
    import { t } from '$lib/i18n';

    function sanitize(html: string, config?: object): string {
        if (!browser) return html;
        return DOMPurify.sanitize(html, {
            ...config,
            RETURN_TRUSTED_TYPE: false
        }) as unknown as string;
    }

    interface Props {
        source: string;
    }

    let { source }: Props = $props();

    const svgIcon = (d: string) =>
        `<svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${d}</svg>`;

    const admSvg = {
        info: svgIcon(
            '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>'
        ),
        tip: svgIcon(
            '<path d="M9 18h6"/><path d="M10 22h4"/><path d="M12 2a7 7 0 0 0-4 12.7V17h8v-2.3A7 7 0 0 0 12 2z"/>'
        ),
        hint: svgIcon(
            '<circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>'
        ),
        important: svgIcon(
            '<path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>'
        ),
        warning: svgIcon(
            '<path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>'
        ),
        danger: svgIcon(
            '<circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>'
        ),
        caution: svgIcon(
            '<path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>'
        )
    };

    let ADMONITION_TYPES = $derived<Record<string, { icon: string; label: string; cls: string }>>({
        note: { icon: admSvg.info, label: $t('md.admNote'), cls: 'adm-note' },
        tip: { icon: admSvg.tip, label: $t('md.admTip'), cls: 'adm-tip' },
        info: { icon: admSvg.info, label: $t('md.admInfo'), cls: 'adm-info' },
        hint: { icon: admSvg.hint, label: $t('md.admHint'), cls: 'adm-hint' },
        important: { icon: admSvg.important, label: $t('md.admImportant'), cls: 'adm-important' },
        warning: { icon: admSvg.warning, label: $t('md.admWarning'), cls: 'adm-warning' },
        danger: { icon: admSvg.danger, label: $t('md.admDanger'), cls: 'adm-danger' },
        caution: { icon: admSvg.caution, label: $t('md.admCaution'), cls: 'adm-caution' }
    });

    function esc(s: string): string {
        return s
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function preserveIndent(s: string): string {
        const m = s.match(/^( +)/);
        if (!m) return '';
        return (
            '<span class="md-indent" style="display:inline-block;width:' +
            m[1].length * 0.5 +
            'em"></span>'
        );
    }

    function parseInline(raw: string): string {
        let result = '';
        let i = 0;
        const len = raw.length;

        while (i < len) {
            if (raw[i] === '`' && raw[i + 1] !== '`') {
                const end = raw.indexOf('`', i + 1);
                if (end !== -1) {
                    result += `<code class="md-code">${esc(raw.slice(i + 1, end))}</code>`;
                    i = end + 1;
                    continue;
                }
            }

            if (raw[i] === '!' && raw[i + 1] === '[') {
                const altEnd = raw.indexOf(']', i + 2);
                if (altEnd !== -1 && raw[altEnd + 1] === '(') {
                    const srcEnd = raw.indexOf(')', altEnd + 2);
                    if (srcEnd !== -1) {
                        result += `<img src="${esc(raw.slice(altEnd + 2, srcEnd))}" alt="${esc(raw.slice(i + 2, altEnd))}" class="md-img" loading="lazy" />`;
                        i = srcEnd + 1;
                        continue;
                    }
                }
            }

            if (raw[i] === '[') {
                const textEnd = raw.indexOf(']', i + 1);
                if (textEnd !== -1 && raw[textEnd + 1] === '(') {
                    const hrefEnd = raw.indexOf(')', textEnd + 2);
                    if (hrefEnd !== -1) {
                        result += `<a href="${esc(raw.slice(textEnd + 2, hrefEnd))}" class="md-link" target="_blank" rel="noopener">${parseInline(raw.slice(i + 1, textEnd))}</a>`;
                        i = hrefEnd + 1;
                        continue;
                    }
                }
            }

            if (raw[i] === '*' && raw[i + 1] === '*') {
                const end = raw.indexOf('**', i + 2);
                if (end !== -1) {
                    result += `<strong>${parseInline(raw.slice(i + 2, end))}</strong>`;
                    i = end + 2;
                    continue;
                }
            }

            if (raw[i] === '*' && raw[i + 1] !== '*') {
                const end = findSingle(raw, i + 1, '*');
                if (end !== -1) {
                    result += `<em>${parseInline(raw.slice(i + 1, end))}</em>`;
                    i = end + 1;
                    continue;
                }
            }

            if (raw[i] === '_' && raw[i + 1] === '_') {
                const end = raw.indexOf('__', i + 2);
                if (end !== -1) {
                    result += `<u>${parseInline(raw.slice(i + 2, end))}</u>`;
                    i = end + 2;
                    continue;
                }
            }

            if (raw[i] === '~' && raw[i + 1] === '~') {
                const end = raw.indexOf('~~', i + 2);
                if (end !== -1) {
                    result += `<s>${parseInline(raw.slice(i + 2, end))}</s>`;
                    i = end + 2;
                    continue;
                }
            }

            if (raw[i] === '|' && raw[i + 1] === '|') {
                const end = raw.indexOf('||', i + 2);
                if (end !== -1) {
                    result += `<span class="md-spoiler" role="button" tabindex="0">${parseInline(raw.slice(i + 2, end))}</span>`;
                    i = end + 2;
                    continue;
                }
            }

            if (raw[i] === '=' && raw[i + 1] === '=') {
                const end = raw.indexOf('==', i + 2);
                if (end !== -1) {
                    result += `<mark class="md-highlight">${parseInline(raw.slice(i + 2, end))}</mark>`;
                    i = end + 2;
                    continue;
                }
            }

            result += esc(raw[i]);
            i++;
        }

        return result;
    }

    function findSingle(text: string, start: number, ch: string): number {
        for (let i = start; i < text.length; i++) {
            if (text[i] === ch && text[i + 1] !== ch && (i === start || text[i - 1] !== ch))
                return i;
        }
        return -1;
    }

    function parseTable(lines: string[], startIdx: number): { html: string; consumed: number } {
        const rows: string[][] = [];
        let aligns: ('left' | 'center' | 'right')[] = [];
        let consumed = 0;

        rows.push(splitTableRow(lines[startIdx]));
        consumed++;

        if (
            startIdx + 1 < lines.length &&
            lines[startIdx + 1].includes('|') &&
            lines[startIdx + 1].match(/^[\s|:-]+$/)
        ) {
            const cells = splitTableRow(lines[startIdx + 1]);
            aligns = cells.map((c) => {
                const t = c.trim();
                if (t.startsWith(':') && t.endsWith(':')) return 'center';
                if (t.endsWith(':')) return 'right';
                return 'left';
            });
            consumed++;
        }

        let j = startIdx + consumed;
        while (j < lines.length && lines[j].includes('|') && lines[j].trim().startsWith('|')) {
            rows.push(splitTableRow(lines[j]));
            consumed++;
            j++;
        }

        const header = rows[0];
        const body = rows.slice(1);
        const alignAttr = (idx: number) =>
            aligns[idx] ? ` style="text-align:${aligns[idx]}"` : '';

        let html = '<div class="md-table-wrap"><table class="md-table"><thead><tr>';
        for (let c = 0; c < header.length; c++) {
            html += `<th${alignAttr(c)}>${parseInline(header[c].trim())}</th>`;
        }
        html += '</tr></thead><tbody>';
        for (const row of body) {
            html += '<tr>';
            for (let c = 0; c < Math.max(row.length, header.length); c++) {
                html += `<td${alignAttr(c)}>${parseInline((row[c] || '').trim())}</td>`;
            }
            html += '</tr>';
        }
        html += '</tbody></table></div>';

        return { html, consumed };
    }

    function splitTableRow(line: string): string[] {
        let trimmed = line.trim();
        if (trimmed.startsWith('|')) trimmed = trimmed.slice(1);
        if (trimmed.endsWith('|')) trimmed = trimmed.slice(0, -1);
        return trimmed.split('|');
    }

    function parse(src: string): string {
        const lines = src.split('\n');
        const out: string[] = [];
        let i = 0;

        while (i < lines.length) {
            const line = lines[i];

            if (line.trimStart().startsWith('```')) {
                const lang = line.trim().slice(3);
                const codeLines: string[] = [];
                i++;
                while (i < lines.length && !lines[i].trimStart().startsWith('```')) {
                    codeLines.push(esc(lines[i]));
                    i++;
                }
                i++;
                out.push(
                    `<pre class="md-codeblock"${lang ? ` data-lang="${esc(lang)}"` : ''}><code>${codeLines.join('\n')}</code></pre>`
                );
                continue;
            }

            const admMatch = line.match(/^>\s*\[!(\w+)\]\s*(.*)/);
            if (admMatch) {
                const type = admMatch[1].toLowerCase();
                const adm = ADMONITION_TYPES[type];
                const bodyLines: string[] = [];
                if (admMatch[2]) bodyLines.push(admMatch[2]);
                i++;
                while (i < lines.length && lines[i].startsWith('>')) {
                    bodyLines.push(lines[i].replace(/^>\s?/, ''));
                    i++;
                }
                const cls = adm?.cls || 'adm-note';
                const icon = adm?.icon || 'ℹ️';
                const label = adm?.label || type;
                out.push(
                    `<div class="md-admonition ${cls}"><div class="adm-header"><span class="adm-icon">${icon}</span><span class="adm-label">${esc(label)}</span></div><div class="adm-body">${bodyLines.map((l) => parseInline(l)).join('<br/>')}</div></div>`
                );
                continue;
            }

            if (
                line.includes('|') &&
                line.trim().startsWith('|') &&
                i + 1 < lines.length &&
                lines[i + 1].match(/^[\s|:-]+$/)
            ) {
                const { html, consumed } = parseTable(lines, i);
                out.push(html);
                i += consumed;
                continue;
            }

            if (line.startsWith('> ') || line === '>') {
                const quoteLines: string[] = [];
                while (i < lines.length && (lines[i].startsWith('> ') || lines[i] === '>')) {
                    quoteLines.push(lines[i].startsWith('> ') ? lines[i].slice(2) : '');
                    i++;
                }
                out.push(
                    `<blockquote class="md-blockquote">${quoteLines.map((l) => parseInline(l)).join('<br/>')}</blockquote>`
                );
                continue;
            }

            if (line.startsWith('-# ')) {
                out.push(
                    `<p class="md-small">${preserveIndent(line.slice(3))}${parseInline(line.slice(3))}</p>`
                );
                i++;
                continue;
            }

            const cbMatch = line.match(/^(\s*)\[( |x)\]\s(.*)/);
            if (cbMatch) {
                const indent = cbMatch[1].length;
                const checked = cbMatch[2] === 'x';
                const content = cbMatch[3];
                out.push(
                    `<label class="md-checkbox" style="padding-left:${indent * 0.5}em"><input type="checkbox" ${checked ? 'checked' : ''} disabled /><span>${parseInline(content)}</span></label>`
                );
                i++;
                continue;
            }

            const ulMatch = line.match(/^(\s*)[-*]\s(.*)/);
            if (ulMatch) {
                const baseIndent = ulMatch[1].length;
                const items: string[] = [];
                while (i < lines.length) {
                    const m = lines[i].match(/^(\s*)[-*]\s(.*)/);
                    if (!m) break;
                    const indentDiff = m[1].length - baseIndent;
                    const pad =
                        indentDiff > 0
                            ? `<span class="md-indent" style="display:inline-block;width:${indentDiff * 0.5}em"></span>`
                            : '';
                    items.push(`${pad}${parseInline(m[2])}`);
                    i++;
                }
                out.push(
                    `<ul class="md-list">${items.map((li) => `<li>${li}</li>`).join('')}</ul>`
                );
                continue;
            }

            const olMatch = line.match(/^(\s*)\d+\.\s(.*)/);
            if (olMatch) {
                const items: string[] = [];
                while (i < lines.length && lines[i].match(/^\s*\d+\.\s/)) {
                    const m = lines[i].match(/^\s*\d+\.\s(.*)/);
                    if (m) items.push(parseInline(m[1]));
                    i++;
                }
                out.push(
                    `<ol class="md-list">${items.map((li) => `<li>${li}</li>`).join('')}</ol>`
                );
                continue;
            }

            if (!line.trim()) {
                out.push('<div class="md-spacer"></div>');
                i++;
                continue;
            }

            const indent = preserveIndent(line);
            out.push(`<p>${indent}${parseInline(line.trimStart() === line ? line : line)}</p>`);
            i++;
        }

        return out.join('');
    }

    const SAFE_STYLE_PROPS = new Set(['text-align', 'display', 'width', 'padding-left']);

    function sanitizeStyle(value: string): string {
        return value
            .split(';')
            .map((s) => s.trim())
            .filter((s) => {
                const prop = s.split(':')[0]?.trim().toLowerCase();
                return prop && SAFE_STYLE_PROPS.has(prop);
            })
            .join('; ');
    }

    let container = $state<HTMLDivElement>();

    $effect(() => {
        if (!container) return;

        function handleClick(e: MouseEvent) {
            const target = (e.target as HTMLElement).closest('.md-spoiler');
            if (target) target.classList.toggle('revealed');
        }

        function handleKeydown(e: KeyboardEvent) {
            if (e.key !== 'Enter' && e.key !== ' ') return;
            const target = (e.target as HTMLElement).closest('.md-spoiler');
            if (target) {
                e.preventDefault();
                target.classList.toggle('revealed');
            }
        }

        container.addEventListener('click', handleClick);
        container.addEventListener('keydown', handleKeydown);
        return () => {
            container!.removeEventListener('click', handleClick);
            container!.removeEventListener('keydown', handleKeydown);
        };
    });

    let html = $derived.by(() => {
        DOMPurify.addHook('uponSanitizeAttribute', (_node, data) => {
            if (data.attrName === 'style') {
                data.attrValue = sanitizeStyle(data.attrValue);
                if (!data.attrValue) data.keepAttr = false;
            }
        });
        DOMPurify.addHook('uponSanitizeElement', (node, data) => {
            if (data.tagName === 'input' && (node as HTMLInputElement).type !== 'checkbox') {
                node.parentNode?.removeChild(node);
            }
        });
        const result = sanitize(parse(source), {
            ALLOWED_TAGS: [
                'p',
                'br',
                'hr',
                'b',
                'strong',
                'i',
                'em',
                'u',
                'del',
                's',
                'mark',
                'code',
                'pre',
                'a',
                'img',
                'blockquote',
                'ul',
                'ol',
                'li',
                'table',
                'thead',
                'tbody',
                'tr',
                'th',
                'td',
                'span',
                'div',
                'details',
                'summary',
                'label',
                'input',
                'small',
                'sub',
                'sup',
                'svg',
                'path',
                'line',
                'circle',
                'polyline',
                'rect',
                'polygon'
            ],
            ALLOWED_ATTR: [
                'href',
                'src',
                'alt',
                'title',
                'class',
                'target',
                'rel',
                'type',
                'checked',
                'disabled',
                'style',
                'align',
                'colspan',
                'rowspan',
                'role',
                'tabindex',
                'viewBox',
                'width',
                'height',
                'fill',
                'stroke',
                'stroke-width',
                'stroke-linecap',
                'stroke-linejoin',
                'd',
                'cx',
                'cy',
                'r',
                'x1',
                'y1',
                'x2',
                'y2',
                'points',
                'rx',
                'ry'
            ],
            ALLOW_DATA_ATTR: false
        });
        DOMPurify.removeHook('uponSanitizeAttribute');
        DOMPurify.removeHook('uponSanitizeElement');
        return result;
    });
</script>

<div class="md-content" bind:this={container}>
    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
    {@html html}
</div>

<style>
    .md-content {
        font-size: var(--font-sm);
        line-height: var(--leading-relaxed);
        color: var(--text-primary);
        white-space: pre-wrap;
        word-wrap: break-word;
    }

    .md-content :global(p) {
        margin-bottom: var(--space-1);
        white-space: pre-wrap;
    }

    .md-content :global(.md-spacer) {
        height: var(--space-3);
    }

    .md-content :global(strong) {
        font-weight: var(--weight-semibold);
    }
    .md-content :global(em) {
        font-style: italic;
    }
    .md-content :global(u) {
        text-decoration: underline;
        text-underline-offset: 2px;
    }
    .md-content :global(s) {
        text-decoration: line-through;
        color: var(--text-tertiary);
    }

    .md-content :global(.md-highlight) {
        background: var(--accent-subtle);
        color: var(--accent);
        padding: 0.05em 0.25em;
        border-radius: var(--radius-sm);
    }

    .md-content :global(.md-spoiler) {
        background: var(--text-primary);
        color: transparent;
        border-radius: var(--radius-sm);
        padding: 0 0.25em;
        cursor: pointer;
        transition: all var(--duration-normal) var(--ease-in-out);
        user-select: none;
    }

    .md-content :global(.md-spoiler.revealed) {
        background: var(--bg-tertiary);
        color: var(--text-primary);
    }

    .md-content :global(.md-code) {
        font-family: 'Courier New', Courier, monospace;
        background: var(--bg-tertiary);
        padding: 0.1em 0.35em;
        border-radius: var(--radius-sm);
        font-size: 0.875em;
        color: var(--accent);
        white-space: pre;
    }

    .md-content :global(.md-codeblock) {
        background: var(--bg-tertiary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-md);
        padding: var(--space-3) var(--space-4);
        overflow-x: auto;
        margin: var(--space-3) 0;
        font-family: 'Courier New', Courier, monospace;
        font-size: 0.8125rem;
        line-height: 1.6;
        white-space: pre;
    }

    .md-content :global(.md-codeblock code) {
        background: none;
        padding: 0;
        color: var(--text-primary);
    }

    .md-content :global(.md-link) {
        color: var(--accent);
        text-decoration: underline;
        text-underline-offset: 2px;
    }

    .md-content :global(.md-link:hover) {
        opacity: 0.8;
    }

    .md-content :global(.md-img) {
        max-width: 100%;
        border-radius: var(--radius-md);
        margin: var(--space-3) 0;
        display: block;
    }

    .md-content :global(.md-blockquote) {
        border-left: 3px solid var(--accent);
        padding: var(--space-2) var(--space-4);
        margin: var(--space-3) 0;
        color: var(--text-secondary);
        background: var(--bg-secondary);
        border-radius: 0 var(--radius-sm) var(--radius-sm) 0;
        white-space: pre-wrap;
    }

    .md-content :global(.md-small) {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .md-content :global(.md-list) {
        padding-left: var(--space-5);
        margin: var(--space-2) 0;
        white-space: normal;
    }

    .md-content :global(.md-list li) {
        margin-bottom: var(--space-1);
    }

    .md-content :global(.md-checkbox) {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        margin: var(--space-1) 0;
        cursor: default;
        white-space: normal;
    }

    .md-content :global(.md-checkbox input) {
        accent-color: var(--accent);
        pointer-events: none;
    }

    /* Tables */
    .md-content :global(.md-table-wrap) {
        overflow-x: auto;
        margin: var(--space-3) 0;
    }

    .md-content :global(.md-table) {
        width: 100%;
        border-collapse: collapse;
        font-size: var(--font-sm);
        white-space: normal;
    }

    .md-content :global(.md-table th),
    .md-content :global(.md-table td) {
        padding: var(--space-2) var(--space-3);
        border: 1px solid var(--border-default);
    }

    .md-content :global(.md-table th) {
        background: var(--bg-tertiary);
        font-weight: var(--weight-semibold);
        font-size: var(--font-xs);
        text-transform: uppercase;
        letter-spacing: 0.03em;
        color: var(--text-secondary);
    }

    .md-content :global(.md-table td) {
        background: var(--bg-secondary);
    }

    .md-content :global(.md-table tr:hover td) {
        background: var(--bg-tertiary);
    }

    /* Admonition blocks */
    .md-content :global(.md-admonition) {
        border-left: 3px solid;
        border-radius: 0 var(--radius-md) var(--radius-md) 0;
        padding: var(--space-3) var(--space-4);
        margin: var(--space-3) 0;
        white-space: normal;
    }

    .md-content :global(.adm-header) {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-weight: var(--weight-semibold);
        font-size: var(--font-sm);
        margin-bottom: var(--space-2);
    }

    .md-content :global(.adm-icon) {
        display: inline-flex;
        flex-shrink: 0;
    }

    .md-content :global(.adm-body) {
        font-size: var(--font-sm);
    }

    .md-content :global(.adm-note) {
        border-color: var(--color-info, #3b82f6);
        background: rgba(59, 130, 246, 0.08);
    }
    .md-content :global(.adm-note .adm-header) {
        color: var(--color-info, #3b82f6);
    }
    .md-content :global(.adm-tip) {
        border-color: var(--color-success, #10b981);
        background: rgba(16, 185, 129, 0.08);
    }
    .md-content :global(.adm-tip .adm-header) {
        color: var(--color-success, #10b981);
    }
    .md-content :global(.adm-info) {
        border-color: var(--color-info-light, #06b6d4);
        background: rgba(6, 182, 212, 0.08);
    }
    .md-content :global(.adm-info .adm-header) {
        color: var(--color-info-light, #06b6d4);
    }
    .md-content :global(.adm-hint) {
        border-color: var(--color-purple, #8b5cf6);
        background: rgba(139, 92, 246, 0.08);
    }
    .md-content :global(.adm-hint .adm-header) {
        color: var(--color-purple, #8b5cf6);
    }
    .md-content :global(.adm-important) {
        border-color: var(--color-purple, #8b5cf6);
        background: rgba(139, 92, 246, 0.08);
    }
    .md-content :global(.adm-important .adm-header) {
        color: var(--color-purple, #8b5cf6);
    }
    .md-content :global(.adm-warning) {
        border-color: var(--color-warning, #f59e0b);
        background: rgba(245, 158, 11, 0.08);
    }
    .md-content :global(.adm-warning .adm-header) {
        color: var(--color-warning, #f59e0b);
    }
    .md-content :global(.adm-danger) {
        border-color: var(--color-danger, #ef4444);
        background: rgba(239, 68, 68, 0.08);
    }
    .md-content :global(.adm-danger .adm-header) {
        color: var(--color-danger, #ef4444);
    }
    .md-content :global(.adm-caution) {
        border-color: var(--color-caution, #f97316);
        background: rgba(249, 115, 22, 0.08);
    }
    .md-content :global(.adm-caution .adm-header) {
        color: var(--color-caution, #f97316);
    }

    .md-content :global(.md-indent) {
        flex-shrink: 0;
    }
</style>
