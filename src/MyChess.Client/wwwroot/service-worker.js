const CACHE = "mychess-pages";
const offlineFallbackPage = "offline.html";

self.addEventListener("install", function (event) {
    self.skipWaiting();
    console.log("[My Chess Install] Install Event processing");

    const myInstall = async () => {
        console.log("[My Chess Install] Caching pages");
        const cache = await caches.open(CACHE)
        await caches.delete(CACHE);

        await cache.addAll([
            offlineFallbackPage,
            '/settings',
            '/friends',
            '/about',
            '/privacy',
            '/play/local'
        ]);
    }
    event.waitUntil(myInstall());
});

self.addEventListener('fetch', (event) => {
    if (event.request.method !== "GET") {
        console.log(`[My Chess] Skip cache for request method: ${event.request.method} ${event.request.url}`);
        return;
    }
    if (!event.request.url.startsWith(self.location.origin)) {
        console.log(`[My Chess] Skip cache for external url: ${event.request.url}`);
        return;
    }

    if (event.request.url.startsWith(self.location.origin + "/api/")) {
        console.log(`[My Chess] Skip cache backend calls: ${event.request.url}`);
        return;
    }

    if (event.request.url.startsWith(self.location.origin + "/authentication/")) {
        console.log(`[My Chess] Skip cache for authentication: ${event.request.url}`);
        return;
    }

    if (event.request.url.startsWith(self.location.origin + "/play/") &&
        !event.request.url.endsWith("/local")) {
        console.log(`[My Chess] Skip cache for play views: ${event.request.url}`);
        return;
    }

    const myFetch = async () => {
        let cachedResponse = false;
        let cacheResponse = await caches.match(event.request);

        try {
            if (event.request.url === self.location.origin + "/") {
                console.log(`[My Chess] Skip cache for root: ${event.request.url}`);
                cacheResponse = undefined;
            }

            if (cacheResponse !== undefined) {
                console.log(`[My Chess] Responding from cache: ${event.request.url}`);
                cachedResponse = true;

                if (event.request.url === self.location.origin + "/_framework/blazor.boot.json") {
                    let networkJson = {};
                    try {
                        const networkResponseBoot = await fetch(event.request);
                        networkJson = await networkResponseBoot.clone().json();
                    }
                    catch (fetchError) {
                        console.warn(`[My Chess Boot] Cannot check app updates`);
                        return cacheResponse;
                    }

                    const cachedJson = await cacheResponse.clone().json();
                    if (JSON.stringify(networkJson) === JSON.stringify(cachedJson)) {
                        console.log(`[My Chess Boot] No App updates`);
                        return cacheResponse;
                    }
                    else {
                        console.log(`[My Chess Boot] App updated!`);
                        await caches.delete(CACHE);

                        const cache = await caches.open(CACHE);
                        await cache.addAll([
                            offlineFallbackPage,
                            '/settings',
                            '/friends',
                            '/about',
                            '/privacy',
                            '/play/local'
                        ]);
                    }
                }
                else {
                    return cacheResponse;
                }
            }

            const networkResponse = await fetch(event.request);
            console.log(`[My Chess] Responding from network: ${event.request.url}`);
            if (event.request.url.startsWith(self.location.origin + "/_framework/") &&
                !(event.request.url.endsWith(".js") || event.request.url.endsWith(".json"))) {
                console.log(`[My Chess] Do not cache framework files: ${event.request.url}`);
                return networkResponse;
            }

            const cache = await caches.open(CACHE);
            cache.put(event.request, networkResponse.clone());
            return networkResponse;
        }
        catch (error) {
            if (!cachedResponse) {
                console.log(`[My Chess] Network request failed for ${event.request.url}. Serving offline page ${error}`, error);
                const cache = await caches.open(CACHE);
                return await cache.match(offlineFallbackPage);
            }
        }
    }

    event.respondWith(myFetch());
});

// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
//self.addEventListener('fetch', () => { });

self.addEventListener('push', (event) => {
    console.log(`[My Chess Push] Push event`);

    if (!self.Notification) {
        console.log(`[My Chess Push] No support for notifications.`);
        return;
    }

    if (!self.Notification.permission === 'granted') {
        console.log(`[My Chess Push] No notification permissions granted.`);
        return;
    }

    let notificationData = {
        text: "Open My Chess to play!",
        uri: "/"
    };
    if (!event.data) {
        console.log(`[My Chess Push] No data`);
    }
    else {
        console.log(`[My Chess Push] Data`, event.data);
        try {
            const data = event.data.json();
            notificationData = data;
        } catch (e) {
            console.error(`[My Chess Push] Error in push data`, e);
        }
    }

    const showNotification = self.registration.showNotification("My Chess", {
        body: notificationData.text,
        vibrate: [250, 100, 250, 100, 250],
        badge: '/logo_96x96_monochrome.png',
        icon: '/logo_192x192.png',
        data: notificationData.uri
    });

    event.waitUntil(showNotification);

    const clientList = await clients.matchAll({
        type: "window", includeUncontrolled: true
    });
    if (clientList.length > 0) {
        let client = clientList[0];
        for (let i = 0; i < clientList.length; i++) {
            if (clientList[i].focused) {
                client = clientList[i];
            }
        }
        client.postMessage(notificationData);
    }
});

self.addEventListener('notificationclick', (event) => {
    console.log('serviceWorker - notificationclick');
    console.log(event);
    let url = event.notification.data;
    console.log(url);

    event.notification.close();
    event.waitUntil(clients.matchAll({
        type: "window", includeUncontrolled: true
    }).then(function (clientList) {
        if (clientList.length > 0) {
            let client = clientList[0];

            for (let i = 0; i < clientList.length; i++) {
                if (clientList[i].focused) {
                    client = clientList[i];
                }
            }
            return client.focus();
        }
        return clients.openWindow(url);
    }));
});

self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});
