import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/svelte';
import type { Component } from 'svelte';
import SnippetWrapper from '../helpers/SnippetWrapper.svelte';

import Badge from '$lib/components/ui/Badge.svelte';
import Tag from '$lib/components/ui/Tag.svelte';
import Avatar from '$lib/components/ui/Avatar.svelte';
import Button from '$lib/components/ui/Button.svelte';
import Toggle from '$lib/components/ui/Toggle.svelte';
import Checkbox from '$lib/components/ui/Checkbox.svelte';
import Input from '$lib/components/ui/Input.svelte';
import Textarea from '$lib/components/ui/Textarea.svelte';
import Skeleton from '$lib/components/ui/Skeleton.svelte';
import JobCardSkeleton from '$lib/components/ui/JobCardSkeleton.svelte';
import CompanyCardSkeleton from '$lib/components/ui/CompanyCardSkeleton.svelte';
import ViewToggle from '$lib/components/ui/ViewToggle.svelte';
import SearchInput from '$lib/components/ui/SearchInput.svelte';
import Pagination from '$lib/components/ui/Pagination.svelte';
import Select from '$lib/components/ui/Select.svelte';
import FileUpload from '$lib/components/ui/FileUpload.svelte';
import PasswordInput from '$lib/components/ui/PasswordInput.svelte';
import ContactInput from '$lib/components/ui/ContactInput.svelte';
import CompanyCard from '$lib/components/ui/CompanyCard.svelte';
import MarkdownRenderer from '$lib/components/ui/MarkdownRenderer.svelte';
import SocialLogin from '$lib/components/auth/SocialLogin.svelte';
import { vi } from 'vitest';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function renderWithSnippet(component: Component<any>, props: Record<string, unknown> = {}, text = 'Content') {
    return render(SnippetWrapper, { props: { component, props, text } });
}

describe('Component Snapshots', () => {
    it('Badge snapshot', () => {
        const { container } = renderWithSnippet(Badge, { variant: 'success' }, 'Active');
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Badge error snapshot', () => {
        const { container } = renderWithSnippet(Badge, { variant: 'error' }, 'Error');
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Tag snapshot', () => {
        const { container } = renderWithSnippet(Tag, { clickable: true }, 'TypeScript');
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Tag selected snapshot', () => {
        const { container } = renderWithSnippet(Tag, { clickable: true, selected: true }, 'React');
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Avatar initials snapshot', () => {
        const { container } = render(Avatar, { props: { name: 'John Doe' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Avatar with image snapshot', () => {
        const { container } = render(Avatar, { props: { name: 'Test', src: '/photo.jpg' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Button primary snapshot', () => {
        const { container } = renderWithSnippet(Button, { variant: 'primary' }, 'Click me');
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Button danger snapshot', () => {
        const { container } = renderWithSnippet(Button, { variant: 'danger' }, 'Delete');
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Button loading snapshot', () => {
        const { container } = renderWithSnippet(Button, { loading: true }, 'Loading...');
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Toggle snapshot', () => {
        const { container } = render(Toggle, { props: { label: 'Dark Mode' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Toggle checked snapshot', () => {
        const { container } = render(Toggle, { props: { label: 'On', checked: true } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Checkbox snapshot', () => {
        const { container } = render(Checkbox, { props: { label: 'Accept terms' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Checkbox checked snapshot', () => {
        const { container } = render(Checkbox, { props: { label: 'Checked', checked: true } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Input snapshot', () => {
        const { container } = render(Input, { props: { label: 'Name', placeholder: 'Enter name' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Input error snapshot', () => {
        const { container } = render(Input, { props: { label: 'Email', error: 'Invalid email' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Textarea snapshot', () => {
        const { container } = render(Textarea, { props: { label: 'Bio', placeholder: 'Tell about yourself' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Skeleton snapshot', () => {
        const { container } = render(Skeleton, { props: { width: '200px', height: '20px' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Skeleton circle snapshot', () => {
        const { container } = render(Skeleton, { props: { width: '48px', height: '48px', circle: true } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('JobCardSkeleton grid snapshot', () => {
        const { container } = render(JobCardSkeleton);
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('JobCardSkeleton list snapshot', () => {
        const { container } = render(JobCardSkeleton, { props: { mode: 'list' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('CompanyCardSkeleton snapshot', () => {
        const { container } = render(CompanyCardSkeleton);
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('ViewToggle snapshot', () => {
        const { container } = render(ViewToggle, { props: { mode: 'grid', onchange: vi.fn() } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('SearchInput snapshot', () => {
        const { container } = render(SearchInput, { props: { placeholder: 'Search...' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Pagination snapshot', () => {
        const { container } = render(Pagination, { props: { page: 3, totalPages: 10 } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('Select snapshot', () => {
        const { container } = render(Select, {
            props: { options: [{ value: '1', label: 'One' }, { value: '2', label: 'Two' }], placeholder: 'Select...' }
        });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('FileUpload snapshot', () => {
        const { container } = render(FileUpload, { props: { label: 'Upload', accept: '.pdf' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('PasswordInput snapshot', () => {
        const { container } = render(PasswordInput, { props: { label: 'Password' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('ContactInput snapshot', () => {
        const { container } = render(ContactInput, { props: { label: 'Contact' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('CompanyCard snapshot', () => {
        const { container } = render(CompanyCard, {
            props: { id: '1', name: 'TechCo', activity: 'IT', isVerified: true, jobCount: 3 }
        });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('MarkdownRenderer snapshot', () => {
        const { container } = render(MarkdownRenderer, { props: { source: '# Title\n\n**Bold** and *italic*\n\n- list item' } });
        expect(container.innerHTML).toMatchSnapshot();
    });

    it('SocialLogin snapshot', () => {
        const { container } = render(SocialLogin);
        expect(container.innerHTML).toMatchSnapshot();
    });
});
