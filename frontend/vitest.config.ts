import { svelte } from '@sveltejs/vite-plugin-svelte';
import { defineConfig } from 'vitest/config';
import path from 'node:path';

export default defineConfig({
    plugins: [
        svelte({
            compilerOptions: {
                runes: true
            }
        })
    ],

    resolve: {
        conditions: ['browser'],
        alias: {
            '$lib': path.resolve('./src/lib'),
            '$app/environment': path.resolve('./src/tests/mocks/app-environment.ts'),
            '$app/navigation': path.resolve('./src/tests/mocks/app-navigation.ts'),
            '$app/stores': path.resolve('./src/tests/mocks/app-stores.ts'),
            '$env/dynamic/public': path.resolve('./src/tests/mocks/env-dynamic-public.ts'),
            '$styles': path.resolve('./src/styles')
        }
    },

    test: {
        include: ['src/**/*.test.ts'],
        environment: 'jsdom',
        globals: true,
        setupFiles: ['src/tests/setup.ts'],
        css: true,
        mockReset: false,
        restoreMocks: false,
        coverage: {
            provider: 'v8',
            include: ['src/lib/**/*.ts', 'src/lib/**/*.svelte'],
            exclude: ['src/lib/i18n/ru.ts', 'src/lib/i18n/en.ts', 'src/lib/i18n/zh.ts', 'src/lib/i18n/pirate.ts'],
            reporter: ['text', 'html', 'lcov']
        }
    }
});
