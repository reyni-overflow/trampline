import { describe, it, expect } from 'vitest';
import { validate, required, email, minLength, inn, url } from '$lib/utils/validation';

describe('validation utils', () => {
    describe('validate()', () => {
        it('returns null when all rules pass', () => {
            expect(validate('hello', [required])).toBeNull();
        });

        it('returns first failing rule message', () => {
            const result = validate('', [required, email]);
            expect(result).toBeTruthy();
            expect(typeof result).toBe('string');
        });

        it('returns null for empty rules array', () => {
            expect(validate('anything', [])).toBeNull();
        });

        it('stops at first failure', () => {
            const result = validate('', [required, minLength(5)]);
            expect(result).toBe(required.message);
        });
    });

    describe('required', () => {
        it('passes for non-empty string', () => {
            expect(required.test('hello')).toBe(true);
        });

        it('fails for empty string', () => {
            expect(required.test('')).toBe(false);
        });

        it('fails for whitespace-only string', () => {
            expect(required.test('   ')).toBe(false);
        });

        it('passes for string with content and spaces', () => {
            expect(required.test('  hello  ')).toBe(true);
        });

        it('has a message string', () => {
            expect(typeof required.message).toBe('string');
        });
    });

    describe('email', () => {
        it('passes for valid email', () => {
            expect(email.test('user@example.com')).toBe(true);
        });

        it('passes for email with subdomain', () => {
            expect(email.test('user@mail.example.com')).toBe(true);
        });

        it('fails for string without @', () => {
            expect(email.test('userexample.com')).toBe(false);
        });

        it('fails for string without domain', () => {
            expect(email.test('user@')).toBe(false);
        });

        it('fails for string without TLD', () => {
            expect(email.test('user@example')).toBe(false);
        });

        it('fails for empty string', () => {
            expect(email.test('')).toBe(false);
        });

        it('fails for string with spaces', () => {
            expect(email.test('user @example.com')).toBe(false);
        });

        it('has a message string', () => {
            expect(typeof email.message).toBe('string');
        });
    });

    describe('minLength()', () => {
        it('passes when string length equals n', () => {
            expect(minLength(3).test('abc')).toBe(true);
        });

        it('passes when string length exceeds n', () => {
            expect(minLength(3).test('abcd')).toBe(true);
        });

        it('fails when string length is less than n', () => {
            expect(minLength(3).test('ab')).toBe(false);
        });

        it('fails for empty string with n > 0', () => {
            expect(minLength(1).test('')).toBe(false);
        });

        it('passes for empty string with n = 0', () => {
            expect(minLength(0).test('')).toBe(true);
        });

        it('has a message string', () => {
            expect(typeof minLength(5).message).toBe('string');
        });
    });

    describe('inn', () => {
        it('passes for 10-digit INN', () => {
            expect(inn.test('1234567890')).toBe(true);
        });

        it('passes for 12-digit INN', () => {
            expect(inn.test('123456789012')).toBe(true);
        });

        it('fails for 9-digit number', () => {
            expect(inn.test('123456789')).toBe(false);
        });

        it('fails for 11-digit number', () => {
            expect(inn.test('12345678901')).toBe(false);
        });

        it('fails for 13-digit number', () => {
            expect(inn.test('1234567890123')).toBe(false);
        });

        it('fails for non-digit characters', () => {
            expect(inn.test('123456789a')).toBe(false);
        });

        it('fails for empty string', () => {
            expect(inn.test('')).toBe(false);
        });

        it('has a message string', () => {
            expect(typeof inn.message).toBe('string');
        });
    });

    describe('url', () => {
        it('passes for http URL', () => {
            expect(url.test('http://example.com')).toBe(true);
        });

        it('passes for https URL', () => {
            expect(url.test('https://example.com')).toBe(true);
        });

        it('passes for empty string (optional field)', () => {
            expect(url.test('')).toBe(true);
        });

        it('fails for URL without protocol', () => {
            expect(url.test('example.com')).toBe(false);
        });

        it('fails for ftp URL', () => {
            expect(url.test('ftp://example.com')).toBe(false);
        });

        it('has a message string', () => {
            expect(typeof url.message).toBe('string');
        });
    });
});
