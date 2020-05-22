// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Internal
{
    internal static class MeFunctionLoggerExtensions
    {
        private static readonly Func<ILogger, IDisposable> _funcMeScope;
        private static readonly Action<ILogger, Exception> _funcMeStarted;
        private static readonly Action<ILogger, string, string, Exception> _funcMeUserDoesNotHavePermission;
        private static readonly Action<ILogger, string, Exception> _funcMeProcessingMethod;
        private static readonly Action<ILogger, Exception> _funcMeFetchMe;

        static MeFunctionLoggerExtensions()
        {
            _funcMeScope = LoggerMessage.DefineScope("Me");
            _funcMeStarted = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(LoggingEvents.FuncMeStarted, nameof(FuncMeStarted)),
                "Me function processing request");
            _funcMeUserDoesNotHavePermission = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.FuncMeUserDoesNotHavePermission, nameof(FuncMeUserDoesNotHavePermission)),
                "User {User} does not have permission {Permission}");
            _funcMeProcessingMethod = LoggerMessage.Define<string>(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncMeUserDoesNotHavePermission, nameof(FuncMeProcessingMethod)),
                "Processing {Method} request");
            _funcMeFetchMe = LoggerMessage.Define(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncMeFetchMe, nameof(FuncMeFetchMe)),
                "Fetch Me");
        }

        public static IDisposable FuncMeScope(this ILogger logger) => _funcMeScope(logger);
        public static void FuncMeStarted(this ILogger logger) => _funcMeStarted(logger, null);
        public static void FuncMeUserDoesNotHavePermission(this ILogger logger, string user, string permission) => _funcMeUserDoesNotHavePermission(logger, user, permission, null);
        public static void FuncMeProcessingMethod(this ILogger logger, string method) => _funcMeProcessingMethod(logger, method, null);
        public static void FuncMeFetchMe(this ILogger logger) => _funcMeFetchMe(logger, null);
    }
}
