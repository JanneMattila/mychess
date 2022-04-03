var MyChess = MyChess || {};

MyChess.isCookieConsentAccepted = () => {
    return document.cookie.indexOf("MyChessCookieConsent=true") >= 0;
}

MyChess.acceptCookieConsent = () => {
    document.cookie = `MyChessCookieConsent=true; secure; samesite=strict; max-age=315360000`;
}

window.addEventListener("focus", event => {
    console.log("Refresh page due to focus");
    DotNet.invokeMethodAsync("MyChess.Client", "OnFocus");
});

if (navigator !== undefined && navigator.serviceWorker !== undefined) {
    navigator.serviceWorker.addEventListener("message", event => {
        console.log("Refresh page due to message", event);
        DotNet.invokeMethodAsync("MyChess.Client", "OnFocus");
    });
}
