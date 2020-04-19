import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector, RootState } from "../reducers";
import { loginEvent } from "../actions";
import { UserAgentApplication, Configuration } from "msal";

type AuthProps = {
    clientId: string;
    applicationIdURI: string;
};

let userAgentApplication: UserAgentApplication;

export function Auth(props: AuthProps) {
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
        userAgentApplication.handleRedirectCallback(error => {
            if (error) {
                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to acquire access token.";
                dispatch(loginEvent(false, errorMessage));
            }
        });
    }

    const selectorLoggedIn = (state: RootState) => state.loggedIn;
    const selectorAccount = (state: RootState) => state.account;

    const loggedIn = useTypedSelector(selectorLoggedIn);
    const account = useTypedSelector(selectorAccount);

    const dispatch = useDispatch();

    useEffect(() => {
        const getAccount = userAgentApplication.getAccount();
        if (getAccount) {
            dispatch(loginEvent(true, "", getAccount));
        }
    });

    const onSignIn = () => {
        const accessTokenRequest = {
            scopes: [
                props.applicationIdURI + "User.ReadWrite",
                props.applicationIdURI + "Games.ReadWrite"
            ]
        };
        return userAgentApplication.loginRedirect(accessTokenRequest);
    }

    if (loggedIn) {
        return (
            <div>
                <h1>Hi {account?.name}!</h1>
            </div>
        );
    }

    return (
        <div>
            <button onClick={onSignIn}>Sign In</button>
        </div>
    );
}
