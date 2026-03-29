import { tGet } from '$lib/i18n';

const PHONE_PREFIXES = [
    { code: '+7', country: 'RU/KZ', length: 11 },
    { code: '+375', country: 'BY', length: 12 },
    { code: '+998', country: 'UZ', length: 12 }
];

export type ContactType = 'phone' | 'email' | 'unknown';

export function detectContactType(value: string): ContactType {
    const trimmed = value.trim();
    if (!trimmed) return 'unknown';
    if (/[a-zA-Zа-яА-ЯёЁ@]/.test(trimmed)) return 'email';
    if (/[\d+\-()\s]/.test(trimmed)) return 'phone';
    return 'unknown';
}

export function formatPhone(raw: string): string {
    const digits = raw.replace(/\D/g, '');

    if (digits.length === 0) return '';

    let normalized = digits;
    if (digits.startsWith('8') && digits.length >= 2) {
        normalized = '7' + digits.slice(1);
    }

    if (normalized.startsWith('7') || normalized.startsWith('375') || normalized.startsWith('998')) {
        let code: string;
        let rest: string;

        if (normalized.startsWith('375')) {
            code = '+375';
            rest = normalized.slice(3);
        } else if (normalized.startsWith('998')) {
            code = '+998';
            rest = normalized.slice(3);
        } else {
            code = '+7';
            rest = normalized.slice(1);
        }

        const parts: string[] = [code];
        if (rest.length > 0) parts.push(' (' + rest.slice(0, 3));
        if (rest.length >= 3) parts[parts.length - 1] += ')';
        if (rest.length > 3) parts.push(' ' + rest.slice(3, 6));
        if (rest.length > 6) parts.push('-' + rest.slice(6, 8));
        if (rest.length > 8) parts.push('-' + rest.slice(8, 10));

        return parts.join('');
    }

    return '+' + normalized;
}

export function validatePhone(raw: string): string | null {
    const digits = raw.replace(/\D/g, '');

    if (digits.length === 0) return tGet('contactInput.enterPhone');

    let normalized = digits;
    if (digits.startsWith('8')) normalized = '7' + digits.slice(1);

    const prefix = PHONE_PREFIXES.find((p) => {
        const code = p.code.replace('+', '');
        return normalized.startsWith(code);
    });

    if (!prefix) return tGet('contactInput.supportedPhones');
    if (normalized.length < prefix.length) return tGet('contactInput.phoneTooShort');
    if (normalized.length > prefix.length) return tGet('contactInput.phoneTooLong');

    return null;
}

export function validateEmail(value: string): string | null {
    const trimmed = value.trim();
    if (!trimmed) return tGet('contactInput.enterEmail');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) return tGet('contactInput.invalidEmail');
    return null;
}

