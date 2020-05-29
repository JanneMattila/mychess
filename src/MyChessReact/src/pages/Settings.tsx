import React, { useEffect, MouseEvent, useState } from "react";
import { Link } from "react-router-dom";
import Switch from "react-switch";
import "./Settings.css";
import { RootState } from "../actions";
import { useTypedSelector } from "../reducers";
import { getAppInsights } from "../components/TelemetryService";
import { Player } from "../models/Player";

type SettingsProps = {
    endpoint: string;
};

export function Settings(props: SettingsProps) {
    const selectorLoginState = (state: RootState) => state.loginState;
    const selectorAccessToken = (state: RootState) => state.accessToken;

    const loginState = useTypedSelector(selectorLoginState);
    const accessToken = useTypedSelector(selectorAccessToken);

    const [playerIdentifier, setPlayerIdentifier] = useState("");
    const [isNotificationsEnabled, setNotifications] = useState(false);
    const ai = getAppInsights();

    useEffect(() => {
        const populateUserInformation = async (accessToken: string) => {
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(props.endpoint + "/api/users/me", request);
                const data = await response.json() as Player;
                console.log(data);
                setPlayerIdentifier(data.id);
            } catch (error) {
                ai.trackException(error);
                console.log(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve settings.";
                console.log(errorMessage);
            }
        }

        if (accessToken !== undefined) {
            populateUserInformation(accessToken);
        }
    }, [accessToken, ai, props.endpoint]);

    const confirm = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
    }

    const cancel = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
    }

    const copy = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();

        navigator.clipboard.writeText(playerIdentifier);
    }

    const share = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();

        navigator.clipboard.writeText(window.origin + "/friends?friendID=" + playerIdentifier);
    }

    const handleNotificationChange = (checked: boolean): void => {
        setNotifications(!isNotificationsEnabled);
    }

    return (
        <div>
            <header className="Settings-header">
                <h4>Profile</h4>

                <label className="Settings-Text">
                    Your player identifier<br />
                    <div className="Settings-SubText">
                        Share this to your friend so that they can connect to you
                    </div>
                    <input type="text" value={playerIdentifier} readOnly={true} className="Settings-Identifier" /><br />
                    <button onClick={copy}><span role="img" aria-label="Copy">&#128203;</span> Copy id</button>
                    <button onClick={share}><span role="img" aria-label="Share">&#128203;</span> Share link</button>
                </label>

                <Link to="/friends" className="Settings-link">
                    <span role="img" aria-label="Manage your friends">👥</span> Manage your friends
                </Link>

                <h4>Settings</h4>
                <label>
                    Notifications<br />
                    <Switch onChange={handleNotificationChange} checked={isNotificationsEnabled} />
                </label>

                <div>
                    <button onClick={confirm}><span role="img" aria-label="OK">✅</span> Save</button>
                    <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                </div>
            </header>
        </div>
    );
}