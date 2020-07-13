import { UserNotifications } from "./UserNotifications";

export class UserSettings {
    id: string;

    playAlwaysUp: boolean;
    notifications: UserNotifications[];

    constructor() {
        this.id = "";

        this.playAlwaysUp = false;
        this.notifications = [];
    }
}
