import React, { useEffect, MouseEvent, useState } from "react";
import { Link, useHistory } from "react-router-dom";
import Switch from "react-switch";
import "./SettingsPage.css";
import { ProcessState } from "../actions";
import { useTypedSelector } from "../reducers";
import { getAppInsights } from "../components/TelemetryService";
import { Database, DatabaseFields } from "../data/Database";
import { BackendService } from "../components/BackendService";
import { UserSettings } from "../models/UserSettings";
import { UserNotifications } from "../models/UserNotifications";

type SettingsProps = {
    endpoint: string;
    webPushPublicKey: string
};

export function SettingsPage(props: SettingsProps) {
    const history = useHistory();
    const loginState = useTypedSelector(state => state.loginState);
    const storedUserSettings = Database.get<UserSettings>(DatabaseFields.ME_SETTINGS);
    const userSettings = useTypedSelector(state => state.userSettings);

    const [executeSetSettings, setExecuteSetSettings] = useState<UserSettings | undefined>(undefined);

    const [playerIdentifier, setPlayerIdentifier] = useState("");
    const [playAlwaysUp, setPlayAlwaysUp] = useState(false);
    const [isNotificationsEnabled, setNotifications] = useState(false);
    const [notificationSettings, setNotificationSettings] = useState<UserNotifications | undefined>(undefined);
    const [deferredPrompt, setDeferredPrompt] = useState<any>(undefined);

    const ai = getAppInsights();

    useEffect(() => {
        const setPrompt = (e: any) => {
            console.log("Show install prompt");
            ai.trackEvent({ name: "ShowInstallPrompt" });
            setDeferredPrompt(e);

            const element = document.getElementById("installAsApp");
            if (element) {
                element.style.display = "";
            }
        }

        if (userSettings) {
            let enabled = false;
            if (userSettings.notifications.length === 1 &&
                userSettings.notifications[0].enabled) {
                setNotificationSettings(userSettings.notifications[0]);
                enabled = true;
            }

            setPlayerIdentifier(userSettings.id);
            setPlayAlwaysUp(userSettings.playAlwaysUp);
            setNotifications(enabled);
        }
        else if (storedUserSettings) {
            // Use cached settings if no newser updates are available
            let enabled = false;
            if (storedUserSettings.notifications.length === 1 &&
                storedUserSettings.notifications[0].enabled) {
                setNotificationSettings(storedUserSettings.notifications[0]);
                enabled = true;
            }

            setPlayerIdentifier(storedUserSettings.id);
            setPlayAlwaysUp(storedUserSettings.playAlwaysUp);
            setNotifications(enabled);
        }

        console.log("add event beforeinstallprompt");
        window.addEventListener('beforeinstallprompt', setPrompt);
        return () => {
            console.log("remove event beforeinstallprompt");
            window.removeEventListener('beforeinstallprompt', setPrompt);
        }
    }, [ai, userSettings, storedUserSettings]);

    const save = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();

        ai.trackEvent({ name: "SaveSettings" });

        if (userSettings) {
            userSettings.playAlwaysUp = playAlwaysUp;
            userSettings.notifications = [];
            if (notificationSettings && isNotificationsEnabled) {
                userSettings.notifications = [
                    notificationSettings
                ];
            }

            console.log("save");
            console.log(userSettings);

            setExecuteSetSettings(userSettings);
        }
    }

    const cancel = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        history.push("/");
    }

    const copy = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();

        ai.trackEvent({ name: "CopyInvitationCode" });

        navigator.clipboard.writeText(playerIdentifier);
    }

    const share = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();

        ai.trackEvent({ name: "ShareInvitationLink" });

        navigator.clipboard.writeText(window.origin + "/friends/add/" + playerIdentifier);
    }

    const handlePlayAlwaysUpChange = async (checked: boolean) => {
        setPlayAlwaysUp(e => !e);
    }

    /*
     * Conversion logic from internet but original source not found.
     * Same code can be found e.g.
     * https://gist.github.com/Klerith/80abd742d726dd587f4bd5d6a0ab26b6
     */
    const urlBase64ToUint8Array = (base64String: string) => {
        var padding = '='.repeat((4 - base64String.length % 4) % 4);
        var base64 = (base64String + padding)
            .replace(/-/g, '+')
            .replace(/_/g, '/');

        var rawData = window.atob(base64);
        var outputArray = new Uint8Array(rawData.length);

        for (var i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }

    const handleNotificationChange = async (checked: boolean) => {
        if (!isNotificationsEnabled && navigator.serviceWorker) {

            ai.trackEvent({ name: "TryingToEnableNotifications" });

            console.log("Enabling notifications");
            console.log(navigator.serviceWorker);
            const registration = await navigator.serviceWorker.getRegistration();
            if (registration) {
                const permission = await Notification.requestPermission();
                console.log(permission);

                if (permission === "granted") {
                    var options: PushSubscriptionOptions = {
                        userVisibleOnly: true,
                        applicationServerKey: urlBase64ToUint8Array(props.webPushPublicKey)
                    };
                    console.log(options);

                    const result = await registration.pushManager.subscribe(options);
                    console.log(result);
                    const json = result.toJSON();
                    const p256dh = json?.keys?.p256dh;
                    const auth = json?.keys?.auth;
                    if (!p256dh || !auth) {
                        throw new Error("Could not get push subscription keys");
                    }

                    setNotificationSettings({
                        enabled: checked,
                        name: "browser1",
                        endpoint: result.endpoint,
                        p256dh: p256dh,
                        auth: auth
                    });
                    setNotifications(true);
                    ai.trackEvent({ name: "NotificationEnabled" });
                    return;
                }
            }
        }

        console.log("Disabling notifications");
        ai.trackEvent({ name: "NotificationDisabled" });
        setNotificationSettings(undefined);
        setNotifications(false);
    }

    const installAsApp = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();

        deferredPrompt.prompt();
    }

    const hidden = {
        display: "none",
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

                    <div id="installAsApp" className="title" style={hidden}>
                        <button onClick={installAsApp}><span role="img" aria-label="Install as App">📦</span> Install as App</button>
                    </div>

                    <Link to="/friends" className="Settings-link">
                        <span role="img" aria-label="Manage your friends">👥</span> Manage your friends
                    </Link>
                </div>
                <BackendService endpoint={props.endpoint} getSettings={1} upsertSettings={executeSetSettings} />
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