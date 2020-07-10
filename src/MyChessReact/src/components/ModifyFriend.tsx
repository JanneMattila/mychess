import React, { useEffect, useState } from "react";
import { useTypedSelector } from "../reducers";
import { ProcessState } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { useHistory } from "react-router-dom";
import "./FriendList.css";
import { User } from "../models/User";
import { Database, DatabaseFields } from "../data/Database";
import { BackendService } from "./BackendService";

type ModifyFriendProps = {
    id?: string;
    title: string;
    endpoint: string;
};

export function ModifyFriend(props: ModifyFriendProps) {
    const history = useHistory();

    const loginState = useTypedSelector(state => state.loginState);
    const friendUpsertState = useTypedSelector(state => state.friendUpsertState);
    const error = useTypedSelector(state => state.error);
    const errorLink = useTypedSelector(state => state.errorLink);

    const [friendName, setFriendName] = useState("");
    const [friendID, setFriendID] = useState("");
    const [player, setPlayer] = useState<User | undefined>(undefined);

    const ai = getAppInsights();

    useEffect(() => {
        ai.trackEvent({
            name: "ModifyFriend-Load", properties: {
                identifier: props.id !== undefined,
            }
        });

        if (props.id) {
            setFriendID(props.id);

            const friends = Database.get<Array<User>>(DatabaseFields.FRIEND_LIST);
            if (friends) {
                const existingFriend = friends.find(f => f.id === props.id);
                if (existingFriend) {
                    setFriendName(existingFriend.name);
                }
            }
        }
    }, [props, ai]);

    const addFriend = () => {
        ai.trackEvent({ name: "ModifyFriend-Save" });

        console.log("save friend");
        setPlayer({
            id: friendID,
            name: friendName
        });
    }

    const cancel = () => {
        ai.trackEvent({ name: "ModifyFriend-Cancel" });

        history.push("/friends");
    }

    const visible = {
    }

    const hidden = {
        display: "none",
    }

    if (loginState === ProcessState.Success) {
        return (
            <div>
                <div className="title">{props.title}</div>
                <div id="addFriend" className="Friends-Container">
                    <label className="subtitle">
                        Friend identifier<br />
                        <div className="FriendList-AddFriendSubText">
                            This is your friends identifier.<br />
                            You need this in order to connect to your friend.
                        </div>
                        <input type="text" value={friendID} className="FriendList-Identifier" onChange={e => setFriendID(e.target.value)} />
                    </label>
                    <br />
                    <label className="subtitle">
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
                    <div style={friendUpsertState === ProcessState.Error ? visible : hidden}>
                        <a className="FriendList-AddFriendError" href={errorLink} target="_blank" rel="noopener noreferrer">{error}</a>
                    </div>
                    <BackendService endpoint={props.endpoint} upsertFriend={player} />
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
