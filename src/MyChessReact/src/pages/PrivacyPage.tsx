import React from "react";
import "./HomePage.css";

export function PrivacyPage() {
    return (
        <div>
            <header className="Home-header">
                <h4>Privacy Policy</h4>
                <h6>Last updated: May 21st, 2020</h6>
                <p>
                    My Chess uses Microsoft Account or Azure AD account for authentication.<br /><br />
                    During the authentication process user's name and account identifier is stored into our system.<br /><br />
                    It's only used for identifying users and nothing else. <br /><br />
                    This information is not shared with anyone.
                </p>
            </header>
        </div>
    );
}