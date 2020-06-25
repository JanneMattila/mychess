import React, { useState } from "react";
import { useTypedSelector } from "../reducers";
import { ProcessState, friendsLoadingEvent } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link, useHistory } from "react-router-dom";
import "./FriendList.css";
import { User } from "../models/User";
import { BackendService } from "./BackendService";

type FriendListProps = {
    title: string;
    endpoint: string;
};

export function FriendList(props: FriendListProps) {
    const history = useHistory();

    const loginState = useTypedSelector(state => state.loginState);
    const friendsState = useTypedSelector(state => state.friendsState);
    const friends = useTypedSelector(state => state.friends);

    const [executeGetFriends, setExecuteGetFriends] = useState(1);

    const ai = getAppInsights();

    const renderFriends = (friends?: User[]) => {
        return (
            <div>
                <div className="row">
                    {friends?.map(friend =>
                        <Link to={{ pathname: "/play/new?friendID=" + friend?.id }} className="FriendList-link" key={friend?.id}>
                            <div className="friendTemplate">
                                <div className="nameTemplate">
                                    {friend?.name}
                                </div>

                                <button className="manageTemplate" onClick={(e) => manageFriend(e, friend?.id)}>Manage friend</button>
                            </div>
                        </Link>
                    )
                    }
                </div >
            </div >
        );
    }

    const manageFriend = (event: any, friendID: string) => {
        event.preventDefault();
        history.push("/friends/" + friendID);
    }

    const refresh = () => {
        setExecuteGetFriends(executeGetFriends + 1);
    }

    const addNewFriend = () => {
        history.push("/friends/add");
    }

    if (loginState === ProcessState.Success) {
        let contents: JSX.Element;
        switch (friendsState) {
            case ProcessState.Success:
                if (friends && friends?.length === 0) {
                    contents =
                        <div className="subtitle">
                            No friends found. Click to <button onClick={refresh}>refresh</button> or
                            <button onClick={addNewFriend}>add new</button> friend.
                        </div>;
                }
                else {
                    contents = renderFriends(friends);
                }

                contents =
                    <div>
                        {contents}
                        <div className="subtitle">
                            Or <button onClick={addNewFriend}>add new</button> friend
                        </div>
                    </div>;
                break;
            case ProcessState.Error:
                contents = <div className="subtitle">Oh no! Couldn't retrieve friends. Click to <button onClick={refresh}>refresh</button></div>;
                break;
            default:
                contents = <div className="subtitle"><em>Loading...</em></div>;
                break;
        }

        return (
            <div>
                <div className="title">{props.title}</div>
                {contents}
                <BackendService endpoint={props.endpoint} getFriends={executeGetFriends} />
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
