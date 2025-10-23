namespace WorkMood.MauiApp.Shims;

/// <summary>
/// Abstraction interface for Page dialog operations.
/// Provides testable wrapper around Page.DisplayAlert API.
/// </summary>
public interface IPageDialogShim
{
    /// <summary>
    /// Displays an alert dialog with a title, message, and single accept button.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="accept">The text for the accept button.</param>
    /// <returns>A task representing the asynchronous dialog operation.</returns>
    Task DisplayAlertAsync(string title, string message, string accept);

    /// <summary>
    /// Displays a confirmation dialog with a title, message, and two buttons.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="accept">The text for the accept button.</param>
    /// <param name="cancel">The text for the cancel button.</param>
    /// <returns>A task representing the asynchronous dialog operation. Returns true if accept was pressed, false if cancel was pressed.</returns>
    Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel);
}