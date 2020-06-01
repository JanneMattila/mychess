import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { RootState, ProcessState, friendsLoadingEvent } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link, useHistory } from "react-router-dom";
import "./FriendList.css";
import { Player } from "../models/Player";
import { Database, DatabaseFields } from "../data/Database";

type FriendListProps = {
    title: string;
    endpoint: string;
};

export function FriendList(props: FriendListProps) {
    const history = useHistory();

    const selectorLoginState = (state: RootState) => state.loginState;
    const selectorAccessToken = (state: RootState) => state.accessToken;
    const selectorFriendsState = (state: RootState) => state.friendsState;
    const selectorFriends = (state: RootState) => state.friends;

    const loginState = useTypedSelector(selectorLoginState);
    const accessToken = useTypedSelector(selectorAccessToken);
    const friendsState = useTypedSelector(selectorFriendsState);
    const friends = useTypedSelector(selectorFriends);

    const dispatch = useDispatch();
    const ai = getAppInsights();

    useEffect(() => {
        const populateFriends = async () => {
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(props.endpoint + "/api/users/me/friends", request);
                const data = await response.json();

                Database.set(DatabaseFields.FRIEND_LIST, data);

                dispatch(friendsLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
            } catch (error) {
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve friends.";
                dispatch(friendsLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (loginState) {
            populateFriends();
        }
    }, [loginState, friendsState, accessToken, ai, props, dispatch]);

    const renderFriends = (friends?: Player[]) => {
        return (
            <div>
                <h6>Click to play with friend</h6>
                <div className="row">
                    {friends?.map(friend =>
                        <Link to={{ pathname: "/play/new?friendID=" + friend?.id }} className="FriendList-link" key={friend?.id}>
                            <div className="template-1">
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
        dispatch(friendsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
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
                        <h6>
                            No friends found. Click to <button onClick={refresh}>refresh</button> or
                            <button onClick={addNewFriend}>add new</button> friend.
                        </h6>;
                }
                else {
                    contents = renderFriends(friends);
                }

                contents =
                    <div>
                        {contents}
                        <h6>
                            Or <button onClick={addNewFriend}>add new</button> friend
                        </h6>
                    </div>;
                break;
            case ProcessState.Error:
                contents = <h6>Oh no! Couldn't retrieve friends. Click to <button onClick={refresh}>refresh</button></h6>;
                break;
            default:
                contents = <h6><em>Loading...</em></h6>;
                break;
        }

        return (
            <div>
                <h4>{props.title}</h4>
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
