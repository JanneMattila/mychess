export class UserNotifications {
    name: string;
    enabled: boolean;

    endpoint: string;
    p256dh: string;
    auth: string;

    constructor() {
        this.name = "";
        this.enabled = false;
        this.endpoint = "";
        this.p256dh = "";
        this.auth = "";
    }
}
