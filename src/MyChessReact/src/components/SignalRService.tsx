import React, { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { getAppInsights } from "./TelemetryService";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { ProcessState } from "../actions";

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
                        hubConnection.start()
                            .then(() => console.log('connected!'))
                            .catch(console.error);
                    }
                })
                .catch(error => {
                    console.log(error);
                });
        }
    }, [loginState, connection, endpoint, accessToken]);

    return (
        <>
        </>
    );
}
