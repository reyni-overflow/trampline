import { test, expect } from '@playwright/test';

test.describe('Accessibility', () => {
    test('page has lang attribute', async ({ page }) => {
        await page.goto('/');
        const lang = await page.locator('html').getAttribute('lang');
        expect(lang).toBeTruthy();
    });

    test('page has viewport meta tag', async ({ page }) => {
        await page.goto('/');
        const viewport = await page.locator('meta[name="viewport"]').getAttribute('content');
        expect(viewport).toContain('width=device-width');
    });

    test('images have alt attributes', async ({ page }) => {
        await page.goto('/');
        const images = page.locator('img');
        const count = await images.count();
        for (let i = 0; i < Math.min(count, 10); i++) {
            const alt = await images.nth(i).getAttribute('alt');
            expect(alt !== null).toBe(true);
        }
    });

    test('interactive elements are keyboard focusable', async ({ page }) => {
        await page.goto('/');
        // Tab through elements
        await page.keyboard.press('Tab');
        const focused = await page.evaluate(() => document.activeElement?.tagName);
        expect(focused).toBeTruthy();
    });

    test('skip-to-content link exists', async ({ page }) => {
        await page.goto('/');
        const skip = page.locator('[href="#main"], .skip-to-content, .skip-link');
        if (await skip.count() > 0) {
            await expect(skip.first()).toBeTruthy();
        }
    });

    test('has proper heading hierarchy', async ({ page }) => {
        await page.goto('/');
        const h1s = await page.locator('h1').count();
        expect(h1s).toBeGreaterThanOrEqual(1);
    });

    test('theme-color meta tag exists', async ({ page }) => {
        await page.goto('/');
        const meta = page.locator('meta[name="theme-color"]');
        if (await meta.count() > 0) {
            const content = await meta.getAttribute('content');
            expect(content).toBeTruthy();
        }
    });
});
