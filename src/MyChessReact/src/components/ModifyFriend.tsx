import React, { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { RootState, ProcessState, friendsLoadingEvent } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link, useLocation, useHistory, useParams } from "react-router-dom";
import "./FriendList.css";
import { Player } from "../models/Player";
import { ProblemDetail } from "../models/ProblemDetail";
import { QueryStringParser } from "../helpers/QueryStringParser";

type ModifyFriendProps = {
    title: string;
    endpoint: string;
};

export function ModifyFriend(props: ModifyFriendProps) {
    const location = useLocation();
    const history = useHistory();

    const selectorLoginState = (state: RootState) => state.loginState;
    const selectorAccessToken = (state: RootState) => state.accessToken;

    const loginState = useTypedSelector(selectorLoginState);
    const accessToken = useTypedSelector(selectorAccessToken);

    const [friendName, setFriendName] = useState("");
    const [friendIDField, setFriendIDField] = useState("");
    const [friendError, setFriendError] = useState({ title: "", link: "" });

    const dispatch = useDispatch();
    const ai = getAppInsights();

    useEffect(() => {
        if (location.search) {
            const map = QueryStringParser.parse(location.search);
            const friendID = map.get("friendID");
            if (friendID !== undefined) {
                setFriendIDField(friendID);
            }
        }
    });

    const addFriend = async () => {
        setFriendError({ title: "", link: "" });
        const json: Player = {
            "id": friendIDField,
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

            if (location.search) {
                history.push("/friends");
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
                        <input type="text" value={friendIDField} className="FriendList-Identifier" onChange={e => setFriendIDField(e.target.value)} />
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
