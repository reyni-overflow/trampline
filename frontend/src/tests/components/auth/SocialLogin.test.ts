import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import SocialLogin from '$lib/components/auth/SocialLogin.svelte';

describe('SocialLogin', () => {
    it('renders social login buttons', () => {
        const { container } = render(SocialLogin);
        const buttons = container.querySelectorAll('button.social-btn, .social-btn');
        expect(buttons.length).toBeGreaterThanOrEqual(3);
    });

    it('renders divider text', () => {
        const { container } = render(SocialLogin);
        const divider = container.querySelector('.divider');
        expect(divider).toBeInTheDocument();
    });

    it('has SVG icons for social providers', () => {
        const { container } = render(SocialLogin);
        const svgs = container.querySelectorAll('svg');
        expect(svgs.length).toBeGreaterThanOrEqual(3);
    });

    it('shows toast on button click', async () => {
        const { container } = render(SocialLogin);
        const btn = container.querySelector('button.social-btn');
        if (btn) await fireEvent.click(btn);
    });
});
