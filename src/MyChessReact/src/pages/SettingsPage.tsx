import React, { useEffect, MouseEvent, useState } from "react";
import { Link } from "react-router-dom";
import Switch from "react-switch";
import "./SettingsPage.css";
import { ProcessState } from "../actions";
import { useTypedSelector } from "../reducers";
import { getAppInsights } from "../components/TelemetryService";
import { Database, DatabaseFields } from "../data/Database";
import { BackendService } from "../components/BackendService";
import { UserSettings } from "../models/UserSettings";

type SettingsProps = {
    endpoint: string;
};

export function SettingsPage(props: SettingsProps) {
    const loginState = useTypedSelector(state => state.loginState);
    const me = useTypedSelector(state => state.me);
    const meID = Database.get<string>(DatabaseFields.ME_ID);
    const userSettings = useTypedSelector(state => state.userSettings);

    const [executeGetMe, setExecuteGetMe] = useState(0);
    const [executeGetSettings, setExecuteGetSettings] = useState(0);
    const [executeSetSettings, setExecuteSetSettings] = useState<UserSettings | undefined>(undefined);

    const [playerIdentifier, setPlayerIdentifier] = useState("");
    const [playAlwaysUp, setPlayAlwaysUp] = useState(false);
    const [isNotificationsEnabled, setNotifications] = useState(false);
    const ai = getAppInsights();

    useEffect(() => {
        if (meID) {
            setPlayerIdentifier(meID);
            setExecuteGetSettings(e => e + 1);
        }
        else {
            setExecuteGetMe(e => e + 1);
        }
    }, [me, meID]);


    useEffect(() => {
        if (userSettings) {
            let enabled = false;
            if (userSettings.notifications.length === 1 &&
                userSettings.notifications[0].enabled) {
                enabled = true;
            }

            setPlayAlwaysUp(userSettings.playAlwaysUp);
            setNotifications(enabled);
        }
    }, [userSettings]);

    const save = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        if (userSettings) {
            userSettings.playAlwaysUp = playAlwaysUp;
            userSettings.notifications = [
                {
                    "enabled": isNotificationsEnabled,
                    "name": "browser1",
                    "uri": ""
                }
            ];

            console.log("save");
            console.log(userSettings);

            setExecuteSetSettings(userSettings);
        }
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

    const handlePlayAlwaysUpChange = async (checked: boolean) => {
        setPlayAlwaysUp(e => !e);
    }

    const handleNotificationChange = async (checked: boolean) => {
        setNotifications(e => !e);

        if (navigator.serviceWorker) {
            console.log(navigator.serviceWorker);
            const registration = await navigator.serviceWorker.getRegistration();
            if (registration) {
                console.log(registration.pushManager.permissionState);
            }
        }
    }

    if (loginState === ProcessState.Success) {
        return (
            <div>
                <div className="title">Profile</div>
                <div className="Settings-Container">
                    <label className="subtitle">
                        Your player identifier<br />
                        <div className="Settings-SubText">
                            Share this to your friend so that they can connect to you
                        </div>
                        <input type="text" value={playerIdentifier} readOnly={true} className="Settings-Identifier" /><br />
                        <button onClick={copy}><span role="img" aria-label="Copy">&#128203;</span> Copy your identifier</button>
                        <button onClick={share}><span role="img" aria-label="Share">&#128203;</span> Copy "Add as friend" link</button>
                    </label>

                    <div className="subtitle">Settings</div>
                    <label>
                        Play always up<br />
                        <Switch onChange={handlePlayAlwaysUpChange} checked={playAlwaysUp} />
                    </label>
                    <br />
                    <br />
                    <label>
                        Notifications<br />
                        <Switch onChange={handleNotificationChange} checked={isNotificationsEnabled} />
                    </label>

                    <div className="title">
                        <button onClick={save}><span role="img" aria-label="OK">✅</span> Save</button>
                        <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                    </div>

                    <Link to="/friends" className="Settings-link">
                        <span role="img" aria-label="Manage your friends">👥</span> Manage your friends
                    </Link>
                </div>
                <BackendService endpoint={props.endpoint} getMe={executeGetMe} getSettings={executeGetSettings} upsertSettings={executeSetSettings} />
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