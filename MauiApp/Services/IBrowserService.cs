namespace WorkMood.MauiApp.Services;

/// <summary>
/// Interface for browser operations following the Dependency Inversion Principle
/// Abstracts browser functionality to allow for testability and flexibility
/// </summary>
public interface IBrowserService
{
    /// <summary>
    /// Opens a URL in the system's default browser
    /// </summary>
    /// <param name="url">The URL to open</param>
    /// <param name="options">Optional browser launch options</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<bool> OpenAsync(string url, BrowserLaunchOptions? options = null);
}