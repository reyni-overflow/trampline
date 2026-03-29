import { tGet, getLocaleDateString, pluralForm } from '$lib/i18n';

export function formatSalary(from?: number | null, to?: number | null): string {
    const fmt = (n: number) => n.toLocaleString(getLocaleDateString());

    if (from && to) return tGet('format.salaryRange', { from: fmt(from), to: fmt(to) });
    if (from) return tGet('format.salaryFrom', { amount: fmt(from) });
    if (to) return tGet('format.salaryTo', { amount: fmt(to) });
    return tGet('format.salaryNotSpecified');
}

export function timeAgo(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diff = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (diff < 60) return tGet('format.justNow');
    if (diff < 3600) return tGet('format.minutesAgo', { n: Math.floor(diff / 60) });
    if (diff < 86400) return tGet('format.hoursAgo', { n: Math.floor(diff / 3600) });
    if (diff < 2592000) {
        const days = Math.floor(diff / 86400);
        return pluralForm(
            days,
            tGet('format.daysOne', { n: days }),
            tGet('format.daysFew', { n: days }),
            tGet('format.daysMany', { n: days })
        );
    }
    return date.toLocaleDateString(getLocaleDateString(), { day: 'numeric', month: 'short', year: 'numeric' });
}

export function formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString(getLocaleDateString(), {
        day: 'numeric',
        month: 'long',
        year: 'numeric'
    });
}

export function jobTypeLabel(type: string): string {
    const map: Record<string, string> = {
        Work: tGet('format.jobWork'),
        Internship: tGet('format.jobInternship'),
        Mentorship: tGet('format.jobMentorship'),
        Event: tGet('format.jobEvent')
    };
    return map[type] || type;
}

export function formatViews(count: number): string {
    return pluralForm(
        count,
        tGet('format.viewsOne', { n: count }),
        tGet('format.viewsFew', { n: count }),
        tGet('format.viewsMany', { n: count })
    );
}

export function workFormatLabel(format: string): string {
    const map: Record<string, string> = {
        Remote: tGet('format.formatRemote'),
        Hybrid: tGet('format.formatHybrid'),
        Office: tGet('format.formatOffice')
    };
    return map[format] || format;
}
