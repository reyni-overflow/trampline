import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import MarkdownRenderer from '$lib/components/ui/MarkdownRenderer.svelte';

describe('MarkdownRenderer', () => {
    it('renders markdown content', () => {
        const { container } = render(MarkdownRenderer, { props: { source: '# Hello World' } });
        expect(container.textContent).toContain('Hello World');
    });

    it('renders bold text', () => {
        const { container } = render(MarkdownRenderer, { props: { source: '**bold text**' } });
        const bold = container.querySelector('strong');
        expect(bold).toBeTruthy();
    });

    it('renders italic text', () => {
        const { container } = render(MarkdownRenderer, { props: { source: '*italic text*' } });
        const italic = container.querySelector('em');
        expect(italic).toBeTruthy();
    });

    it('renders links', () => {
        const { container } = render(MarkdownRenderer, {
            props: { source: '[link](https://example.com)' }
        });
        const link = container.querySelector('a');
        expect(link).toBeTruthy();
    });

    it('renders unordered lists', () => {
        const { container } = render(MarkdownRenderer, { props: { source: '- item 1\n- item 2' } });
        const list = container.querySelector('ul');
        expect(list).toBeTruthy();
    });

    it('renders code blocks', () => {
        const { container } = render(MarkdownRenderer, { props: { source: '`code`' } });
        const code = container.querySelector('code');
        expect(code).toBeTruthy();
    });

    it('sanitizes script tags', () => {
        const { container } = render(MarkdownRenderer, {
            props: { source: '<script>alert("xss")</script>' }
        });
        const script = container.querySelector('script');
        expect(script).toBeNull();
    });

    it('handles empty source', () => {
        const { container } = render(MarkdownRenderer, { props: { source: '' } });
        expect(container).toBeInTheDocument();
    });

    it('renders headings', () => {
        const { container } = render(MarkdownRenderer, {
            props: { source: '## Heading 2\n\nSome text' }
        });
        expect(container.textContent).toContain('Heading 2');
    });

    it('renders paragraphs', () => {
        const { container } = render(MarkdownRenderer, {
            props: { source: 'Simple text paragraph' }
        });
        expect(container.textContent).toContain('Simple text paragraph');
    });
});
