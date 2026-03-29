import { describe, it, expect, beforeEach, vi } from 'vitest';
import { api } from '$lib/api/client';

vi.mock('$lib/api/client', () => ({
    api: {
        get: vi.fn().mockResolvedValue({}),
        post: vi.fn().mockResolvedValue({}),
        put: vi.fn().mockResolvedValue({}),
        delete: vi.fn().mockResolvedValue(undefined),
        upload: vi.fn().mockResolvedValue([])
    },
    handleApiError: vi.fn()
}));

describe('authApi TOTP', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('totpVerify sends POST /auth/totp/verify', async () => {
        const { authApi } = await import('$lib/api/auth');
        await authApi.totpVerify('challenge-123', '654321');
        expect(api.post).toHaveBeenCalledWith(
            '/auth/totp/verify',
            expect.any(Object)
        );
    });

    it('totpVerify sends challengeId (not userId) in the body', async () => {
        const { authApi } = await import('$lib/api/auth');
        await authApi.totpVerify('challenge-abc', '123456');
        expect(api.post).toHaveBeenCalledWith('/auth/totp/verify', {
            challengeId: 'challenge-abc',
            code: '123456'
        });
    });

    it('LoginResponse type uses challengeId field', async () => {
        const { authApi } = await import('$lib/api/auth');
        (api.post as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            requiresTotp: true,
            challengeId: 'challenge-xyz'
        });
        const result = await authApi.login({ contact: 'test@test.com', password: '123456' });
        expect(result).toEqual({
            requiresTotp: true,
            challengeId: 'challenge-xyz'
        });
        expect(result).not.toHaveProperty('userId');
    });
});
