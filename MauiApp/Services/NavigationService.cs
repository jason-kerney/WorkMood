using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Navigation service implementation for MAUI applications.
/// Centralizes navigation and dialog operations with consistent error handling.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Page _page;

    public NavigationService(Page page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    public async Task GoBackAsync()
    {
        try
        {
            await _page.Navigation.PopAsync();
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
            await _page.Navigation.PushAsync(page);
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
            await _page.Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to navigate", ex);
        }
    }

    public async Task ShowAlertAsync(string title, string message, string accept)
    {
        await _page.DisplayAlert(title, message, accept);
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        return await _page.DisplayAlert(title, message, accept, cancel);
    }

    public async Task ShowErrorAsync(string message, Exception? exception = null)
    {
        var fullMessage = exception != null
            ? $"{message}: {exception.Message}"
            : message;

        await ShowAlertAsync("Error", fullMessage, "OK");
    }
}