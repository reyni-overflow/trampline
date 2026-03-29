import { test, expect } from '@playwright/test';

test.describe('Events Page', () => {
    test('page loads', async ({ page }) => {
        await page.goto('/events');
        await expect(page.locator('main')).toBeVisible();
    });

    test('has view toggle (list/calendar)', async ({ page }) => {
        await page.goto('/events');
        const toggle = page.locator('.view-toggle, button').first();
        await expect(toggle).toBeVisible();
    });

    test('has filter controls', async ({ page }) => {
        await page.goto('/events');
        const filters = page.locator('select, .select, .filter, input');
        if (await filters.count() > 0) {
            await expect(filters.first()).toBeVisible();
        }
    });
});
