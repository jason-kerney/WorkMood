using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Infrastructure;

/// <summary>
/// Base page class that provides common functionality for all pages
/// Follows the Template Method pattern and DRY principle
/// </summary>
public abstract class BasePage : ContentPage
{
    protected INavigationService NavigationService { get; private set; }
    private readonly List<IDisposable> _subscriptions = new();

    protected BasePage(INavigationService? navigationService = null)
    {
        NavigationService = navigationService ?? new NavigationService(this);
    }

    /// <summary>
    /// Initializes the page after XAML is loaded
    /// Call this from derived classes after InitializeComponent()
    /// </summary>
    protected void InitializeBasePage()
    {
        // No special handling necessary; ensure hooks are set up
        SetupViewModelBindings();
        SetupEventSubscriptions();
    }

    /// <summary>
    /// Template method for initializing the page
    /// Derived classes can override specific initialization steps
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            await OnPageAppearingAsync();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync("Failed to initialize page", ex);
        }
    }

    /// <summary>
    /// Template method for page cleanup
    /// Derived classes can override specific cleanup steps
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        try
        {
            OnPageDisappearing();
        }
        catch (Exception ex)
        {
            // Log error but don't show UI as page is disappearing
            System.Diagnostics.Debug.WriteLine($"Error during page cleanup: {ex}");
        }
    }

    /// <summary>
    /// Virtual method for page-specific appearing logic
    /// Override in derived classes as needed
    /// </summary>
    protected virtual Task OnPageAppearingAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Virtual method for page-specific disappearing logic
    /// Override in derived classes as needed
    /// </summary>
    protected virtual void OnPageDisappearing()
    {
        // Clean up subscriptions to prevent memory leaks
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
        _subscriptions.Clear();
    }

    /// <summary>
    /// Centralized error handling for all pages
    /// </summary>
    protected virtual async Task HandleErrorAsync(string message, Exception? exception = null)
    {
        await NavigationService.ShowErrorAsync(message, exception);
    }

    /// <summary>
    /// Helper method to track disposable subscriptions
    /// Automatically disposed when page disappears
    /// </summary>
    protected void AddSubscription(IDisposable subscription)
    {
        _subscriptions.Add(subscription);
    }

    /// <summary>
    /// Template method for setting up ViewModel bindings
    /// Override in derived classes to implement specific binding logic
    /// </summary>
    protected virtual void SetupViewModelBindings()
    {
        // Default implementation - override in derived classes
    }

    /// <summary>
    /// Template method for setting up event subscriptions
    /// Override in derived classes to implement specific event handling
    /// </summary>
    protected virtual void SetupEventSubscriptions()
    {
        // Default implementation - override in derived classes
    }
}