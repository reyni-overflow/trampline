import { api } from './client';

export interface LoginRequest {
    contact: string;
    password: string;
}

export interface RegisterRequest {
    name: string;
    email: string;
    password: string;
    role: 'Worker' | 'Employee';
}

export interface AuthResponse {
    id: string;
}

export interface LoginResponse {
    id?: string;
    requiresTotp?: boolean;
    challengeId?: string;
}

export interface UserResponse {
    id: string;
    email: string;
    nickname: string;
    avatar: string | null;
    role: 'Worker' | 'Employee' | 'Admin';
    isTotpEnabled?: boolean;
    isSuperAdmin?: boolean;
    mustChangePassword?: boolean;
    workerProfile: WorkerProfileResponse | null;
    employeeProfile: EmployeeProfileResponse | null;
}

export interface WorkerInfo {
    university: string;
    course: number;
    admissionAt?: string;
    graduationAt?: string;
}

export interface WorkerProfileResponse {
    name: string;
    lastName: string;
    patronymic: string;
    about: string | null;
    photo: string | null;
    resume: string | null;
    info: WorkerInfo | null;
    skills: string[];
    repos: string[];
}

export interface EmployeeProfileResponse {
    id: string;
    userId: string;
    name: string;
    description: string;
    activity: string;
    link: string | null;
    socials: string[];
    photos: string[];
    videos: string[];
    isVerified: boolean;
    verificationLevel: number;
    verifiedName: string | null;
    info: {
        address: string;
        inn: string;
        email: string;
    };
    jobs?: import('./jobs').JobResponse[];
}

export const authApi = {
    login(data: LoginRequest) {
        return api.post<LoginResponse>('/auth/login', data);
    },

    register(data: RegisterRequest) {
        return api.post<AuthResponse>('/auth/register', data);
    },

    refresh() {
        return api.get<AuthResponse>('/auth/refresh');
    },

    me() {
        return api.get<UserResponse>('/auth/me');
    },

    logout() {
        return api.post('/auth/logout');
    },

    sessions() {
        return api.get<SessionResponse[]>('/auth/sessions');
    },

    closeAllSessions() {
        return api.post('/auth/sessions');
    },

    closeSession(sessionId: string) {
        return api.delete(`/auth/sessions/${sessionId}`);
    },

    forgotPassword(email: string) {
        return api.post<{ message: string; debugCode?: string }>('/auth/forgot-password', {
            email
        });
    },

    resetPassword(email: string, code: string, newPassword: string) {
        return api.post('/auth/reset-password', { email, code, newPassword });
    },

    verifyEmail(email: string, code: string) {
        return api.post<{ message: string }>('/auth/verify-email', { email, code });
    },

    resendVerification(email: string) {
        return api.post<{ message: string; debugCode?: string }>('/auth/resend-verification', {
            email
        });
    },

    changePassword(currentPassword: string, newPassword: string) {
        return api.put('/auth/change-password', { currentPassword, newPassword });
    },

    deleteAccount(password: string) {
        return api.post('/auth/delete-account', { password });
    },

    uploadAvatar(file: File) {
        return api.upload<string>('/auth/avatar', [file], 'file');
    },

    updatePrivacy(isPrivate: boolean) {
        return api.put('/auth/privacy', { isPrivate });
    },

    totpSetup() {
        return api.post<{ secret: string; uri: string }>('/auth/totp/setup');
    },

    totpEnable(code: string) {
        return api.post('/auth/totp/enable', { code });
    },

    totpDisable(code: string) {
        return api.post('/auth/totp/disable', { code });
    },

    totpVerify(challengeId: string, code: string) {
        return api.post<AuthResponse>('/auth/totp/verify', { challengeId, code });
    }
};

export interface SessionResponse {
    id: string;
    userId: string;
    deviceName: string | null;
    userAgent: { ip: string; agent: string };
    createdAt: string;
    lastUsedAt: string | null;
    expiresAt: string;
    revokedAt: string | null;
    isActive: boolean;
}
