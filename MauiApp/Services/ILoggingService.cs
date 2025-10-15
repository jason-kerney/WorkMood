namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for application logging operations
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Gets or sets whether logging is enabled. When false, no logs will be generated.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Logs a message with the current timestamp
    /// </summary>
    /// <param name="message">The message to log</param>
    void Log(string message);

    /// <summary>
    /// Logs a message with the specified log level
    /// </summary>
    /// <param name="level">The log level</param>
    /// <param name="message">The message to log</param>
    void Log(LogLevel level, string message);

    /// <summary>
    /// Logs an exception with the specified message
    /// </summary>
    /// <param name="exception">The exception to log</param>
    /// <param name="message">Additional message</param>
    void LogException(Exception exception, string message = "");
}

/// <summary>
/// Extension methods for ILoggingService
/// </summary>
public static class LoggingServiceExtensions
{
    public static void LogInfo(this ILoggingService loggingService, string message)
    {
        loggingService.Log(LogLevel.Info, message);
    }

    public static void LogError(this ILoggingService loggingService, string message)
    {
        loggingService.Log(LogLevel.Error, message);
    }

    public static void LogWarning(this ILoggingService loggingService, string message)
    {
        loggingService.Log(LogLevel.Warning, message);
    }

    public static void LogDebug(this ILoggingService loggingService, string message)
    {
        loggingService.Log(LogLevel.Debug, message);
    }
}

/// <summary>
/// Log levels for categorizing log entries
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}