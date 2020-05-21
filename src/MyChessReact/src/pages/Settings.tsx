import React, { useEffect, MouseEvent, useState } from "react";
import { Link } from "react-router-dom";
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
                <h4>Profile</h4>

                <label className="Settings-Text">
                    Your player identifier<br />
                    <div className="Settings-SubText">
                        Share this to your friend so that they can connect to you
                    </div>
                    <input type="text" />
                </label>

                <Link to="/friends" className="Settings-link">
                    <span role="img" aria-label="Invite friend">👥</span> Invite Friends
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