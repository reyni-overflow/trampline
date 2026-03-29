<script lang="ts">
    import { onMount, onDestroy } from 'svelte';
    import { browser } from '$app/environment';
    import { t } from '$lib/i18n';

    interface MapMarker {
        id: string;
        lat: number;
        lng: number;
        title: string;
        company: string;
        salary?: string;
        tags?: string[];
        type: string;
        isFavorite?: boolean;
        link?: string;
    }

    interface Props {
        markers?: MapMarker[];
        center?: [number, number];
        zoom?: number;
        height?: string;
        rounded?: boolean;
        onmarkerclick?: (id: string) => void;
    }

    let {
        markers = [],
        center = [55.751, 37.618],
        zoom = 10,
        height = '100%',
        rounded = true,
        onmarkerclick
    }: Props = $props();

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    type Leaflet = any;

    let mapContainer: HTMLDivElement;
    let map: Leaflet;
    let markerGroup: Leaflet;
    let L: Leaflet;
    let currentTheme = $state('dark');

    const DARK_TILES = 'https://tiles.stadiamaps.com/tiles/alidade_smooth_dark/{z}/{x}/{y}{r}.png';
    const LIGHT_TILES = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
    const TILE_ATTR = '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>';

    let tileLayer: Leaflet;

    function getTheme(): string {
        if (!browser) return 'dark';
        return document.documentElement.getAttribute('data-theme') || 'dark';
    }

    function createMarkerIcon(marker: MapMarker): Leaflet {
        const typeClass = marker.isFavorite ? 'marker--favorite' : (marker.type === 'Event' ? 'marker--event' : (marker.type === 'Mentorship' ? 'marker--mentorship' : 'marker--job'));
        const initial = marker.company.charAt(0).toUpperCase();

        return L.divIcon({
            className: 'custom-marker',
            html: `<div class="marker-circle ${typeClass}"><span>${initial}</span></div>`,
            iconSize: [36, 36],
            iconAnchor: [18, 18],
            popupAnchor: [0, -22]
        });
    }

    function escapeHtml(s: string): string {
        return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function createPopup(marker: MapMarker): string {
        const tags = marker.tags?.slice(0, 3).map((t) => `<span class="popup-tag">${escapeHtml(t)}</span>`).join('') || '';
        return `
            <div class="map-popup">
                <strong class="popup-title">${escapeHtml(marker.title)}</strong>
                <span class="popup-company">${escapeHtml(marker.company)}</span>
                ${marker.salary ? `<span class="popup-salary">${escapeHtml(marker.salary)}</span>` : ''}
                ${tags ? `<div class="popup-tags">${tags}</div>` : ''}
                <a class="popup-link" href="${marker.link || `/jobs/${marker.id}`}">${$t('ui.mapDetails')}</a>
            </div>
        `;
    }

    function updateMarkers() {
        if (!map || !L || !markerGroup) return;
        markerGroup.clearLayers();

        for (const m of markers) {
            const leafletMarker = L.marker([m.lat, m.lng], { icon: createMarkerIcon(m) });
            leafletMarker.bindPopup(createPopup(m), { maxWidth: 260, className: 'custom-popup' });
            if (onmarkerclick) {
                leafletMarker.on('click', () => onmarkerclick(m.id));
            }
            markerGroup.addLayer(leafletMarker);
        }
    }

    function updateTiles() {
        if (!map || !L) return;
        const theme = getTheme();
        if (theme === currentTheme && tileLayer) return;
        currentTheme = theme;
        if (tileLayer) map.removeLayer(tileLayer);
        tileLayer = L.tileLayer(theme === 'dark' ? DARK_TILES : LIGHT_TILES, { attribution: TILE_ATTR, maxZoom: 19 });
        tileLayer.addTo(map);
    }

    let themeObserver: MutationObserver | null = null;
    let resizeObserver: ResizeObserver | null = null;

    onMount(() => {
        if (!browser) return;

        resizeObserver = new ResizeObserver(() => {
            if (map) map.invalidateSize();
        });
        resizeObserver.observe(mapContainer);

        (async () => {
            const leafletModule = await import('leaflet');
            L = leafletModule.default || leafletModule;

            await import('leaflet/dist/leaflet.css');

            await import('leaflet.markercluster');
            await import('leaflet.markercluster/dist/MarkerCluster.css');
            await import('leaflet.markercluster/dist/MarkerCluster.Default.css');

            map = L.map(mapContainer, {
                center,
                zoom,
                zoomControl: false,
                attributionControl: false
            });

            L.control.attribution({ prefix: false }).addTo(map);
            L.control.zoom({ position: 'bottomright' }).addTo(map);

            currentTheme = getTheme();
            tileLayer = L.tileLayer(currentTheme === 'dark' ? DARK_TILES : LIGHT_TILES, { attribution: TILE_ATTR, maxZoom: 19 });
            tileLayer.addTo(map);

            markerGroup = L.markerClusterGroup({
                maxClusterRadius: 50,
                spiderfyOnMaxZoom: true,
                showCoverageOnHover: false,
                iconCreateFunction: (cluster: Leaflet) => {
                    const count = cluster.getChildCount();
                    const size = count < 10 ? 36 : count < 50 ? 44 : 52;
                    return L.divIcon({
                        html: `<div class="cluster-icon" style="width:${size}px;height:${size}px"><span>${count}</span></div>`,
                        className: 'custom-cluster',
                        iconSize: [size, size]
                    });
                }
            });
            map.addLayer(markerGroup);

            updateMarkers();

            themeObserver = new MutationObserver(() => updateTiles());
            themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });

            let pulseMarker: Leaflet = null;
            const handleFlyTo = (e: Event) => {
                const { lat, lng } = (e as CustomEvent).detail;
                map.flyTo([lat, lng], 13, { duration: 1.5 });
                if (pulseMarker) map.removeLayer(pulseMarker);
                pulseMarker = L.circleMarker([lat, lng], {
                    radius: 8, color: 'var(--color-blue, #3B82F6)', fillColor: 'var(--color-blue, #3B82F6)', fillOpacity: 0.4, weight: 2
                });
                pulseMarker.addTo(map);
            };
            document.addEventListener('flyto', handleFlyTo);

            flyToCleanup = () => document.removeEventListener('flyto', handleFlyTo);
        })();

        return () => {
            themeObserver?.disconnect();
            resizeObserver?.disconnect();
        };
    });

    let flyToCleanup: (() => void) | null = null;

    onDestroy(() => {
        flyToCleanup?.();
        if (map) { map.remove(); map = null; }
    });

    $effect(() => {
        void markers;
        updateMarkers();
    });
