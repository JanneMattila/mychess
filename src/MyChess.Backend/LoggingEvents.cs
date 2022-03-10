namespace MyChess.Backend;

public static class LoggingEvents
{
    public const int DataContextInitializing = 100;
    public const int DataContextInitialized = 101;
    public const int DataContextInitializeTable = 102;

    public const int FuncGamesStarted = 1000;
    public const int FuncGamesUserDoesNotHavePermission = 1001;
    public const int FuncGamesProcessingMethod = 1002;
    public const int FuncGamesFetchAllGames = 1003;
    public const int FuncGamesFetchSingleGame = 1004;
    public const int FuncGamesCreateNewGame = 1005;
    public const int FuncGamesDeleteGame = 1006;

    public const int FuncSecNoAuthHeader = 1100;
    public const int FuncSecNoBearerToken = 1101;
    public const int FuncSecInitializing = 1102;
    public const int FuncSecInitialized = 1103;
    public const int FuncSecInitializingFailed = 1104;
    public const int FuncSecTokenValidationFailed = 1105;
    public const int FuncSecInvalidIssuer = 1106;
    public const int FuncSecIssuer = 1107;

    public const int FuncGamesMoveStarted = 1200;
    public const int FuncGamesMoveUserDoesNotHavePermission = 1201;
    public const int FuncGamesMoveProcessingMethod = 1202;

    public const int FuncFriendsStarted = 1300;
    public const int FuncFriendsUserDoesNotHavePermission = 1301;
    public const int FuncFriendsProcessingMethod = 1302;
    public const int FuncFriendsFetchAllFriends = 1303;
    public const int FuncFriendsFetchSingleFriend = 1304;
    public const int FuncFriendsAddNewFriend = 1305;

    public const int FuncMeStarted = 1400;
    public const int FuncMeUserDoesNotHavePermission = 1401;
    public const int FuncMeProcessingMethod = 1402;
    public const int FuncMeFetchMe = 1403;

    public const int FuncSettingsStarted = 1500;
    public const int FuncSettingsUserDoesNotHavePermission = 1501;
    public const int FuncSettingsProcessingMethod = 1502;
    public const int FuncSettingsFetchSettings = 1503;
    public const int FuncSettingsUpdateSettings = 1504;

    public const int BaseHandlerCreateNewUser = 2000;
    public const int BaseHandlerNewUserCreated = 2001;
    public const int BaseHandlerExistingUserFound = 2002;
    public const int BaseHandlerUserLookupFoundByUserID = 2003;
    public const int BaseHandlerUserFoundByUserID = 2004;
    public const int BaseHandlerUserNotFoundByUserID = 2005;
    public const int BaseHandlerUserLookupNotFoundByUserID = 2006;

    public const int GameHandlerGameFound = 2100;
    public const int GameHandlerGameNotFound = 2101;
    public const int GameHandlerGamesFound = 2102;
    public const int GameHandlerOpponentNotFound = 2103;
    public const int GameHandlerMoveGameNotFound = 2104;
    public const int GameHandlerMoveInvalidPlayer = 2105;
    public const int GameHandlerMoveNotPlayerTurn = 2106;
    public const int GameHandlerGameNotFoundFromTable = 2107;
    public const int GameHandlerDeleteGameNotFound = 2108;
    public const int GameHandlerDeleteGameAlreadyArchived = 2109;

    public const int FriendHandlerFriendFound = 2200;
    public const int FriendHandlerFriendNotFound = 2201;
    public const int FriendHandlerFriendsFound = 2202;
    public const int FriendHandlerPlayerNotFound = 2203;
    public const int FriendHandlerAddingNameToFriend = 2204;
    public const int FriendHandlerExistingFriend = 2205;

    public const int SettingsHandlerSettingsFound = 2300;
    public const int SettingsHandlerSettingsNotFound = 2301;
    public const int SettingsHandlerNotificationsFound = 2302;
    public const int SettingsHandlerUpdateSettings = 2303;

    public const int NotificationsHandlerSendingNotifications = 2400;
    public const int NotificationsHandlerSendStatistics = 2401;
    public const int NotificationsHandlerSendFailed = 2402;

    public static string CreateLinkToProblemDescription(int eventID)
    {
        return $"https://bit.ly/MyChessProblems#{eventID}";
    }
}
