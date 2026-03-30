import { api } from './client';
import type { EmployeeProfileResponse } from './auth';
import type { PaginatedResponse } from './jobs';

export interface EmployeeProfileRequest {
    name: string;
    description: string;
    activity: string;
    link?: string;
    info?: {
        address: string;
        inn: string;
        email: string;
    };
}

export interface FindResponse {
    value: string;
    inn: string;
    kpp: string;
    orgn: string;
    type: string;
}

export const employeesApi = {
    getAll(pageNumber = 1, pageSize = 10, filters?: { search?: string; activity?: string }) {
        const params = new URLSearchParams({
            pageNumber: String(pageNumber),
            pageSize: String(pageSize)
        });
        if (filters?.search) params.set('search', filters.search);
        if (filters?.activity) params.set('activity', filters.activity);
        return api.get<PaginatedResponse<EmployeeProfileResponse>>(`/employee?${params}`);
    },

    getById(id: string) {
        return api.get<EmployeeProfileResponse>(`/employee/${id}`);
    },

    getByIds(ids: string[]) {
        return api.post<EmployeeProfileResponse[]>('/employee/batch', { ids });
    },

    updateProfile(data: EmployeeProfileRequest) {
        return api.put<EmployeeProfileResponse>('/employee', data);
    },

    uploadPhotos(files: File[]) {
        return api.upload<string[]>('/employee/photo', files);
    },

    uploadVideos(files: File[]) {
        return api.upload<string[]>('/employee/video', files);
    },

    verify() {
        return api.get<FindResponse>('/employee/verify');
    },

    deletePhoto(path: string) {
        return api.delete(`/employee/photo?path=${encodeURIComponent(path)}`);
    },

    deleteVideo(path: string) {
        return api.delete(`/employee/video?path=${encodeURIComponent(path)}`);
    }
};
