import React, { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { getAppInsights } from "./TelemetryService";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { gamesMoveUpdateEvent, ProcessState } from "../actions";
import { MyChessGameMove } from "../models/MyChessGameMove";

type SignalRServiceProps = {
    endpoint: string;
};

export function SignalRService(props: SignalRServiceProps) {
    const loginState = useTypedSelector(state => state.loginState);
    const accessToken = useTypedSelector(state => state.accessToken);
    const [connection, setConnection] = useState<HubConnection | undefined>(undefined);

    const dispatch = useDispatch();
    const ai = getAppInsights();
    const endpoint = props.endpoint;

    useEffect(() => {
        if (loginState === ProcessState.Success &&
            connection === undefined &&
            accessToken !== undefined) {

            const request: RequestInit = {
                method: "POST",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            fetch(`${endpoint}/api/negotiate`, request)
                .then(response => {
                    return response.json();
                })
                .then(data => {
                    const options = {
                        accessTokenFactory: () => data.accessToken
                    };
                    const hubConnection = new HubConnectionBuilder()
                        .withUrl(data.url, options)
                        .configureLogging(LogLevel.Information)
                        .withAutomaticReconnect()
                        .build();

                    setConnection(hubConnection);
                    if (hubConnection) {
                        hubConnection.on("MoveUpdate", (data) => {
                            console.log("Incoming signalr message - MoveUpdate:");
                            const move = JSON.parse(data) as MyChessGameMove;
                            console.log(move);

                            // Post move update
                            dispatch(gamesMoveUpdateEvent(move));
                        });
                        hubConnection.start()
                            .then(() => console.log('connected!'))
                            .catch(error => {
                                console.error(error);
                                ai.trackException({ exception: error });
                            });
                    }
                })
                .catch(error => {
                    console.log(error);
                    ai.trackException({ exception: error });
                });
        }
    }, [loginState, connection, endpoint, accessToken, ai, dispatch]);

    return (
        <>
        </>
    );
}
