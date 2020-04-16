import React from "react";
import {
    UserAgentApplication, AuthenticationParameters,
    Account, Configuration
} from "msal";

type Game = {
    id: string;
    title: string;
};

type AuthProps = {
    clientId: string;
    applicationIdURI: string;
};

type LoginState = {
    loggedIn: boolean;
    error: string;
    account?: Account;
};

export class Auth extends React.Component<AuthProps, LoginState> {
    static displayName = Auth.name;
    userAgentApplication?: UserAgentApplication;
    accessTokenRequest?: AuthenticationParameters

    constructor(props: AuthProps) {
        super(props);
        this.state = { loggedIn: false, error: "" };
    }

    componentDidMount() {
        this.accessTokenRequest = {
            scopes: [
                this.props.applicationIdURI + "User.ReadWrite",
                this.props.applicationIdURI + "Games.ReadWrite"
            ]
        };

        const config: Configuration = {
            auth: {
                clientId: this.props.clientId,
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

        this.userAgentApplication = new UserAgentApplication(config);
        this.userAgentApplication.handleRedirectCallback(error => {
            if (error) {
                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to acquire access token.";
                this.setState({
                    error: errorMessage
                });
            }
        });

        const account = this.userAgentApplication.getAccount();
        if (account) {
            this.setState({
                account,
                loggedIn: true
            });
        }
    }

    render() {
        if (this.state.loggedIn) {
            return (
                <div>
                    <h1>Hi {this.state.account?.name}!</h1>
                </div>
            );
        }

        return (
            <div>
                <button onClick={this.onSignIn}>Sign In</button>
            </div>
        );
    }

    onSignIn = () => {
        if (!this.userAgentApplication) {
            // Not loaded yet.
            return;
        }
        return this.userAgentApplication.loginRedirect(this.accessTokenRequest);
    }
}
