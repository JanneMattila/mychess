var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var MyChessSettings = MyChessSettings || {};
/*
 * Conversion logic from internet but original source not found.
 * Same code can be found e.g.
 * https://gist.github.com/Klerith/80abd742d726dd587f4bd5d6a0ab26b6
 */
const urlBase64ToUint8Array = (base64String) => {
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
};
MyChessSettings.enableNotifications = (webPushPublicKey) => __awaiter(this, void 0, void 0, function* () {
    var _a, _b;
    if (!navigator.serviceWorker) {
        throw new Error("Service worker is not enabled");
    }
    console.log("Enabling notifications");
    const registration = yield navigator.serviceWorker.getRegistration();
    if (!registration) {
        throw new Error("Service worker registration could not be found");
    }
    const permission = yield Notification.requestPermission();
    console.log(permission);
    if (permission !== "granted") {
        throw new Error(`Notification permission should be 'granted' but is ${permission}`);
    }
    var options = {
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(webPushPublicKey)
    };
    const result = yield registration.pushManager.subscribe(options);
    console.log(result);
    const json = result.toJSON();
    const p256dh = (_a = json === null || json === void 0 ? void 0 : json.keys) === null || _a === void 0 ? void 0 : _a.p256dh;
    const auth = (_b = json === null || json === void 0 ? void 0 : json.keys) === null || _b === void 0 ? void 0 : _b.auth;
    if (!p256dh || !auth) {
        throw new Error("Could not get push subscription keys from browser");
    }
    return {
        endpoint: result.endpoint,
        p256dh: p256dh,
        auth: auth
    };
});
//MyChessSettings.beforeinstallprompt = async (deferredPrompt: any) => {
//    deferredPrompt.prompt();
//}
//window.addEventListener('beforeinstallprompt', MyChessSettings.beforeinstallprompt);
//# sourceMappingURL=settings.js.map