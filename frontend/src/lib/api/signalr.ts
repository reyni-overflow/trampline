import { browser } from '$app/environment';
import { env } from '$env/dynamic/public';
import * as signalR from '@microsoft/signalr';
import { notifications } from '$lib/stores/notifications';
import { tGet } from '$lib/i18n';

let connection: signalR.HubConnection | null = null;

function resolveHubUrl(): string {
    if (env.PUBLIC_API_URL) return `${env.PUBLIC_API_URL}/hubs/notifications`;
    if (!browser) return 'http://localhost:7103/hubs/notifications';

    const host = window.location.hostname;
    if (host === 'trampline.localhost') return '/api/hubs/notifications';

    return `${window.location.protocol}//${host}:7103/hubs/notifications`;
}

const notificationTitles: Record<string, string> = {
    application_status: 'notifications.applicationStatus',
    new_application: 'notifications.newApplication',
    contact_request: 'notifications.contactRequest',
    job_recommendation: 'notifications.jobRecommendation',
    verification_status: 'notifications.verificationStatus',
    job_moderation: 'notifications.jobModeration',
    event_moderation: 'notifications.eventModeration',
};

export async function startConnection() {
    if (!browser || connection) return;

    connection = new signalR.HubConnectionBuilder()
        .withUrl(resolveHubUrl(), { withCredentials: true })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Error)
        .build();

    connection.on('Notification', (data: { type: string; payload: Record<string, unknown> }) => {
        const titleKey = notificationTitles[data.type] || 'notifications.generic';
        notifications.add({
            type: (data.type as 'application_status' | 'contact_request' | 'new_application' | 'job_recommendation' | 'verification_status' | 'job_moderation' | 'event_moderation') || 'system',
            title: tGet(titleKey),
            message: JSON.stringify(data.payload),
            link: buildLink(data.type, data.payload),
        });
    });

    try {
        await connection.start();
    } catch {
        connection = null;
    }
}

export async function stopConnection() {
    if (connection) {
        await connection.stop();
        connection = null;
    }
}

function buildLink(type: string, payload: Record<string, unknown>): string | undefined {
    switch (type) {
        case 'application_status':
            return '/dashboard/applications';
        case 'new_application':
            return payload.jobId ? `/dashboard/jobs/${payload.jobId}/responses` : '/dashboard/jobs';
        case 'contact_request':
            return '/dashboard/contacts';
        case 'job_recommendation':
            return payload.jobId ? `/jobs/${payload.jobId}` : '/jobs';
        case 'verification_status':
            return '/dashboard/profile';
        case 'job_moderation':
        case 'event_moderation':
            return '/dashboard/jobs';
        default:
            return undefined;
    }
}
