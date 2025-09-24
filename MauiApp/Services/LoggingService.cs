namespace WorkMood.MauiApp.Services;

/// <summary>
/// File-based logging service implementation
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly string _logFilePath;
    private readonly object _lockObject = new object();

    public LoggingService()
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        _logFilePath = Path.Combine(desktopPath, "WorkMood_Debug.log");
    }

    /// <summary>
    /// Logs a message with Info level
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Log(string message)
    {
        Log(LogLevel.Info, message);
    }

    /// <summary>
    /// Logs a message with the specified log level
    /// </summary>
    /// <param name="level">The log level</param>
    /// <param name="message">The message to log</param>
    public void Log(LogLevel level, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var levelString = level.ToString().ToUpper().PadRight(7);
            var logEntry = $"[{timestamp}] [{levelString}] {message}";

            lock (_lockObject)
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
        }
        catch
        {
            // Silently ignore logging errors to prevent cascading failures
            // In a production app, you might want to use a fallback logging mechanism
        }
    }

    /// <summary>
    /// Logs an exception with additional message
    /// </summary>
    /// <param name="exception">The exception to log</param>
    /// <param name="message">Additional message</param>
    public void LogException(Exception exception, string message = "")
    {
        if (exception == null)
            return;

        var fullMessage = string.IsNullOrWhiteSpace(message) 
            ? $"Exception: {exception.Message}" 
            : $"{message} - Exception: {exception.Message}";

        Log(LogLevel.Error, fullMessage);
        
        // Log stack trace on separate line for better readability
        if (!string.IsNullOrWhiteSpace(exception.StackTrace))
        {
            Log(LogLevel.Error, $"Stack Trace: {exception.StackTrace}");
        }

        // Log inner exception if present
        if (exception.InnerException != null)
        {
            LogException(exception.InnerException, "Inner Exception");
        }
    }
}