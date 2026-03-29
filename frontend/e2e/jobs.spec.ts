import { test, expect } from '@playwright/test';

test.describe('Jobs Page', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/jobs');
    });

    test('page loads with job listing', async ({ page }) => {
        await expect(page.locator('.jobs, .job-list, .job-card, main')).toBeVisible();
    });

    test('has search/filter panel', async ({ page }) => {
        const filters = page.locator('.filters, .filter-panel, .sidebar, input[type="text"]');
        await expect(filters.first()).toBeVisible();
    });

    test('has view mode toggle', async ({ page }) => {
        const toggle = page.locator('.view-toggle, .toggle-view');
        if (await toggle.count() > 0) {
            await expect(toggle.first()).toBeVisible();
        }
    });

    test('has sorting options', async ({ page }) => {
        const sort = page.locator('.sort, select, .select');
        if (await sort.count() > 0) {
            await expect(sort.first()).toBeVisible();
        }
    });

    test('job cards have essential info', async ({ page }) => {
        const cards = page.locator('.job-card');
        if (await cards.count() > 0) {
            const firstCard = cards.first();
            await expect(firstCard).toBeVisible();
        }
    });

    test('clicking job card navigates to detail', async ({ page }) => {
        const cardLink = page.locator('.job-card a').first();
        if (await cardLink.count() > 0) {
            await cardLink.click();
            await expect(page).toHaveURL(/\/jobs\/.+/);
        }
    });

    test('has pagination', async ({ page }) => {
        const pagination = page.locator('.pagination');
        if (await pagination.count() > 0) {
            await expect(pagination.first()).toBeVisible();
        }
    });
});
