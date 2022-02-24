// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using Microsoft.Extensions.Logging;
using MyChess.Backend;

namespace MyChess.Functions.Internal;

public static class SecurityValidatorLoggerExtensions
{
    private static readonly Action<ILogger, string, Exception> _funcSecInvalidIssuer;
    private static readonly Action<ILogger, string, Exception> _funcSecIssuer;
    private static readonly Action<ILogger, Exception> _funcSecNoAuthHeader;
    private static readonly Action<ILogger, Exception> _funcSecNoBearerToken;
    private static readonly Action<ILogger, Exception> _funcSecInitializing;
    private static readonly Action<ILogger, Exception> _funcSecInitialized;
    private static readonly Action<ILogger, Exception> _funcSecInitializingFailed;
    private static readonly Action<ILogger, Exception> _funcSecTokenValidationFailed;

    static SecurityValidatorLoggerExtensions()
    {
        _funcSecInvalidIssuer = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(LoggingEvents.FuncSecInvalidIssuer, nameof(FuncSecInvalidIssuer)),
            "Invalid issuer {Issuer}");
        _funcSecIssuer = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(LoggingEvents.FuncSecIssuer, nameof(FuncSecIssuer)),
            "Issuer tenant {Issuer}");
        _funcSecNoAuthHeader = LoggerMessage.Define(
            LogLevel.Trace,
            new EventId(LoggingEvents.FuncSecNoAuthHeader, nameof(FuncSecNoAuthHeader)),
            "Request does not contain authorization header");
        _funcSecNoBearerToken = LoggerMessage.Define(
            LogLevel.Trace,
            new EventId(LoggingEvents.FuncSecNoBearerToken, nameof(FuncSecNoBearerToken)),
            "Request does not contain Bearer token");
        _funcSecInitializing = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(LoggingEvents.FuncSecInitializing, nameof(FuncSecInitializing)),
            "Initializing OpenID configuration");
        _funcSecInitialized = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(LoggingEvents.FuncSecInitialized, nameof(FuncSecInitialized)),
            "Initialized OpenID configuration successfully");
        _funcSecInitializingFailed = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(LoggingEvents.FuncSecInitializingFailed, nameof(FuncSecInitializingFailed)),
            "Could not initialize OpenID configuration");
        _funcSecTokenValidationFailed = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(LoggingEvents.FuncSecTokenValidationFailed, nameof(FuncSecTokenValidationFailed)),
            "Token validation failed");
    }

    public static void FuncSecInvalidIssuer(this ILogger logger, string issuer) => _funcSecInvalidIssuer(logger, issuer, null);
    public static void FuncSecIssuer(this ILogger logger, string issuer) => _funcSecIssuer(logger, issuer, null);
    public static void FuncSecNoAuthHeader(this ILogger logger) => _funcSecNoAuthHeader(logger, null);
    public static void FuncSecNoBearerToken(this ILogger logger) => _funcSecNoBearerToken(logger, null);
    public static void FuncSecInitializing(this ILogger logger) => _funcSecInitializing(logger, null);
    public static void FuncSecInitialized(this ILogger logger) => _funcSecInitialized(logger, null);
    public static void FuncSecInitializingFailed(this ILogger logger, Exception ex) => _funcSecInitializingFailed(logger, ex);
    public static void FuncSecTokenValidationFailed(this ILogger logger, Exception ex) => _funcSecTokenValidationFailed(logger, ex);
}
