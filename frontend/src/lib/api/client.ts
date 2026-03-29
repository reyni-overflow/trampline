import { env } from '$env/dynamic/public';
import { browser } from '$app/environment';
import { toast } from '$lib/stores/toast';
import { tGet } from '$lib/i18n';

function resolveApiUrl(): string {
    if (env.PUBLIC_API_URL) return env.PUBLIC_API_URL;
    if (!browser) return '/api';

    return '/api';
}

const BASE_URL = resolveApiUrl();

interface RequestOptions extends Omit<RequestInit, 'body'> {
    body?: unknown;
    noAuth?: boolean;
    signal?: AbortSignal;
}

interface ApiError {
    title?: string;
    status?: number;
    detail?: string;
    errors?: Record<string, string[]>;
}

class HttpClient {
    private base: string;
    private refreshFailed = false;
    private refreshCooldown: ReturnType<typeof setTimeout> | null = null;
    private activeControllers = new Set<AbortController>();

    constructor(base: string) {
        this.base = base;
    }

    createAbortController(): AbortController {
        const controller = new AbortController();
        this.activeControllers.add(controller);
        return controller;
    }

    abortAll() {
        for (const c of this.activeControllers) c.abort();
        this.activeControllers.clear();
    }

    async request<T>(path: string, options: RequestOptions = {}): Promise<T> {
        const { body, noAuth, signal, ...init } = options;

        const controller = this.createAbortController();
        const mergedSignal = signal
            ? AbortSignal.any([signal, controller.signal])
            : controller.signal;

        const headers = new Headers(init.headers);
        headers.set('X-Requested-With', 'XMLHttpRequest');
        if (body && !(body instanceof FormData)) {
            headers.set('Content-Type', 'application/json');
        }

        const config: RequestInit = {
            ...init,
            headers,
            credentials: 'include',
            signal: mergedSignal,
            body: body instanceof FormData ? body : body ? JSON.stringify(body) : undefined
        };

        try {
            let response = await fetch(`${this.base}${path}`, config);

            if (
                response.status === 401 &&
                !noAuth &&
                !path.includes('/auth/refresh') &&
                !this.refreshFailed
            ) {
                const refreshed = await this.refreshToken();
                if (refreshed) {
                    this.refreshFailed = false;
                    response = await fetch(`${this.base}${path}`, config);
                } else {
                    this.refreshFailed = true;
                    if (this.refreshCooldown) clearTimeout(this.refreshCooldown);
                    this.refreshCooldown = setTimeout(() => {
                        this.refreshFailed = false;
                    }, 10_000);
                }
            }

            if (!response.ok) {
                const error = await this.parseError(response);
                throw error;
            }

            if (response.status === 204) {
                return undefined as T;
            }

            const text = await response.text();
            if (!text) return undefined as T;

            try {
                return JSON.parse(text);
            } catch {
                return text as T;
            }
        } finally {
            this.activeControllers.delete(controller);
        }
    }

    private async refreshToken(): Promise<boolean> {
        try {
            const res = await fetch(`${this.base}/auth/refresh`, {
                credentials: 'include'
            });
            return res.ok;
        } catch {
            return false;
        }
    }

    private async parseError(response: Response): Promise<ApiError> {
        try {
            const data = await response.json();
            return { status: response.status, ...data };
        } catch {
            return {
                status: response.status,
                title: tGet('api.error'),
                detail: `HTTP ${response.status}: ${response.statusText}`
            };
        }
    }

    get<T>(path: string, options?: RequestOptions) {
        return this.request<T>(path, { ...options, method: 'GET' });
    }

    post<T>(path: string, body?: unknown, options?: RequestOptions) {
        return this.request<T>(path, { ...options, method: 'POST', body });
    }

    put<T>(path: string, body?: unknown, options?: RequestOptions) {
        return this.request<T>(path, { ...options, method: 'PUT', body });
    }

    delete<T>(path: string, options?: RequestOptions) {
        return this.request<T>(path, { ...options, method: 'DELETE' });
    }

    upload<T>(path: string, files: File[], fieldName = 'files', options?: RequestOptions) {
        const form = new FormData();
        for (const file of files) {
            form.append(fieldName, file);
        }
        return this.request<T>(path, { ...options, method: 'POST', body: form });
    }
}

export const api = new HttpClient(BASE_URL);

const errorTranslations: Record<string, string> = {
    'Invalid credentials': 'api.invalidCredentials',
    'User not found': 'api.userNotFound',
    'user not found': 'api.userNotFound',
    'Token not found': 'api.tokenNotFound',
    'token is invalid': 'api.tokenInvalid',
    'Invalid status value': 'api.invalidStatus',
    'Invalid email format': 'api.invalidEmail',
    'User with this email already exists': 'api.emailAlreadyExists',
    'Photo not found': 'api.photoNotFound',
    'Video not found': 'api.videoNotFound',
    'Not Found': 'api.notFound',
    Unauthorized: 'api.unauthorized',
    Forbidden: 'api.forbidden',
    'Bad Request': 'api.badRequest',
    'Internal Server Error': 'api.serverError',
    'Only the super administrator can create curators': 'api.superAdminOnly',
    'Only the super administrator can delete curators': 'api.superAdminOnly',
    'Only the super administrator can assign the Admin role': 'api.superAdminOnly',
    'Only the super administrator can modify admin users': 'api.superAdminOnly',
    'Only the super administrator can block admin users': 'api.superAdminOnly',
    'Only the super administrator can unblock admin users': 'api.superAdminOnly',
    'Cannot delete the super administrator': 'api.cannotDeleteSuperAdmin',
    'Invalid favorite type. Allowed: Job, Company, Event, Mentorship': 'api.invalidFavoriteType',
    'No pending TOTP verification': 'api.noPendingTotp',
    'TOTP not configured': 'api.totpNotConfigured',
    'TOTP already enabled': 'api.totpAlreadyEnabled',
    'TOTP not enabled': 'api.totpNotEnabled',
    'Company must be verified to create opportunities': 'api.companyNotVerified',
    'Password must be at least 8 characters': 'api.passwordTooShort',
    'Password must contain at least one uppercase letter': 'api.passwordNoUpper',
    'Password must contain at least one lowercase letter': 'api.passwordNoLower',
    'Password must contain at least one digit': 'api.passwordNoDigit',
    'Invalid TOTP code': 'api.invalidTotpCode',
    'User is not a curator': 'api.notCurator'
};

function localizeError(message: string): string {
    const key = errorTranslations[message];
    if (key) return tGet(key);

    for (const [pattern, translationKey] of Object.entries(errorTranslations)) {
        if (message.toLowerCase().includes(pattern.toLowerCase())) {
            return tGet(translationKey);
        }
    }

    if (/^HTTP \d{3}:/.test(message)) {
        const code = message.match(/^HTTP (\d{3})/)?.[1];
        if (code === '401') return tGet('api.unauthorized');
        if (code === '403') return tGet('api.forbidden');
        if (code === '404') return tGet('api.notFound');
        if (code === '409') return tGet('api.emailAlreadyExists');
        if (code === '500') return tGet('api.serverError');
    }

    return message;
}

export function handleApiError(error: unknown) {
    const e = error as ApiError;
    const raw = e.detail || e.title || tGet('api.unknownError');
    const message = localizeError(raw);
    toast.error(message);
}
