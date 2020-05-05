import React, { useEffect, MouseEvent, useState } from "react";
import Switch from "react-switch";
import "./Settings.css";

export function Settings() {

    const [isNotificationsEnabled, setNotifications] = useState(false);

    useEffect(() => {

    });

    const confirm = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
    }

    const cancel = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
    }

    const handleNotificationChange = (checked: boolean): void => {
        setNotifications(!isNotificationsEnabled);
    }

    return (
        <div>
            <header className="Settings-header">
                <h4>Settings</h4>

                <label>
                    Username<br />
                    <input type="text" />
                </label>
                <br />
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