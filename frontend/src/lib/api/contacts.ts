import { api } from './client';

export type ContactStatus = 'Pending' | 'Accepted' | 'Declined';

export interface ContactResponse {
    id: string;
    contactUserId: string;
    name: string;
    lastName: string;
    patronymic: string;
    photo: string | null;
    skills: string[];
    status: ContactStatus;
    createdAt: string;
}

export interface IncomingContactResponse {
    id: string;
    contactUserId: string;
    name: string;
    lastName: string;
    patronymic: string;
    photo: string | null;
    skills: string[];
    status: ContactStatus;
    createdAt: string;
}

export interface RecommendationResponse {
    id: string;
    fromUserId: string;
    fromUserName: string;
    jobId: string;
    jobTitle: string;
    companyName: string;
    message: string | null;
    createdAt: string;
}

export const contactsApi = {
    getContacts() {
        return api.get<ContactResponse[]>('/contact');
    },

    getIncoming() {
        return api.get<IncomingContactResponse[]>('/contact/incoming');
    },

    sendRequest(receiverId: string) {
        return api.post<string>(`/contact/${receiverId}`);
    },

    accept(id: string) {
        return api.put(`/contact/${id}/accept`);
    },

    decline(id: string) {
        return api.put(`/contact/${id}/decline`);
    },

    remove(id: string) {
        return api.delete(`/contact/${id}`);
    },

    recommend(toUserId: string, jobId: string, message?: string) {
        return api.post<string>('/contact/recommend', { toUserId, jobId, message });
    },

    getRecommendations() {
        return api.get<RecommendationResponse[]>('/contact/recommendations');
    }
};
