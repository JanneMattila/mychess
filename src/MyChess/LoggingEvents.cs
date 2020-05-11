namespace MyChess
{
    public static class LoggingEvents
    {
        public const int FuncGamesStarted = 1000;
        public const int FuncGamesUserDoesNotHavePermission = 1001;
        public const int FuncGamesProcessingMethod = 1002;
        public const int FuncGamesFetchAllGames = 1003;
        public const int FuncGamesFetchSingleGame = 1004;

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
    }
}
