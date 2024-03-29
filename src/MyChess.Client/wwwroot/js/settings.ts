﻿var MyChessSettings = MyChessSettings || {};

/*
 * Conversion logic from internet but original source not found.
 * Same code can be found e.g.
 * https://gist.github.com/Klerith/80abd742d726dd587f4bd5d6a0ab26b6
 */
const urlBase64ToUint8Array = (base64String: string) => {
    var padding = '='.repeat((4 - base64String.length % 4) % 4);
    var base64 = (base64String + padding)
        .replace(/-/g, '+')
        .replace(/_/g, '/');

    var rawData = window.atob(base64);
    var outputArray = new Uint8Array(rawData.length);

    for (var i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

MyChessSettings.enableNotifications = async (webPushPublicKey: string) => {

    if (!navigator.serviceWorker) {
        throw new Error("Service worker is not enabled");
    }

    console.log("Enabling notifications");
    const registration = await navigator.serviceWorker.getRegistration();

    if (!registration) {
        throw new Error("Service worker registration could not be found");
    }

    const permission = await Notification.requestPermission();
    console.log(permission);

    if (permission !== "granted") {
        throw new Error(`Notification permission should be 'granted' but is ${permission}`);
    }

    var options: PushSubscriptionOptionsInit = {
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(webPushPublicKey)
    };

    const result = await registration.pushManager.subscribe(options);
    console.log(result);
    const json = result.toJSON();
    const p256dh = json?.keys?.p256dh;
    const auth = json?.keys?.auth;
    if (!p256dh || !auth) {
        throw new Error("Could not get push subscription keys from browser");
    }

    return {
        endpoint: result.endpoint,
        p256dh: p256dh,
        auth: auth
    };
}

let _deferredPrompt;
let _dotnetRefSetting;
let _installApp;

MyChessSettings.initialize = (dotnetRefSetting: any): void => {
    console.log("[My Chess Settings] Settings initialize");

    _dotnetRefSetting = dotnetRefSetting;
    if (_deferredPrompt !== undefined) {
        _dotnetRefSetting.invokeMethod("SetInstallAsAppVisibility", true);
    }
};

MyChessSettings.promptInstallAsApp = () => {
    _deferredPrompt.prompt();
    _deferredPrompt.userChoice.then((choiceResult) => {
        if (choiceResult.outcome === 'accepted') {
            console.log('[My Chess Settings] User accepted the prompt');
            console.log(_dotnetRefSetting);

            if (_dotnetRefSetting !== undefined) {
                _dotnetRefSetting.invokeMethod("SetInstallAsAppVisibility", false);
            }
        } else {
            console.log('[My Chess Settings] User dismissed the prompt');
        }
        _deferredPrompt = null;
    });
}

window.addEventListener('beforeinstallprompt', event => {
    console.log("[My Chess Settings] beforeinstallprompt");
    console.log(event);
    event.preventDefault();
    _deferredPrompt = event;

    if (_dotnetRefSetting !== undefined) {
        _dotnetRefSetting.invokeMethod("SetInstallAsAppVisibility", true);
    }
});
