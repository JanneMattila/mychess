var MyChess = MyChess || {};

MyChess.isCookieConsentAccepted = () => {
    return document.cookie.indexOf("MyChessCookieConsent=true") >= 0;
}

MyChess.acceptCookieConsent = () => {
    document.cookie = `MyChessCookieConsent=true; secure; samesite=strict`;
}