</script>

<div class="map-wrapper" class:map-wrapper--square={!rounded} style="height: {height}">
    <div class="map-container" bind:this={mapContainer}></div>
</div>

<style>
    .map-wrapper {
        position: relative;
        width: 100%;
        border-radius: var(--radius-lg);
        overflow: hidden;
    }

    .map-wrapper--square {
        border-radius: 0;
    }

    .map-container {
        width: 100%;
        height: 100%;
    }

    :global(.custom-marker) {
        background: none !important;
        border: none !important;
    }

    :global(.marker-circle) {
        width: 2.25rem;
        height: 2.25rem;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        color: var(--text-inverse, #fff);
        font-weight: 700;
        font-size: 0.875rem;
        font-family: var(--font-family);
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
        border: 2px solid rgba(255, 255, 255, 0.9);
        transition: transform 0.15s ease;
    }

    :global(.marker-circle:hover) {
        transform: scale(1.15);
    }

    :global(.marker--job) { background: var(--color-info, #3B82F6); }
    :global(.marker--mentorship) { background: var(--color-success, #10B981); }
    :global(.marker--event) { background: var(--color-purple, #8B5CF6); }
    :global(.marker--favorite) { background: var(--accent, #FACC15); }

    :global(.custom-popup .leaflet-popup-content-wrapper) {
        background: var(--bg-elevated, #111);
        color: var(--text-primary, #fff);
        border: 1px solid var(--border-default, #1F1F1F);
        border-radius: var(--radius-lg, 14px);
        box-shadow: var(--shadow-lg);
        padding: 0;
    }

    :global(.custom-popup .leaflet-popup-tip) {
        background: var(--bg-elevated, #111);
        border: 1px solid var(--border-default, #1F1F1F);
    }

    :global(.custom-popup .leaflet-popup-close-button) {
        color: var(--text-tertiary, #666) !important;
        font-size: 1.25rem;
        top: 0.5rem !important;
        right: 0.5rem !important;
    }

    :global(.map-popup) {
        display: flex;
        flex-direction: column;
        gap: 0.375rem;
        padding: 0.875rem;
        font-family: var(--font-family);
    }

    :global(.popup-title) {
        font-size: 0.9375rem;
        font-weight: 600;
    }

    :global(.popup-company) {
        font-size: 0.8125rem;
        color: var(--text-secondary, #A0A0A0);
    }

    :global(.popup-salary) {
        font-size: 0.8125rem;
        font-weight: 600;
        color: var(--accent, #3B82F6);
    }

    :global(.popup-tags) {
        display: flex;
        gap: 0.25rem;
        flex-wrap: wrap;
    }

    :global(.popup-tag) {
        font-size: 0.6875rem;
        padding: 0.125rem 0.5rem;
        background: var(--bg-tertiary, #141414);
        border-radius: 9999px;
        color: var(--text-secondary, #A0A0A0);
    }

    :global(.popup-link) {
        font-size: 0.8125rem;
        font-weight: 500;
        color: var(--accent, #3B82F6);
        text-decoration: none;
        margin-top: 0.25rem;
    }

    :global(.popup-link:hover) {
        text-decoration: underline;
    }

    :global(.custom-cluster) {
        background: none !important;
        border: none !important;
    }

    :global(.cluster-icon) {
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        background: var(--accent, #3B82F6);
        color: var(--text-inverse, #fff);
        font-weight: 700;
        font-size: 0.8125rem;
        font-family: var(--font-family);
        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.35);
        border: 2.5px solid rgba(255, 255, 255, 0.9);
    }
</style>
