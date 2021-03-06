import React, { useEffect } from "react";
import { useTypedSelector } from "../reducers";
import { ProcessState, friendsRequestedEvent } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link, useHistory } from "react-router-dom";
import "./FriendList.css";
import { User } from "../models/User";
import { useDispatch } from "react-redux";
import { useIsAuthenticated } from "@azure/msal-react";

type FriendListProps = {
    title: string;
    endpoint: string;
};

export function FriendList(props: FriendListProps) {
    const isAuthenticated = useIsAuthenticated();
    const history = useHistory();

    const friendsState = useTypedSelector(state => state.friendsState);
    const friends = useTypedSelector(state => state.friends);

    const dispatch = useDispatch();

    const ai = getAppInsights();

    useEffect(() => {
        if (!isAuthenticated) {
            console.log("Not logged in");
            return;
        }

        console.log("fetch friends");
        dispatch(friendsRequestedEvent());
    }, [dispatch, isAuthenticated]);

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
        ai.trackEvent({ name: "Friends-Manage" });

        event.preventDefault();
        history.push("/friends/" + friendID);
    }

    const refresh = () => {
        ai.trackEvent({ name: "Friends-Refresh" });

        dispatch(friendsRequestedEvent());
    }

    const addNewFriend = () => {
        ai.trackEvent({ name: "Friends-Add" });

        history.push("/friends/add");
    }

    if (isAuthenticated) {
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
                    contents = <div>
                        {renderFriends(friends)}
                        <div className="subtitle">
                            Or <button onClick={addNewFriend}>add new</button> friend
                        </div>
                    </div>;
                }

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
