namespace WorkMood.MauiApp.Shims;

public interface IBrowserShim
{
    Task<bool> OpenDefaultAsync(string url, BrowserLaunchOptions? options = null);
}
