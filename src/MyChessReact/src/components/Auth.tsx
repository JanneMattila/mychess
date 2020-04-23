import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { loginEvent, RootState } from "../actions";
import { UserAgentApplication, Configuration } from "msal";
import { Link } from "react-router-dom";

type AuthProps = {
    clientId: string;
    applicationIdURI: string;
};

let userAgentApplication: UserAgentApplication;

export function Auth(props: AuthProps) {
    const selectorLoggedIn = (state: RootState) => state.loggedIn;
    const selectorAccount = (state: RootState) => state.account;

    const loggedIn = useTypedSelector(selectorLoggedIn);
    const account = useTypedSelector(selectorAccount);

    const dispatch = useDispatch();

    const accessTokenRequest = {
        scopes: [
            props.applicationIdURI + "User.ReadWrite",
            props.applicationIdURI + "Games.ReadWrite"
        ]
    };

    useEffect(() => {
        if (!userAgentApplication) {
            const config: Configuration = {
                auth: {
                    clientId: props.clientId,
                    authority: "https://login.microsoftonline.com/common",
                    navigateToLoginRequestUrl: false
                },
                cache: {
                    cacheLocation: "localStorage"
                },
                system: {
                    navigateFrameWait: 0
                }
            };

            userAgentApplication = new UserAgentApplication(config);
            userAgentApplication.handleRedirectCallback((error, response) => {
                if (error) {
                    console.log("Auth error");
                    console.log(error);
                    const errorMessage = error.errorMessage ? error.errorMessage : "Unable to acquire access token.";
                    dispatch(loginEvent(false, errorMessage));
                }
                else if (response) {
                    const loggedInAccount = userAgentApplication.getAccount();
                    if (loggedInAccount) {
                        dispatch(loginEvent(true, "" /* Clear error message */, loggedInAccount, response.accessToken));
                    }
                }
            });

            userAgentApplication.acquireTokenSilent(accessTokenRequest).then(function (accessTokenResponse) {
                // Acquire token silent success
                const loggedInAccount = userAgentApplication.getAccount();
                if (loggedInAccount) {
                    dispatch(loginEvent(true, "" /* Clear error message */, loggedInAccount, accessTokenResponse.accessToken));
                }
            }).catch(function (error) {
                // Acquire token silent failure, wait for user sign in
            });
        }
    });

    const onSignIn = () => {
        return userAgentApplication.loginRedirect(accessTokenRequest);
    }

    if (loggedIn) {
        return (
            <div>
                <h4><Link to="/settings" className="App-link">Hi {account?.name}!</Link></h4>
            </div>
        );
    }

    return (
        <div>
            <button onClick={onSignIn}>Sign In</button>
        </div>
    );
}
