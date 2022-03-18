var MyChess = MyChess || {};

MyChess.isCookieConsentAccepted = () => {
    return document.cookie.indexOf("MyChessCookieConsent=true") >= 0;
}

MyChess.acceptCookieConsent = () => {
    document.cookie = `MyChessCookieConsent=true; secure; samesite=strict; max-age=315360000`;
}

window.addEventListener("focus", function () {
    console.log("Refresh page due to focus");
    DotNet.invokeMethodAsync("MyChess.Client", "OnFocus");
});
