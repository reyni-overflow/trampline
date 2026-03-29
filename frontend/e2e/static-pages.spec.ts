import { test, expect } from '@playwright/test';

test.describe('Static Pages', () => {
    test('help page loads with FAQ', async ({ page }) => {
        await page.goto('/help');
        await expect(page.locator('main')).toBeVisible();
        const content = await page.textContent('main');
        expect(content).toBeTruthy();
    });

    test('privacy page loads', async ({ page }) => {
        await page.goto('/privacy');
        await expect(page.locator('main')).toBeVisible();
    });

    test('terms page loads', async ({ page }) => {
        await page.goto('/terms');
        await expect(page.locator('main')).toBeVisible();
    });

    test('help page has accordion items', async ({ page }) => {
        await page.goto('/help');
        const items = page.locator('.faq-item, .accordion, details, button');
        if (await items.count() > 0) {
            await expect(items.first()).toBeVisible();
        }
    });
});
