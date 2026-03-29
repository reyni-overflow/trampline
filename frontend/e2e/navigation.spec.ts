import { test, expect } from '@playwright/test';

test.describe('Navigation', () => {
    test('homepage loads with hero section', async ({ page }) => {
        await page.goto('/');
        await expect(page.locator('h1, .hero-title')).toBeVisible();
    });

    test('homepage has header with logo', async ({ page }) => {
        await page.goto('/');
        await expect(page.locator('header')).toBeVisible();
    });

    test('homepage has footer', async ({ page }) => {
        await page.goto('/');
        await expect(page.locator('footer')).toBeVisible();
    });

    test('navigate to jobs page', async ({ page }) => {
        await page.goto('/');
        await page.click('a[href="/jobs"]');
        await expect(page).toHaveURL(/\/jobs/);
    });

    test('navigate to companies page', async ({ page }) => {
        await page.goto('/');
        await page.click('a[href="/companies"]');
        await expect(page).toHaveURL(/\/companies/);
    });

    test('navigate to events page', async ({ page }) => {
        await page.goto('/');
        await page.click('a[href="/events"]');
        await expect(page).toHaveURL(/\/events/);
    });

    test('navigate to map page', async ({ page }) => {
        await page.goto('/');
        await page.click('a[href="/map"]');
        await expect(page).toHaveURL(/\/map/);
    });

    test('navigate to help page via footer', async ({ page }) => {
        await page.goto('/');
        const helpLink = page.locator('footer a[href="/help"]');
        if (await helpLink.count() > 0) {
            await helpLink.first().click();
            await expect(page).toHaveURL(/\/help/);
        }
    });

    test('navigate to privacy page via footer', async ({ page }) => {
        await page.goto('/');
        const link = page.locator('footer a[href="/privacy"]');
        if (await link.count() > 0) {
            await link.first().click();
            await expect(page).toHaveURL(/\/privacy/);
        }
    });

    test('navigate to terms page via footer', async ({ page }) => {
        await page.goto('/');
        const link = page.locator('footer a[href="/terms"]');
        if (await link.count() > 0) {
            await link.first().click();
            await expect(page).toHaveURL(/\/terms/);
        }
    });

    test('404 page shows for invalid URL', async ({ page }) => {
        await page.goto('/nonexistent-page-xyz');
        await expect(page.locator('body')).toContainText(/404|не найден|not found/i);
    });
});
