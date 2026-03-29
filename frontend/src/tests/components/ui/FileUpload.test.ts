import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import FileUpload from '$lib/components/ui/FileUpload.svelte';

describe('FileUpload', () => {
    it('renders drop zone', () => {
        const { container } = render(FileUpload);
        const zone = container.querySelector('.file-upload, .dropzone, .upload');
        expect(zone).toBeInTheDocument();
    });

    it('renders file input', () => {
        const { container } = render(FileUpload);
        const input = container.querySelector('input[type="file"]');
        expect(input).toBeInTheDocument();
    });

    it('renders with label', () => {
        const { container } = render(FileUpload, { props: { label: 'Upload Resume' } });
        expect(container.textContent).toContain('Upload Resume');
    });

    it('accepts specific file types', () => {
        const { container } = render(FileUpload, { props: { accept: '.pdf,.doc' } });
        const input = container.querySelector('input[type="file"]');
        expect(input?.getAttribute('accept')).toBe('.pdf,.doc');
    });
});
