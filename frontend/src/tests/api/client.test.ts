import { describe, it, expect, beforeEach, vi } from 'vitest';
import { api, handleApiError } from '$lib/api/client';
import { toast } from '$lib/stores/toast';

describe('API client', () => {
    beforeEach(() => {
        vi.restoreAllMocks();
        toast.clear();
    });

    describe('GET requests', () => {
        it('sends GET request with correct URL', async () => {
            const mockData = { id: '1', name: 'Test' };
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: true,
                status: 200,
                text: () => Promise.resolve(JSON.stringify(mockData))
            });

            const result = await api.get('/test');
            expect(fetch).toHaveBeenCalledWith(
                expect.stringContaining('/test'),
                expect.objectContaining({ method: 'GET', credentials: 'include' })
            );
            expect(result).toEqual(mockData);
        });

        it('includes credentials in all requests', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: true,
                status: 200,
                text: () => Promise.resolve(JSON.stringify({}))
            });

            await api.get('/test');
            expect(fetch).toHaveBeenCalledWith(
                expect.any(String),
                expect.objectContaining({ credentials: 'include' })
            );
        });
    });

    describe('POST requests', () => {
        it('sends POST with JSON body', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: true,
                status: 200,
                text: () => Promise.resolve(JSON.stringify({ success: true }))
            });

            await api.post('/test', { name: 'data' });
            const [, options] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0];
            expect(options.method).toBe('POST');
            expect(options.body).toBe(JSON.stringify({ name: 'data' }));
        });

        it('sets Content-Type for JSON body', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: true,
                status: 200,
                text: () => Promise.resolve(JSON.stringify({}))
            });

            await api.post('/test', { data: true });
            const [, options] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0];
            expect(options.headers.get('Content-Type')).toBe('application/json');
        });
    });

    describe('PUT requests', () => {
        it('sends PUT request', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: true,
                status: 200,
                text: () => Promise.resolve(JSON.stringify({}))
            });

            await api.put('/test/1', { name: 'updated' });
            const [, options] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0];
            expect(options.method).toBe('PUT');
        });
    });

    describe('DELETE requests', () => {
        it('sends DELETE request', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: true,
                status: 204
            });

            await api.delete('/test/1');
            const [, options] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0];
            expect(options.method).toBe('DELETE');
        });
    });

    describe('204 No Content', () => {
        it('returns undefined for 204 status', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: true,
                status: 204
            });

            const result = await api.delete('/test');
            expect(result).toBeUndefined();
        });
    });

    describe('error handling', () => {
        it('throws on non-ok response', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: false,
                status: 400,
                statusText: 'Bad Request',
                json: () => Promise.resolve({ title: 'Bad Request', detail: 'Invalid data' })
            });

            await expect(api.get('/test')).rejects.toEqual(
                expect.objectContaining({ status: 400, detail: 'Invalid data' })
            );
        });

        it('handles non-JSON error response', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: false,
                status: 500,
                statusText: 'Internal Server Error',
                json: () => Promise.reject(new Error('not json'))
            });

            await expect(api.get('/test')).rejects.toEqual(
                expect.objectContaining({ status: 500 })
            );
        });
    });

    describe('401 auto-refresh', () => {
        it('attempts token refresh on 401', async () => {
            let callCount = 0;
            globalThis.fetch = vi.fn().mockImplementation((url: string) => {
                if (url.includes('/auth/refresh')) {
                    return Promise.resolve({
                        ok: true,
                        text: () => Promise.resolve(JSON.stringify({}))
                    });
                }
                callCount++;
                if (callCount === 1) {
                    return Promise.resolve({
                        ok: false,
                        status: 401,
                        statusText: 'Unauthorized',
                        json: () => Promise.resolve({ title: 'Unauthorized' })
                    });
                }
                return Promise.resolve({
                    ok: true,
                    status: 200,
                    text: () => Promise.resolve(JSON.stringify({ data: 'refreshed' }))
                });
            });

            const result = await api.get('/protected');
            expect(result).toEqual({ data: 'refreshed' });
        });

        it('does not refresh on 401 if path is refresh endpoint', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: false,
                status: 401,
                statusText: 'Unauthorized',
                json: () => Promise.resolve({ title: 'Unauthorized' })
            });

            await expect(api.get('/auth/refresh')).rejects.toBeTruthy();
            expect(fetch).toHaveBeenCalledTimes(1);
        });
    });

    describe('FormData upload', () => {
        it('sends FormData without Content-Type header', async () => {
            globalThis.fetch = vi.fn().mockResolvedValue({
                ok: true,
                status: 200,
                text: () => Promise.resolve(JSON.stringify(['url1']))
            });

            const file = new File(['test'], 'test.txt', { type: 'text/plain' });
            await api.upload('/upload', [file]);
            const [, options] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0];
            expect(options.body).toBeInstanceOf(FormData);
            expect(options.headers.has('Content-Type')).toBe(false);
        });
    });

    describe('handleApiError()', () => {
        it('shows error toast with detail message', () => {
            const spy = vi.spyOn(toast, 'error');
            handleApiError({ detail: 'Something went wrong', status: 400 });
            expect(spy).toHaveBeenCalledWith('Something went wrong');
        });

        it('shows error toast with title when no detail', () => {
            const spy = vi.spyOn(toast, 'error');
            handleApiError({ title: 'Bad Request', status: 400 });
            expect(spy).toHaveBeenCalledWith('Bad request');
        });

        it('shows fallback message when no detail or title', () => {
            const spy = vi.spyOn(toast, 'error');
            handleApiError({});
            expect(spy).toHaveBeenCalled();
        });
    });
});
