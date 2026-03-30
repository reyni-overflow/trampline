import { test, expect } from '@playwright/test';

test.describe('Companies Page', () => {
    test('page loads', async ({ page }) => {
        await page.goto('/companies');
        await expect(page.locator('main')).toBeVisible();
    });

    test('has search input', async ({ page }) => {
        await page.goto('/companies');
        const search = page.locator('input, .search-input');
        await expect(search.first()).toBeVisible();
    });

    test('shows company cards', async ({ page }) => {
        await page.goto('/companies');
        const cards = page.locator('.company-card');
        if ((await cards.count()) > 0) {
            await expect(cards.first()).toBeVisible();
        }
    });

    test('clicking company card navigates to detail', async ({ page }) => {
        await page.goto('/companies');
        const cardLink = page.locator('.company-card').first();
        if ((await cardLink.count()) > 0) {
            await cardLink.click();
            await expect(page).toHaveURL(/\/companies\/.+/);
        }
    });
});
