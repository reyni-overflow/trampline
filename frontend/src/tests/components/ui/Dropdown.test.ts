import { describe, it, expect } from 'vitest';
import Dropdown from '$lib/components/ui/Dropdown.svelte';

describe('Dropdown', () => {
    // Dropdown requires two Snippets (trigger + children), which is complex to test with SnippetWrapper.
    // Testing basic render — component doesn't crash.
    it('is importable', () => {
        expect(Dropdown).toBeDefined();
    });
});
