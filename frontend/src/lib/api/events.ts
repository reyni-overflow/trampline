import { api } from './client';
import type { PaginatedResponse, TagRequest } from './jobs';

export interface EventResponse {
    id: string;
    employeeId: string;
    userId: string;
    title: string;
    description: string;
    format: 'Remote' | 'Hybrid' | 'Office';
    address: string;
    city: string;
    street: string;
    country: string;
    geoLat: number;
    geoLon: number;
    salaryFrom: number | null;
    salaryTo: number | null;
    views: number;
    isActive: boolean;
    tags: { id: string; name: string; category: string; lvl: number }[];
    photos: string[];
    videos: string[];
    createdAt: string;
    updatedAt: string;
    endedAt: string;
    deletedAt: string | null;
    startDate: string | null;
    isFavorited?: boolean;
    companyName?: string;
}

export interface CreateEventRequest {
    title: string;
    description: string;
    address: string;
    format?: 'Remote' | 'Hybrid' | 'Office';
    salaryFrom?: number;
    salaryTo?: number;
    tags?: TagRequest[];
    startDate?: string;
    endedAt?: string;
}

export interface UpdateEventRequest {
    title?: string;
    description?: string;
    address?: string;
    city?: string;
    country?: string;
    isActive?: boolean;
    salaryFrom?: number;
    salaryTo?: number;
    tags?: TagRequest[];
    startDate?: string;
    endedAt?: string;
}

export interface EventApplicationResponse {
    id: string;
    coverLetter: string;
    eventId: string;
    eventTitle?: string;
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

export const eventsApi = {
    getAll(pageNumber = 1, pageSize = 10, filters?: { city?: string; search?: string }) {
        const params = new URLSearchParams({
            pageNumber: String(pageNumber),
            pageSize: String(pageSize)
        });
        if (filters?.city) params.set('city', filters.city);
        if (filters?.search) params.set('search', filters.search);
        return api.get<PaginatedResponse<EventResponse>>(`/event?${params}`);
    },

    getById(id: string) {
        return api.get<EventResponse>(`/event/${id}`);
    },

    getByIds(ids: string[]) {
        return api.post<EventResponse[]>('/event/batch', { ids });
    },

    create(data: CreateEventRequest) {
        return api.post<string>('/event', data);
    },

    update(id: string, data: UpdateEventRequest) {
        return api.put<string>(`/event/${id}`, data);
    },

    delete(id: string) {
        return api.delete(`/event/${id}`);
    },

    getByUser(userId: string, pageNumber = 1, pageSize = 10) {
        return api.get<EventResponse[]>(
            `/event/all-by/${userId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
        );
    },

    apply(eventId: string, coverLetter: string = '') {
        return api.post('/event/application-event', { eventId, coverLetter });
    },

    getResponses(eventId: string) {
        return api.get<EventApplicationResponse[]>(`/event/${eventId}/responses`);
    },

    updateApplicationStatus(applicationId: string, status: string) {
        return api.put(`/event/application/${applicationId}/status`, { status });
    },

    uploadPhotos(eventId: string, files: File[]) {
        return api.upload<string[]>(`/event/${eventId}/photo`, files, 'files');
    },

    uploadVideos(eventId: string, files: File[]) {
        return api.upload<string[]>(`/event/${eventId}/video`, files, 'files');
    },

    deletePhoto(eventId: string, path: string) {
        return api.delete(`/event/${eventId}/photo?path=${encodeURIComponent(path)}`);
    },

    deleteVideo(eventId: string, path: string) {
        return api.delete(`/event/${eventId}/video?path=${encodeURIComponent(path)}`);
    }
};
