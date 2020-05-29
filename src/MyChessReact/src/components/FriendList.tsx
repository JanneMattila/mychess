import React, { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { RootState, ProcessState, friendsLoadingEvent } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link } from "react-router-dom";
import "./FriendList.css";
import { Player } from "../models/Player";
import { ProblemDetail } from "../models/ProblemDetail";

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
    const [friendError, setFriendError] = useState({ title: "", link: "" });

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

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve friends.";
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
        cancel();
        dispatch(friendsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
    }

    const showAddNewFriend = () => {
        showFriendDialog(true)
    }

    const addFriend = async () => {
        setFriendError({ title: "", link: "" });
        const json: Player = {
            "id": friendID,
            "name": friendName,
        };

        const request: RequestInit = {
            method: "POST",
            body: JSON.stringify(json),
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + accessToken
            }
        };

        try {
            const response = await fetch(props.endpoint + "/api/users/me/friends", request);
            const data = await response.json();

            if (response.status !== 200) {
                const ex = data as ProblemDetail;
                if (ex.title !== undefined && ex.instance !== undefined) {
                    console.log(ex);
                    setFriendError({ title: ex.title, link: ex.instance });
                }
            }
            console.log(data);
        } catch (error) {
            ai.trackException(error);

            const errorMessage = error.errorMessage ? error.errorMessage : "Unable to add friend.";
            console.log(error);
            console.log(errorMessage);
        }
    }

    const cancel = () => {
        setFriendID("");
        setFriendName("");
        showFriendDialog(false);
        setFriendError({ title: "", link: "" });
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
                        <h6>
                            No friends found. Click to <button onClick={refresh}>refresh</button> or
                            <button onClick={showAddNewFriend}>add new</button> friend.
                        </h6>;
                }
                else {
                    contents = renderFriends(friends);
                }

                contents =
                    <div>
                        {contents}
                        <h6>
                            Or <button onClick={showAddNewFriend}>add new</button> friend
                        </h6>
                        <div id="addFriend" style={(isFriendDialogOpen ? visible : hidden)}>
                            <label className="FriendList-AddFriend">
                                Friend identifier<br />
                                <div className="FriendList-AddFriendSubText">
                                    This is your friends identifier.<br />
                                        You need this in order to connect to your friend.
                                    </div>
                                <input type="text" value={friendID} className="FriendList-Identifier" onChange={e => setFriendID(e.target.value)} />
                            </label>
                            <br />
                            <label className="FriendList-AddFriend">
                                Friend name<br />
                                <div className="FriendList-AddFriendSubText">
                                    This is your friends name.<br />
                                        This is <b>only visible to you</b>.
                                    </div>
                                <input type="text" value={friendName} onChange={e => setFriendName(e.target.value)} />
                            </label>
                            <br />
                            <button onClick={addFriend}><span role="img" aria-label="Add friend">✅</span> Add friend</button>
                            <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                            <div>
                                <a className="FriendList-AddFriendError" href={friendError.link} target="_blank" rel="noopener noreferrer">{friendError.title}</a>
                            </div>
                        </div>
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
