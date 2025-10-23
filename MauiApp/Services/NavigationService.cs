using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Navigation service implementation for MAUI applications.
/// Centralizes navigation and dialog operations with consistent error handling.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IPageNavigationShim _navigationShim;
    private readonly IPageDialogShim _dialogShim;

    // Original constructor for existing production code (no breaking changes)
    public NavigationService(Page page)
        : this(new PageNavigationShim(page), new PageDialogShim(page))
    {
    }

    // Testable constructor with dependency injection
    public NavigationService(IPageNavigationShim navigationShim, IPageDialogShim dialogShim)
    {
        _navigationShim = navigationShim ?? throw new ArgumentNullException(nameof(navigationShim));
        _dialogShim = dialogShim ?? throw new ArgumentNullException(nameof(dialogShim));
    }

    public async Task GoBackAsync()
    {
        try
        {
            await _navigationShim.PopAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to navigate back", ex);
        }
    }

    public async Task NavigateAsync(Page page)
    {
        try
        {
            await _navigationShim.PushAsync(page);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to navigate", ex);
        }
    }

    public async Task NavigateAsync(Func<Page> pageFactory)
    {
        try
        {
            var page = pageFactory();
            await _navigationShim.PushAsync(page);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to navigate", ex);
        }
    }

    public async Task ShowAlertAsync(string title, string message, string accept)
    {
        await _dialogShim.DisplayAlertAsync(title, message, accept);
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        return await _dialogShim.DisplayAlertAsync(title, message, accept, cancel);
    }

    public async Task ShowErrorAsync(string message, Exception? exception = null)
    {
        var fullMessage = exception != null
            ? $"{message}: {exception.Message}"
            : message;

        await ShowAlertAsync("Error", fullMessage, "OK");
    }
}