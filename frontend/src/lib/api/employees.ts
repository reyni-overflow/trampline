import { api } from './client';
import type { EmployeeProfileResponse } from './auth';

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
    getAll(pageNumber = 1, pageSize = 10) {
        return api.get<{ items: EmployeeProfileResponse[]; total: number }>(`/employee?pageNumber=${pageNumber}&pageSize=${pageSize}`);
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
