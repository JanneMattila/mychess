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

// If any fetch fails, it will show the offline page.
self.addEventListener("fetch", function (event) {
    if (event.request.method !== "GET") return;

    event.respondWith(
        fetch(event.request).catch(function (error) {
            // The following validates that the request was for a navigation to a new document
            if (
                event.request.destination !== "document" ||
                event.request.mode !== "navigate"
            ) {
                return;
            }

            console.error("[My Chess] Network request Failed. Serving offline page " + error);
            return caches.open(CACHE).then(function (cache) {
                return cache.match(offlineFallbackPage);
            });
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
