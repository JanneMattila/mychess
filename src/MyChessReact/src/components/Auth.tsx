import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { loginEvent, RootState, ProcessState } from "../actions";
import { UserAgentApplication, Configuration } from "msal";
import { Link } from "react-router-dom";
import "./Auth.css";

type AuthProps = {
    clientId: string;
    applicationIdURI: string;
};

let userAgentApplication: UserAgentApplication;

export function Auth(props: AuthProps) {
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
                    dispatch(loginEvent(ProcessState.Error, errorMessage));
                }
                else if (response) {
                    const loggedInAccount = userAgentApplication.getAccount();
                    if (loggedInAccount) {
                        dispatch(loginEvent(ProcessState.Success, "" /* Clear error message */, loggedInAccount, response.accessToken));
                    }
                }
            });

            userAgentApplication.acquireTokenSilent(accessTokenRequest).then(function (accessTokenResponse) {
                // Acquire token silent success
                const loggedInAccount = userAgentApplication.getAccount();
                if (loggedInAccount) {
                    dispatch(loginEvent(ProcessState.Success, "" /* Clear error message */, loggedInAccount, accessTokenResponse.accessToken));
                }
            }).catch(function (error) {
                // Acquire token silent failure, wait for user sign in
            });
        }
    });

    const onSignIn = () => {
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
