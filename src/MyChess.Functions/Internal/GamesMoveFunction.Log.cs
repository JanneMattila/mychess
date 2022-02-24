// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using Microsoft.Extensions.Logging;
using MyChess.Backend;

namespace MyChess.Functions.Internal;

internal static class GamesMoveFunctionLoggerExtensions
{
    private static readonly Func<ILogger, IDisposable> _funcGamesMoveScope;
    private static readonly Action<ILogger, Exception> _funcGamesMoveStarted;
    private static readonly Action<ILogger, string, string, Exception> _funcGamesMoveUserDoesNotHavePermission;
    private static readonly Action<ILogger, string, Exception> _funcGamesMoveProcessingMethod;

    static GamesMoveFunctionLoggerExtensions()
    {
        _funcGamesMoveScope = LoggerMessage.DefineScope("GamesMove");
        _funcGamesMoveStarted = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(LoggingEvents.FuncGamesMoveStarted, nameof(FuncGamesMoveStarted)),
            "GamesMove function processing request.");
        _funcGamesMoveUserDoesNotHavePermission = LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(LoggingEvents.FuncGamesMoveUserDoesNotHavePermission, nameof(FuncGamesMoveUserDoesNotHavePermission)),
            "User {User} does not have permission {Permission}");
        _funcGamesMoveProcessingMethod = LoggerMessage.Define<string>(
            LogLevel.Trace,
            new EventId(LoggingEvents.FuncGamesMoveProcessingMethod, nameof(FuncGamesMoveProcessingMethod)),
            "Processing {Method} request");
    }

    public static IDisposable FuncGamesMoveScope(this ILogger logger) => _funcGamesMoveScope(logger);
    public static void FuncGamesMoveStarted(this ILogger logger) => _funcGamesMoveStarted(logger, null);
    public static void FuncGamesMoveUserDoesNotHavePermission(this ILogger logger, string user, string permission) => _funcGamesMoveUserDoesNotHavePermission(logger, user, permission, null);
    public static void FuncGamesMoveProcessingMethod(this ILogger logger, string method) => _funcGamesMoveProcessingMethod(logger, method, null);
}
