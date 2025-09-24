namespace WorkMood.MauiApp.Services;

/// <summary>
/// Interface for navigation operations following the Dependency Inversion Principle
/// Provides generic navigation helpers with centralized error handling
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates back to the previous page
    /// </summary>
    Task GoBackAsync();

    /// <summary>
    /// Navigates to the specified page instance
    /// </summary>
    Task NavigateAsync(Page page);

    /// <summary>
    /// Navigates to a page created by the provided factory
    /// </summary>
    Task NavigateAsync(Func<Page> pageFactory);

    /// <summary>
    /// Shows an alert dialog
    /// </summary>
    /// <param name="title">Alert title</param>
    /// <param name="message">Alert message</param>
    /// <param name="accept">Accept button text</param>
    Task ShowAlertAsync(string title, string message, string accept);

    /// <summary>
    /// Shows a confirmation dialog
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    /// <param name="accept">Accept button text</param>
    /// <param name="cancel">Cancel button text</param>
    /// <returns>True if user accepted, false if cancelled</returns>
    Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel);

    /// <summary>
    /// Shows an error alert with standardized formatting
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="exception">Optional exception for additional context</param>
    Task ShowErrorAsync(string message, Exception? exception = null);
}