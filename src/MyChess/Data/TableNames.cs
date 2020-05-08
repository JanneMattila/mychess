namespace MyChess.Data
{
    public static class TableNames
    {
        public const string Users = "Users";
        public const string UserFriends = "UserFriends";
        public const string UserNotifications = "UserNotifications";
        public const string UserSettings = "UserSettings";

        // Mapping tables
        public const string UserID2User = "UserID2User";

        // Games tables
        public const string GamesWaitingForYou = "GamesWaitingForYou";
        public const string GamesWaitingForOpponent = "GamesWaitingForOpponent";
        public const string GamesArchive = "GamesArchive";
    }
}
