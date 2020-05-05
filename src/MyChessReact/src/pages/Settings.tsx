import React, { useEffect, MouseEvent } from "react";
import "./Settings.css";

export function Settings() {

    useEffect(() => {
        
    });

    const confirm = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
    }

    const cancel = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
    }

    return (
        <div>
            <header className="Settings-header">
                <h4>Settings</h4>

                <label>
                    Username<br />
                    <input type="text" />
                </label>

                <button onClick={confirm}><span role="img" aria-label="OK">✅</span> Save</button>
                <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
            </header>
        </div>
    );
}