<script lang="ts">
    import Avatar from '$lib/components/ui/Avatar.svelte';
    import Badge from '$lib/components/ui/Badge.svelte';
    import Button from '$lib/components/ui/Button.svelte';
    import { toast } from '$lib/stores/toast';
    import { onMount } from 'svelte';
    import { adminApi } from '$lib/api/admin';
    import { handleApiError } from '$lib/api/client';
    import { t, getLocaleDateString } from '$lib/i18n';

    let apiPending = $state<{ id: string; name: string; inn: string; email: string; link: string | null; date: string; dadata: { found: boolean; fullName: string; kpp: string; loading: boolean } }[]>([]);
    let pending = $derived(apiPending);

    async function approve(id: string, name: string) {
        try {
            await adminApi.approveVerification(id);
            toast.success($t('adminVerif.approvedMsg', { name }));
            apiPending = apiPending.filter((c) => c.id !== id);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function reject(id: string, name: string) {
        try {
            await adminApi.rejectVerification(id);
            toast.warning($t('adminVerif.rejectedMsg', { name }));
            apiPending = apiPending.filter((c) => c.id !== id);
        } catch (err) {
            handleApiError(err);
        }
    }

    async function checkInn(index: number, inn: string) {
        if (!inn) return;
        apiPending[index].dadata.loading = true;
        try {
            const result = await adminApi.checkInn(inn);
            apiPending[index].dadata = {
                found: result.found,
                fullName: result.value || '',
                kpp: result.kpp || '',
                loading: false
            };
        } catch {
            apiPending[index].dadata = { found: false, fullName: '', kpp: '', loading: false };
        }
    }

    onMount(async () => {
        try {
            const data = await adminApi.getPendingVerifications();
            apiPending = data.map((v) => ({
                id: v.id,
                name: v.companyName,
                inn: v.inn,
                email: v.email,
                link: v.link,
                date: v.createdAt,
                dadata: { found: false, fullName: '', kpp: '', loading: true }
            }));
            await Promise.allSettled(
                apiPending.map((item, i) => item.inn ? checkInn(i, item.inn) : Promise.resolve())
            );
        } catch (err) {
            handleApiError(err);
        }
    });
</script>

<svelte:head><title>{$t('adminVerif.pageTitle')}</title></svelte:head>

<div class="verification">
    <div class="page-header">
        <h1 class="page-heading">{$t('adminVerif.title')}</h1>
        <Badge variant="warning">{pending.length} {$t('adminVerif.pending')}</Badge>
    </div>

    {#if pending.length === 0}
        <div class="empty"><p>{$t('adminVerif.noRequests')}</p></div>
    {:else}
        <div class="cards-list">
            {#each pending as company (company.id)}
                <div class="verify-card">
                    <div class="card-header">
                        <Avatar name={company.name} size={44} />
                        <div class="card-info">
                            <span class="card-name">{company.name}</span>
                            <span class="card-date">{$t('adminVerif.applicationFrom')} {new Date(company.date).toLocaleDateString(getLocaleDateString(), { day: 'numeric', month: 'short' })}</span>
                        </div>
                        <Badge variant="success" size="sm">{$t('adminVerif.autoVerified')}</Badge>
                    </div>
                    <div class="card-details">
                        <div class="detail-row"><span class="detail-label">{$t('adminVerif.inn')}</span><span>{company.inn}</span></div>
                        <div class="detail-row"><span class="detail-label">{$t('adminVerif.email')}</span><span>{company.email}</span></div>
                        {#if company.link}<div class="detail-row"><span class="detail-label">{$t('adminVerif.website')}</span><a href={company.link} target="_blank" rel="noopener">{company.link}</a></div>{/if}
                    </div>
                    <div class="dadata-result" class:not-found={!company.dadata.loading && !company.dadata.found}>
                        <span class="dadata-label">{$t('adminVerif.dadata')}</span>
                        {#if company.dadata.loading}
                            <span style="color: var(--text-tertiary);">{$t('common.loading')}</span>
                        {:else if company.dadata.found}
                            <span>{company.dadata.fullName} {company.dadata.kpp ? `(КПП: ${company.dadata.kpp})` : ''}</span>
                        {:else}
                            <span>{$t('adminVerif.notFound')}</span>
                        {/if}
                    </div>
                    <div class="card-actions">
                        <Button onclick={() => approve(company.id, company.name)}>{$t('common.approve')}</Button>
                        <Button variant="danger" onclick={() => reject(company.id, company.name)}>{$t('common.reject')}</Button>
                    </div>
                </div>
            {/each}
        </div>
    {/if}
</div>

<style>
    .verification { display: flex; flex-direction: column; gap: var(--space-6); }
    .page-header { display: flex; align-items: center; gap: var(--space-3); }
    .page-heading { font-size: var(--font-2xl); font-weight: var(--weight-bold); }
    .cards-list { display: flex; flex-direction: column; gap: var(--space-4); }

    .verify-card { display: flex; flex-direction: column; gap: var(--space-4); padding: var(--space-5); background: var(--bg-secondary); border: 1px solid var(--border-default); border-radius: var(--radius-lg); }
    .card-header { display: flex; align-items: center; gap: var(--space-3); }
    .card-info { display: flex; flex-direction: column; gap: 2px; }
    .card-name { font-size: var(--font-base); font-weight: var(--weight-semibold); }
    .card-date { font-size: var(--font-xs); color: var(--text-tertiary); }

    .card-details { display: flex; flex-direction: column; gap: var(--space-1); }
    .detail-row { display: flex; gap: var(--space-2); font-size: var(--font-sm); }
    .detail-label { color: var(--text-tertiary); min-width: 4rem; }
    .detail-row a { color: var(--accent); }
    .detail-row a:hover { text-decoration: underline; }

    .dadata-result { padding: var(--space-3); background: var(--color-success-subtle); border-radius: var(--radius-md); font-size: var(--font-sm); display: flex; gap: var(--space-2); }
    .dadata-result.not-found { background: var(--color-error-subtle); }
    .dadata-label { font-weight: var(--weight-semibold); }

    .card-actions { display: flex; gap: var(--space-3); }
    .empty { padding: var(--space-12); text-align: center; color: var(--text-tertiary); }
</style>
