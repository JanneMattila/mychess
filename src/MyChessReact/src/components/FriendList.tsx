import React, { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { RootState, ProcessState, friendsLoadingEvent } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link } from "react-router-dom";
import "./FriendList.css";
import { Player } from "../models/Player";

type FriendListProps = {
    title: string;
    endpoint: string;
};

export function FriendList(props: FriendListProps) {
    const selectorLoginState = (state: RootState) => state.loginState;
    const selectorAccessToken = (state: RootState) => state.accessToken;
    const selectorFriendsState = (state: RootState) => state.friendsState;
    const selectorFriends = (state: RootState) => state.friends;

    const loginState = useTypedSelector(selectorLoginState);
    const accessToken = useTypedSelector(selectorAccessToken);
    const friendsState = useTypedSelector(selectorFriendsState);
    const friends = useTypedSelector(selectorFriends);

    const [isFriendDialogOpen, showFriendDialog] = useState(false);
    const [friendName, setFriendName] = useState("");
    const [friendID, setFriendID] = useState("");

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

                dispatch(friendsLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
            } catch (error) {
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
                dispatch(friendsLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (loginState && !friendsState) {
            populateFriends();
        }
    }, [loginState, friendsState, accessToken, ai, props, dispatch]);

    const renderFriends = (friends?: Player[]) => {
        return (
            <div className="row">
                {friends?.map(friend =>
                    <Link to={{ pathname: "/friend/" + friend?.id }} className="FriendList-link" key={friend?.id}>
                        <div className="template-1">
                            <div className="nameTemplate">
                                {friend?.name}
                            </div>
                        </div>
                    </Link>
                )
                }
            </div >
        );
    }

    const refresh = () => {
        dispatch(friendsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
    }

    const showAddNewFriend = () => {
        showFriendDialog(true)
    }

    const addFriend = () => {
    }

    const cancel = () => {
        setFriendID("");
        setFriendName("");
        showFriendDialog(false);
    }

    const visible = {
    }


    const hidden = {
        display: "none",
    }

    if (loginState === ProcessState.Success) {

        let contents: JSX.Element;
        switch (friendsState) {
            case ProcessState.Success:
                if (friends && friends?.length === 0) {
                    contents =
                        <div>
                            <h6>
                                No friends found. Click to <button onClick={refresh}>refresh</button> or
                                <button onClick={showAddNewFriend}>add new</button> friend.
                            </h6>
                            <div id="addFriend" style={(isFriendDialogOpen ? visible : hidden)}>
                                <label>
                                    Friend identifier<br />
                                    <input type="text" value={friendID} onChange={e => setFriendID(e.target.value)} />
                                </label>
                                <br />
                                <label>
                                    Friend name<br />
                                    <input type="text" value={friendName} onChange={e => setFriendName(e.target.value)} />
                                </label>
                                <br />
                                <button onClick={addFriend}><span role="img" aria-label="Add friend">✅</span> Add friend</button>
                                <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                            </div>
                        </div>;
                }
                else {
                    contents = renderFriends(friends);
                }
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
