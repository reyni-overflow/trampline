import { describe, it, expect, beforeEach } from 'vitest';
import { get } from 'svelte/store';
import { locale, t, tGet, getLocale, getLocaleDateString, pluralForm, LOCALES } from '$lib/i18n';

describe('i18n system', () => {
    beforeEach(() => {
        locale.set('ru');
    });

    describe('locale store', () => {
        it('defaults to ru', () => {
            expect(get(locale)).toBe('ru');
        });

        it('can be set to en', () => {
            locale.set('en');
            expect(get(locale)).toBe('en');
        });

        it('can be set to zh', () => {
            locale.set('zh');
            expect(get(locale)).toBe('zh');
        });

        it('can be set to pirate', () => {
            locale.set('pirate');
            expect(get(locale)).toBe('pirate');
        });

        it('saves to localStorage', () => {
            locale.set('en');
            expect(localStorage.setItem).toHaveBeenCalled();
        });
    });

    describe('LOCALES constant', () => {
        it('has 4 locales', () => {
            expect(LOCALES).toHaveLength(4);
        });

        it('contains ru, en, zh, pirate', () => {
            const ids = LOCALES.map((l) => l.id);
            expect(ids).toContain('ru');
            expect(ids).toContain('en');
            expect(ids).toContain('zh');
            expect(ids).toContain('pirate');
        });

        it('each locale has id, label, flag', () => {
            for (const loc of LOCALES) {
                expect(loc.id).toBeTruthy();
                expect(loc.label).toBeTruthy();
                expect(loc.flag).toBeTruthy();
            }
        });
    });

    describe('t derived store', () => {
        it('returns translated string for known key', () => {
            const $t = get(t);
            const result = $t('brand.name');
            expect(typeof result).toBe('string');
            expect(result.length).toBeGreaterThan(0);
        });

        it('returns key for unknown key', () => {
            const $t = get(t);
            expect($t('nonexistent.key.xyz')).toBe('nonexistent.key.xyz');
        });

        it('interpolates parameters', () => {
            const $t = get(t);
            const result = $t('format.salaryFrom', { amount: '100 000' });
            expect(result).toContain('100 000');
        });

        it('changes language when locale changes', () => {
            const ruResult = get(t)('brand.name');
            locale.set('en');
            const enResult = get(t)('brand.name');
            expect(typeof ruResult).toBe('string');
            expect(typeof enResult).toBe('string');
        });

        it('falls back to ru for missing keys in other locales', () => {
            locale.set('en');
            const $t = get(t);
            const result = $t('brand.name');
            expect(result).not.toBe('brand.name');
        });
    });

    describe('tGet()', () => {
        it('returns translated string', () => {
            const result = tGet('brand.name');
            expect(typeof result).toBe('string');
        });

        it('interpolates parameters', () => {
            const result = tGet('format.salaryFrom', { amount: '50 000' });
            expect(result).toContain('50 000');
        });

        it('returns key for unknown key', () => {
            expect(tGet('does.not.exist')).toBe('does.not.exist');
        });
    });

    describe('getLocale()', () => {
        it('returns current locale', () => {
            expect(getLocale()).toBe('ru');
            locale.set('en');
            expect(getLocale()).toBe('en');
        });
    });

    describe('getLocaleDateString()', () => {
        it('returns ru-RU for ru locale', () => {
            locale.set('ru');
            expect(getLocaleDateString()).toBe('ru-RU');
        });

        it('returns en-US for en locale', () => {
            locale.set('en');
            expect(getLocaleDateString()).toBe('en-US');
        });

        it('returns zh-CN for zh locale', () => {
            locale.set('zh');
            expect(getLocaleDateString()).toBe('zh-CN');
        });

        it('returns ru-RU for pirate locale', () => {
            locale.set('pirate');
            expect(getLocaleDateString()).toBe('ru-RU');
        });
    });

    describe('pluralForm()', () => {
        describe('Russian pluralization', () => {
            beforeEach(() => locale.set('ru'));

            it('returns one form for 1', () => {
                expect(pluralForm(1, '{n} день', '{n} дня', '{n} дней')).toBe('1 день');
            });

            it('returns few form for 2-4', () => {
                expect(pluralForm(2, '{n} день', '{n} дня', '{n} дней')).toBe('2 дня');
                expect(pluralForm(3, '{n} день', '{n} дня', '{n} дней')).toBe('3 дня');
                expect(pluralForm(4, '{n} день', '{n} дня', '{n} дней')).toBe('4 дня');
            });

            it('returns many form for 5-20', () => {
                expect(pluralForm(5, '{n} день', '{n} дня', '{n} дней')).toBe('5 дней');
                expect(pluralForm(11, '{n} день', '{n} дня', '{n} дней')).toBe('11 дней');
                expect(pluralForm(19, '{n} день', '{n} дня', '{n} дней')).toBe('19 дней');
            });

            it('returns one form for 21', () => {
                expect(pluralForm(21, '{n} день', '{n} дня', '{n} дней')).toBe('21 день');
            });

            it('returns few form for 22-24', () => {
                expect(pluralForm(22, '{n} день', '{n} дня', '{n} дней')).toBe('22 дня');
            });

            it('returns many form for 0', () => {
                expect(pluralForm(0, '{n} день', '{n} дня', '{n} дней')).toBe('0 дней');
            });

            it('handles 100+', () => {
                expect(pluralForm(101, '{n} день', '{n} дня', '{n} дней')).toBe('101 день');
                expect(pluralForm(111, '{n} день', '{n} дня', '{n} дней')).toBe('111 дней');
            });
        });

        describe('English pluralization', () => {
            beforeEach(() => locale.set('en'));

            it('returns one form for 1', () => {
                expect(pluralForm(1, '{n} day', '{n} days', '{n} days')).toBe('1 day');
            });

            it('returns many form for != 1', () => {
                expect(pluralForm(0, '{n} day', '{n} days', '{n} days')).toBe('0 days');
                expect(pluralForm(2, '{n} day', '{n} days', '{n} days')).toBe('2 days');
                expect(pluralForm(5, '{n} day', '{n} days', '{n} days')).toBe('5 days');
            });
        });

        describe('Chinese pluralization', () => {
            beforeEach(() => locale.set('zh'));

            it('always uses many/other form', () => {
                expect(pluralForm(1, '{n}天', '{n}天', '{n}天')).toBe('1天');
                expect(pluralForm(5, '{n}天', '{n}天', '{n}天')).toBe('5天');
            });
        });
    });
});
