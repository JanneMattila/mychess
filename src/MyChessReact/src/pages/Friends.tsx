import React from "react";
import { GetConfiguration } from "../ConfigurationManager";
import { FriendList } from "../components/FriendList";
import "./Friends.css";

let configuration = GetConfiguration();

export function Friends() {
    return (
        <div>
            <header className="Friends-header">
                <FriendList title="Friends" endpoint={configuration.endpoint} />
            </header>
        </div>
    );
}
