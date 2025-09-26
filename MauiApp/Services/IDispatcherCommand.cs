using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Command interface for handling time-aware operations in the dispatcher service
/// </summary>
public interface IDispatcherCommand
{
    /// <summary>
    /// Processes a timer tick, typically handling date changes and related operations
    /// </summary>
    /// <param name="oldDate">The previous date</param>
    /// <param name="newDate">The new date</param>
    /// <param name="currentRecord">The current mood record state, if any</param>
    /// <returns>The result of the tick processing operation</returns>
    Task<CommandResult> ProcessTickAsync(DateOnly oldDate, DateOnly newDate, MoodEntryOld? currentRecord = null);
}

/// <summary>
/// Result of a dispatcher command operation
/// </summary>
public class CommandResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    
    public static CommandResult Succeeded(string? message = null, object? data = null) 
        => new() { Success = true, Message = message, Data = data };
    
    public static CommandResult Failed(string message, object? data = null) 
        => new() { Success = false, Message = message, Data = data };
    
    public static CommandResult NoAction(string message = "No action needed") 
        => new() { Success = true, Message = message };
}