import { describe, it, expect, beforeEach, vi } from 'vitest';
import { get } from 'svelte/store';
import { toast } from '$lib/stores/toast';

describe('toast store', () => {
    beforeEach(() => {
        toast.clear();
        vi.useFakeTimers();
    });

    it('starts empty', () => {
        expect(get(toast)).toHaveLength(0);
    });

    it('adds info toast', () => {
        toast.info('Hello');
        const toasts = get(toast);
        expect(toasts).toHaveLength(1);
        expect(toasts[0].type).toBe('info');
        expect(toasts[0].message).toBe('Hello');
    });

    it('adds success toast', () => {
        toast.success('Done');
        expect(get(toast)[0].type).toBe('success');
    });

    it('adds warning toast', () => {
        toast.warning('Careful');
        expect(get(toast)[0].type).toBe('warning');
    });

    it('adds error toast', () => {
        toast.error('Oops');
        expect(get(toast)[0].type).toBe('error');
    });

    it('adds danger toast (persistent by default)', () => {
        toast.danger('Critical');
        const item = get(toast)[0];
        expect(item.type).toBe('danger');
        expect(item.persistent).toBe(true);
    });

    it('adds tip toast', () => {
        toast.tip('Tip');
        expect(get(toast)[0].type).toBe('tip');
    });

    it('assigns unique id to each toast', () => {
        toast.info('One');
        toast.info('Two');
        const toasts = get(toast);
        expect(toasts[0].id).not.toBe(toasts[1].id);
    });

    it('limits to MAX_TOASTS (3)', () => {
        toast.info('1');
        toast.info('2');
        toast.info('3');
        toast.info('4');
        expect(get(toast)).toHaveLength(3);
    });

    it('removes oldest toast when exceeding limit', () => {
        toast.info('1');
        toast.info('2');
        toast.info('3');
        toast.info('4');
        const messages = get(toast).map((t) => t.message);
        expect(messages).not.toContain('1');
        expect(messages).toContain('4');
    });

    it('removes toast by id', () => {
        const id = toast.info('ToRemove');
        expect(get(toast)).toHaveLength(1);
        toast.remove(id);
        expect(get(toast)).toHaveLength(0);
    });

    it('clears all toasts', () => {
        toast.info('1');
        toast.error('2');
        toast.clear();
        expect(get(toast)).toHaveLength(0);
    });

    it('auto-removes non-persistent toast after duration', () => {
        toast.info('Auto');
        expect(get(toast)).toHaveLength(1);
        vi.advanceTimersByTime(6000);
        expect(get(toast)).toHaveLength(0);
    });

    it('does not auto-remove persistent toast', () => {
        toast.danger('Persistent');
        vi.advanceTimersByTime(60000);
        expect(get(toast)).toHaveLength(1);
    });

    it('respects custom duration', () => {
        toast.info('Custom', { duration: 1000 });
        vi.advanceTimersByTime(500);
        expect(get(toast)).toHaveLength(1);
        vi.advanceTimersByTime(600);
        expect(get(toast)).toHaveLength(0);
    });

    it('respects persistent option override', () => {
        toast.info('ForcePersist', { persistent: true });
        vi.advanceTimersByTime(60000);
        expect(get(toast)).toHaveLength(1);
    });

    it('returns toast id from add methods', () => {
        const id = toast.info('test');
        expect(typeof id).toBe('string');
        expect(id.length).toBeGreaterThan(0);
    });
});
