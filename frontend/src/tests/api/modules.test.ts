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

describe('API modules', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe('authApi', () => {
        it('login sends POST /auth/login', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.login({ contact: 'test@test.com', password: '123456' });
            expect(api.post).toHaveBeenCalledWith('/auth/login', {
                contact: 'test@test.com',
                password: '123456'
            });
        });

        it('register sends POST /auth/register', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.register({
                name: 'Test',
                email: 'test@test.com',
                password: '123456',
                role: 'Worker'
            });
            expect(api.post).toHaveBeenCalledWith(
                '/auth/register',
                expect.objectContaining({ email: 'test@test.com', role: 'Worker' })
            );
        });

        it('me sends GET /auth/me', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.me();
            expect(api.get).toHaveBeenCalledWith('/auth/me');
        });

        it('logout sends POST /auth/logout', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.logout();
            expect(api.post).toHaveBeenCalledWith('/auth/logout');
        });

        it('refresh sends GET /auth/refresh', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.refresh();
            expect(api.get).toHaveBeenCalledWith('/auth/refresh');
        });

        it('sessions sends GET /auth/sessions', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.sessions();
            expect(api.get).toHaveBeenCalledWith('/auth/sessions');
        });

        it('forgotPassword sends POST /auth/forgot-password', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.forgotPassword('test@test.com');
            expect(api.post).toHaveBeenCalledWith('/auth/forgot-password', {
                email: 'test@test.com'
            });
        });

        it('resetPassword sends POST /auth/reset-password', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.resetPassword('test@test.com', '123456', 'newpass');
            expect(api.post).toHaveBeenCalledWith('/auth/reset-password', {
                email: 'test@test.com',
                code: '123456',
                newPassword: 'newpass'
            });
        });

        it('changePassword sends PUT /auth/change-password', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.changePassword('old', 'new');
            expect(api.put).toHaveBeenCalledWith('/auth/change-password', {
                currentPassword: 'old',
                newPassword: 'new'
            });
        });

        it('deleteAccount sends POST /auth/delete-account', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.deleteAccount('password');
            expect(api.post).toHaveBeenCalledWith('/auth/delete-account', { password: 'password' });
        });

        it('updatePrivacy sends PUT /auth/privacy', async () => {
            const { authApi } = await import('$lib/api/auth');
            await authApi.updatePrivacy({ isPrivate: true });
            expect(api.put).toHaveBeenCalledWith('/auth/privacy', { isPrivate: true });
        });
    });

    describe('jobsApi', () => {
        it('getAll sends GET /job with query params', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getAll(1, 10, { city: 'Moscow', search: 'dev' });
            expect(api.get).toHaveBeenCalledWith(
                expect.stringMatching(/^\/job\?.*city=Moscow.*search=dev/)
            );
        });

        it('getAll uses default pagination', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getAll();
            expect(api.get).toHaveBeenCalledWith(expect.stringContaining('pageNumber=1'));
        });

        it('getById sends GET /job/:id', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getById('abc');
            expect(api.get).toHaveBeenCalledWith('/job/abc');
        });

        it('create sends POST /job', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.create({ title: 'Dev', description: 'Desc', address: 'Moscow' });
            expect(api.post).toHaveBeenCalledWith(
                '/job',
                expect.objectContaining({ title: 'Dev' })
            );
        });

        it('update sends PUT /job/:id', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.update('abc', { title: 'Updated' });
            expect(api.put).toHaveBeenCalledWith('/job/abc', { title: 'Updated' });
        });

        it('delete sends DELETE /job/:id', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.delete('abc');
            expect(api.delete).toHaveBeenCalledWith('/job/abc');
        });

        it('apply sends POST /job/application-job', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.apply('job-1', 'Cover letter');
            expect(api.post).toHaveBeenCalledWith('/job/application-job', {
                jobId: 'job-1',
                coverLetter: 'Cover letter'
            });
        });

        it('getResponses sends GET /job/:id/responses', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getResponses('job-1');
            expect(api.get).toHaveBeenCalledWith('/job/job-1/responses');
        });

        it('getTags sends GET /job/tags', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getTags();
            expect(api.get).toHaveBeenCalledWith('/job/tags');
        });

        it('getTagStats sends GET /job/tags/stats', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getTagStats();
            expect(api.get).toHaveBeenCalledWith('/job/tags/stats');
        });

        it('getByUser sends GET /job/all-by/:userId', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getByUser('user-1');
            expect(api.get).toHaveBeenCalledWith(expect.stringContaining('/job/all-by/user-1'));
        });

        it('updateApplicationStatus sends PUT', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.updateApplicationStatus('app-1', 'Viewed');
            expect(api.put).toHaveBeenCalledWith('/job/application/app-1/status', {
                status: 'Viewed'
            });
        });

        it('filters salaryMax < 500000', async () => {
            const { jobsApi } = await import('$lib/api/jobs');
            await jobsApi.getAll(1, 10, { salaryMax: 500000 });
            expect(api.get).toHaveBeenCalledWith(expect.not.stringContaining('salaryMax'));
        });
    });

    describe('employeesApi', () => {
        it('getAll sends GET /employee', async () => {
            const { employeesApi } = await import('$lib/api/employees');
            await employeesApi.getAll();
            expect(api.get).toHaveBeenCalledWith(expect.stringContaining('/employee'));
        });

        it('getById sends GET /employee/:id', async () => {
            const { employeesApi } = await import('$lib/api/employees');
            await employeesApi.getById('emp-1');
            expect(api.get).toHaveBeenCalledWith('/employee/emp-1');
        });

        it('updateProfile sends PUT /employee', async () => {
            const { employeesApi } = await import('$lib/api/employees');
            await employeesApi.updateProfile({ name: 'Co', description: 'Desc', activity: 'IT' });
            expect(api.put).toHaveBeenCalledWith(
                '/employee',
                expect.objectContaining({ name: 'Co' })
            );
        });

        it('uploadPhotos sends upload to /employee/photo', async () => {
            const { employeesApi } = await import('$lib/api/employees');
            const file = new File([''], 'photo.jpg');
            await employeesApi.uploadPhotos([file]);
            expect(api.upload).toHaveBeenCalledWith('/employee/photo', [file]);
        });

        it('verify sends GET /employee/verify', async () => {
            const { employeesApi } = await import('$lib/api/employees');
            await employeesApi.verify();
            expect(api.get).toHaveBeenCalledWith('/employee/verify');
        });
    });

    describe('eventsApi', () => {
        it('getAll sends GET /event', async () => {
            const { eventsApi } = await import('$lib/api/events');
            await eventsApi.getAll();
            expect(api.get).toHaveBeenCalledWith(expect.stringContaining('/event'));
        });

        it('getById sends GET /event/:id', async () => {
            const { eventsApi } = await import('$lib/api/events');
            await eventsApi.getById('evt-1');
            expect(api.get).toHaveBeenCalledWith('/event/evt-1');
        });

        it('create sends POST /event', async () => {
            const { eventsApi } = await import('$lib/api/events');
            await eventsApi.create({ title: 'Hackathon', description: 'Desc', address: 'Moscow' });
            expect(api.post).toHaveBeenCalledWith(
                '/event',
                expect.objectContaining({ title: 'Hackathon' })
            );
        });

        it('delete sends DELETE /event/:id', async () => {
            const { eventsApi } = await import('$lib/api/events');
            await eventsApi.delete('evt-1');
            expect(api.delete).toHaveBeenCalledWith('/event/evt-1');
        });

        it('apply sends POST /event/application-event', async () => {
            const { eventsApi } = await import('$lib/api/events');
            await eventsApi.apply('evt-1', 'Letter');
            expect(api.post).toHaveBeenCalledWith('/event/application-event', {
                eventId: 'evt-1',
                coverLetter: 'Letter'
            });
        });
    });

    describe('contactsApi', () => {
        it('getContacts sends GET /contact', async () => {
            const { contactsApi } = await import('$lib/api/contacts');
            await contactsApi.getContacts();
            expect(api.get).toHaveBeenCalledWith('/contact');
        });

        it('getIncoming sends GET /contact/incoming', async () => {
            const { contactsApi } = await import('$lib/api/contacts');
            await contactsApi.getIncoming();
            expect(api.get).toHaveBeenCalledWith('/contact/incoming');
        });

        it('sendRequest sends POST /contact/:id', async () => {
            const { contactsApi } = await import('$lib/api/contacts');
            await contactsApi.sendRequest('user-1');
            expect(api.post).toHaveBeenCalledWith('/contact/user-1');
        });

        it('accept sends PUT /contact/:id/accept', async () => {
            const { contactsApi } = await import('$lib/api/contacts');
            await contactsApi.accept('req-1');
            expect(api.put).toHaveBeenCalledWith('/contact/req-1/accept');
        });

        it('decline sends PUT /contact/:id/decline', async () => {
            const { contactsApi } = await import('$lib/api/contacts');
            await contactsApi.decline('req-1');
            expect(api.put).toHaveBeenCalledWith('/contact/req-1/decline');
        });

        it('remove sends DELETE /contact/:id', async () => {
            const { contactsApi } = await import('$lib/api/contacts');
            await contactsApi.remove('contact-1');
            expect(api.delete).toHaveBeenCalledWith('/contact/contact-1');
        });

        it('recommend sends POST /contact/recommend', async () => {
            const { contactsApi } = await import('$lib/api/contacts');
            await contactsApi.recommend('user-1', 'job-1', 'Check this out');
            expect(api.post).toHaveBeenCalledWith('/contact/recommend', {
                toUserId: 'user-1',
                jobId: 'job-1',
                message: 'Check this out'
            });
        });

        it('getRecommendations sends GET', async () => {
            const { contactsApi } = await import('$lib/api/contacts');
            await contactsApi.getRecommendations();
            expect(api.get).toHaveBeenCalledWith('/contact/recommendations');
        });
    });

    describe('favoritesApi', () => {
        it('getAll sends GET /favorite', async () => {
            const { favoritesApi } = await import('$lib/api/favorites');
            await favoritesApi.getAll();
            expect(api.get).toHaveBeenCalledWith('/favorite');
        });

        it('toggle sends POST /favorite', async () => {
            const { favoritesApi } = await import('$lib/api/favorites');
            await favoritesApi.toggle('job-1', 'Job');
            expect(api.post).toHaveBeenCalledWith('/favorite', { targetId: 'job-1', type: 'Job' });
        });
    });

    describe('workersApi', () => {
        it('updateProfile sends PUT /worker', async () => {
            const { workersApi } = await import('$lib/api/workers');
            await workersApi.updateProfile({ name: 'John', lastName: 'Doe', patronymic: 'A' });
            expect(api.put).toHaveBeenCalledWith(
                '/worker',
                expect.objectContaining({ name: 'John' })
            );
        });

        it('uploadResume sends upload to /worker/upload-resume', async () => {
            const { workersApi } = await import('$lib/api/workers');
            const file = new File([''], 'resume.pdf');
            await workersApi.uploadResume(file);
            expect(api.upload).toHaveBeenCalledWith('/worker/upload-resume', [file], 'file');
        });

        it('uploadAvatar sends upload to /worker/avatar', async () => {
            const { workersApi } = await import('$lib/api/workers');
            const file = new File([''], 'avatar.png');
            await workersApi.uploadAvatar(file);
            expect(api.upload).toHaveBeenCalledWith('/worker/avatar', [file], 'file');
        });

        it('getApplications sends GET /worker/applications', async () => {
            const { workersApi } = await import('$lib/api/workers');
            await workersApi.getApplications();
            expect(api.get).toHaveBeenCalledWith('/worker/applications');
        });

        it('getEventApplications sends GET', async () => {
            const { workersApi } = await import('$lib/api/workers');
            await workersApi.getEventApplications();
            expect(api.get).toHaveBeenCalledWith('/worker/event-applications');
        });

        it('getCount sends GET /worker/count', async () => {
            const { workersApi } = await import('$lib/api/workers');
            await workersApi.getCount();
            expect(api.get).toHaveBeenCalledWith('/worker/count');
        });

        it('search sends GET /worker with query', async () => {
            const { workersApi } = await import('$lib/api/workers');
            await workersApi.search({ search: 'dev', skills: 'TypeScript' });
            expect(api.get).toHaveBeenCalledWith(expect.stringMatching(/\/worker\?.*search=dev/));
        });
    });

    describe('adminApi', () => {
        it('getUsers sends GET /admin/users', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.getUsers();
            expect(api.get).toHaveBeenCalledWith(expect.stringContaining('/admin/users'));
        });

        it('blockUser sends PUT /admin/users/:id/block', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.blockUser('u1');
            expect(api.put).toHaveBeenCalledWith('/admin/users/u1/block');
        });

        it('unblockUser sends PUT', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.unblockUser('u1');
            expect(api.put).toHaveBeenCalledWith('/admin/users/u1/unblock');
        });

        it('changeRole sends PUT', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.changeRole('u1', 'Admin');
            expect(api.put).toHaveBeenCalledWith('/admin/users/u1/role', { role: 'Admin' });
        });

        it('getPendingVerifications sends GET', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.getPendingVerifications();
            expect(api.get).toHaveBeenCalledWith('/admin/verification/pending');
        });

        it('approveVerification sends PUT', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.approveVerification('v1');
            expect(api.put).toHaveBeenCalledWith('/admin/verification/v1/approve');
        });

        it('rejectVerification sends PUT', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.rejectVerification('v1', 'Bad');
            expect(api.put).toHaveBeenCalledWith('/admin/verification/v1/reject', {
                reason: 'Bad'
            });
        });

        it('checkInn sends GET', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.checkInn('1234567890');
            expect(api.get).toHaveBeenCalledWith('/admin/verification/check-inn/1234567890');
        });

        it('getPendingJobs sends GET', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.getPendingJobs();
            expect(api.get).toHaveBeenCalledWith('/admin/moderation/jobs');
        });

        it('approveJob sends PUT', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.approveJob('j1');
            expect(api.put).toHaveBeenCalledWith('/admin/moderation/jobs/j1/approve');
        });

        it('getTags sends GET /admin/tags', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.getTags();
            expect(api.get).toHaveBeenCalledWith('/admin/tags');
        });

        it('createTag sends POST /admin/tags', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.createTag('React');
            expect(api.post).toHaveBeenCalledWith('/admin/tags', { name: 'React' });
        });

        it('deleteTag sends DELETE with encoded name', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.deleteTag('C++');
            expect(api.delete).toHaveBeenCalledWith(`/admin/tags/${encodeURIComponent('C++')}`);
        });

        it('getCurators sends GET', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.getCurators();
            expect(api.get).toHaveBeenCalledWith('/admin/curators');
        });

        it('createCurator sends POST', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.createCurator({ name: 'Mod', email: 'mod@test.com', password: '123' });
            expect(api.post).toHaveBeenCalledWith(
                '/admin/curators',
                expect.objectContaining({ name: 'Mod' })
            );
        });

        it('deleteCurator sends DELETE', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.deleteCurator('c1');
            expect(api.delete).toHaveBeenCalledWith('/admin/curators/c1');
        });

        it('getAuditLogs sends GET with params', async () => {
            const { adminApi } = await import('$lib/api/admin');
            await adminApi.getAuditLogs(1, 20, 'Login', 'User');
            expect(api.get).toHaveBeenCalledWith(
                expect.stringMatching(/\/admin\/audit\?.*action=Login/)
            );
        });
    });

    describe('reviewsApi', () => {
        it('getApproved sends GET /review', async () => {
            const { reviewsApi } = await import('$lib/api/reviews');
            await reviewsApi.getApproved();
            expect(api.get).toHaveBeenCalledWith('/review');
        });

        it('getAll sends GET /review/all', async () => {
            const { reviewsApi } = await import('$lib/api/reviews');
            await reviewsApi.getAll();
            expect(api.get).toHaveBeenCalledWith('/review/all');
        });

        it('create sends POST /review', async () => {
            const { reviewsApi } = await import('$lib/api/reviews');
            await reviewsApi.create({ text: 'Great', rating: 5 });
            expect(api.post).toHaveBeenCalledWith('/review', { text: 'Great', rating: 5 });
        });

        it('approve sends PUT /review/:id/approve', async () => {
            const { reviewsApi } = await import('$lib/api/reviews');
            await reviewsApi.approve('r1');
            expect(api.put).toHaveBeenCalledWith('/review/r1/approve');
        });

        it('delete sends DELETE /review/:id', async () => {
            const { reviewsApi } = await import('$lib/api/reviews');
            await reviewsApi.delete('r1');
            expect(api.delete).toHaveBeenCalledWith('/review/r1');
        });
    });

    describe('notificationsApi', () => {
        it('getAll sends GET /notification', async () => {
            const { notificationsApi } = await import('$lib/api/notifications');
            await notificationsApi.getAll(1, 20);
            expect(api.get).toHaveBeenCalledWith('/notification?page=1&size=20');
        });

        it('markAsRead sends PUT', async () => {
            const { notificationsApi } = await import('$lib/api/notifications');
            await notificationsApi.markAsRead('n1');
            expect(api.put).toHaveBeenCalledWith('/notification/n1/read');
        });

        it('markAllAsRead sends PUT', async () => {
            const { notificationsApi } = await import('$lib/api/notifications');
            await notificationsApi.markAllAsRead();
            expect(api.put).toHaveBeenCalledWith('/notification/read-all');
        });
    });
});
