import { api } from './client';
import type { WorkerProfileResponse, WorkerInfo } from './auth';

export interface WorkerProfileRequest {
    name: string;
    lastName: string;
    patronymic: string;
    about?: string;
    photo?: string;
    info?: WorkerInfo;
    skills?: string[];
    repos?: string[];
}

export interface WorkerApplicationResponse {
    id: string;
    workerProfileId: string;
    jobId: string;
    job: {
        id: string;
        title: string;
        city: string;
        type: string;
        format: string;
    };
    status:
        | 'Pending'
        | 'Viewed'
        | 'Rejected'
        | 'Invited'
        | 'InProgress'
        | 'Hired'
        | 'Withdrawn'
        | 'Reserved';
    coverLetter: string | null;
    isReadByEmployer: boolean;
    createdAt: string;
}

export interface WorkerMentorshipApplicationResponse {
    id: string;
    workerProfileId: string;
    mentorshipId: string;
    mentorship: {
        id: string;
        title: string;
        city: string;
        format: string;
    };
    status:
        | 'Pending'
        | 'Viewed'
        | 'Rejected'
        | 'Invited'
        | 'InProgress'
        | 'Hired'
        | 'Withdrawn'
        | 'Reserved';
    coverLetter: string;
    isReadByEmployer: boolean;
    createdAt: string;
}

export interface WorkerEventApplicationResponse {
    id: string;
    workerProfileId: string;
    eventId: string;
    event: {
        id: string;
        title: string;
        city: string;
        format: string;
    };
    status:
        | 'Pending'
        | 'Viewed'
        | 'Rejected'
        | 'Invited'
        | 'InProgress'
        | 'Hired'
        | 'Withdrawn'
        | 'Reserved';
    coverLetter: string | null;
    isReadByEmployer: boolean;
    createdAt: string;
}

export interface WorkerSearchResult {
    id: string;
    userId: string;
    name: string;
    lastName: string;
    patronymic: string;
    photo: string | null;
    about: string | null;
    skills: string[];
    university: string | null;
    course: number | null;
    isPrivate: boolean;
}

export const workersApi = {
    getById(id: string) {
        return api.get<WorkerProfileResponse>(`/worker/${id}`);
    },

    getByUser(userId: string) {
        return api.get<WorkerProfileResponse>(`/worker/by-user/${userId}`);
    },

    updateProfile(data: WorkerProfileRequest) {
        return api.put<WorkerProfileResponse>('/worker', data);
    },

    uploadResume(file: File) {
        return api.upload<string>('/worker/upload-resume', [file], 'file');
    },

    uploadAvatar(file: File) {
        return api.upload<string>('/worker/avatar', [file], 'file');
    },

    getApplications() {
        return api.get<WorkerApplicationResponse[]>('/worker/applications');
    },

    getEventApplications() {
        return api.get<WorkerEventApplicationResponse[]>('/worker/event-applications');
    },

    getMentorshipApplications() {
        return api.get<WorkerMentorshipApplicationResponse[]>('/worker/mentorship-applications');
    },

    getCount() {
        return api.get<{ count: number }>('/worker/count');
    },

    search(params: {
        search?: string;
        skills?: string;
        university?: string;
        pageSize?: number;
        pageNumber?: number;
    }) {
        const query = new URLSearchParams();
        if (params.search) query.set('search', params.search);
        if (params.skills) query.set('skills', params.skills);
        if (params.university) query.set('university', params.university);
        if (params.pageSize) query.set('pageSize', String(params.pageSize));
        if (params.pageNumber) query.set('pageNumber', String(params.pageNumber));
        return api.get<{ items: WorkerSearchResult[]; totalCount: number }>(`/worker?${query}`);
    }
};
