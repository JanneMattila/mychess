import React, { useEffect, MouseEvent, useState } from "react";
import { Link } from "react-router-dom";
import Switch from "react-switch";
import "./SettingsPage.css";
import { ProcessState, meLoadingEvent } from "../actions";
import { useTypedSelector } from "../reducers";
import { getAppInsights } from "../components/TelemetryService";
import { Player } from "../models/Player";
import { useDispatch } from "react-redux";
import { Database, DatabaseFields } from "../data/Database";

type SettingsProps = {
    endpoint: string;
};

export function SettingsPage(props: SettingsProps) {
    const loginState = useTypedSelector(state => state.loginState);
    const accessToken = useTypedSelector(state => state.accessToken);
    const me = Database.get<string>(DatabaseFields.ME_ID);

    const [playerIdentifier, setPlayerIdentifier] = useState("");
    const [isNotificationsEnabled, setNotifications] = useState(false);
    const ai = getAppInsights();

    const dispatch = useDispatch();

    useEffect(() => {
        const populateUserInformation = async (accessToken: string) => {
            dispatch(meLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
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

                Database.set(DatabaseFields.ME_ID, data.id);

                dispatch(meLoadingEvent(ProcessState.Success, "" /* Clear error message */, data.id));
            } catch (error) {
                ai.trackException(error);
                console.log(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve settings.";
                console.log(errorMessage);
                dispatch(meLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (accessToken !== undefined) {
            if (me) {
                setPlayerIdentifier(me);
            }
            populateUserInformation(accessToken);
        }
    }, [accessToken, ai, props.endpoint, dispatch, me]);

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

        navigator.clipboard.writeText(window.origin + "/friends/add/" + playerIdentifier);
    }

    const handleNotificationChange = (checked: boolean): void => {
        setNotifications(!isNotificationsEnabled);
    }

    if (loginState === ProcessState.Success) {
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
                        <button onClick={copy}><span role="img" aria-label="Copy">&#128203;</span> Copy your identifier</button>
                        <button onClick={share}><span role="img" aria-label="Share">&#128203;</span> Copy "Add as friend" link</button>
                    </label>

                    <h4>Settings</h4>
                    <label>
                        Notifications<br />
                        <Switch onChange={handleNotificationChange} checked={isNotificationsEnabled} />
                    </label>

                    <div>
                        <button onClick={confirm}><span role="img" aria-label="OK">✅</span> Save</button>
                        <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                    </div>

                    <Link to="/friends" className="Settings-link">
                        <span role="img" aria-label="Manage your friends">👥</span> Manage your friends
                </Link>
                </header>
            </div>
        );
    }
    else {
        // Not logged in so render blank.
        return (
            <>
            </>
        );
    }
}