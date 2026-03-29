import { api } from './client';

export interface FavoriteResponse {
    id: string;
    targetId: string;
    type: string;
    createdAt: string;
}

export const favoritesApi = {
    getAll() {
        return api.get<FavoriteResponse[]>('/favorite');
    },

    toggle(targetId: string, type: 'Job' | 'Company' | 'Event' | 'Mentorship') {
        return api.post<{ added: boolean; id?: string }>('/favorite', { targetId, type });
    }
};
