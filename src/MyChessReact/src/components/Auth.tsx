import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { loginEvent, RootState, ProcessState } from "../actions";
import { UserAgentApplication, Configuration } from "msal";
import { Link, useHistory, useLocation } from "react-router-dom";
import "./Auth.css";
import { Database, DatabaseFields } from "../data/Database";

type AuthProps = {
    clientId: string;
    applicationIdURI: string;
};

let userAgentApplication: UserAgentApplication;

export function Auth(props: AuthProps) {
    let location = useLocation();
    const history = useHistory();
    const selectorLoginState = (state: RootState) => state.loginState;
    const selectorAccount = (state: RootState) => state.account;

    const loginState = useTypedSelector(selectorLoginState);
    const account = useTypedSelector(selectorAccount);

    const dispatch = useDispatch();

    const accessTokenRequest = {
        scopes: [
            props.applicationIdURI + "/User.ReadWrite",
            props.applicationIdURI + "/Games.ReadWrite"
        ]
    };

    useEffect(() => {
        if (!userAgentApplication) {
            const config: Configuration = {
                auth: {
                    clientId: props.clientId,
                    authority: "https://login.microsoftonline.com/common",
                    navigateToLoginRequestUrl: false,
                    redirectUri: window.location.origin,
                    postLogoutRedirectUri: window.location.origin
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
                    dispatch(loginEvent(ProcessState.Error, errorMessage));
                }
                else if (response) {
                    const loggedInAccount = userAgentApplication.getAccount();
                    if (loggedInAccount) {
                        dispatch(loginEvent(ProcessState.Success, "" /* Clear error message */, loggedInAccount, response.accessToken));
                    }

                    postLogin();
                }
            });

            userAgentApplication.acquireTokenSilent(accessTokenRequest).then(function (accessTokenResponse) {
                // Acquire token silent success
                const loggedInAccount = userAgentApplication.getAccount();
                if (loggedInAccount) {
                    dispatch(loginEvent(ProcessState.Success, "" /* Clear error message */, loggedInAccount, accessTokenResponse.accessToken));
                }

                postLogin();
            }).catch(function (error) {
                // Acquire token silent failure, wait for user sign in
            });
        }
    });

    const preLogin = () => {
        if (location.pathname !== "/") {
            Database.set(DatabaseFields.AUTH_REDIRECT, location.pathname);
        }
    }

    const postLogin = () => {
        const redirectUrl = Database.get<string>(DatabaseFields.AUTH_REDIRECT);
        Database.delete(DatabaseFields.AUTH_REDIRECT);
        if (redirectUrl) {
            history.push(redirectUrl);
        }
    }

    const onSignIn = () => {
        preLogin();
        return userAgentApplication.loginRedirect(accessTokenRequest);
    }

    const onSignOut = () => {
        userAgentApplication.logout();
    }

    if (loginState === ProcessState.Success) {
        return (
            <div className="Auth">
                <Link to="/settings" className="Auth-link">{account?.name}</Link>
                <button onClick={onSignOut} className="Auth-button">Sign out</button>
            </div>
        );
    }
    return (
        <div className="Auth">
            Want to play? Please
            <button onClick={onSignIn} className="Auth-button">sign In</button>
        </div>
    );
}
