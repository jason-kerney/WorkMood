namespace WorkMood.MauiApp.Services;

/// <summary>
/// Implementation of IBrowserService for MAUI applications
/// Handles opening URLs in the system's default browser
/// </summary>
public class BrowserService : IBrowserService
{
    /// <summary>
    /// Opens a URL in the system's default browser
    /// </summary>
    /// <param name="url">The URL to open</param>
    /// <param name="options">Optional browser launch options</param>
    /// <returns>A task that represents the asynchronous operation, returning true if successful</returns>
    public async Task<bool> OpenAsync(string url, BrowserLaunchOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        try
        {
            var uri = new Uri(url);
            return await OpenAsync(uri, options);
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Opens a URI in the system's default browser
    /// </summary>
    /// <param name="uri">The URI to open</param>
    /// <param name="options">Optional browser launch options</param>
    /// <returns>A task that represents the asynchronous operation, returning true if successful</returns>
    public async Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions? options = null)
    {
        if (uri == null)
        {
            return false;
        }

        try
        {
            var launchMode = options?.LaunchMode ?? BrowserLaunchMode.SystemPreferred;
            return await Browser.Default.OpenAsync(uri, launchMode);
        }
        catch (Exception)
        {
            // Return false if the browser couldn't be opened
            return false;
        }
    }
}