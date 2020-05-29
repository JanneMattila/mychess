import React from "react";
import { ModifyFriend } from "../components/ModifyFriend";
import "./Friends.css";

type ModifyFriendPageProps = {
    title: string;
    endpoint: string;
};

export function ModifyFriendPage(props: ModifyFriendPageProps) {
    return (
        <div>
            <header className="Friends-header">
                <ModifyFriend title={props.title} endpoint={props.endpoint} />
            </header>
        </div>
    );
}
