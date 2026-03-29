import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/svelte';
import Tabs from '$lib/components/ui/Tabs.svelte';

const tabs = [
    { id: 'tab1', label: 'Tab 1' },
    { id: 'tab2', label: 'Tab 2' },
    { id: 'tab3', label: 'Tab 3' }
];

describe('Tabs', () => {
    it('renders all tab buttons', () => {
        const { container } = render(Tabs, { props: { tabs, active: 'tab1' } });
        const buttons = container.querySelectorAll('button');
        expect(buttons.length).toBeGreaterThanOrEqual(3);
    });

    it('marks active tab', () => {
        const { container } = render(Tabs, { props: { tabs, active: 'tab2' } });
        const activeBtn = container.querySelector('.active');
        expect(activeBtn).toBeInTheDocument();
        expect(activeBtn?.textContent).toContain('Tab 2');
    });

    it('fires onchange when tab clicked', async () => {
        const onchange = vi.fn();
        const { container } = render(Tabs, { props: { tabs, active: 'tab1', onchange } });
        const buttons = container.querySelectorAll('button');
        const tab2 = Array.from(buttons).find((b) => b.textContent?.includes('Tab 2'));
        if (tab2) await fireEvent.click(tab2);
        expect(onchange).toHaveBeenCalledWith('tab2');
    });

    it('renders tab labels', () => {
        const { container } = render(Tabs, { props: { tabs } });
        expect(container.textContent).toContain('Tab 1');
        expect(container.textContent).toContain('Tab 2');
        expect(container.textContent).toContain('Tab 3');
    });
});
