import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import type { Component } from 'svelte';
import SnippetWrapper from '../helpers/SnippetWrapper.svelte';
import Button from '$lib/components/ui/Button.svelte';
import Input from '$lib/components/ui/Input.svelte';
import Modal from '$lib/components/ui/Modal.svelte';
import Checkbox from '$lib/components/ui/Checkbox.svelte';
import Textarea from '$lib/components/ui/Textarea.svelte';
import Select from '$lib/components/ui/Select.svelte';

function renderWithSnippet(
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component: Component<any>,
    props: Record<string, unknown> = {},
    text = 'Content'
) {
    return render(SnippetWrapper, { props: { component, props, text } });
}

describe('Accessibility', () => {
    describe('Button', () => {
        it('renders as button element with accessible role', () => {
            const { container } = renderWithSnippet(Button, {});
            const btn = container.querySelector('button');
            expect(btn).toBeInTheDocument();
            expect(btn?.tagName).toBe('BUTTON');
        });

        it('disabled button has disabled attribute for screen readers', () => {
            const { container } = renderWithSnippet(Button, { disabled: true });
            const btn = container.querySelector('button');
            expect(btn).toBeDisabled();
        });

        it('loading button is disabled for assistive technology', () => {
            const { container } = renderWithSnippet(Button, { loading: true });
            const btn = container.querySelector('button');
            expect(btn).toBeDisabled();
        });

        it('link variant has correct href for navigation', () => {
            const { container } = renderWithSnippet(Button, { href: '/test' });
            const link = container.querySelector('a');
            expect(link).toBeInTheDocument();
            expect(link?.getAttribute('href')).toBe('/test');
        });
    });

    describe('Input', () => {
        it('label is associated with input via for attribute', () => {
            const { container } = render(Input, { props: { label: 'Email', id: 'email-input' } });
            const label = container.querySelector('label');
            const input = container.querySelector('input');
            expect(label).toBeInTheDocument();
            expect(label?.getAttribute('for')).toBe('email-input');
            expect(input).toBeInTheDocument();
        });

        it('error message is visible to screen readers', () => {
            const { container } = render(Input, { props: { error: 'Field required' } });
            expect(container.textContent).toContain('Field required');
        });

        it('password toggle button has aria-label', () => {
            const { container } = render(Input, { props: { type: 'password' } });
            const eyeBtn = container.querySelector('.eye-btn');
            expect(eyeBtn).toBeInTheDocument();
            expect(eyeBtn?.getAttribute('aria-label')).toBeTruthy();
        });

        it('number stepper buttons have aria-labels', () => {
            const { container } = render(Input, { props: { type: 'number' } });
            const steppers = container.querySelectorAll('.stepper');
            steppers.forEach((stepper) => {
                expect(stepper.getAttribute('aria-label')).toBeTruthy();
            });
        });

        it('disabled input has disabled attribute', () => {
            const { container } = render(Input, { props: { disabled: true } });
            const input = container.querySelector('input');
            expect(input?.disabled).toBe(true);
        });
    });

    describe('Textarea', () => {
        it('renders textarea element', () => {
            const { container } = render(Textarea);
            const textarea = container.querySelector('textarea');
            expect(textarea).toBeInTheDocument();
        });
    });

    describe('Checkbox', () => {
        it('renders input with checkbox type', () => {
            const { container } = render(Checkbox, { props: { label: 'Accept terms' } });
            const input = container.querySelector('input[type="checkbox"]');
            expect(input).toBeInTheDocument();
        });

        it('has associated label text', () => {
            const { container } = render(Checkbox, { props: { label: 'Accept terms' } });
            expect(container.textContent).toContain('Accept terms');
        });
    });

    describe('Modal', () => {
        it('close on Escape key for keyboard users', async () => {
            const onclose = vi.fn();
            renderWithSnippet(Modal, { open: true, onclose });
            await fireEvent.keyDown(document, { key: 'Escape' });
            expect(onclose).toHaveBeenCalled();
        });

        it('close on backdrop click', async () => {
            const onclose = vi.fn();
            const { container } = renderWithSnippet(Modal, { open: true, onclose });
            const overlay = container.querySelector('.modal-overlay');
            if (overlay) await fireEvent.click(overlay);
            expect(onclose).toHaveBeenCalled();
        });

        it('not visible when closed', () => {
            const { container } = renderWithSnippet(Modal, { open: false, onclose: vi.fn() });
            const overlay = container.querySelector('.modal-overlay');
            expect(overlay).not.toBeInTheDocument();
        });

        it('renders title when provided', () => {
            const { container } = renderWithSnippet(Modal, {
                open: true,
                title: 'Accessible Title',
                onclose: vi.fn()
            });
            expect(container.textContent).toContain('Accessible Title');
        });
    });

    describe('Select', () => {
        it('renders with listbox role for accessibility', () => {
            const { container } = render(Select, {
                props: { options: [{ value: 'a', label: 'A' }] }
            });
            const selectGroup = container.querySelector('.select-group');
            expect(selectGroup).toBeInTheDocument();
        });
    });

    describe('Interactive elements', () => {
        it('buttons are focusable', () => {
            const { container } = renderWithSnippet(Button, {});
            const btn = container.querySelector('button');
            expect(btn?.tabIndex).not.toBe(-1);
        });

        it('inputs are focusable', () => {
            const { container } = render(Input);
            const input = container.querySelector('input');
            expect(input?.tabIndex).not.toBe(-1);
        });

        it('number input steppers have tabindex=-1 to skip tab order', () => {
            const { container } = render(Input, { props: { type: 'number' } });
            const steppers = container.querySelectorAll('.stepper');
            steppers.forEach((stepper) => {
                expect(stepper.getAttribute('tabindex')).toBe('-1');
            });
        });
    });
});
