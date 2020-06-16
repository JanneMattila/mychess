import React from "react";
import { GetConfiguration } from "../ConfigurationManager";
import { FriendList } from "../components/FriendList";
import "./FriendsPage.css";

let configuration = GetConfiguration();

export function FriendsPage() {
    return (
        <FriendList title="Friends" endpoint={configuration.endpoint} />
    );
}
