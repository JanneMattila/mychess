let userAgentApplication;
let accessTokenRequest;
let accessToken;

function processConfiguration(data) {
    endpoint = data.endpoint;
    accessTokenRequest = {
        scopes: [
            data.applicationIdURI + "User.ReadWrite",
            data.applicationIdURI + "Games.ReadWrite"
        ]
    }

    const config = {
        auth: {
            clientId: data.clientId,
            authority: "https://login.microsoftonline.com/common",
        }
    }

    userAgentApplication = new Msal.UserAgentApplication(config);
    function authCallback(error, response) {
        // Handle redirect responses
        if (error) {
            console.log(error);
        }
        else {
            if (response.tokenType === "id_token") {
                console.log('id_token acquired at: ' + new Date().toString());
            }
            else if (response.tokenType === "access_token") {
                console.log('access_token acquired at: ' + new Date().toString());
                accessToken = response.accessToken;
                afterLogin(response.account);
            }
            else {
                console.log("token type is:" + response.tokenType);
            }
        }
    }

    userAgentApplication.handleRedirectCallback(authCallback);

    userAgentApplication.acquireTokenSilent(accessTokenRequest).then(function (accessTokenResponse) {
        // Acquire token silent success
        // Call API with token
        console.log(accessTokenResponse);
        accessToken = accessTokenResponse.accessToken;
        afterLogin(accessTokenResponse.account);
    }).catch(function (error) {
        // Acquire token silent failure, and send an interactive request
        if (error.errorMessage.indexOf("User login is required") !== -1) {
            userAgentApplication.acquireTokenRedirect(accessTokenRequest);
        }
    });
}

function login() {
    userAgentApplication.loginRedirect(accessTokenRequest);
}
