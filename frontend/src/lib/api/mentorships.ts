import { api } from './client';
import type { PaginatedResponse, TagRequest } from './jobs';

export interface MentorshipResponse {
    id: string;
    employeeId: string;
    userId: string;
    title: string;
    description: string;
    address: string;
    city: string;
    street: string;
    geoLon: number;
    geoLat: number;
    country: string;
    format: 'Remote' | 'Hybrid' | 'Office';
    maxParticipants?: number;
    duration?: string;
    startDate?: string;
    salaryFrom?: number;
    salaryTo?: number;
    tags: { id: string; name: string; category: string }[];
    photos: string[];
    videos: string[];
    isActive: boolean;
    isPublished: boolean;
    isFavorited?: boolean;
    views: number;
    companyName?: string;
    createdAt: string;
    updatedAt?: string;
    endedAt?: string;
    deletedAt?: string;
}

export interface CreateMentorshipRequest {
    title: string;
    description: string;
    address: string;
    format?: 'Remote' | 'Hybrid' | 'Office';
    maxParticipants?: number;
    duration?: string;
    startDate?: string;
    salaryFrom?: number;
    salaryTo?: number;
    tags?: TagRequest[];
    endedAt?: string;
}

export interface UpdateMentorshipRequest {
    title?: string;
    description?: string;
    address?: string;
    city?: string;
    country?: string;
    isPublished?: boolean;
    maxParticipants?: number;
    duration?: string;
    startDate?: string;
    salaryFrom?: number;
    salaryTo?: number;
    tags?: TagRequest[];
    endedAt?: string;
}

export interface MentorshipApplicationResponse {
    id: string;
    coverLetter: string;
    mentorshipId: string;
    mentorshipTitle?: string;
    companyName?: string;
    profile: {
        name: string;
        lastName: string;
        patronymic: string;
        about: string | null;
        photo: string | null;
        resume: string | null;
        info: {
            university: string;
            course: number;
            admissionAt: string;
            graduationAt: string;
        } | null;
    };
    workerProfileId: string;
    createdAt: string;
    isReadByEmployer: boolean;
    status: 'Pending' | 'Viewed' | 'Rejected' | 'Invited' | 'InProgress' | 'Hired' | 'Withdrawn';
}

export const mentorshipsApi = {
    getAll(
        pageNumber = 1,
        pageSize = 10,
        filters?: { city?: string; search?: string; format?: string; tags?: string }
    ) {
        const params = new URLSearchParams({
            pageNumber: String(pageNumber),
            pageSize: String(pageSize)
        });
        if (filters?.city) params.set('city', filters.city);
        if (filters?.search) params.set('search', filters.search);
        if (filters?.format) params.set('format', filters.format);
        if (filters?.tags) params.set('tags', filters.tags);
        return api.get<PaginatedResponse<MentorshipResponse>>(`/mentorship?${params}`);
    },

    getById(id: string) {
        return api.get<MentorshipResponse>(`/mentorship/${id}`);
    },

    create(data: CreateMentorshipRequest) {
        return api.post<string>('/mentorship', data);
    },

    update(id: string, data: UpdateMentorshipRequest) {
        return api.put<string>(`/mentorship/${id}`, data);
    },

    delete(id: string) {
        return api.delete(`/mentorship/${id}`);
    },

    getByUser(userId: string, pageNumber = 1, pageSize = 10) {
        return api.get<MentorshipResponse[]>(
            `/mentorship/all-by/${userId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
        );
    },

    apply(mentorshipId: string, coverLetter: string = '') {
        return api.post('/mentorship/application-mentorship', { mentorshipId, coverLetter });
    },

    getResponses(mentorshipId: string) {
        return api.get<MentorshipApplicationResponse[]>(`/mentorship/${mentorshipId}/responses`);
    },

    updateApplicationStatus(applicationId: string, status: string) {
        return api.put(`/mentorship/application/${applicationId}/status`, { status });
    },

    uploadPhotos(mentorshipId: string, files: File[]) {
        return api.upload<string[]>(`/mentorship/${mentorshipId}/photo`, files, 'files');
    },

    uploadVideos(mentorshipId: string, files: File[]) {
        return api.upload<string[]>(`/mentorship/${mentorshipId}/video`, files, 'files');
    },

    deletePhoto(mentorshipId: string, path: string) {
        return api.delete(`/mentorship/${mentorshipId}/photo?path=${encodeURIComponent(path)}`);
    },

    deleteVideo(mentorshipId: string, path: string) {
        return api.delete(`/mentorship/${mentorshipId}/video?path=${encodeURIComponent(path)}`);
    }
};
