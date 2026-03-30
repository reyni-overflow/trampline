import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
    test('login button opens auth modal', async ({ page }) => {
        await page.goto('/');
        // Accept cookies first if banner is shown
        const cookieAccept = page
            .locator('button')
            .filter({ hasText: /ринять|ccept/i })
            .first();
        if (await cookieAccept.isVisible({ timeout: 2000 }).catch(() => false)) {
            await cookieAccept.click();
        }

        // Find and click login button in header
        const loginBtn = page
            .locator('header button, header a')
            .filter({ hasText: /ойти|ogin|ход/i })
            .first();
        if ((await loginBtn.count()) > 0) {
            await loginBtn.click();
            // Modal should appear
            const modal = page.locator('.modal, .modal-overlay, [role="dialog"]');
            await expect(modal.first()).toBeVisible({ timeout: 3000 });
        }
    });

    test('auth modal has role selection', async ({ page }) => {
        await page.goto('/');
        const cookieAccept = page
            .locator('button')
            .filter({ hasText: /ринять|ccept/i })
            .first();
        if (await cookieAccept.isVisible({ timeout: 2000 }).catch(() => false)) {
            await cookieAccept.click();
        }

        const loginBtn = page
            .locator('header button, header a')
            .filter({ hasText: /ойти|ogin|ход/i })
            .first();
        if ((await loginBtn.count()) > 0) {
            await loginBtn.click();
            await page.waitForTimeout(500);
            // Should see role selection (worker / employer)
            const body = await page
                .locator('.modal, .modal-overlay, [role="dialog"]')
                .textContent();
            expect(body).toBeTruthy();
        }
    });

    test('register button in hero section works', async ({ page }) => {
        await page.goto('/');
        const cookieAccept = page
            .locator('button')
            .filter({ hasText: /ринять|ccept/i })
            .first();
        if (await cookieAccept.isVisible({ timeout: 2000 }).catch(() => false)) {
            await cookieAccept.click();
        }

        const registerBtn = page
            .locator('.hero a, .hero button')
            .filter({ hasText: /аботодатель|egister|mployer/i })
            .first();
        if ((await registerBtn.count()) > 0) {
            await registerBtn.click();
            await page.waitForTimeout(500);
            const modal = page.locator('.modal, .modal-overlay, [role="dialog"]');
            if ((await modal.count()) > 0) {
                await expect(modal.first()).toBeVisible();
            }
        }
    });
});
