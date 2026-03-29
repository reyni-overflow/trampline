<script lang="ts">
    import type { Component } from 'svelte';

    interface Props {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        component: Component<any>;
        props?: Record<string, unknown>;
        text?: string;
    }

    let { component: Comp, props: componentProps = {}, text = 'Test Content' }: Props = $props();
</script>

<Comp {...componentProps}>
    {text}
</Comp>
