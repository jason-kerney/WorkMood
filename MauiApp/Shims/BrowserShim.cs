namespace WorkMood.MauiApp.Shims;

public class BrowserShim: IBrowserShim
{
    public Task<bool> OpenDefaultAsync(string url, BrowserLaunchOptions? options = null)
    {
        var uri = new Uri(url);
        return Browser.Default.OpenAsync(uri, options?.LaunchMode ?? BrowserLaunchMode.SystemPreferred);
    }
}
