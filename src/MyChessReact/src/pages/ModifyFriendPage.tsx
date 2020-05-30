import React from "react";
import { ModifyFriend } from "../components/ModifyFriend";
import "./FriendsPage.css";
import { useParams } from "react-router-dom";

type ModifyFriendPageProps = {
    title: string;
    endpoint: string;
};

export function ModifyFriendPage(props: ModifyFriendPageProps) {
    const { id } = useParams();
    return (
        <div>
            <header className="Friends-header">
                <ModifyFriend id={id} title={props.title} endpoint={props.endpoint} />
            </header>
        </div>
    );
}
