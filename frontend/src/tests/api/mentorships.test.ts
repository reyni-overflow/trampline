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

describe('mentorshipsApi', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('getAll sends GET /mentorship with query params', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getAll(1, 10, { city: 'Moscow', search: 'react' });
        expect(api.get).toHaveBeenCalledWith(
            expect.stringMatching(/^\/mentorship\?.*city=Moscow.*search=react/)
        );
    });

    it('getAll uses default pagination', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getAll();
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('pageNumber=1'));
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('pageSize=10'));
    });

    it('getAll passes format filter', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getAll(1, 10, { format: 'Remote' });
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('format=Remote'));
    });

    it('getAll passes tags filter', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getAll(1, 10, { tags: 'React,TypeScript' });
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('tags=React'));
    });

    it('getById sends GET /mentorship/:id', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getById('abc');
        expect(api.get).toHaveBeenCalledWith('/mentorship/abc');
    });

    it('create sends POST /mentorship', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.create({ title: 'Mentor', description: 'Desc', address: 'Moscow' });
        expect(api.post).toHaveBeenCalledWith(
            '/mentorship',
            expect.objectContaining({ title: 'Mentor' })
        );
    });

    it('update sends PUT /mentorship/:id', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.update('abc', { title: 'Updated' });
        expect(api.put).toHaveBeenCalledWith('/mentorship/abc', { title: 'Updated' });
    });

    it('delete sends DELETE /mentorship/:id', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.delete('abc');
        expect(api.delete).toHaveBeenCalledWith('/mentorship/abc');
    });

    it('getByUser sends GET /mentorship/all-by/:userId', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getByUser('user-1');
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('/mentorship/all-by/user-1'));
    });

    it('getByUser uses default pagination', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getByUser('user-1');
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('pageNumber=1'));
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('pageSize=10'));
    });

    it('getByUser accepts custom pagination', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getByUser('user-1', 2, 20);
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('pageNumber=2'));
        expect(api.get).toHaveBeenCalledWith(expect.stringContaining('pageSize=20'));
    });

    it('apply sends POST /mentorship/application-mentorship', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.apply('m-1', 'Cover letter text');
        expect(api.post).toHaveBeenCalledWith('/mentorship/application-mentorship', {
            mentorshipId: 'm-1',
            coverLetter: 'Cover letter text'
        });
    });

    it('apply uses empty string as default cover letter', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.apply('m-1');
        expect(api.post).toHaveBeenCalledWith('/mentorship/application-mentorship', {
            mentorshipId: 'm-1',
            coverLetter: ''
        });
    });

    it('getResponses sends GET /mentorship/:id/responses', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.getResponses('m-1');
        expect(api.get).toHaveBeenCalledWith('/mentorship/m-1/responses');
    });

    it('updateApplicationStatus sends PUT', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.updateApplicationStatus('app-1', 'Viewed');
        expect(api.put).toHaveBeenCalledWith('/mentorship/application/app-1/status', {
            status: 'Viewed'
        });
    });

    it('uploadPhotos sends upload to /mentorship/:id/photo', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        const file = new File([''], 'photo.jpg');
        await mentorshipsApi.uploadPhotos('m-1', [file]);
        expect(api.upload).toHaveBeenCalledWith('/mentorship/m-1/photo', [file], 'files');
    });

    it('uploadVideos sends upload to /mentorship/:id/video', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        const file = new File([''], 'video.mp4');
        await mentorshipsApi.uploadVideos('m-1', [file]);
        expect(api.upload).toHaveBeenCalledWith('/mentorship/m-1/video', [file], 'files');
    });

    it('deletePhoto sends DELETE with encoded path', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.deletePhoto('m-1', '/files/photos/test.jpg');
        expect(api.delete).toHaveBeenCalledWith(
            `/mentorship/m-1/photo?path=${encodeURIComponent('/files/photos/test.jpg')}`
        );
    });

    it('deleteVideo sends DELETE with encoded path', async () => {
        const { mentorshipsApi } = await import('$lib/api/mentorships');
        await mentorshipsApi.deleteVideo('m-1', '/files/videos/test.mp4');
        expect(api.delete).toHaveBeenCalledWith(
            `/mentorship/m-1/video?path=${encodeURIComponent('/files/videos/test.mp4')}`
        );
    });
});
