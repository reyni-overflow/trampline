import { api } from './client';
import { cached } from '$lib/utils/cache';

export interface TagResponse {
    id: string;
    name: string;
    category: string;
    lvl: number;
}

export interface TagRequest {
    name: string;
    category: string;
    lvl: number;
}

export interface JobResponse {
    id: string;
    employeeId: string;
    userId: string;
    type: 'Work' | 'Internship' | 'Mentorship' | 'Event';
    title: string;
    description: string;
    address: string;
    city: string;
    country: string;
    street: string;
    geoLat: number;
    geoLon: number;
    salaryFrom: number | null;
    salaryTo: number | null;
    tags: TagResponse[];
    photos: string[];
    videos: string[];
    format: 'Remote' | 'Hybrid' | 'Office';
    createdAt: string;
    updatedAt: string;
    deletedAt: string | null;
    endedAt: string;
    isActive: boolean;
    isPublished: boolean;
    views: number;
    isFavorited?: boolean;
    companyName?: string;
    customTags?: string[];
}

export interface CreateJobRequest {
    title: string;
    description: string;
    address: string;
    salaryFrom?: number;
    salaryTo?: number;
    tags?: TagRequest[];
    type?: 'Work' | 'Internship' | 'Mentorship' | 'Event';
    format?: 'Remote' | 'Hybrid' | 'Office';
    endedAt?: string;
    customTags?: string[];
}

export interface UpdateJobRequest {
    title?: string;
    description?: string;
    address?: string;
    city?: string;
    country?: string;
    isPublished?: boolean;
    salaryFrom?: number;
    salaryTo?: number;
    tags?: TagRequest[];
    endedAt?: string;
    customTags?: string[];
}

export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    totalPages: number;
    pageSize: number;
    currentPage: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}

export interface JobApplicationResponse {
    id: string;
    workerProfileId: string;
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
    jobId: string;
    jobTitle: string | null;
    companyName: string | null;
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

export interface TagStatsResponse {
    id: string;
    name: string;
    category: string;
    jobCount: number;
    eventCount: number;
    totalCount: number;
}

export const jobsApi = {
    getAll(
        pageNumber = 1,
        pageSize = 10,
        filters?: {
            city?: string;
            salaryMin?: number;
            salaryMax?: number;
            search?: string;
            type?: string;
            format?: string;
            tags?: string;
        }
    ) {
        const params = new URLSearchParams({
            pageNumber: String(pageNumber),
            pageSize: String(pageSize)
        });
        if (filters?.city) params.set('city', filters.city);
        if (filters?.salaryMin) params.set('salaryMin', String(filters.salaryMin));
        if (filters?.salaryMax && filters.salaryMax < 100000000)
            params.set('salaryMax', String(filters.salaryMax));
        if (filters?.search) params.set('search', filters.search);
        if (filters?.type) params.set('type', filters.type);
        if (filters?.format) params.set('format', filters.format);
        if (filters?.tags) params.set('tags', filters.tags);
        return api.get<PaginatedResponse<JobResponse>>(`/job?${params}`);
    },

    getById(id: string) {
        return api.get<JobResponse>(`/job/${id}`);
    },

    getByIds(ids: string[]) {
        return api.post<JobResponse[]>('/job/batch', { ids });
    },

    create(data: CreateJobRequest) {
        return api.post<JobResponse>('/job', data);
    },

    update(id: string, data: UpdateJobRequest) {
        return api.put<JobResponse>(`/job/${id}`, data);
    },

    delete(id: string) {
        return api.delete(`/job/${id}`);
    },

    getByUser(userId: string, pageNumber = 1, pageSize = 10) {
        return api.get<JobResponse[]>(
            `/job/all-by/${userId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
        );
    },

    getMyResponseStats() {
        return api.get<{ totalResponses: number; pendingResponses: number }>(
            '/job/my-response-stats'
        );
    },

    apply(jobId: string, coverLetter: string = '') {
        return api.post('/job/application-job', { jobId, coverLetter });
    },

    getResponses(jobId: string) {
        return api.get<JobApplicationResponse[]>(`/job/${jobId}/responses`);
    },

    updateApplicationStatus(applicationId: string, status: string) {
        return api.put(`/job/application/${applicationId}/status`, { status });
    },

    getTags: cached('tags', () => api.get<TagResponse[]>('/job/tags'), 120_000),

    getTagStats: cached('tagStats', () => api.get<TagStatsResponse[]>('/job/tags/stats'), 120_000),

    uploadPhotos(jobId: string, files: File[]) {
        return api.upload<string[]>(`/job/${jobId}/photo`, files, 'files');
    },

    uploadVideos(jobId: string, files: File[]) {
        return api.upload<string[]>(`/job/${jobId}/video`, files, 'files');
    },

    deletePhoto(jobId: string, path: string) {
        return api.delete(`/job/${jobId}/photo?path=${encodeURIComponent(path)}`);
    },

    deleteVideo(jobId: string, path: string) {
        return api.delete(`/job/${jobId}/video?path=${encodeURIComponent(path)}`);
    },

    withdrawApplication(applicationId: string) {
        return api.put(`/job/application/${applicationId}/withdraw`);
    }
};
