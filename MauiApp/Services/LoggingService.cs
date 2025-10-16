using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// File-based logging service implementation
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly string _logFilePath;
    private readonly object _lockObject = new object();
    private readonly IFileShim _fileShim;
    private readonly IDateShim _dateShim;

    /// <summary>
    /// Gets or sets whether logging is enabled. When false, no logs will be generated.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the minimum log level that will be written to the log file.
    /// Logs below this level will be ignored.
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; }

    public LoggingService(IFileShim fileShim, IDateShim dateShim, IFolderShim folderShim)
    {
        _fileShim = fileShim ?? throw new ArgumentNullException(nameof(fileShim));
        _dateShim = dateShim ?? throw new ArgumentNullException(nameof(dateShim));
        
        var desktopPath = folderShim?.GetDesktopFolder() ?? throw new ArgumentNullException(nameof(folderShim));
        _logFilePath = folderShim.CombinePaths(desktopPath, "WorkMood_Debug.log");
        IsEnabled = false;
        MinimumLogLevel = LogLevel.Info; // Default to Info level
    }

    /// <summary>
    /// Creates a new logging service with default shims (for backwards compatibility)
    /// </summary>
    public LoggingService() : this(new FileShim(), new DateShim(), new FolderShim())
    {
    }

    /// <summary>
    /// Logs a message with Info level
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Log(string message)
    {
        if (!IsEnabled)
            return;
            
        Log(LogLevel.Info, message);
    }

    /// <summary>
    /// Logs a message with the specified log level
    /// </summary>
    /// <param name="level">The log level</param>
    /// <param name="message">The message to log</param>
    public void Log(LogLevel level, string message)
    {
        if (!IsEnabled)
            return;
            
        if (string.IsNullOrWhiteSpace(message))
            return;

        // Check if the log level meets the minimum threshold
        if ((int)level < (int)MinimumLogLevel)
            return;

        try
        {
            var timestamp = _dateShim.Now().ToString("yyyy-MM-dd HH:mm:ss.fff");
            var levelString = level.ToString().ToUpper().PadRight(7);
            var logEntry = $"[{timestamp}] [{levelString}] {message}";

            lock (_lockObject)
            {
                _fileShim.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
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
        if (!IsEnabled)
            return;
            
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