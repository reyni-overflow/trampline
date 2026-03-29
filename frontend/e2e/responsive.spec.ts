import { test, expect } from '@playwright/test';

test.describe('Responsive Design', () => {
    test('mobile: header shows burger menu', async ({ page }) => {
        await page.setViewportSize({ width: 375, height: 812 });
        await page.goto('/');
        const burger = page.locator('.burger, .hamburger, .menu-toggle, header button').first();
        await expect(burger).toBeVisible();
    });

    test('mobile: navigation is hidden by default', async ({ page }) => {
        await page.setViewportSize({ width: 375, height: 812 });
        await page.goto('/');
        const nav = page.locator('header nav');
        if (await nav.count() > 0) {
            await expect(nav.first()).not.toBeVisible();
        }
    });

    test('desktop: navigation is visible', async ({ page }) => {
        await page.setViewportSize({ width: 1280, height: 800 });
        await page.goto('/');
        const nav = page.locator('header nav');
        if (await nav.count() > 0) {
            await expect(nav.first()).toBeVisible();
        }
    });

    test('tablet: page renders correctly', async ({ page }) => {
        await page.setViewportSize({ width: 768, height: 1024 });
        await page.goto('/');
        await expect(page.locator('body')).toBeVisible();
    });

    test('mobile: jobs page is usable', async ({ page }) => {
        await page.setViewportSize({ width: 375, height: 812 });
        await page.goto('/jobs');
        await expect(page.locator('main')).toBeVisible();
    });

    test('mobile: footer renders', async ({ page }) => {
        await page.setViewportSize({ width: 375, height: 812 });
        await page.goto('/');
        await expect(page.locator('footer')).toBeVisible();
    });
});
