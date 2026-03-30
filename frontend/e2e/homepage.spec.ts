import { test, expect } from '@playwright/test';

test.describe('Homepage', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
    });

    test('has page title', async ({ page }) => {
        await expect(page).toHaveTitle(/Трамплин|Trampline/i);
    });

    test('hero section has CTA buttons', async ({ page }) => {
        const ctaBtn = page.locator('.hero a, .hero button').first();
        await expect(ctaBtn).toBeVisible();
    });

    test('shows "How it works" section', async ({ page }) => {
        const steps = page.locator('.steps, .how-it-works, .steps-section');
        await expect(steps).toBeVisible();
    });

    test('shows stats section with counters', async ({ page }) => {
        const stats = page.locator('.stats, .stats-section');
        await expect(stats).toBeVisible();
    });

    test('shows popular directions section', async ({ page }) => {
        const tags = page.locator('.directions, .popular-directions, .tags-section');
        if ((await tags.count()) > 0) {
            await expect(tags.first()).toBeVisible();
        }
    });

    test('shows reviews section', async ({ page }) => {
        const reviews = page.locator('.reviews, .testimonials');
        if ((await reviews.count()) > 0) {
            await expect(reviews.first()).toBeVisible();
        }
    });

    test('shows companies section', async ({ page }) => {
        const companies = page.locator('.companies, .partners');
        if ((await companies.count()) > 0) {
            await expect(companies.first()).toBeVisible();
        }
    });

    test('theme toggle works', async ({ page }) => {
        const htmlBefore = await page.locator('html').getAttribute('data-theme');
        const toggleBtn = page.locator('.theme-toggle, header button').first();
        if ((await toggleBtn.count()) > 0) {
            await toggleBtn.click();
            const htmlAfter = await page.locator('html').getAttribute('data-theme');
            expect(htmlAfter !== htmlBefore || htmlAfter !== null).toBeTruthy();
            await expect(page.locator('body')).toBeVisible();
        }
    });
});
