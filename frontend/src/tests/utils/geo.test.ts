import { describe, it, expect } from 'vitest';
import { CITY_COORDS, getCityCoords } from '$lib/utils/geo';

describe('geo utils', () => {
    describe('CITY_COORDS', () => {
        it('contains Moscow coordinates', () => {
            expect(CITY_COORDS['Москва']).toBeDefined();
            expect(CITY_COORDS['Москва'][0]).toBeCloseTo(55.7558, 2);
            expect(CITY_COORDS['Москва'][1]).toBeCloseTo(37.6173, 2);
        });

        it('contains Saint Petersburg coordinates', () => {
            expect(CITY_COORDS['Санкт-Петербург']).toBeDefined();
        });

        it('contains Kazan coordinates', () => {
            expect(CITY_COORDS['Казань']).toBeDefined();
        });

        it('contains Novosibirsk coordinates', () => {
            expect(CITY_COORDS['Новосибирск']).toBeDefined();
        });

        it('contains Yekaterinburg coordinates', () => {
            expect(CITY_COORDS['Екатеринбург']).toBeDefined();
        });

        it('has 5 cities', () => {
            expect(Object.keys(CITY_COORDS)).toHaveLength(5);
        });

        it('all coordinates are valid lat/lon pairs', () => {
            for (const [_city, [lat, lon]] of Object.entries(CITY_COORDS)) {
                expect(lat).toBeGreaterThanOrEqual(-90);
                expect(lat).toBeLessThanOrEqual(90);
                expect(lon).toBeGreaterThanOrEqual(-180);
                expect(lon).toBeLessThanOrEqual(180);
            }
        });
    });

    describe('getCityCoords()', () => {
        it('returns known city coordinates', () => {
            const [lat, lon] = getCityCoords('Москва');
            expect(lat).toBeCloseTo(55.7558, 2);
            expect(lon).toBeCloseTo(37.6173, 2);
        });

        it('returns fallback coords for unknown city', () => {
            const [lat, lon] = getCityCoords('НеизвестныйГород');
            expect(lat).toBeCloseTo(55.75, 1);
            expect(lon).toBeCloseTo(37.62, 1);
        });

        it('offsets fallback coords by index', () => {
            const [lat1, lon1] = getCityCoords('Unknown1', 0);
            const [lat2, lon2] = getCityCoords('Unknown2', 1);
            expect(lat2).toBeGreaterThan(lat1);
            expect(lon2).toBeGreaterThan(lon1);
        });

        it('returns tuple of two numbers', () => {
            const result = getCityCoords('Москва');
            expect(Array.isArray(result)).toBe(true);
            expect(result).toHaveLength(2);
            expect(typeof result[0]).toBe('number');
            expect(typeof result[1]).toBe('number');
        });
    });
});
