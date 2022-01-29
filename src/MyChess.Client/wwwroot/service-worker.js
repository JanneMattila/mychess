// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });

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
