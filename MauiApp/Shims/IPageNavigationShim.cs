using Microsoft.Maui.Controls;

namespace WorkMood.MauiApp.Shims;

/// <summary>
/// Abstraction interface for Page navigation operations.
/// Provides testable wrapper around Page.Navigation API.
/// </summary>
public interface IPageNavigationShim
{
    /// <summary>
    /// Navigates back by popping the current page from the navigation stack.
    /// </summary>
    /// <returns>A task representing the asynchronous navigation operation.</returns>
    Task PopAsync();

    /// <summary>
    /// Navigates forward by pushing a new page onto the navigation stack.
    /// </summary>
    /// <param name="page">The page to navigate to.</param>
    /// <returns>A task representing the asynchronous navigation operation.</returns>
    Task PushAsync(Page page);
}