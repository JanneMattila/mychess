const CACHE = "mychess-pages";
const offlineFallbackPage = "offline.html";

self.addEventListener("install", function (event) {
    console.log("[My Chess] Install Event processing");

    event.waitUntil(
        caches.open(CACHE).then(function (cache) {
            console.log("[My Chess] Cached offline page during install");

            return cache.add(offlineFallbackPage);
        })
    );
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

    event.respondWith(
        caches.match(event.request).then((resp) => {
            let cachedResponse = false;

            if (event.request.url === self.location.origin + "/") {
                console.log(`[My Chess] Skip cache for root: ${event.request.url}`);
                resp = undefined;
            }

            if (resp !== undefined) {
                console.log(`[My Chess] Responding from cache: ${event.request.url}`);
                cachedResponse = true;
            }

            return resp || fetch(event.request).then((response) => {
                console.log(`[My Chess] Responding from network: ${event.request.url}`);
                if (event.request.url.startsWith(self.location.origin + "/_framework/") &&
                    !(event.request.url.endsWith(".js") || event.request.url.endsWith(".json"))) {
                    console.log(`[My Chess] Do not cache framework files: ${event.request.url}`);
                    return response;
                }

                return caches.open(CACHE).then((cache) => {
                    cache.put(event.request, response.clone());
                    return response;
                });
            }).catch(function (error) {
                if (!cachedResponse) {
                    console.error(`[My Chess] Network request failed for ${event.request.url}. Serving offline page ${error}`);
                    return caches.open(CACHE).then((cache) => {
                        return cache.match(offlineFallbackPage);
                    });
                }
            });;
        })
    );
});

// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
//self.addEventListener('fetch', () => { });

self.addEventListener('push', (event) => {
    console.log("serviceWorker - push");

    if (!self.Notification) {
        console.log("No support for notifications.");
        return;
    }

    if (!self.Notification.permission === 'granted') {
        console.log("No notification permissions granted.");
        return;
    }

    if (!event.data) {
        console.log("serviceWorker - local notification data:");
        event.waitUntil(self.registration.showNotification("My Chess", {
            body: "Open My Chess to play!",
            vibrate: [250, 100, 250, 100, 250],
            badge: '/logo_96x96_monochrome.png',
            icon: '/logo_192x192.png',
            data: "/"
        }));
    }
    else {
        const data = event.data.json();
        console.log("serviceWorker - notification data:");
        console.log(data);

        event.waitUntil(self.registration.showNotification("My Chess", {
            body: data.text,
            vibrate: [250, 100, 250, 100, 250],
            badge: '/logo_96x96_monochrome.png',
            icon: '/logo_192x192.png',
            data: data.uri
        }));
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
