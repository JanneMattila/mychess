import { useDispatch } from "react-redux";
import { getAppInsights } from "../components/TelemetryService";
import { gamesLoadingEvent, ProcessState, friendsLoadingEvent, friendUpsertEvent } from "../actions";
import { DatabaseFields, Database } from "./Database";
import { ProblemDetail } from "../models/ProblemDetail";
import { Player } from "../models/Player";

export class BackendService {

    private dispatch = useDispatch();
    private ai = getAppInsights();

    private endpoint: string = "";
    private accessToken: string = "";

    public constructor(endpoint: string, accessToken: string) {
        this.endpoint = endpoint;
        this.accessToken = accessToken;
    }

    public getFriends = async () => {
        this.dispatch(friendsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
        const request: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + this.accessToken
            }
        };

        try {
            const response = await fetch(this.endpoint + "/api/users/me/friends", request);
            const data = await response.json();

            Database.set(DatabaseFields.FRIEND_LIST, data);

            this.dispatch(friendsLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
        } catch (error) {
            this.ai.trackException(error);

            const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve friends.";
            this.dispatch(friendsLoadingEvent(ProcessState.Error, errorMessage));
        }
    }

    public upsertFriend = async (id: string, name: string) => {
        this.dispatch(friendUpsertEvent(ProcessState.NotStarted, "" /* Clear error message */, "" /* Clear error link*/));
        const json: Player = {
            "id": id,
            "name": name,
        };

        const request: RequestInit = {
            method: "POST",
            body: JSON.stringify(json),
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + this.accessToken
            }
        };

        try {
            const response = await fetch(this.endpoint + "/api/users/me/friends", request);
            const data = await response.json();
            console.log(data);

            if (response.ok) {
                this.dispatch(friendUpsertEvent(ProcessState.Success, "" /* Clear error message */, "" /* Clear error link*/));
                //history.push("/friends");
            } else {
                const ex = data as ProblemDetail;
                if (ex.title !== undefined && ex.instance !== undefined) {
                    console.log(ex);
                    this.dispatch(friendUpsertEvent(ProcessState.Error, ex.title, ex.instance));
                    //setFriendError({ title: ex.title, link: ex.instance });
                }
            }
        } catch (error) {
            this.ai.trackException(error);

            const errorMessage = error.errorMessage ? error.errorMessage : "Unable to modify friend.";
            this.dispatch(friendUpsertEvent(ProcessState.Error, errorMessage, ""));

            console.log(error);
            console.log(errorMessage);
        }
    }

    public getGames = async () => {
        this.dispatch(gamesLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
        const request: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + this.accessToken
            }
        };

        try {
            const response = await fetch(this.endpoint + "/api/games", request);
            const data = await response.json();

            this.dispatch(gamesLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
        } catch (error) {
            this.ai.trackException(error);

            const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
            this.dispatch(gamesLoadingEvent(ProcessState.Error, errorMessage));
        }
    }
}
