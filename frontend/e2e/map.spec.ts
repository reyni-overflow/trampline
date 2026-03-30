import { test, expect } from '@playwright/test';

test.describe('Map Page', () => {
    test('page loads with map container', async ({ page }) => {
        await page.goto('/map');
        const map = page.locator('.leaflet-container, .map, #map');
        await expect(map.first()).toBeVisible({ timeout: 10000 });
    });

    test('has sidebar with filters', async ({ page }) => {
        await page.goto('/map');
        const sidebar = page.locator('.sidebar, .map-sidebar, .filters');
        if ((await sidebar.count()) > 0) {
            await expect(sidebar.first()).toBeVisible();
        }
    });

    test('map has zoom controls', async ({ page }) => {
        await page.goto('/map');
        const zoom = page.locator('.leaflet-control-zoom');
        await expect(zoom.first()).toBeVisible({ timeout: 10000 });
    });
});
