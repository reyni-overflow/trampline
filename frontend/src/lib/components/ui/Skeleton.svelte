<script lang="ts">
    interface Props {
        width?: string;
        height?: string;
        radius?: string;
        circle?: boolean;
    }

    let { width = '100%', height = '1rem', radius = 'var(--radius-md)', circle = false }: Props = $props();
</script>

<span
    class="skeleton"
    class:circle
    style="width: {circle ? height : width}; height: {height}; border-radius: {circle ? '50%' : radius}"
></span>

<style>
    .skeleton {
        display: block;
        background: linear-gradient(90deg, var(--skeleton-base) 25%, var(--skeleton-shine) 50%, var(--skeleton-base) 75%);
        background-size: 200% 100%;
        animation: shimmer 1.5s infinite;
    }
</style>
