// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;
using MyChess.Backend;

namespace MyChess.Functions.Internal
{
    internal static class SettingsFunctionLoggerExtensions
    {
        private static readonly Func<ILogger, IDisposable> _funcSettingsScope;
        private static readonly Action<ILogger, Exception> _funcSettingsStarted;
        private static readonly Action<ILogger, string, string, Exception> _funcSettingsUserDoesNotHavePermission;
        private static readonly Action<ILogger, string, Exception> _funcSettingsProcessingMethod;
        private static readonly Action<ILogger, Exception> _funcSettingsFetchSettings;
        private static readonly Action<ILogger, Exception> _funcSettingsUpdateSettings;

        static SettingsFunctionLoggerExtensions()
        {
            _funcSettingsScope = LoggerMessage.DefineScope("Me");
            _funcSettingsStarted = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(LoggingEvents.FuncSettingsStarted, nameof(FuncSettingsStarted)),
                "Me function processing request");
            _funcSettingsUserDoesNotHavePermission = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.FuncSettingsUserDoesNotHavePermission, nameof(FuncSettingsUserDoesNotHavePermission)),
                "User {User} does not have permission {Permission}");
            _funcSettingsProcessingMethod = LoggerMessage.Define<string>(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncSettingsUserDoesNotHavePermission, nameof(FuncSettingsProcessingMethod)),
                "Processing {Method} request");
            _funcSettingsFetchSettings = LoggerMessage.Define(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncSettingsFetchSettings, nameof(FuncSettingsFetchSettings)),
                "Fetch settings");
            _funcSettingsUpdateSettings = LoggerMessage.Define(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncSettingsUpdateSettings, nameof(FuncSettingsUpdateSettings)),
                "Update settings");
        }

        public static IDisposable FuncSettingsScope(this ILogger logger) => _funcSettingsScope(logger);
        public static void FuncSettingsStarted(this ILogger logger) => _funcSettingsStarted(logger, null);
        public static void FuncSettingsUserDoesNotHavePermission(this ILogger logger, string user, string permission) => _funcSettingsUserDoesNotHavePermission(logger, user, permission, null);
        public static void FuncSettingsProcessingMethod(this ILogger logger, string method) => _funcSettingsProcessingMethod(logger, method, null);
        public static void FuncSettingsFetchSettings(this ILogger logger) => _funcSettingsFetchSettings(logger, null);
        public static void FuncSettingsUpdateSettings(this ILogger logger) => _funcSettingsUpdateSettings(logger, null);
    }
}
