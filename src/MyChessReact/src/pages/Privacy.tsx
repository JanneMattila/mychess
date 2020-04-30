import React, { Component } from "react";
import "./Home.css";

export class Privacy extends Component {
    static displayName = Privacy.name;

    render() {
        return (
            <div>
                <header className="Home-header">
                    <h4>Privacy Policy</h4>
                    <h6>Last updated: April 30th, 2020</h6>
                    <p>
                        My Chess uses Microsoft Account or Azure AD account for authentication.<br /><br />
                        During the authentication process user's name and <b>*hashed*</b> email is stored into our system.<br /><br />
                        It's only used for identifying users and nothing else. <br /><br />
                        This information is not shared with anyone.
                    </p>
                </header>
            </div>
        );
    }
}