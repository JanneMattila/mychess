import React from "react";
import "./PrivacyPage.css";

export function PrivacyPage() {
    return (
        <div>
            <div className="title">Privacy Policy</div>
            <div className="lastUpdatedText">Last updated: May 21st, 2020</div>
            <div className="privacyText-Container">
                <div className="privacyText">
                    My Chess uses Microsoft Account or Azure AD account for authentication.<br /><br />
                    During the authentication process user's name and account identifier is stored into our system.<br /><br />
                    It's only used for identifying users and nothing else. <br /><br />
                    This information is not shared with anyone.
                </div>
            </div>
        </div>
    );
}