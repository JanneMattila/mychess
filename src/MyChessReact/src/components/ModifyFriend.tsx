import React, { useEffect, useState } from "react";
import { useTypedSelector } from "../reducers";
import { RootState, ProcessState } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { useHistory } from "react-router-dom";
import "./FriendList.css";
import { Player } from "../models/Player";
import { ProblemDetail } from "../models/ProblemDetail";
import { Database, DatabaseFields } from "../data/Database";

type ModifyFriendProps = {
    id?: string;
    title: string;
    endpoint: string;
};

export function ModifyFriend(props: ModifyFriendProps) {
    const history = useHistory();

    const selectorLoginState = (state: RootState) => state.loginState;
    const selectorAccessToken = (state: RootState) => state.accessToken;

    const loginState = useTypedSelector(selectorLoginState);
    const accessToken = useTypedSelector(selectorAccessToken);

    const [friendName, setFriendName] = useState("");
    const [friendID, setFriendID] = useState("");
    const [friendError, setFriendError] = useState({ title: "", link: "" });

    const ai = getAppInsights();

    useEffect(() => {
        if (props.id) {
            setFriendID(props.id);

            const friends = Database.get<Array<Player>>(DatabaseFields.FRIEND_LIST);
            if (friends) {
                const existingFriend = friends.find(f => f.id === props.id);
                if (existingFriend) {
                    setFriendName(existingFriend.name);
                }
            }
        }
    }, [props]);

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
            console.log(data);

            if (response.ok) {
                history.push("/friends");
            } else {
                const ex = data as ProblemDetail;
                if (ex.title !== undefined && ex.instance !== undefined) {
                    console.log(ex);
                    setFriendError({ title: ex.title, link: ex.instance });
                }
            }
        } catch (error) {
            ai.trackException(error);

            const errorMessage = error.errorMessage ? error.errorMessage : "Unable to modify friend.";
            console.log(error);
            console.log(errorMessage);
        }
    }

    const cancel = () => {
        history.push("/friends");
    }

    if (loginState === ProcessState.Success) {
        return (
            <div>
                <h4>{props.title}</h4>
                <div id="addFriend">
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
                    <button onClick={addFriend}><span role="img" aria-label={props.title}>✅</span> {props.title}</button>
                    <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                    <div>
                        <a className="FriendList-AddFriendError" href={friendError.link} target="_blank" rel="noopener noreferrer">{friendError.title}</a>
                    </div>
                </div>
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
