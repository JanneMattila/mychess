export enum DatabaseFields {
    // Auth related fields
    AUTH_REDIRECT = "Auth/Redirect",

    // Game related fields
    GAMES_LIST = "Games/List",

    // Friend related fields
    FRIEND_LIST = "Friends/List"
};

export class Database {
    private static prefix: string = "mychess";

    public static get<T>(key: string): T | undefined {
        const value = localStorage.getItem(`${this.prefix}-${key}`);
        if (value) {
            const json = JSON.parse(value);
            return json as T;
        }
        return undefined;
    }

    public static set(key: string, value: any): void {
        const json = JSON.stringify(value);
        localStorage.setItem(`${this.prefix}-${key}`, json);
    }

    public static delete(key: string): void {
        localStorage.removeItem(`${this.prefix}-${key}`);
    }
}
