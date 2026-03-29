import { tGet } from '$lib/i18n';

export interface ValidationRule {
    test: (value: string) => boolean;
    message: string;
}

export function validate(value: string, rules: ValidationRule[]): string | null {
    for (const rule of rules) {
        if (!rule.test(value)) return rule.message;
    }
    return null;
}

export const required: ValidationRule = {
    test: (v) => v.trim().length > 0,
    get message() {
        return tGet('validation.required');
    }
};

export const email: ValidationRule = {
    test: (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v),
    get message() {
        return tGet('validation.email');
    }
};

export const minLength = (n: number): ValidationRule => ({
    test: (v) => v.length >= n,
    get message() {
        return tGet('validation.minLength', { n });
    }
});

export const inn: ValidationRule = {
    test: (v) => /^\d{10}$|^\d{12}$/.test(v),
    get message() {
        return tGet('validation.inn');
    }
};

export const url: ValidationRule = {
    test: (v) => !v || /^https?:\/\/.+/.test(v),
    get message() {
        return tGet('validation.url');
    }
};
