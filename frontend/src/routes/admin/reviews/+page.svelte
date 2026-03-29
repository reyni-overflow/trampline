<script lang="ts">
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import { toast } from '$lib/stores/toast';
    import { reviewsApi, type ReviewResponse } from '$lib/api/reviews';
    import { handleApiError } from '$lib/api/client';
    import { onMount } from 'svelte';
    import { t } from '$lib/i18n';

    let loading = $state(true);
    let reviews = $state<ReviewResponse[]>([]);

    onMount(async () => {
        try {
            reviews = await reviewsApi.getAll();
        } catch (err) {
            handleApiError(err);
        } finally {
            loading = false;
        }
    });

    async function approveReview(id: string) {
        try {
            await reviewsApi.approve(id);
            const review = reviews.find((r) => r.id === id);
            if (review) review.isApproved = true;
            reviews = [...reviews];
            toast.success($t('adminReviews.approvedMsg'));
        } catch (err) {
            handleApiError(err);
        }
    }

    async function deleteReview(id: string) {
        try {
            await reviewsApi.delete(id);
            reviews = reviews.filter((r) => r.id !== id);
            toast.success($t('adminReviews.deletedMsg'));
        } catch (err) {
            handleApiError(err);
        }
    }

    function renderStars(rating: number): string {
        return '\u2605'.repeat(rating) + '\u2606'.repeat(5 - rating);
    }
</script>

<svelte:head>
    <title>{$t('adminReviews.pageTitle')}</title>
</svelte:head>

<div class="reviews-page">
    <h1 class="page-heading">{$t('adminReviews.title')}</h1>

    {#if loading}
        <div class="reviews-list">
            {#each Array(3) as _, i (i)}
                <div class="review-row skeleton-row">
                    <div class="skeleton-avatar"></div>
                    <div class="skeleton-lines">
                        <div class="skeleton-line w50"></div>
                        <div class="skeleton-line w80"></div>
                        <div class="skeleton-line w30"></div>
                    </div>
                </div>
            {/each}
        </div>
    {:else if reviews.length === 0}
        <div class="empty">
            <svg
                width="48"
                height="48"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
                ><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" /></svg
            >
            <p>{$t('adminReviews.noReviews')}</p>
            <p>{$t('adminReviews.noReviewsHint')}</p>
        </div>
    {:else}
        <div class="reviews-list">
            {#each reviews as review (review.id)}
                <div class="review-row">
                    <div class="review-main">
                        <div class="review-header">
                            <Avatar name={review.authorName} size={36} />
                            <div class="review-author">
                                <span class="review-name">{review.authorName}</span>
                                <span class="review-role">{review.authorRole}</span>
                            </div>
                            <Badge variant={review.isApproved ? 'success' : 'warning'} size="sm">
                                {review.isApproved
                                    ? $t('adminReviews.approved')
                                    : $t('adminReviews.pending')}
                            </Badge>
                        </div>
                        <div class="review-stars">{renderStars(review.rating)}</div>
                        <p class="review-text">{review.text}</p>
                        <span class="review-date"
                            >{new Date(review.createdAt).toLocaleDateString()}</span
                        >
                    </div>
                    <div class="review-actions">
                        {#if !review.isApproved}
                            <Button size="sm" onclick={() => approveReview(review.id)}
                                >{$t('common.approve')}</Button
                            >
                        {/if}
                        <Button size="sm" variant="ghost" onclick={() => deleteReview(review.id)}>
                            <svg
                                viewBox="0 0 24 24"
                                width="14"
                                height="14"
                                fill="none"
                                stroke="var(--color-error)"
                                stroke-width="2"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                ><path d="M3 6h18" /><path
                                    d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"
                                /><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2" /></svg
                            >
                        </Button>
                    </div>
                </div>
            {/each}
        </div>
    {/if}
</div>

<style>
    .reviews-page {
        display: flex;
        flex-direction: column;
        gap: var(--space-6);
    }
    .page-heading {
        font-size: var(--font-2xl);
        font-weight: var(--weight-bold);
    }

    .reviews-list {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
    }

    .review-row {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: var(--space-4);
        padding: var(--space-5);
        background: var(--bg-secondary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-lg);
        transition: var(--transition-colors);
    }

    .review-row:hover {
        border-color: var(--border-hover);
    }

    .review-main {
        flex: 1;
        min-width: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
    }

    .review-header {
        display: flex;
        align-items: center;
        gap: var(--space-3);
    }

    .review-author {
        display: flex;
        flex-direction: column;
        flex: 1;
    }
    .review-name {
        font-size: var(--font-sm);
        font-weight: var(--weight-semibold);
    }
    .review-role {
        font-size: var(--font-xs);
        color: var(--text-secondary);
    }

    .review-stars {
        font-size: var(--font-md);
        color: var(--color-warning, #f59e0b);
        letter-spacing: 0.1em;
    }

    .review-text {
        font-size: var(--font-sm);
        color: var(--text-secondary);
        line-height: var(--leading-normal);
    }

    .review-date {
        font-size: var(--font-xs);
        color: var(--text-tertiary);
    }

    .review-actions {
        display: flex;
        gap: var(--space-2);
        flex-shrink: 0;
        align-items: center;
    }

    .empty {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-3);
        padding: var(--space-16) var(--space-4);
        text-align: center;
        color: var(--text-tertiary);
    }
    .empty svg {
        opacity: 0.5;
    }
    .empty p {
        max-width: 20rem;
    }

    .skeleton-row {
        min-height: 6rem;
    }
    .skeleton-avatar {
        width: 2.25rem;
        height: 2.25rem;
        border-radius: var(--radius-full);
        background: var(--bg-tertiary);
        animation: pulse 1.5s infinite;
        flex-shrink: 0;
    }
    .skeleton-lines {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
        flex: 1;
    }
    .skeleton-line {
        height: 0.75rem;
        border-radius: var(--radius-sm);
        background: var(--bg-tertiary);
        animation: pulse 1.5s infinite;
    }
    .skeleton-line.w50 {
        width: 50%;
    }
    .skeleton-line.w80 {
        width: 80%;
    }
    .skeleton-line.w30 {
        width: 30%;
    }
    @keyframes pulse {
        0%,
        100% {
            opacity: 1;
        }
        50% {
            opacity: 0.5;
        }
    }

    @media (max-width: 768px) {
        .review-row {
            flex-direction: column;
        }
        .review-actions {
            justify-content: flex-end;
            width: 100%;
        }
    }
</style>
