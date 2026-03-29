import { describe, it, expect } from 'vitest';
import {
    detectContactType,
    formatPhone,
    validatePhone,
    validateEmail
} from '$lib/utils/contact-input';

describe('contact-input utils', () => {
    describe('detectContactType()', () => {
        it('returns "email" for string with @', () => {
            expect(detectContactType('user@mail.com')).toBe('email');
        });

        it('returns "email" for string with latin letters', () => {
            expect(detectContactType('hello')).toBe('email');
        });

        it('returns "email" for string with cyrillic letters', () => {
            expect(detectContactType('привет')).toBe('email');
        });

        it('returns "phone" for digits only', () => {
            expect(detectContactType('79991234567')).toBe('phone');
        });

        it('returns "phone" for formatted phone', () => {
            expect(detectContactType('+7 (999) 123-45-67')).toBe('phone');
        });

        it('returns "phone" for string starting with +', () => {
            expect(detectContactType('+7')).toBe('phone');
        });

        it('returns "unknown" for empty string', () => {
            expect(detectContactType('')).toBe('unknown');
        });

        it('returns "unknown" for whitespace', () => {
            expect(detectContactType('   ')).toBe('unknown');
        });
    });

    describe('formatPhone()', () => {
        it('formats Russian number starting with 7', () => {
            const result = formatPhone('79991234567');
            expect(result).toBe('+7 (999) 123-45-67');
        });

        it('converts 8 prefix to +7', () => {
            const result = formatPhone('89991234567');
            expect(result).toBe('+7 (999) 123-45-67');
        });

        it('formats partial Russian number', () => {
            const result = formatPhone('7999');
            expect(result).toContain('+7');
            expect(result).toContain('999');
        });

        it('formats Belarus number', () => {
            const result = formatPhone('375291234567');
            expect(result).toContain('+375');
        });

        it('formats Uzbekistan number', () => {
            const result = formatPhone('998901234567');
            expect(result).toContain('+998');
        });

        it('returns empty string for empty input', () => {
            expect(formatPhone('')).toBe('');
        });

        it('strips non-digit characters', () => {
            const result = formatPhone('+7 (999) 123-45-67');
            expect(result).toBe('+7 (999) 123-45-67');
        });

        it('handles single digit', () => {
            const result = formatPhone('7');
            expect(result).toContain('+7');
        });

        it('handles unknown country code with + prefix', () => {
            const result = formatPhone('11234567890');
            expect(result).toContain('+');
        });
    });

    describe('validatePhone()', () => {
        it('returns null for valid Russian number', () => {
            expect(validatePhone('79991234567')).toBeNull();
        });

        it('returns null for valid number with 8 prefix', () => {
            expect(validatePhone('89991234567')).toBeNull();
        });

        it('returns error for empty input', () => {
            expect(validatePhone('')).toBeTruthy();
        });

        it('returns error for too short number', () => {
            expect(validatePhone('7999123')).toBeTruthy();
        });

        it('returns error for too long number', () => {
            expect(validatePhone('799912345678')).toBeTruthy();
        });

        it('returns error for unsupported country code', () => {
            expect(validatePhone('11234567890')).toBeTruthy();
        });

        it('returns null for valid Belarus number', () => {
            expect(validatePhone('375291234567')).toBeNull();
        });

        it('returns null for valid Uzbekistan number', () => {
            expect(validatePhone('998901234567')).toBeNull();
        });
    });

    describe('validateEmail()', () => {
        it('returns null for valid email', () => {
            expect(validateEmail('user@example.com')).toBeNull();
        });

        it('returns error for empty string', () => {
            expect(validateEmail('')).toBeTruthy();
        });

        it('returns error for invalid email', () => {
            expect(validateEmail('not-an-email')).toBeTruthy();
        });

        it('returns error for email without TLD', () => {
            expect(validateEmail('user@example')).toBeTruthy();
        });

        it('trims whitespace before validating', () => {
            expect(validateEmail('  user@example.com  ')).toBeNull();
        });

        it('returns error for whitespace-only string', () => {
            expect(validateEmail('   ')).toBeTruthy();
        });
    });
});
