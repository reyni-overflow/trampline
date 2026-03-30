const CACHE_NAME  = 'trampline-v2';
const OFFLINE_URL = '/offline.html';

const PRECACHE = ['/', '/offline.html', '/logo.svg', '/favicon.svg'];


self.addEventListener('install', (e) => {
    e.waitUntil(
        caches.open(CACHE_NAME).then(cache => cache.addAll(PRECACHE))
    );

    self.skipWaiting();
});


self.addEventListener('activate', (e) => {
    e.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
        )
    );

    self.clients.claim();
});


self.addEventListener('push', (e) => {
    if (!e.data) return;
    const data = e.data.json();
    e.waitUntil(
        self.registration.showNotification(data.title || 'Трамплин', {
            body: data.body || '',
            icon: data.icon || '/favicon.svg',
            badge: '/favicon.svg',
            data: { url: data.url }
        })
    );
});

self.addEventListener('notificationclick', (e) => {
    e.notification.close();
    const url = e.notification.data?.url;
    if (url) {
        e.waitUntil(clients.openWindow(url));
    }
});

self.addEventListener('fetch', (e) => {
    if (e.request.mode === 'navigate') {
        e.respondWith(
            fetch(e.request).catch(() => caches.match(OFFLINE_URL))
        );

        return;
    }

    e.respondWith(
        caches.match(e.request).then(r => r || fetch(e.request))
    );
});
