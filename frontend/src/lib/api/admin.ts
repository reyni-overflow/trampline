import { api } from './client';
import type { TagResponse, UpdateJobRequest } from './jobs';
import type { EventResponse, UpdateEventRequest } from './events';
import type { MentorshipResponse, UpdateMentorshipRequest } from './mentorships';
import type { WorkerProfileRequest } from './workers';
import type { EmployeeProfileRequest } from './employees';

export interface AdminUserResponse {
    id: string;
    nickname: string;
    email: string;
    role: 'Worker' | 'Employee' | 'Admin';
    avatar: string | null;
    isBlocked: boolean;
    createdAt: string;
    isSuperAdmin?: boolean;
}

export interface VerificationRequest {
    id: string;
    companyName: string;
    inn: string;
    email: string;
    link: string | null;
    activity: string;
    createdAt: string;
}

export interface AdminTagResponse {
    name: string;
    count: number;
}

export interface PendingJobResponse {
    id: string;
    title: string;
    description: string;
    company?: string;
    companyName?: string;
    type: string;
    salaryFrom: number | null;
    salaryTo: number | null;
    tags: TagResponse[];
    isActive: boolean;
    createdAt: string;
    date?: string;
}

export interface AuditLogEntry {
    id: string;
    userId: string | null;
    userName: string;
    userRole: string;
    action: string;
    entityType: string;
    entityId: string | null;
    details: string | null;
    ipAddress: string | null;
    createdAt: string;
}

export const adminApi = {
    getAuditLogs(page = 1, size = 20, action?: string, entityType?: string, userId?: string) {
        const params = new URLSearchParams({ page: String(page), size: String(size) });
        if (action) params.set('action', action);
        if (entityType) params.set('entityType', entityType);
        if (userId) params.set('userId', userId);
        return api.get<{ items: AuditLogEntry[]; total: number }>(`/admin/audit?${params}`);
    },

    async getUsers(page = 1, size = 20) {
        const res = await api.get<{ items: AdminUserResponse[]; total: number }>(`/admin/users?page=${page}&size=${size}`);
        return res.items;
    },

    blockUser(userId: string) {
        return api.put(`/admin/users/${userId}/block`);
    },

    unblockUser(userId: string) {
        return api.put(`/admin/users/${userId}/unblock`);
    },

    changeRole(userId: string, role: string) {
        return api.put(`/admin/users/${userId}/role`, { role });
    },

    getPendingVerifications() {
        return api.get<VerificationRequest[]>('/admin/verification/pending');
    },

    approveVerification(id: string) {
        return api.put(`/admin/verification/${id}/approve`);
    },

    rejectVerification(id: string, reason?: string) {
        return api.put(`/admin/verification/${id}/reject`, { reason });
    },

    checkInn(inn: string) {
        return api.get<{ found: boolean; value?: string; inn?: string; kpp?: string; orgn?: string; type?: string }>(`/admin/verification/check-inn/${inn}`);
    },

    getPendingJobs() {
        return api.get<PendingJobResponse[]>('/admin/moderation/jobs');
    },

    approveJob(jobId: string) {
        return api.put(`/admin/moderation/jobs/${jobId}/approve`);
    },

    rejectJob(jobId: string) {
        return api.put(`/admin/moderation/jobs/${jobId}/reject`);
    },

    getPendingEvents() {
        return api.get<EventResponse[]>('/admin/moderation/events');
    },

    approveEvent(eventId: string) {
        return api.put(`/admin/moderation/events/${eventId}/approve`);
    },

    rejectEvent(eventId: string) {
        return api.put(`/admin/moderation/events/${eventId}/reject`);
    },

    getPendingMentorships() {
        return api.get<MentorshipResponse[]>('/admin/moderation/mentorships');
    },

    approveMentorship(mentorshipId: string) {
        return api.put(`/admin/moderation/mentorships/${mentorshipId}/approve`);
    },

    rejectMentorship(mentorshipId: string) {
        return api.put(`/admin/moderation/mentorships/${mentorshipId}/reject`);
    },

    updateMentorship(id: string, data: UpdateMentorshipRequest) {
        return api.put(`/admin/mentorships/${id}`, data);
    },

    getTags() {
        return api.get<AdminTagResponse[]>('/admin/tags');
    },

    createTag(name: string) {
        return api.post('/admin/tags', { name });
    },

    deleteTag(name: string) {
        return api.delete(`/admin/tags/${encodeURIComponent(name)}`);
    },

    getCurators() {
        return api.get<AdminUserResponse[]>('/admin/curators');
    },

    createCurator(data: { name: string; email: string; password: string }) {
        return api.post('/admin/curators', data);
    },

    deleteCurator(id: string) {
        return api.delete(`/admin/curators/${id}`);
    },

    updateJob(id: string, data: UpdateJobRequest) {
        return api.put(`/admin/jobs/${id}`, data);
    },

    updateEvent(id: string, data: UpdateEventRequest) {
        return api.put(`/admin/events/${id}`, data);
    },

    updateWorkerProfile(userId: string, data: WorkerProfileRequest) {
        return api.put(`/admin/workers/${userId}`, data);
    },

    updateEmployeeProfile(userId: string, data: EmployeeProfileRequest) {
        return api.put(`/admin/employees/${userId}`, data);
    }
};
