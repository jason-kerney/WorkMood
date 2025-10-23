using Microsoft.Maui.Controls;

namespace WorkMood.MauiApp.Shims;

/// <summary>
/// Concrete implementation of IPageDialogShim that wraps Page.DisplayAlert operations.
/// Provides a testable abstraction layer over MAUI's dialog API.
/// </summary>
public class PageDialogShim : IPageDialogShim
{
    private readonly Page _page;

    /// <summary>
    /// Initializes a new instance of PageDialogShim.
    /// </summary>
    /// <param name="page">The page instance to wrap for dialog operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when page is null.</exception>
    public PageDialogShim(Page page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    /// <inheritdoc/>
    public async Task DisplayAlertAsync(string title, string message, string accept)
    {
        await _page.DisplayAlert(title, message, accept);
    }

    /// <inheritdoc/>
    public async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
    {
        return await _page.DisplayAlert(title, message, accept, cancel);
    }
}