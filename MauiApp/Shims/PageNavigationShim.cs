using Microsoft.Maui.Controls;

namespace WorkMood.MauiApp.Shims;

/// <summary>
/// Concrete implementation of IPageNavigationShim that wraps Page.Navigation operations.
/// Provides a testable abstraction layer over MAUI's navigation API.
/// </summary>
public class PageNavigationShim : IPageNavigationShim
{
    private readonly Page _page;

    /// <summary>
    /// Initializes a new instance of PageNavigationShim.
    /// </summary>
    /// <param name="page">The page instance to wrap for navigation operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when page is null.</exception>
    public PageNavigationShim(Page page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    /// <inheritdoc/>
    public async Task PopAsync()
    {
        await _page.Navigation.PopAsync();
    }

    /// <inheritdoc/>
    public async Task PushAsync(Page page)
    {
        await _page.Navigation.PushAsync(page);
    }
}