import { UserNotifications } from "./UserNotifications";

export class UserSettings {
    playAlwaysUp: boolean;
    notifications: UserNotifications[];

    constructor() {
        this.playAlwaysUp = false;
        this.notifications = [];
    }
}
