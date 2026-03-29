import { api } from './client';

export interface ReviewResponse {
    id: string;
    authorName: string;
    authorRole: string;
    text: string;
    rating: number;
    isApproved: boolean;
    createdAt: string;
}

export const reviewsApi = {
    getApproved() {
        return api.get<ReviewResponse[]>('/review');
    },

    getAll() {
        return api.get<ReviewResponse[]>('/review/all');
    },

    create(data: { text: string; rating: number }) {
        return api.post('/review', data);
    },

    approve(id: string) {
        return api.put(`/review/${id}/approve`);
    },

    delete(id: string) {
        return api.delete(`/review/${id}`);
    }
};
