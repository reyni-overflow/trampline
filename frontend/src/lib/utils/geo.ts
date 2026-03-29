export const CITY_COORDS: Record<string, [number, number]> = {
    'Москва': [55.7558, 37.6173],
    'Санкт-Петербург': [59.9343, 30.3351],
    'Казань': [55.7887, 49.1221],
    'Новосибирск': [55.0084, 82.9357],
    'Екатеринбург': [56.8389, 60.6057],
};

export function getCityCoords(city: string, index: number = 0): [number, number] {
    return CITY_COORDS[city] || [55.75 + index * 0.01, 37.62 + index * 0.01];
}
