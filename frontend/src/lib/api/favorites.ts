import { api } from './client';

export type FavoriteType = 'Job' | 'Company' | 'Event' | 'Mentorship';

export interface FavoriteResponse {
    id: string;
    targetId: string;
    type: FavoriteType;
    createdAt: string;
}

export const favoritesApi = {
    getAll() {
        return api.get<FavoriteResponse[]>('/favorite');
    },

    toggle(targetId: string, type: FavoriteType) {
        return api.post<{ added: boolean; id?: string }>('/favorite', { targetId, type });
    }
};
