using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Implementation of IBrowserService for MAUI applications
/// Handles opening URLs in the system's default browser
/// </summary>
public class BrowserService(IBrowserShim browserShim) : IBrowserService
{
    private readonly IBrowserShim _browserShim = browserShim;

    public BrowserService() : this(new BrowserShim()) { }

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
            var launchMode = options?.LaunchMode ?? BrowserLaunchMode.SystemPreferred;
            return await _browserShim.OpenDefaultAsync(url, options);
        }
        catch (UriFormatException)
        {
            return false;
        }
    }
}